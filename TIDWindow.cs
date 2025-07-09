using Dapplo.Microsoft.Extensions.Hosting.WinForms;
using System.Collections.ObjectModel;
using OpenIddict.Client;
using TrainCrewTIDWindow.Communications;
using TrainCrewTIDWindow.Manager;
using TrainCrewTIDWindow.Models;
using System.Diagnostics;
using System.Windows.Input;

namespace TrainCrewTIDWindow {

    public partial class TIDWindow : Form, IWinFormsShell {


        /// <summary>
        /// TIDManagerオブジェクト
        /// </summary>
        private readonly TIDManager displayManager;

        /// <summary>
        /// TrackManagerオブジェクト
        /// </summary>
        private readonly TrackManager trackManager;

        /// <summary>
        /// サーバから取得した転轍器の情報
        /// </summary>
        private readonly Dictionary<string, PointData> pointDataDict = [];

        /// <summary>
        /// 方向てこの状態
        /// </summary>
        private readonly Dictionary<string, LCR> directionDataDict = [];

        /// <summary>
        /// TRAIN CREW本体接続用
        /// </summary>
        private TrainCrewCommunication tcCommunication = new TrainCrewCommunication();

        /// <summary>
        /// サーバ接続用
        /// </summary>
        private ServerCommunication? serverCommunication;

        /// <summary>
        /// データの取得元（traincrew/server、もしくはサーバのURL）
        /// </summary>
        private string source = "";

        private bool topMostSetting = false;

        /// <summary>
        /// 現実との時差
        /// </summary>
        public TimeSpan TimeOffset {
            get;
            private set;
        } = new(14, 0, 0);

        private int showOffset = 0;

        public int TIDScale {
            get;
            private set;
        } = 100;


        private OpenIddictClientService service;

        public string LabelStatusText {
            get => labelStatus.Text;
            set {
                if (InvokeRequired) {
                    Invoke(() => labelStatus.Text = value);
                }
                else {
                    labelStatus.Text = value;
                }
            }
        }

        public void SetLabelStatusText(string text) {
            labelStatus.Text = text;
        }

        public ReadOnlyDictionary<string, TrackData> TrackDataDict => trackManager.TrackDataDict;

        public ReadOnlyDictionary<string, PointData> PointDataDict => pointDataDict.AsReadOnly();

        public ReadOnlyDictionary<string, LCR> DirectionDataDict => directionDataDict.AsReadOnly();

        public TrackManager TrackManager => trackManager;

        public TIDWindow(OpenIddictClientService service) {
            this.service = service;
            InitializeComponent();

            displayManager = new TIDManager(pictureBox1, this);

            trackManager = new TrackManager(displayManager);

            try {
                using var sr = new StreamReader(".\\setting\\setting.txt");
                var line = sr.ReadLine();
                while (line != null) {
                    var texts = line.Replace(" ", "").Split('=');
                    line = sr.ReadLine();

                    if (texts.Length < 2 || texts.Any(t => t == "")) {
                        continue;
                    }

                    switch (texts[0]) {
                        case "source":
                            source = texts[1];
                            break;
                        case "topMost":
                            topMostSetting = texts[1].ToLower() == "true";
                            break;
                    }
                }
            }
            catch {
            }


            Load += TIDWindow_Load;
        }


        private async void TIDWindow_Load(object? sender, EventArgs? e) {
            _ = Task.Run(ClockUpdateLoop);

            var s = source.Replace(" ", "").ToLower();

            if (s == "select") {
                DialogResult result = MessageBox.Show($"TIDをサーバに接続しますか？\n（いいえを選択するとTRAIN CREW本体に接続します）", "接続先選択 | TID", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (result == DialogResult.Yes) {
                    s = "server";
                }
                else {
                    s = "traincrew";
                }
            }

            SetTopMost(topMostSetting);


            switch (s) {
                case "traincrew":
                    tcCommunication.ConnectionStatusChanged += UpdateConnectionStatus;
                    tcCommunication.TCDataUpdated += UpdateTCData;
                    await TryConnectTrainCrew();
                    break;
                default:
                    /*trackManager.CountStart = 0;*/

                    //デフォルトのサーバへの接続処理
                    serverCommunication = new(this, ServerAddress.SignalAddress, service);
                    serverCommunication.DataUpdated += UpdateServerData;
                    await TryConnectServer();
                    break;
            }
        }

        /// <summary>
        /// TRAIN CREW本体と接続する
        /// </summary>
        /// <returns></returns>
        private async Task TryConnectTrainCrew() {
            //引数にはallの他、trackcircuit, signal, trainが使えます。
            tcCommunication.Request = ["trackcircuit"];
            await tcCommunication.TryConnectWebSocket();
        }

        /// <summary>
        /// 運転会サーバと接続する
        /// </summary>
        /// <param name="url">接続先のURL</param>
        /// <returns></returns>
        private async Task TryConnectServer() {
            if (serverCommunication != null) {
                await serverCommunication.Authorize();
            }
        }


        private void UpdateConnectionStatus(string status) {
            labelStatus.Text = status;
        }

        /// <summary>
        /// TRAIN CREW本体からのデータが更新された際に呼ばれる
        /// </summary>
        /// <param name="tcData"></param>
        private void UpdateTCData(TrainCrewStateData tcData) {
            var tcList = tcData.trackCircuitList;
            if (tcList == null) {
                return;
            }
            if (trackManager.UpdateTCData(tcList)) {
                displayManager.UpdateTID();
            }
        }

        /// <summary>
        /// サーバからのデータが更新された際に呼ばれる
        /// </summary>
        /// <param name="tcData"></param>
        private void UpdateServerData(ConstantDataToServer? data) {
            if (data == null) {
                return;
            }
            var tcList = data.TrackCircuitDatas;
            var sList = data.SwitchDatas;
            var dList = data.DirectionDatas;

            if (tcList != null && trackManager.UpdateTCData(tcList) || sList != null && UpdatePointData(sList) || dList != null && UpdateDirectionData(dList)) {
                displayManager.UpdateTID();
            }
        }

        private bool UpdatePointData(List<SwitchData> switchData) {
            var updatedTID = false;
            lock (pointDataDict) {
                foreach (var s in switchData) {
                    if (!pointDataDict.TryAdd(s.Name, new PointData(s.Name, s.State != NRC.Center, s.State == NRC.Reversed))) {
                        updatedTID |= pointDataDict[s.Name].SetStates(s.State != NRC.Center, s.State == NRC.Reversed);
                    }
                    else {
                        updatedTID = true;
                    }
                }
            }
            return updatedTID;

        }

        private bool UpdateDirectionData(List<DirectionData> directionData) {
            var updatedTID = false;
            lock (directionDataDict) {
                foreach (var d in directionData) {
                    if (!directionDataDict.TryAdd(d.Name, d.State)) {
                        updatedTID |= directionDataDict[d.Name] != d.State;
                        directionDataDict[d.Name] = d.State;
                    }
                    else {
                        updatedTID = true;
                    }
                }
            }
            return updatedTID;
        }

        private async void ClockUpdateLoop() {
            try {
                while (true) {
                    var timer = Task.Delay(10);
                    if (InvokeRequired) {
                        Invoke(new Action(UpdateClock));
                    }
                    else {
                        UpdateClock();
                    }
                    await timer;
                }
            }
            catch (ObjectDisposedException) {
            }
        }

        private void UpdateClock() {
            var time = DateTime.Now;
            if (showOffset > 0) {
                showOffset--;
            }
            else {
                labelClock.Text = (time + TimeOffset).ToString("H:mm:ss");
            }
            if (serverCommunication == null) {
                return;
            }
            var updatedTime = serverCommunication.UpdatedTime;
            if (updatedTime == null) {
                return;
            }
            var delaySeconds = (time - (DateTime)updatedTime).TotalSeconds;
            updatedTime = updatedTime?.Add(TimeOffset);
            if (delaySeconds > 10) {
                if (!serverCommunication.Error) {
                    serverCommunication.Error = true;
                    LabelStatusText = $"Status：データ受信不能(最終受信：{updatedTime?.ToString("H:mm:ss")})";
                    Debug.WriteLine($"データ受信不能: {delaySeconds}");
                    TaskDialog.ShowDialog(new TaskDialogPage {
                        Caption = "データ受信不能 | TID - ダイヤ運転会",
                        Heading = "データ受信不能",
                        Icon = TaskDialogIcon.Error,
                        Text = "サーバ側からのデータ受信が10秒以上ありませんでした。\n復旧を試みますが、しばらく経っても復旧しない場合はアプリケーションの再起動をおすすめします。"
                    });
                }
            }
            else if (delaySeconds > 1) {
                LabelStatusText = $"Status：データ正常受信(最終受信：{updatedTime?.ToString("H:mm:ss")})";
                Debug.WriteLine($"データ受信不能: {delaySeconds}");
            }
        }

        private void labelClock_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e) {
            var hour = TimeOffset.Hours;
            var min = TimeOffset.Minutes;
            var sec = TimeOffset.Seconds;
            if (e.Button == MouseButtons.Right) {
                if ((int)Keyboard.GetKeyStates(Key.LeftCtrl) % 2 == 0 && (int)Keyboard.GetKeyStates(Key.LeftCtrl) % 2 == 0) {
                    hour++;
                }
                else if ((int)Keyboard.GetKeyStates(Key.LeftShift) % 2 == 0 && (int)Keyboard.GetKeyStates(Key.RightShift) % 2 == 0) {
                    min++;
                    if (min >= 60) {
                        hour++;
                    }
                    showOffset = 40;
                }
                else {
                    sec++;
                    if (sec >= 60) {
                        min++;
                        if (min >= 60) {
                            hour++;
                        }
                    }
                    showOffset = 40;
                }
            }
            else if (e.Button == MouseButtons.Left) {
                if ((int)Keyboard.GetKeyStates(Key.LeftCtrl) % 2 == 0 && (int)Keyboard.GetKeyStates(Key.RightCtrl) % 2 == 0) {
                    hour += 23;
                }
                else if ((int)Keyboard.GetKeyStates(Key.LeftShift) % 2 == 0 && (int)Keyboard.GetKeyStates(Key.RightShift) % 2 == 0) {
                    if (min == 0) {
                        hour += 23;
                    }
                    min += 59;
                    showOffset = 40;
                }
                else {
                    if (sec == 0) {
                        if (min == 0) {
                            hour += 23;
                        }
                        min += 59;
                    }
                    sec += 59;
                    showOffset = 40;
                }
            }
            else {
                return;
            }
            TimeOffset = new TimeSpan(hour % 24, min % 60, sec % 60);
            if (showOffset > 0) {
                labelClock.Text = $"+{TimeOffset.Hours}h{TimeOffset.Minutes}m{TimeOffset.Seconds}s";
            }
            var key = Keyboard.GetKeyStates(Key.LeftCtrl);
        }

        private void labelTopMost_Click(object sender, EventArgs e) {
            SetTopMost(!TopMost);
        }

        private void SetTopMost(bool topMost) {
            TopMost = topMost;
            labelTopMost.Text = $"最前面：{(topMost ? "ON" : "OFF")}";
            labelTopMost.ForeColor = topMost ? Color.Yellow : Color.Gray;
        }

        private void labelTopMost_Hover(object sender, EventArgs e) {
            labelTopMost.BackColor = Color.FromArgb(55, 55, 55);
        }

        private void labelTopMost_Leave(object sender, EventArgs e) {
            labelTopMost.BackColor = Color.FromArgb(30, 30, 30);
        }

        private void menuItemCopy_Click(object sender, EventArgs e) {
            displayManager.CopyImage();
        }

        private void SetScale(int scale) {
            if (scale < 50) {
                scale = 50;
            }
            if (scale > 200) {
                scale = 200;
            }

            menuItemScale50.Text = "50%";
            menuItemScale75.Text = "75%";
            menuItemScale100.Text = "100%";
            menuItemScale125.Text = "125%";
            menuItemScale150.Text = "150%";
            menuItemScale175.Text = "175%";
            menuItemScale200.Text = "200%";

            switch (scale) {
                case 50:
                    menuItemScale50.Text = "50%（現在）";
                    break;
                case 75:
                    menuItemScale75.Text = "75%（現在）";
                    break;
                case 100:
                    menuItemScale100.Text = "100%（現在）";
                    break;
                case 125:
                    menuItemScale125.Text = "125%（現在）";
                    break;
                case 150:
                    menuItemScale150.Text = "150%（現在）";
                    break;
                case 175:
                    menuItemScale175.Text = "175%（現在）";
                    break;
                case 200:
                    menuItemScale200.Text = "200%（現在）";
                    break;
            }

            TIDScale = scale;
            labelScale.Text = $"Scale：{scale}%";

            displayManager.ChangeScale();
        }



        private void menuItemScale50_Click(object sender, EventArgs e) {
            SetScale(50);
        }

        private void menuItemScale75_Click(object sender, EventArgs e) {
            SetScale(75);
        }

        private void menuItemScale100_Click(object sender, EventArgs e) {
            SetScale(100);
        }

        private void menuItemScale125_Click(object sender, EventArgs e) {
            SetScale(125);
        }

        private void menuItemScale150_Click(object sender, EventArgs e) {
            SetScale(150);
        }

        private void menuItemScale175_Click(object sender, EventArgs e) {
            SetScale(175);
        }

        private void menuItemScale200_Click(object sender, EventArgs e) {
            SetScale(200);
        }

        private void labelScale_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e) {
            if (e.Button == MouseButtons.Right) {
                SetScale(TIDScale + 25);
            }
            else if (e.Button == MouseButtons.Left) {
                SetScale(TIDScale - 25);
            }
        }

        private void TIDWindow_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e) {
            if (e.KeyData == (Keys.C | Keys.Control)) {
                displayManager.CopyImage();
            }
        }

        private void PictureBox1_MouseWheel(object sender, System.Windows.Forms.MouseEventArgs e) {
            if (ModifierKeys.HasFlag(Keys.Control)) {
                if (e.Delta > 0) {
                    SetScale(TIDScale + 25);
                }
                else {
                    SetScale(TIDScale - 25);
                }
            }
            else if (ModifierKeys.HasFlag(Keys.Shift)) {
                panel1.AutoScrollPosition = new Point(panel1.HorizontalScroll.Value - e.Delta, panel1.VerticalScroll.Value);
            }
            else {
                panel1.AutoScrollPosition = new Point(panel1.HorizontalScroll.Value, panel1.VerticalScroll.Value - e.Delta);
            }
            ((HandledMouseEventArgs)e).Handled = true;
        }
    }
}

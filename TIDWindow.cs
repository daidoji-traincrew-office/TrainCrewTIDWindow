using Dapplo.Microsoft.Extensions.Hosting.WinForms;
using System.Collections.ObjectModel;
using OpenIddict.Client;
using TrainCrewTIDWindow.Communications;
using TrainCrewTIDWindow.Manager;
using TrainCrewTIDWindow.Models;
using System.Diagnostics;
using System.Text;

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
        /// データの取得元（traincrew/server/select）
        /// </summary>
        private string source = "select";

        /// <summary>
        /// 最前面表示であるか
        /// </summary>
        private bool topMostSetting = true;

        /// <summary>
        /// 表示される時刻の時差を足す前
        /// </summary>
        public DateTime Clock {
            get;
            set;
        }

        /// <summary>
        /// 現実との時差
        /// </summary>
        public TimeSpan TimeOffset {
            get;
            private set;
        } = new(14, 0, 0);

        /// <summary>
        /// 時差を表示するか（0は表示せずそれ以外は0までのカウントダウン）
        /// </summary>
        private int showOffset = 0;

        /// <summary>
        /// 拡大率（0未満はフィット表示）
        /// </summary>
        public int TIDScale {
            get;
            private set;
        } = 100;

        /// <summary>
        /// マウス位置（ドラッグ操作対応用）
        /// </summary>
        private Point mouseLoc = Point.Empty;

        /// <summary>
        /// WASDキーなど使用時の移動量
        /// </summary>
        private int scrollDelta = 15;

        /// <summary>
        /// デバッグモード参照軌道回路管理用（-1は非デバッグモード）
        /// </summary>
        private int debugIndex = -1;

        /// <summary>
        /// デバッグモード表示時間管理用（正数:カウントダウン中 0:更新待ち -10000:カウントダウン停止中で更新待ち -10000未満:初期状態 その他負数:カウントダウン停止中）
        /// </summary>
        private int debugCount = -99999;

        private OpenIddictClientService service;

        public string LabelStatusText {
            get => labelStatus.Text;
            set {
                if(serverCommunication != null) {
                    value = $"Status：{(ServerAddress.SignalAddress.Contains("dev") ? "Devサーバ" : "Prodサーバ")} {value}";
                }
                else {
                    value = $"Status：{value}";
                }
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

            var loaded = false;

            loaded |= LoadSetting(Environment.GetFolderPath(Environment.SpecialFolder.Personal) + @"\TRAIN CREW Tool\TrainCrewTIDWindow\setting.txt");

            loaded |= LoadSetting(".\\setting\\setting.txt");

            if (!loaded) {
                using (StreamWriter w = new(".\\setting\\setting.txt", false, new UTF8Encoding(false))) {
                    w.Write("source=select\ntopMost=true\nscale=100\ntimeOffset=14");
                }
            }

            displayManager = new TIDManager(pictureBox1, this);

            if (TIDScale > 0) {
                labelScale.ForeColor = Color.White;
                labelScale.Text = $"Scale：{TIDScale}%";
            }
            else {
                labelScale.ForeColor = Color.LightGreen;
                labelScale.Text = $"Scale：{(int)((double)pictureBox1.Image.Width / displayManager.OriginalBitmap.Width * 100 + 0.5)}%";
            }

            trackManager = new TrackManager(displayManager);

            Load += TIDWindow_Load;
            menuItemScale50.Click += (sender, e) => { SetScale(50); };
            menuItemScale75.Click += (sender, e) => { SetScale(75); };
            menuItemScale90.Click += (sender, e) => { SetScale(90); };
            menuItemScale100.Click += (sender, e) => { SetScale(100); };
            menuItemScale110.Click += (sender, e) => { SetScale(110); };
            menuItemScale125.Click += (sender, e) => { SetScale(125); };
            menuItemScale150.Click += (sender, e) => { SetScale(150); };
            menuItemScale175.Click += (sender, e) => { SetScale(175); };
            menuItemScale200.Click += (sender, e) => { SetScale(200); };
            menuItemScaleFit.Click += (sender, e) => { SetScale(-1); };
        }

        private bool LoadSetting(string path) {

            try {
                if (!File.Exists(path)) {
                    return false;
                }
                using var sr = new StreamReader(path);
                var line = sr.ReadLine();
                while (line != null) {
                    var texts = line.Replace(" ", "").Split('=');
                    line = sr.ReadLine();

                    if (texts.Length < 2 || texts.Any(t => t == "")) {
                        continue;
                    }

                    switch (texts[0]) {
                        case "source":
                            source = texts[1].Replace(" ", "").ToLower();
                            break;
                        case "topMost":
                            topMostSetting = texts[1].ToLower() == "true";
                            break;
                        case "scale":
                            menuItemScale50.Text = "50%";
                            menuItemScale75.Text = "75%";
                            menuItemScale90.Text = "90%";
                            menuItemScale100.Text = "100%";
                            menuItemScale110.Text = "110%";
                            menuItemScale125.Text = "125%";
                            menuItemScale150.Text = "150%";
                            menuItemScale175.Text = "175%";
                            menuItemScale200.Text = "200%";
                            menuItemScaleFit.Text = "フィット表示";

                            if (texts[1].ToLower() == "fit") {
                                TIDScale = -1;
                                menuItemScaleFit.Text = "フィット表示（現在）";
                                break;
                            }
                            switch (texts[1]) {
                                case "50":
                                    TIDScale = 50;
                                    menuItemScale50.Text = "50%（現在）";
                                    break;
                                case "75":
                                    TIDScale = 75;
                                    menuItemScale75.Text = "75%（現在）";
                                    break;
                                case "90":
                                    TIDScale = 90;
                                    menuItemScale90.Text = "90%（現在）";
                                    break;
                                case "100":
                                    TIDScale = 100;
                                    menuItemScale100.Text = "100%（現在）";
                                    break;
                                case "110":
                                    TIDScale = 110;
                                    menuItemScale110.Text = "110%（現在）";
                                    break;
                                case "125":
                                    TIDScale = 125;
                                    menuItemScale125.Text = "125%（現在）";
                                    break;
                                case "150":
                                    TIDScale = 150;
                                    menuItemScale150.Text = "150%（現在）";
                                    break;
                                case "175":
                                    TIDScale = 175;
                                    menuItemScale175.Text = "175%（現在）";
                                    break;
                                case "200":
                                    TIDScale = 200;
                                    menuItemScale200.Text = "200%（現在）";
                                    break;
                            }
                            break;
                        case "timeOffset":
                            if (int.TryParse(texts[1], out var hours)) {
                                TimeOffset = new TimeSpan(((hours % 24) + 24) % 24, 0, 0);
                            }
                            break;
                    }
                }
            }
            catch {
            }
            return true;
        }


        private async void TIDWindow_Load(object? sender, EventArgs? e) {
            _ = Task.Run(ClockUpdateLoop);

            var s = source;

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
                    TimeOffset = new(0, 0, 0);
                    tcCommunication.ConnectionStatusChanged += UpdateConnectionStatus;
                    tcCommunication.TCDataUpdated += UpdateTCData;
                    await TryConnectTrainCrew();
                    break;
                case "debug":
                    debugIndex = 0;
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
            if (showOffset <= 0) {
                var now = DateTime.Now;
                Clock = new DateTime(now.Year, now.Month, now.Day, tcData.nowTime.hour, tcData.nowTime.minute, (int)tcData.nowTime.second);
                if (showOffset <= 0) {
                    labelClock.Text = (Clock + TimeOffset).ToString("H:mm:ss");
                }
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
            if (showOffset > 0) {
                showOffset--;
            }
            UpdateDebug();
            if (debugIndex < 0 && serverCommunication == null) {
                return;
            }
            Clock = DateTime.Now;
            if (showOffset <= 0) {
                labelClock.Text = (Clock + TimeOffset).ToString("H:mm:ss");
            }
            var updatedTime = serverCommunication?.UpdatedTime;
            if (updatedTime == null || serverCommunication == null) {
                return;
            }
            var delaySeconds = (Clock - (DateTime)updatedTime).TotalSeconds;
            updatedTime = updatedTime?.Add(TimeOffset);
            if (delaySeconds > 10) {
                if (!serverCommunication.Error) {
                    serverCommunication.Error = true;
                    LabelStatusText = $"データ受信不能(最終受信：{updatedTime?.ToString("H:mm:ss")})";
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
                LabelStatusText = $"データ正常受信(最終受信：{updatedTime?.ToString("H:mm:ss")})";
                Debug.WriteLine($"データ受信不能: {delaySeconds}");
            }
        }

        private void UpdateDebug(bool reversed = false) {
            if (debugIndex >= 0) {
                if (debugCount == 0 || debugCount <= -10000) {
                    var lineData = displayManager.LineSettings;

                    var line = lineData[debugIndex % lineData.Count];
                    if (debugCount >= 0 || debugCount == -10000) {
                        if (line.PointName != "") {
                            UpdatePointData(new List<SwitchData> { new() { Name = line.PointName, State = NRC.Center } });
                        }
                        if (reversed) {
                            debugIndex = (debugIndex + lineData.Count * 2 - 1) % (lineData.Count * 2);
                        }
                        else {
                            debugIndex = (debugIndex + 1) % (lineData.Count * 2);
                        }
                    }
                    line = lineData[debugIndex % lineData.Count];
                    trackManager.UpdateTCData(new List<TrackCircuitData> { new TrackCircuitData() { Name = line.TrackName, Last = debugIndex < lineData.Count ? "1111" : "1112", On = true } });
                    if (line.PointName != "") {
                        UpdatePointData(new List<SwitchData> { new SwitchData() { Name = line.PointName, State = line.Reversed ? NRC.Reversed : NRC.Normal } });
                        LabelStatusText = $"デバッグモード（{(debugIndex < lineData.Count ? "下り" : "上り")}） track: {line.TrackName}  switch: {line.PointName} {(line.Reversed ? "R" : "N")}";
                    }
                    else {
                        LabelStatusText = $"デバッグモード（{(debugIndex < lineData.Count ? "下り" : "上り")}） track: {line.TrackName}";
                    }
                    displayManager.UpdateTID();
                    debugCount = debugCount == -10000 ? -100 : 100;
                }
                if (debugCount > 0) {
                    debugCount--;
                }
            }
        }

        private void labelClock_MouseDown(object sender, MouseEventArgs e) {
            if(e.Button != MouseButtons.Left && e.Button != MouseButtons.Right) {
                return;
            }
            ChangeTime(e.Button == MouseButtons.Right, !ModifierKeys.HasFlag(Keys.Control), !ModifierKeys.HasFlag(Keys.Shift));
        }

        private void ChangeTime(bool isPlus, bool changeHours, bool changeMinutes) {
            var hour = TimeOffset.Hours;
            var min = TimeOffset.Minutes;
            var sec = TimeOffset.Seconds;
            if (isPlus) {
                if (changeHours) {
                    hour++;
                }
                else if (changeMinutes) {
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
            else {
                if (changeHours) {
                    hour += 23;
                }
                else if (changeMinutes) {
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
            TimeOffset = new TimeSpan(hour % 24, min % 60, sec % 60);
            if (showOffset > 0) {
                labelClock.Text = $"+{TimeOffset.Hours}h{TimeOffset.Minutes}m{TimeOffset.Seconds}s";
            }
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
            if (scale < 50 && scale != -1) {
                scale = 50;
            }
            if (scale > 200) {
                scale = 200;
            }

            menuItemScale50.Text = "50%";
            menuItemScale75.Text = "75%";
            menuItemScale90.Text = "90%";
            menuItemScale100.Text = "100%";
            menuItemScale110.Text = "110%";
            menuItemScale125.Text = "125%";
            menuItemScale150.Text = "150%";
            menuItemScale175.Text = "175%";
            menuItemScale200.Text = "200%";
            menuItemScaleFit.Text = "フィット表示";

            switch (scale) {
                case 50:
                    menuItemScale50.Text = "50%（現在）";
                    break;
                case 75:
                    menuItemScale75.Text = "75%（現在）";
                    break;
                case 90:
                    menuItemScale90.Text = "90%（現在）";
                    break;
                case 100:
                    menuItemScale100.Text = "100%（現在）";
                    break;
                case 110:
                    menuItemScale110.Text = "110%（現在）";
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
                case -1:
                    menuItemScaleFit.Text = "フィット表示（現在）";
                    break;

            }

            TIDScale = scale;

            displayManager.ChangeScale();
            if (scale > 0) {
                labelScale.ForeColor = Color.White;
                labelScale.Text = $"Scale：{scale}%";
                pictureBox1.Cursor = Cursors.SizeAll;
            }
            else {
                labelScale.ForeColor = Color.LightGreen;
                labelScale.Text = $"Scale：{(int)((double)pictureBox1.Image.Width / displayManager.OriginalBitmap.Width * 100 + 0.5)}%";
                pictureBox1.Cursor = Cursors.Default;
            }
        }




        private void labelScale_MouseDown(object sender, MouseEventArgs e) {
            if(TIDScale > 0) {
                if (e.Button == MouseButtons.Right) {
                    switch (TIDScale) {
                        case 75:
                        case 110:
                            SetScale(TIDScale + 15);
                            break;
                        case 90:
                        case 100:
                            SetScale(TIDScale + 10);
                            break;
                        default:
                            SetScale(TIDScale + 25);
                            break;
                    }
                }
                else if (e.Button == MouseButtons.Left) {
                    switch (TIDScale) {
                        case 90:
                        case 125:
                            SetScale(TIDScale - 15);
                            break;
                        case 100:
                        case 110:
                            SetScale(TIDScale - 10);
                            break;
                        default:
                            SetScale(TIDScale - 25);
                            break;
                    }
                }
            }
        }

        private void TIDWindow_KeyDown(object sender, KeyEventArgs e) {
            var code = e.KeyData & Keys.KeyCode;
            var mod = e.KeyData & Keys.Modifiers;
            if (e.KeyData == (Keys.C | Keys.Control)) {
                if(debugIndex >= 0) {
                    var lineData = displayManager.LineSettings;
                    var line = lineData[debugIndex % lineData.Count];
                    if (line != null) {
                        if(line.PointName != "") {
                            Clipboard.SetText($"\n{line.TrackName}\tS\t列番位置x\t列番位置y\t{line.PointName}\t{(line.Reversed ? "True" : "False")}");
                        }
                        else {
                            Clipboard.SetText($"\n{line.TrackName}\tS\t列番位置x\t列番位置y\t\t");
                        }
                    }
                }
                else {
                    displayManager.CopyImage();
                }
            }
            if(e.KeyData == Keys.Tab) {
                SetTopMost(!TopMost);
            }
            
            if(code == Keys.Right || code == Keys.D) {
                panel1.AutoScrollPosition = new Point(panel1.HorizontalScroll.Value + scrollDelta * (mod == Keys.Shift ? 1 : 3), panel1.VerticalScroll.Value);
            }
            if (code == Keys.Left || code == Keys.A) {
                panel1.AutoScrollPosition = new Point(panel1.HorizontalScroll.Value - scrollDelta * (mod == Keys.Shift ? 1 : 3), panel1.VerticalScroll.Value);
            }
            if (code == Keys.Up || code == Keys.W) {
                panel1.AutoScrollPosition = new Point(panel1.HorizontalScroll.Value, panel1.VerticalScroll.Value - scrollDelta * (mod == Keys.Shift ? 1 : 3));
            }
            if (code == Keys.Down || code == Keys.S) {
                panel1.AutoScrollPosition = new Point(panel1.HorizontalScroll.Value, panel1.VerticalScroll.Value + scrollDelta * (mod == Keys.Shift ? 1 : 3));
            }
            if(e.KeyData == Keys.D1) {
                panel1.AutoScrollPosition = new Point(0, panel1.VerticalScroll.Value);
            }
            if (e.KeyData == Keys.D2) {
                panel1.AutoScrollPosition = new Point((pictureBox1.Size.Width - panel1.Size.Width + 17) * 1 / 9, panel1.VerticalScroll.Value);
            }
            if (e.KeyData == Keys.D3) {
                panel1.AutoScrollPosition = new Point((pictureBox1.Size.Width - panel1.Size.Width + 17) * 2 / 9, panel1.VerticalScroll.Value);
            }
            if (e.KeyData == Keys.D4) {
                panel1.AutoScrollPosition = new Point((pictureBox1.Size.Width - panel1.Size.Width + 17) * 3 / 9, panel1.VerticalScroll.Value);
            }
            if (e.KeyData == Keys.D5) {
                panel1.AutoScrollPosition = new Point((pictureBox1.Size.Width - panel1.Size.Width + 17) * 4 / 9, panel1.VerticalScroll.Value);
            }
            if (e.KeyData == Keys.D6) {
                panel1.AutoScrollPosition = new Point((pictureBox1.Size.Width - panel1.Size.Width + 17) * 5 / 9, panel1.VerticalScroll.Value);
            }
            if (e.KeyData == Keys.D7) {
                panel1.AutoScrollPosition = new Point((pictureBox1.Size.Width - panel1.Size.Width + 17) * 6 / 9, panel1.VerticalScroll.Value);
            }
            if (e.KeyData == Keys.D8) {
                panel1.AutoScrollPosition = new Point((pictureBox1.Size.Width - panel1.Size.Width + 17) * 7 / 9, panel1.VerticalScroll.Value);
            }
            if (e.KeyData == Keys.D9) {
                panel1.AutoScrollPosition = new Point((pictureBox1.Size.Width - panel1.Size.Width + 17) * 8 / 9, panel1.VerticalScroll.Value);
            }
            if (e.KeyData == Keys.D0) {
                panel1.AutoScrollPosition = new Point(pictureBox1.Size.Width - panel1.Size.Width + 17, panel1.VerticalScroll.Value);
            }
            if (e.KeyData == Keys.NumPad7) {
                panel1.AutoScrollPosition = new Point(0, 0);
            }
            if (e.KeyData == Keys.NumPad8) {
                panel1.AutoScrollPosition = new Point((pictureBox1.Size.Width - panel1.Size.Width + 17) / 2, 0);
            }
            if (e.KeyData == Keys.NumPad9) {
                panel1.AutoScrollPosition = new Point(pictureBox1.Size.Width - panel1.Size.Width + 17, 0);
            }
            if (e.KeyData == Keys.NumPad4) {
                panel1.AutoScrollPosition = new Point(0, (pictureBox1.Size.Height - panel1.Size.Height + 17) / 2);
            }
            if (e.KeyData == Keys.NumPad5) {
                panel1.AutoScrollPosition = new Point((pictureBox1.Size.Width - panel1.Size.Width + 17) / 2, (pictureBox1.Size.Height - panel1.Size.Height + 17) / 2);
            }
            if (e.KeyData == Keys.NumPad6) {
                panel1.AutoScrollPosition = new Point(pictureBox1.Size.Width - panel1.Size.Width + 17, (pictureBox1.Size.Height - panel1.Size.Height + 17) / 2);
            }
            if (e.KeyData == Keys.NumPad1) {
                panel1.AutoScrollPosition = new Point(0, pictureBox1.Size.Height - panel1.Size.Height + 17);
            }
            if (e.KeyData == Keys.NumPad2) {
                panel1.AutoScrollPosition = new Point((pictureBox1.Size.Width - panel1.Size.Width + 17) / 2, pictureBox1.Size.Height - panel1.Size.Height + 17);
            }
            if (e.KeyData == Keys.NumPad3) {
                panel1.AutoScrollPosition = new Point(pictureBox1.Size.Width - panel1.Size.Width + 17, pictureBox1.Size.Height - panel1.Size.Height + 17);
            }
            if (debugIndex >= 0) {
                if (e.KeyData == Keys.Enter) {
                    debugCount *= -1;
                    labelStatus.ForeColor = debugCount >= 0 ? Color.White : Color.Orange;
                }
                if (e.KeyData == Keys.PageUp) {
                    debugCount = debugCount >= 0 ? 0 : -10000;
                    UpdateDebug();
                }
                if (e.KeyData == Keys.PageDown) {
                    debugCount = debugCount >= 0 ? 0 : -10000;
                    UpdateDebug(true);
                }
            }
            else {
                if (code == Keys.PageUp || code == Keys.PageDown) {
                    ChangeTime(code == Keys.PageUp, (mod & Keys.Control) != Keys.Control, (mod & Keys.Shift) != Keys.Shift);
                }
            }
            if (code == Keys.Oemplus || code == Keys.OemSemicolon) {
                ChangeTime(code == Keys.OemSemicolon, (mod & Keys.Control) != Keys.Control, (mod & Keys.Shift) != Keys.Shift);
            }

        }

        private void PictureBox1_MouseWheel(object sender, MouseEventArgs e) {
            if (ModifierKeys.HasFlag(Keys.Control)) {
                if(TIDScale > 0) {
                    if (e.Delta > 0) {
                        switch (TIDScale) {
                            case 75:
                            case 110:
                                SetScale(TIDScale + 15);
                                break;
                            case 90:
                            case 100:
                                SetScale(TIDScale + 10);
                                break;
                            default:
                                SetScale(TIDScale + 25);
                                break;
                        }
                    }
                    else {
                        switch (TIDScale) {
                            case 90:
                            case 125:
                                SetScale(TIDScale - 15);
                                break;
                            case 100:
                            case 110:
                                SetScale(TIDScale - 10);
                                break;
                            default:
                                SetScale(TIDScale - 25);
                                break;
                        }
                    }
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

        private void TIDWindow_Resize(object sender, EventArgs e) {
            if(displayManager != null && TIDScale == -1) {
                displayManager.ChangeScale();
                labelScale.Text = $"Scale：{(int)((double)pictureBox1.Image.Width / displayManager.OriginalBitmap.Width * 100 + 0.5)}%";
            }
        }

        private void PictureBox1_MouseDown(object sender, MouseEventArgs e) {
            if (e.Button == MouseButtons.Left) {
                mouseLoc = e.Location;
            }
        }

        private void PictureBox1_MouseMove(object sender, MouseEventArgs e) {
            if (e.Button == MouseButtons.Left) {
                panel1.AutoScrollPosition = new Point(panel1.HorizontalScroll.Value - e.Location.X + mouseLoc.X, panel1.VerticalScroll.Value - e.Location.Y + mouseLoc.Y);
            }
        }

        private void PictureBox1_MouseUp(object sender, MouseEventArgs e) {
            if (e.Button == MouseButtons.Left) {
                mouseLoc = Point.Empty;
            }
        }
    }
}

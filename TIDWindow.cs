using Dapplo.Microsoft.Extensions.Hosting.WinForms;
using System.Collections.ObjectModel;
using OpenIddict.Client;
using TrainCrewTIDWindow.Communications;
using TrainCrewTIDWindow.Manager;
using TrainCrewTIDWindow.Models;
using System.Text.RegularExpressions;

namespace TrainCrewTIDWindow
{

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

        /// <summary>
        /// 現実との時差
        /// </summary>
        private int timeOffset = -10;

        private OpenIddictClientService service;

        public string LabelStatusText {
            get => label1.Text;
            set => label1.Text = value;
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

            if(s == "select") {
                DialogResult result = MessageBox.Show($"TIDをサーバに接続しますか？\n（いいえを選択するとTRAIN CREW本体に接続します）", "接続先選択 | TID", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (result == DialogResult.Yes) {
                    s = "server";
                }
                else {
                    s = "traincrew";
                }
            }

            switch (s) {
                case "traincrew":
                    tcCommunication.ConnectionStatusChanged += UpdateConnectionStatus;
                    tcCommunication.TCDataUpdated += UpdateTCData;
                    await TryConnectTrainCrew();
                    break;
                default:
                    trackManager.CountStart = 0;

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
                await serverCommunication.CheckUserAuthenticationAsync();
            }


        }


        private void UpdateConnectionStatus(string status) {
            label1.Text = status;
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
            if (trackManager.UpdateTCData(tcList) || true/* 受信状況を更新したいので常時更新 */) {
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

            if (tcList != null && trackManager.UpdateTCData(tcList) || sList != null && UpdatePointData(sList) || dList != null && UpdateDirectionData(dList) || true/* 受信状況を更新したいので常時更新 */) {
                displayManager.UpdateTID();
            }
        }

        private bool UpdatePointData(List<SwitchData> switchData) {
            var updatedTID = false;
            foreach (var s in switchData) {
                if (pointDataDict.ContainsKey(s.Name)) {
                    updatedTID |= pointDataDict[s.Name].SetStates(s.State != NRC.Center, s.State == NRC.Reversed);
                }
                else {
                    pointDataDict.Add(s.Name, new PointData(s.Name, s.State != NRC.Center, s.State == NRC.Reversed));
                    updatedTID = true;
                }
            }
            return updatedTID;

        }

        private bool UpdateDirectionData(List<DirectionData> directionData) {
            var updatedTID = false;
            foreach (var d in directionData) {
                if (directionDataDict.ContainsKey(d.Name)) {
                    updatedTID |= directionDataDict[d.Name] != d.State;
                    directionDataDict[d.Name] = d.State;
                }
                else {
                    directionDataDict.Add(d.Name, d.State);
                    updatedTID = true;
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
            catch (ObjectDisposedException ex) {
            }
        }

        private void UpdateClock() {
            var time = DateTime.Now.AddHours(timeOffset);
            label2.Text = time.ToString("H:mm:ss");
        }

        private void label2_MouseDown(object sender, MouseEventArgs e) {
            if (e.Button == MouseButtons.Right) {
                timeOffset++;
            }
            else if(e.Button == MouseButtons.Left) {
                timeOffset--;
            }
        }

    }
}

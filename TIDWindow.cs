using Microsoft.VisualBasic;
using System.Collections.ObjectModel;
using System.Drawing.Imaging;
using System.Text.RegularExpressions;
using TrainCrewTIDWindow.Communications;
using TrainCrewTIDWindow.Manager;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ToolTip;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TrackBar;

namespace TrainCrewTIDWindow
{

    public partial class TIDWindow : Form {


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
        /// TRAIN CREW本体接続用
        /// </summary>
        private TrainCrewCommunication communication = new TrainCrewCommunication();

        /// <summary>
        /// データの取得元（traincrew/server、もしくはサーバのURL）
        /// </summary>
        private string source = "";

        /// <summary>
        /// 現実との時差
        /// </summary>
        private int timeDifference = -10;

        public ReadOnlyDictionary<string, TrackData> TrackDataDict => trackManager.TrackDataDict;

        public ReadOnlyDictionary<string, PointData> PointDataDict => pointDataDict.AsReadOnly();

        public TrackManager TrackManager => trackManager;

        public TIDWindow() {
            InitializeComponent();

            displayManager = new TIDManager(pictureBox1, this);

            trackManager = new TrackManager(displayManager);

            try {
                using var sr = new StreamReader(".\\setting\\setting.txt");
                var line = sr.ReadLine();
                while (line != null) {
                    var texts = line.Split('=');
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


            if (source == "traincrew") {
                communication.ConnectionStatusChanged += UpdateConnectionStatus;
                communication.TCDataUpdated += UpdateTCData;
            }
            Load += TIDWindow_Load;
        }


        private async void TIDWindow_Load(object? sender, EventArgs? e) {
            _ = Task.Run(ClockUpdateLoop);

            switch (source) {
                case "traincrew":
                    await TryConnectTrainCrew();
                    break;
                case "server":
                    //デフォルトのサーバへの接続処理
                    await TryConnectServer(ServerAddress.SignalAddress);
                    break;
                default:
                    //指定した任意のサーバへの接続処理
                    await TryConnectServer(source);
                    break;
            }
        }

        /// <summary>
        /// TRAIN CREW本体と接続する
        /// </summary>
        /// <returns></returns>
        private async Task TryConnectTrainCrew() {
            //引数にはallの他、trackcircuit, signal, trainが使えます。
            communication.Request = ["trackcircuit"];
            await communication.TryConnectWebSocket();
        }

        /// <summary>
        /// 運転会サーバと接続する
        /// </summary>
        /// <param name="url">接続先のURL</param>
        /// <returns></returns>
        private async Task TryConnectServer(string url) {

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
            if (trackManager.UpdateTCData(tcList) || true) {
                displayManager.UpdateTID();
            }
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
            var time = DateTime.Now.AddHours(timeDifference);
            label2.Text = time.ToString("H:mm:ss");
        }

        private void label2_MouseDown(object sender, MouseEventArgs e) {
            if (e.Button == MouseButtons.Right) {
                timeDifference++;
            }
            else if(e.Button == MouseButtons.Left) {
                timeDifference--;
            }
        }
    }
}

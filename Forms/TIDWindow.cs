using Dapplo.Microsoft.Extensions.Hosting.WinForms;
using System.Collections.ObjectModel;
using OpenIddict.Client;
using System.Diagnostics;
using System.Text;
using System.Drawing.Drawing2D;
using TrainCrewTIDWindow.Settings;
using System.Media;
using TrainCrewTIDWindow.Communications;
using TrainCrewTIDWindow.Manager;
using TrainCrewTIDWindow.Models;
using System.Text.RegularExpressions;
using System.Linq;

namespace TrainCrewTIDWindow.Forms
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
        /// 列車情報
        /// </summary>
        private readonly Dictionary<string, TrainData> trainDataDict = new() { { "9999", new TrainData("9999", 0, true) } };

        /// <summary>
        /// 右クリックメニューの列車ボタン
        /// </summary>
        private readonly Dictionary<string, ToolStripMenuItem> trainMenuDict = [];

        private readonly Dictionary<int, ToolStripMenuItem> scaleMenuDict = [];



        private readonly Dictionary<string, List<SignalSwitchSetting>> signalSwitchDict = [];

        private readonly Dictionary<string, SignalDirectionSetting> signalDirectionDict = [];

        /// <summary>
        /// TRAIN CREW本体接続用
        /// </summary>
        private TrainCrewCommunication tcCommunication = new TrainCrewCommunication();

        /// <summary>
        /// サーバ接続用
        /// </summary>
        private ServerCommunication? serverCommunication;

        public bool HasServerCommunication => serverCommunication != null;

        /// <summary>
        /// データの取得元（traincrew/server/select）
        /// </summary>
        private string source = "select";

        /// <summary>
        /// 最前面表示であるか
        /// </summary>
        private bool topMostSetting = true;

        /// <summary>
        /// 拡大鏡を使用している状態であるか
        /// </summary>
        private bool usingMagnifyingGlass = false;

        /// <summary>
        /// 拡大鏡をトグル式で表示するか
        /// </summary>
        private bool toggleMagnifyingGlass = false;

        /// <summary>
        /// 拡大鏡の直径
        /// </summary>
        private int magnifyingGlassSize = 240;

        public Panel Panel1 => panel1;

        /// <summary>
        /// 表示される時刻の時差を足す前
        /// </summary>
        public DateTime Clock {
            get;
            set;
        }

        /// <summary>
        /// 現実の時刻
        /// </summary>
        public DateTime RealTime {
            get;
            set;
        } = DateTime.Now;

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

        private int initialScale = 100;

        private int[] scaleArray = { 50, 75, 90, 100, 110, 125, 150, 175, 200 };

        public bool FixedScale {
            get;
            private set;
        } = false;

        /// <summary>
        /// マウス位置（ドラッグ操作対応用）
        /// </summary>
        private Point? mouseLoc = null;

        private Cursor defaultCursor = Cursors.SizeAll;

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

        private SoundPlayer? warningSound = null;

        private bool windowMinimized = false;

        private float flashInterval = 0.5f;

        private float flashState = 0f;

        private Point? selectionStarting = null;

        public int MarkupType {
            get;
            private set;
        } = 0;

        public bool FlashState => flashInterval <= 0 || flashState > flashInterval;

        public bool ReservedUpdate {
            get;
            set;
        } = false;

        public bool MarkupDuplication {
            get;
            private set;
        } = false;

        public bool MarkupFillZero {
            get;
            private set;
        } = false;

        public bool MarkupNotTrain {
            get;
            private set;
        } = false;

        public bool MarkupSpawned {
            get;
            private set;
        } = false;

        public int MarkupDelayed {
            get;
            private set;
        } = 0;

        public bool MarkupHandover {
            get;
            private set;
        } = false;

        public bool HideNumber {
            get;
            private set;
        } = false;

        public bool LockHideNumber {
            get;
            private set;
        } = false;

        public bool UseServerTime {
            get;
            private set;
        } = false;

        public int ServerTime {
            get;
            private set;
        } = 14;

        private List<int> markupUmban = [];

        public ToolStripMenuItem MenuItemMarkupClass => menuItemMarkupClass;

        public bool DetectResize {
            get; set;
        } = false;

        public bool OpeningDialog {
            get;
            set;
        } = false;

        public void PlayWarningSound() {
            if (warningSound != null) {
                warningSound.Play();
            }
            else {
                SystemSounds.Hand.Play();
            }
        }

        public bool Silent { get; private set; } = false;

        private string debugNumberDown = "1111";
        private string debugNumberUp = "1112";
        private int debugDelayDown = 0;
        private int debugDelayUp = 0;

        private OpenIddictClientService service;

        public string LabelStatusText {
            get => labelStatus.Text;
            set {
                if (serverCommunication != null) {
                    value = $"Status：{(ServerAddress.SignalAddress.Contains("dev") ? "Dev" : "Prod")}サーバ {value}";
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

        public ReadOnlyDictionary<string, PointData> PointDataDict { get; init; }

        public ReadOnlyDictionary<string, LCR> DirectionDataDict { get; init; }

        public ReadOnlyDictionary<string, TrainData> TrainDataDict { get; init; }

        public TrackManager TrackManager => trackManager;

        public TIDWindow(OpenIddictClientService service) {
            this.service = service;
            InitializeComponent();
            LogManager.AddInfoLog($"起動 ver. {ServerAddress.Version}");

            pictureBox2.Parent = pictureBox1;
            pictureBox3.Parent = pictureBox1;

            PointDataDict = pointDataDict.AsReadOnly();
            DirectionDataDict = directionDataDict.AsReadOnly();
            TrainDataDict = trainDataDict.AsReadOnly();

            var loaded = false;

            var docuPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal) + @"\TRAIN CREW Tool\TrainCrewTIDWindow";
            var lg = LoadSetting($"{docuPath}\\setting.txt"); ;
            loaded |= lg;
            if (lg) {
                LogManager.AddInfoLog("グローバル設定ファイルを読み込みました");
            }
            var ll = LoadSetting(".\\setting\\setting.txt");
            loaded |= ll;
            if (ll) {
                LogManager.AddInfoLog("ローカル設定ファイルを読み込みました");
            }

            if (!loaded) {
                using (StreamWriter w = new(".\\setting\\setting.txt", false, new UTF8Encoding(false))) {
                    w.Write($"#このファイルは {docuPath} に配置しても動作します。\nsource=select\ntopMost=true\nscaleList=50,75,90,100,110,125,150,175,200\ninitialScale=100\ntimeOffset=14\nzoomMode=pushtozoom\nzoomSize=240\nsilent=false\nflashInterval=0.5\nmarkupType=0\nmarkupDelayed=0\nmarkupDuplication=false\nmarkupFillZero=false\nmarkup9999=false\nmarkupNotTrain=false\nmarkupSpawned=false\nmarkupHandover=false\nhideNumber=false");
                }
                LogManager.AddInfoLog("ローカル設定ファイルを作成しました");

                TaskDialog.ShowDialog(new TaskDialogPage {
                    Caption = "設定ファイル作成 | TID - ダイヤ運転会",
                    Heading = "設定ファイルが作成されました",
                    Icon = TaskDialogIcon.Information,
                    Text = $"{Path.GetFullPath(".\\setting\\setting.txt")}\nに設定ファイルを作成しました。\n設定ファイルを編集することで起動時の設定などが変更できます。"
                });
            }


            if (File.Exists(".\\sound\\warning.wav")) {
                warningSound = new SoundPlayer(".\\sound\\warning.wav");
            }


            foreach (var scale in scaleArray) {
                AddScale(scale);
            }


            if (initialScale < 0) {
                initialScale = -1;
                TIDScale = initialScale;
                menuItemScaleFit.CheckState = CheckState.Indeterminate;
            }
            else {
                if (!scaleMenuDict.ContainsKey(initialScale)) {
                    if (scaleMenuDict.ContainsKey(100)) {
                        initialScale = 100;
                    }
                    else {
                        initialScale = scaleMenuDict.Keys.FirstOrDefault();
                    }
                }
                TIDScale = initialScale;
                scaleMenuDict[initialScale].CheckState = CheckState.Indeterminate;
            }


            displayManager = new TIDManager(pictureBox1, this);

            flowLayoutPanel1.Location = new Point(flowLayoutPanel1.Location.X - Size.Width + ClientSize.Width + 16, flowLayoutPanel1.Location.Y);


            if (TIDScale > 0) {
                labelScale.ForeColor = Color.White;
                labelScale.Text = $"Scale：{TIDScale}%";
            }
            else {
                labelScale.ForeColor = Color.LightGreen;
                labelScale.Text = $"Scale：{(int)((double)pictureBox1.Image.Width / displayManager.OriginalWidth * 100 + 0.5)}%";
            }

            trackManager = new TrackManager(displayManager);

            Load += TIDWindow_Load;
            menuItemScaleFit.Click += (sender, e) => { SetScale(-1); };

            ChangeDefaultCursor();

            for (var i = 0; i < 24; i++) {
                var time = i;
                var menu = new ToolStripMenuItem();
                menuItemQuickTimeSetting.DropDownItems.Add(menu);
                menu.Name = $"menuItemHour{time}";
                menu.Size = new Size(110, 22);
                menu.Text = $"{time}時台";
                menu.Click += (sender, e) => { SetHourQuick(time); };
            }

            trainMenuDict.Add("9999", menuItemMarkup9999);
            if (HideNumber) {
                menuItemMarkup9999.Text = "??????";
            }
        }

        private void AddScale(int scale) {
            var menu = new ToolStripMenuItem();
            scaleMenuDict.Add(scale, menu);
            menuItemScale.DropDownItems.Insert(menuItemScale.DropDownItems.Count - 3, menu);
            menu.Name = $"menuItemScale{scale}";
            menu.Size = new Size(110, 22);
            menu.Text = $"{scale}%";
            menu.Click += (sender, e) => {
                SetScale(scale);
            };
        }

        private bool LoadSetting(string path) {

            try {
                if (!File.Exists(path)) {
                    return false;
                }
                using var sr = new StreamReader(path);
                var line = sr.ReadLine();
                while (line != null) {
                    if (line.StartsWith('#')) {
                        line = sr.ReadLine();
                        continue;
                    }
                    var texts = line.Replace(" ", "").Split('=');
                    line = sr.ReadLine();

                    if (texts.Length < 2 || texts.Any(t => t == "")) {
                        continue;
                    }
                    var v = texts[1].Replace(" ", "").ToLower();

                    switch (texts[0]) {
                        case "source":
                            source = v;
                            break;
                        case "topMost":
                            topMostSetting = v == "true";
                            break;
                        case "scaleList":
                            var scaleList = new List<int>();
                            foreach (var str in texts[1].Split(',')) {
                                if (!int.TryParse(str, out var scale) || scale <= 0 || scale > 500) {
                                    continue;
                                }
                                scaleList.Add(scale);
                            }
                            if (scaleList.Count > 0) {
                                scaleArray = scaleList.ToArray();
                            }
                            break;
                        case "initialScale":
                        case "scale":
                            foreach (var m in scaleMenuDict.Values) {
                                m.CheckState = CheckState.Unchecked;
                            }
                            menuItemScaleFit.CheckState = CheckState.Unchecked;

                            if (v == "fit") {
                                initialScale = -1;
                                break;
                            }
                            if (int.TryParse(texts[1], out var s)) {
                                initialScale = s;
                            }
                            break;
                        case "timeOffset":
                            if (int.TryParse(texts[1], out var hours)) {
                                TimeOffset = new TimeSpan(((hours % 24) + 24) % 24, 0, 0);
                            }
                            break;
                        case "zoomMode":
                            toggleMagnifyingGlass = v == "toggle";
                            if (toggleMagnifyingGlass) {
                                menuItemPushToZoom.CheckState = CheckState.Unchecked;
                                menuItemToggle.CheckState = CheckState.Indeterminate;
                            }
                            break;
                        case "zoomSize":
                            if (int.TryParse(texts[1], out var size) && size >= 20) {
                                magnifyingGlassSize = size;
                            }
                            break;
                        case "silent":
                            SetSilent(v == "true");
                            break;
                        case "flashInterval":
                            if (float.TryParse(texts[1], out var interval) && interval >= 0) {
                                flashInterval = interval;
                            }
                            break;
                        case "markupType":
                            if (int.TryParse(texts[1], out var mt) && mt >= 0) {
                                SetMarkupType(mt);
                            }
                            break;
                        case "markupDelayed":
                            if (int.TryParse(texts[1], out var md) && md > 0) {
                                SetMarkupDelayed(md);
                            }
                            break;
                        case "markupDuplication":
                            MarkupDuplication = v == "true";
                            menuItemMarkupDuplication.CheckState = MarkupDuplication ? CheckState.Checked : CheckState.Unchecked;
                            break;
                        case "markupFillZero":
                            MarkupFillZero = v == "true";
                            menuItemMarkupFillZero.CheckState = MarkupFillZero ? CheckState.Checked : CheckState.Unchecked;
                            break;
                        case "markup9999":
                            var vb = v == "true";
                            trainDataDict["9999"].Markup = vb;
                            menuItemMarkup9999.CheckState = vb ? CheckState.Checked : CheckState.Unchecked;
                            break;
                        case "markupNotTrain":
                            MarkupNotTrain = v == "true";
                            menuItemMarkupNotTrain.CheckState = MarkupNotTrain ? CheckState.Checked : CheckState.Unchecked;
                            break;
                        case "markupSpawned":
                            MarkupSpawned = v == "true";
                            menuItemMarkupSpawned.CheckState = MarkupSpawned ? CheckState.Checked : CheckState.Unchecked;
                            break;
                        case "markupHandover":
                            MarkupHandover = v == "true";
                            menuItemMarkupHandover.CheckState = MarkupHandover ? CheckState.Checked : CheckState.Unchecked;
                            break;
                        case "hideNumber":
                            HideNumber = v == "true" || v == "lock";
                            LockHideNumber = v == "lock";
                            menuItemHideNumber.CheckState = HideNumber ? CheckState.Checked : CheckState.Unchecked;
                            break;
                        case "debugNumberDown":
                            debugNumberDown = texts[1];
                            break;
                        case "debugNumberUp":
                            debugNumberUp = texts[1];
                            break;
                        case "debugDelayDown":
                            if (!int.TryParse(texts[1], out debugDelayDown) || debugDelayDown < 0) {
                                debugDelayDown = 0;
                            }
                            break;
                        case "debugDelayUp":
                            if (!int.TryParse(texts[1], out debugDelayUp) || debugDelayUp < 0) {
                                debugDelayUp = 0;
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

            if (s == "select" || s == "sct" || s == "sel" || s == "sl") {
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
                case "tc":
                case "t":
                    try {
                        using var sr = new StreamReader(".\\setting\\signal_switch.tsv");
                        sr.ReadLine();
                        var line = sr.ReadLine();
                        var signalName = "";
                        while (line != null) {
                            if (line.StartsWith('#')) {
                                line = sr.ReadLine();
                                continue;
                            }
                            var texts = line.Split('\t');
                            line = sr.ReadLine();

                            if (texts.Length < 3 || texts[1] == "") {
                                continue;
                            }

                            if (texts[0] != "") {
                                signalName = texts[0];
                            }
                            if (signalName == "") {
                                continue;
                            }
                            var sss = new SignalSwitchSetting(texts[1], texts[2] switch { "False" => NRC.Normal, "True" => NRC.Reversed, _ => NRC.Center });

                            if (!signalSwitchDict.TryAdd(signalName, [sss])) {
                                signalSwitchDict[signalName].Add(sss);
                            }

                        }
                    }
                    catch {
                    }

                    try {
                        using var sr = new StreamReader(".\\setting\\signal_direction.tsv");
                        sr.ReadLine();
                        var line = sr.ReadLine();
                        while (line != null) {
                            if (line.StartsWith('#')) {
                                line = sr.ReadLine();
                                continue;
                            }
                            var texts = line.Split('\t');
                            line = sr.ReadLine();

                            if (texts.Length < 4 || texts.Any(t => t == "")) {
                                continue;
                            }


                            var sds = new SignalDirectionSetting(texts[0], texts[1] == "R" ? LCR.Right : LCR.Left, texts[2], texts[3]);

                            if (!signalDirectionDict.TryAdd(texts[0], sds)) {
                                signalDirectionDict[texts[0]] = sds;
                            }

                        }
                    }
                    catch {
                    }

                    TimeOffset = new(0, 0, 0);
                    tcCommunication.ConnectionStatusChanged += UpdateConnectionStatus;
                    tcCommunication.TCDataUpdated += UpdateTCData;
                    menuItemQuickTimeSetting.DropDownItems.Remove(menuItemServerTime);
                    menuItemQuickTimeSetting.DropDownItems.Remove(toolStripSeparator8);
                    LogManager.AddInfoLog("TRAIN CREWに接続します");
                    await TryConnectTrainCrew();
                    break;
                case "debug":
                case "dbg":
                case "d":
                    LogManager.AddInfoLog("デバッグモードを開始します");
                    debugIndex = 0;

                    if (debugNumberDown != "9999") {
                        var td1 = new TrainData(debugNumberDown, debugDelayDown);
                        trainDataDict.Add(debugNumberDown, td1);
                        var menu1 = new ToolStripMenuItem();
                        trainMenuDict.Add(debugNumberDown, menu1);
                        menuItemTrainMarkup.DropDownItems.Add(menu1);
                        menu1.Name = debugNumberDown;
                        menu1.Size = new Size(110, 22);
                        menu1.Text = HideNumber ? "??????" : debugNumberDown;
                        menu1.Click += (sender, e) => {
                            SetTrainMarkup(td1.Number);
                        };
                        foreach (var w in displayManager.SubWindows) {
                            w.AddTrain(debugNumberDown);
                        }
                    }

                    if (debugNumberUp != "9999" && debugNumberDown != debugNumberUp) {
                        var td2 = new TrainData(debugNumberUp, debugDelayUp);
                        trainDataDict.Add(debugNumberUp, td2);
                        var menu2 = new ToolStripMenuItem();
                        trainMenuDict.Add(debugNumberUp, menu2);
                        menuItemTrainMarkup.DropDownItems.Add(menu2);
                        menu2.Name = debugNumberUp;
                        menu2.Size = new Size(110, 22);
                        menu2.Text = HideNumber ? "??????" : debugNumberUp;
                        menu2.Click += (sender, e) => {
                            SetTrainMarkup(td2.Number);
                            /*td2.Markup = !td2.Markup;
                            menu2.CheckState = td2.Markup ? CheckState.Checked : CheckState.Unchecked;
                            ReservedUpdate = true;*/
                        };
                        foreach (var w in displayManager.SubWindows) {
                            w.AddTrain(debugNumberUp);
                        }
                    }
                    menuItemQuickTimeSetting.DropDownItems.Remove(menuItemServerTime);
                    menuItemQuickTimeSetting.DropDownItems.Remove(toolStripSeparator8);
                    break;
                default:
                    /*trackManager.CountStart = 0;*/

                    //デフォルトのサーバへの接続処理
                    serverCommunication = new(this, ServerAddress.SignalAddress, service);
                    serverCommunication.DataUpdated += UpdateServerData;
                    UseServerTime = true;
                    menuItemServerTime.CheckState = CheckState.Checked;
                    LogManager.AddInfoLog($"{(ServerAddress.SignalAddress.Contains("dev") ? "Dev" : "Prod")}サーバに接続します");
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
            tcCommunication.Request = ["trackcircuit", "signal"];
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
                var time = Clock + TimeOffset;
                displayManager.SetClockSubWindows(time);
                if (showOffset <= 0) {
                    labelClock.Text = time.ToString("H:mm:ss");
                }
            }

            var sdl = tcData.signalDataList;
            var updatedTID = false;
            if (sdl != null) {
                foreach (var s in sdl) {
                    if (s.phase != Phase.R && s.phase != Phase.None) {
                        if (signalSwitchDict.TryGetValue(s.Name, out var ssss)) {
                            foreach (var sss in ssss) {
                                if (!pointDataDict.TryAdd(sss.SwitchName, new PointData(sss.SwitchName, sss.State != NRC.Center, sss.State == NRC.Reversed))) {
                                    pointDataDict[sss.SwitchName].SetStates(sss.State != NRC.Center, sss.State == NRC.Reversed);
                                }
                            }
                        }

                        if (signalDirectionDict.TryGetValue(s.Name, out var sds)) {

                            lock (directionDataDict) {
                                if (!directionDataDict.TryAdd(sds.Lever1Name, sds.Type)) {
                                    updatedTID |= directionDataDict[sds.Lever1Name] != sds.Type;
                                    directionDataDict[sds.Lever1Name] = sds.Type;
                                }
                                else {
                                    updatedTID = true;
                                }
                                if (!directionDataDict.TryAdd(sds.Lever2Name, sds.Type)) {
                                    updatedTID |= directionDataDict[sds.Lever2Name] != sds.Type;
                                    directionDataDict[sds.Lever2Name] = sds.Type;
                                }
                                else {
                                    updatedTID = true;
                                }
                            }
                        }
                    }

                }
            }

            updatedTID |= UpdateTrainData(tcList);
            updatedTID |= trackManager.UpdateTCData(tcList);
            updatedTID |= trackManager.UpdateNumberWindow();

            if (updatedTID) {
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
            var trainList = data.TrainStateDatas;
            ServerTime = data.TimeOffset;
            while (ServerTime < 0) {
                ServerTime += 24;
            }

            var updated = tcList != null && UpdateTrainData(tcList, trainList);
            updated |= tcList != null && trackManager.UpdateTCData(tcList);
            updated |= sList != null && UpdatePointData(sList);
            updated |= dList != null && UpdateDirectionData(dList);
            updated |= trackManager.UpdateNumberWindow();

            if (updated) {
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

        private bool UpdateTrainData(List<TrackCircuitData> tcData, List<TrainStateData>? trainData = null) {
            var updatedTID = false;
            lock (trainDataDict) {
                if (trainData != null) {
                    foreach (var t in trainData) {
                        var td = new TrainData(t.TrainNumber, t.Delay);
                        if (!trainDataDict.TryAdd(t.TrainNumber, td)) {
                            updatedTID |= trainDataDict[t.TrainNumber].SetStates(t.Delay);
                        }
                        else {
                            var menu = new ToolStripMenuItem();
                            trainMenuDict.Add(t.TrainNumber, menu);
                            if (InvokeRequired) {
                                Invoke(() => {
                                    for (var i = 6; i <= menuItemTrainMarkup.DropDownItems.Count; i++) {
                                        if (menuItemTrainMarkup.DropDownItems.Count == i) {
                                            menuItemTrainMarkup.DropDownItems.Add(menu);
                                            break;
                                        }
                                        if (menuItemTrainMarkup.DropDownItems[i].Name?.CompareTo(t.TrainNumber) >= 0) {
                                            menuItemTrainMarkup.DropDownItems.Insert(i, menu);
                                            break;
                                        }
                                    }
                                    menu.Name = t.TrainNumber;
                                    menu.Size = new Size(110, 22);
                                    menu.Text = HideNumber ? "??????" : t.TrainNumber;
                                    menu.Click += (sender, e) => {
                                        SetTrainMarkup(td.Number);
                                    };
                                    var isTrain = int.TryParse(Regex.Replace(t.TrainNumber, @"[^0-9]", ""), out var numBody);
                                    td.Markup = MarkupSpawned || markupUmban.Contains(numBody / 3000 * 100 + numBody / 2 * 2 % 100);
                                    menu.CheckState = td.Markup ? CheckState.Checked : CheckState.Unchecked;
                                    foreach (var w in displayManager.SubWindows) {
                                        w.AddTrain(t.TrainNumber);
                                    }
                                });
                            }
                            else {
                                for (var i = 6; i <= menuItemTrainMarkup.DropDownItems.Count; i++) {
                                    if (menuItemTrainMarkup.DropDownItems.Count == i) {
                                        menuItemTrainMarkup.DropDownItems.Add(menu);
                                        break;
                                    }
                                    if (menuItemTrainMarkup.DropDownItems[i].Name?.CompareTo(t.TrainNumber) >= 0) {
                                        menuItemTrainMarkup.DropDownItems.Insert(i, menu);
                                        break;
                                    }
                                }
                                menu.Name = t.TrainNumber;
                                menu.Size = new Size(110, 22);
                                menu.Text = HideNumber ? "??????" : t.TrainNumber;
                                menu.Click += (sender, e) => {
                                    SetTrainMarkup(td.Number);
                                };
                                var isTrain = int.TryParse(Regex.Replace(t.TrainNumber, @"[^0-9]", ""), out var numBody);
                                td.Markup = MarkupSpawned || markupUmban.Contains(numBody / 3000 * 100 + numBody / 2 * 2 % 100);
                                menu.CheckState = td.Markup ? CheckState.Checked : CheckState.Unchecked;
                                foreach (var w in displayManager.SubWindows) {
                                    w.AddTrain(t.TrainNumber);
                                }
                            }
                            updatedTID = true;
                        }
                    }
                }
                foreach (var tc in tcData) {
                    if (!tc.On || tc.Last == "") {
                        continue;
                    }
                    var td = new TrainData(tc.Last, 0);
                    if (!trainDataDict.TryAdd(tc.Last, td)) {
                        updatedTID |= trainDataDict[tc.Last].SetStates(-1);
                    }
                    else {
                        var menu = new ToolStripMenuItem();
                        trainMenuDict.Add(tc.Last, menu);
                        if (InvokeRequired) {
                            Invoke(() => {
                                for (var i = 6; i <= menuItemTrainMarkup.DropDownItems.Count; i++) {
                                    if (menuItemTrainMarkup.DropDownItems.Count == i) {
                                        menuItemTrainMarkup.DropDownItems.Add(menu);
                                        break;
                                    }
                                    if (menuItemTrainMarkup.DropDownItems[i].Name?.CompareTo(tc.Last) >= 0) {
                                        menuItemTrainMarkup.DropDownItems.Insert(i, menu);
                                        break;
                                    }
                                }
                                menu.Name = tc.Last;
                                menu.Size = new Size(110, 22);
                                menu.Text = HideNumber ? "??????" : tc.Last;
                                menu.Click += (sender, e) => {
                                    SetTrainMarkup(td.Number);
                                };
                                var isTrain = int.TryParse(Regex.Replace(tc.Last, @"[^0-9]", ""), out var numBody);
                                td.Markup = MarkupSpawned || markupUmban.Contains(numBody / 3000 * 100 + numBody / 2 * 2 % 100);
                                menu.CheckState = td.Markup ? CheckState.Checked : CheckState.Unchecked;
                                foreach (var w in displayManager.SubWindows) {
                                    w.AddTrain(tc.Last);
                                }
                            });
                        }
                        else {
                            for (var i = 6; i <= menuItemTrainMarkup.DropDownItems.Count; i++) {
                                if (menuItemTrainMarkup.DropDownItems.Count == i) {
                                    menuItemTrainMarkup.DropDownItems.Add(menu);
                                    break;
                                }
                                if (menuItemTrainMarkup.DropDownItems[i].Name?.CompareTo(tc.Last) >= 0) {
                                    menuItemTrainMarkup.DropDownItems.Insert(i, menu);
                                    break;
                                }
                            }
                            menu.Name = tc.Last;
                            menu.Size = new Size(110, 22);
                            menu.Text = HideNumber ? "??????" : tc.Last;
                            menu.Click += (sender, e) => {
                                SetTrainMarkup(td.Number);
                            };
                            var isTrain = int.TryParse(Regex.Replace(tc.Last, @"[^0-9]", ""), out var numBody);
                            td.Markup = MarkupSpawned || markupUmban.Contains(numBody / 3000 * 100 + numBody / 2 * 2 % 100);
                            menu.CheckState = td.Markup ? CheckState.Checked : CheckState.Unchecked;
                            foreach (var w in displayManager.SubWindows) {
                                w.AddTrain(tc.Last);
                            }
                        }
                        updatedTID = true;
                    }

                }

                foreach (var k in trainDataDict.Keys.ToArray()) {
                    if (trainDataDict[k].UpdateTrack()) {
                        trainDataDict.Remove(k);
                        var menu = trainMenuDict[k];
                        trainMenuDict.Remove(k);
                        if (InvokeRequired) {
                            Invoke(() => {
                                menuItemTrainMarkup.DropDownItems.Remove(menu);
                                foreach (var w in displayManager.SubWindows) {
                                    w.RemoveTrain(k);
                                }
                            });
                        }
                        else {
                            menuItemTrainMarkup.DropDownItems.Remove(menu);
                            foreach (var w in displayManager.SubWindows) {
                                w.RemoveTrain(k);
                            }
                        }
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
            if (!OpeningDialog && ActiveForm != this && !displayManager.IsActiveForm) {
                UpdateMouseCursor();
            }
            if (WindowState == FormWindowState.Maximized) {
                DetectResize = false;
                var width = Width;
                var height = Height;
                var loc = new Point(Location.X, Location.Y + 8);
                WindowState = FormWindowState.Normal;
                Width = width;
                Height = height;
                Location = loc;
                DetectResize = true;
            }
            if (showOffset > 0) {
                showOffset--;
            }

            var oldFlashState = FlashState;
            var now = DateTime.Now;
            var deltaSeconds = (now - RealTime).TotalSeconds;
            RealTime = now;
            if (flashInterval > 0) {
                flashState -= (float)deltaSeconds;
                while (flashState <= 0) {
                    flashState += flashInterval * 2;
                }
            }

            if (!UpdateDebug() && displayManager.Started && (ReservedUpdate || (oldFlashState != FlashState) && MarkupType < 2 && (trainDataDict.Values.Any(td => td.Markup) || MarkupDuplication || MarkupFillZero || MarkupNotTrain || MarkupDelayed > 0 || displayManager.Markuped))) {
                ReservedUpdate = false;
                displayManager.UpdateTID();
            }


            if (usingMagnifyingGlass) {
                var cp1 = pictureBox1.PointToClient(Cursor.Position);
                var cp2 = PointToClient(Cursor.Position);

                if (toggleMagnifyingGlass && (cp1.X < 0 || cp1.Y < 0 || cp1.X > pictureBox1.Width || cp1.Y > pictureBox1.Height || cp2.X < 0 || cp2.Y < 0 || cp2.X > ClientSize.Width || cp2.Y > ClientSize.Height)) {
                    var width = pictureBox1.Width - cp1.X + magnifyingGlassSize / 2;
                    var height = pictureBox1.Height - cp1.Y + magnifyingGlassSize / 2;
                    var mouseX = cp1.X;
                    var mouseY = cp1.Y;
                    if (width <= 1 || height <= 1) {
                        pictureBox2.Location = new Point(-300, -300);
                        pictureBox2.Size = new Size(240, 240);
                    }
                    else {
                        pictureBox2.Location = new Point(mouseX - magnifyingGlassSize / 2, mouseY - magnifyingGlassSize / 2);
                        pictureBox2.Size = new Size(Math.Min(magnifyingGlassSize, Math.Max(0, pictureBox1.Width - cp1.X + magnifyingGlassSize / 2)), Math.Min(magnifyingGlassSize, Math.Max(0, pictureBox1.Height - cp1.Y + magnifyingGlassSize / 2)));
                    }

                    SetMagnifyingGlass(cp1.X, cp1.Y);
                }
            }


            if (debugIndex < 0 && serverCommunication == null) {
                return;
            }
            Clock = RealTime;
            if (UseServerTime) {
                TimeOffset = new TimeSpan(ServerTime, 0, 0);
            }
            var time = Clock + TimeOffset;
            displayManager.SetClockSubWindows(time);
            if (showOffset <= 0) {
                labelClock.Text = time.ToString("H:mm:ss");
            }
            var updatedTime = serverCommunication?.UpdatedTime;
            if (updatedTime == null || serverCommunication == null) {
                return;
            }
            var delaySeconds = (Clock - (DateTime)updatedTime).TotalSeconds;
            updatedTime = updatedTime?.Add(TimeOffset);
            if (delaySeconds > 10) {
                if (!ServerCommunication.Error) {
                    ServerCommunication.Error = true;
                    LogManager.AddWarningLog("サーバからの受信が10秒以上ありません");
                    LabelStatusText = $"データ受信不能(最終受信：{updatedTime?.ToString("H:mm:ss")})";
                    SetStatusSubWindow("×", Color.Red);
                    Debug.WriteLine($"データ受信不能: {delaySeconds}");
                    if (!Silent) {
                        OpeningDialog = true;
                        TaskDialog.ShowDialog(this, new TaskDialogPage {
                            Caption = "データ受信不能 | TID - ダイヤ運転会",
                            Heading = "データ受信不能",
                            Icon = TaskDialogIcon.Error,
                            Text = "サーバ側からのデータ受信が10秒以上ありませんでした。\n復旧を試みますが、しばらく経っても復旧しない場合はアプリケーションの再起動をおすすめします。"
                        });
                        OpeningDialog = false;
                    }
                    else {
                        PlayWarningSound();
                    }
                }
            }
            else if (delaySeconds > 1) {
                if (!LabelStatusText.Contains("最終受信")) {
                    LogManager.AddWarningLog("サーバからの受信が1秒以上ありません");
                }
                LabelStatusText = $"データ正常受信(最終受信：{updatedTime?.ToString("H:mm:ss")})";
                SetStatusSubWindow("▲", Color.Yellow);
                Debug.WriteLine($"データ受信不能: {delaySeconds}");
            }
        }

        private bool UpdateDebug(bool reversed = false) {
            var v = false;
            if (debugIndex >= 0) {
                if (debugCount == 0 || debugCount <= -10000) {
                    var lineData = displayManager.LineSettings;

                    var line = lineData[debugIndex % lineData.Count];
                    if (debugCount >= 0 || debugCount == -10000) {
                        if (line.PointName != "") {
                            UpdatePointData([new() { Name = line.PointName, State = NRC.Center }]);
                        }
                        if (reversed) {
                            debugIndex = (debugIndex + lineData.Count * 2 - 1) % (lineData.Count * 2);
                        }
                        else {
                            debugIndex = (debugIndex + 1) % (lineData.Count * 2);
                        }
                    }
                    line = lineData[debugIndex % lineData.Count];
                    trackManager.UpdateTCData([new TrackCircuitData() { Name = line.TrackName, Last = debugIndex < lineData.Count ? debugNumberDown : debugNumberUp, On = true }]);
                    if (line.PointName != "") {
                        UpdatePointData([new SwitchData() { Name = line.PointName, State = line.Reversed ? NRC.Reversed : NRC.Normal }]);
                        LabelStatusText = $"デバッグモード（{(debugIndex < lineData.Count ? "下り" : "上り")}） track: {line.TrackName}  switch: {line.PointName} {(line.Reversed ? "R" : "N")}";
                    }
                    else {
                        LabelStatusText = $"デバッグモード（{(debugIndex < lineData.Count ? "下り" : "上り")}） track: {line.TrackName}";
                    }
                    trackManager.UpdateNumberWindow();
                    displayManager.UpdateTID();
                    v = true;
                    debugCount = debugCount == -10000 ? -100 : 100;
                }
                if (debugCount > 0) {
                    debugCount--;
                }
            }
            return v;
        }

        private void labelClock_MouseDown(object sender, MouseEventArgs e) {
            if(e.Button == MouseButtons.Middle) {
                SetUseServerTime(true);
            }
            if (e.Button != MouseButtons.Left && e.Button != MouseButtons.Right) {
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
            SetUseServerTime(false);
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
            menuItemTopMost.CheckState = topMost ? CheckState.Checked : CheckState.Unchecked;
            labelTopMost.Text = $"最前面：{(topMost ? "ON" : "OFF")}";
            labelTopMost.ForeColor = topMost ? Color.Yellow : Color.Gray;
        }

        private void labelTopMost_Hover(object sender, EventArgs e) {
            labelTopMost.BackColor = Color.FromArgb(55, 55, 55);
        }

        private void labelTopMost_Leave(object sender, EventArgs e) {
            labelTopMost.BackColor = Color.FromArgb(30, 30, 30);
        }

        private void labelSilent_Hover(object sender, EventArgs e) {
            labelSilent.BackColor = Color.FromArgb(55, 55, 55);
        }

        private void labelSilent_Leave(object sender, EventArgs e) {
            labelSilent.BackColor = Color.FromArgb(30, 30, 30);
        }

        private void labelSilent_Click(object sender, EventArgs e) {
            SetSilent(!Silent);
        }

        public void SetSilent(bool silent) {
            Silent = silent;
            menuItemSilent.CheckState = silent ? CheckState.Checked : CheckState.Unchecked;
            labelSilent.Text = $"サイレント：{(silent ? "ON" : "OFF")}";
            labelSilent.ForeColor = silent ? Color.Gray : Color.White;
            if (displayManager != null) {
                foreach (var w in displayManager.SubWindows) {
                    w.SetSilent(silent);
                }
            }
        }

        public void SetMarkupType(int type) {

            MarkupType = type < 3 ? (type >= 0 ? type : 0) : 2;
            menuItemMarkupType1.CheckState = type == 0 ? CheckState.Indeterminate : CheckState.Unchecked;
            menuItemMarkupType2.CheckState = type == 1 ? CheckState.Indeterminate : CheckState.Unchecked;
            menuItemMarkupType3.CheckState = type == 2 ? CheckState.Indeterminate : CheckState.Unchecked;
            if (displayManager == null) {
                return;
            }
            foreach (var w in displayManager.SubWindows) {
                w.SetMarkupType(type);
            }
        }

        private void menuItemCopy_Click(object sender, EventArgs e) {
            displayManager.CopyImage();
        }

        private void SetScale(int scale) {
            var min = scaleMenuDict.Keys.Min();
            var max = scaleMenuDict.Keys.Max();
            if (scale < min && scale != -1) {
                scale = min;
            }
            if (scale > max) {
                scale = max;
            }
            LogManager.AddInfoLog($"拡大率変更：{(scale > 0 ? $"{scale}%" : "fit")}");

            foreach (var k in scaleMenuDict.Keys) {
                scaleMenuDict[k].CheckState = k == scale ? CheckState.Indeterminate : CheckState.Unchecked;
            }

            menuItemScaleFit.CheckState = scale < 0 ? CheckState.Indeterminate : CheckState.Unchecked;
            menuItemFixedScale.CheckState = CheckState.Unchecked;



            FixedScale = false;
            TIDScale = scale;

            displayManager.ChangeScale();
            if (scale > 0) {
                labelScale.ForeColor = Color.White;
                labelScale.Text = $"Scale：{scale}%";
            }
            else {
                labelScale.ForeColor = Color.LightGreen;
                labelScale.Text = $"Scale：{(int)((double)pictureBox1.Image.Width / displayManager.OriginalWidth * 100 + 0.5)}%";
            }
            ChangeDefaultCursor();
        }

        public void SetFixedScale(bool value) {
            LogManager.AddInfoLog($"拡大率変更：{(value ? "倍率固定" : "fit")}");

            foreach (var k in scaleMenuDict.Keys) {
                scaleMenuDict[k].CheckState = CheckState.Unchecked;
            }

            menuItemScaleFit.CheckState = !value ? CheckState.Indeterminate : CheckState.Unchecked;
            menuItemFixedScale.CheckState = value ? CheckState.Indeterminate : CheckState.Unchecked;


            FixedScale = value;
            TIDScale = -1;

            if (value) {
                labelScale.ForeColor = Color.Red;
            }
            else {
                labelScale.ForeColor = Color.LightGreen;
                displayManager.ChangeScale();
            }
            labelScale.Text = $"Scale：{(int)((double)pictureBox1.Image.Width / displayManager.OriginalWidth * 100 + 0.5)}%";
            ChangeDefaultCursor();
        }

        private void SetHourQuick(int hour) {
            SetUseServerTime(false);
            TimeOffset = new TimeSpan((hour + 24 - Clock.Hour) % 24, TimeOffset.Minutes, TimeOffset.Seconds);
        }




        private void labelScale_MouseDown(object sender, MouseEventArgs e) {
            if (TIDScale > 0) {
                if (ModifierKeys.HasFlag(Keys.Shift)) {
                    SetScale(-1);
                }
                else if (ModifierKeys.HasFlag(Keys.Control)) {
                    SetFixedScale(true);
                }
                else {
                    var i = -1;
                    if (e.Button == MouseButtons.Right) {
                        i = Math.Min(Array.IndexOf(scaleArray, TIDScale) + 1, scaleArray.Length - 1);
                    }
                    else if (e.Button == MouseButtons.Left) {
                        i = Math.Max(Array.IndexOf(scaleArray, TIDScale) - 1, 0);
                    }
                    if (i >= 0) {
                        SetScale(scaleArray[i]);
                    }
                }
            }
            else {
                if (ModifierKeys.HasFlag(Keys.Control)) {
                    SetFixedScale(!FixedScale);
                }
                else if (FixedScale && ModifierKeys.HasFlag(Keys.Shift)) {
                    SetFixedScale(false);
                }
                else {
                    SetScale(initialScale);
                }
            }
        }

        private void TIDWindow_KeyDown(object sender, KeyEventArgs e) {
            var code = e.KeyData & Keys.KeyCode;
            var mod = e.KeyData & Keys.Modifiers;
            if ((mod & Keys.Shift) == Keys.Shift) {
                pictureBox1.Cursor = Cursors.Hand;
            }
            else if ((mod & Keys.Control) == Keys.Control) {
                pictureBox1.Cursor = Cursors.Cross;
            }
            if (e.KeyData == (Keys.C | Keys.Control)) {
                if (selectionStarting.HasValue) {
                    var start = selectionStarting.Value;
                    selectionStarting = null;
                    lock (pictureBox3) {
                        pictureBox3.Location = new Point(-300, -300);
                        pictureBox3.Size = new Size(100, 100);
                    }
                    var end = pictureBox1.PointToClient(Cursor.Position);
                    var center = new Point((start.X + end.X) / 2 - end.X + Cursor.Position.X, (start.Y + end.Y) / 2 - end.Y + Cursor.Position.Y);
                    end = new Point(end.X > 10 ? (start.X < pictureBox1.Width && end.X < pictureBox1.Width - 10 ? end.X : pictureBox1.Width) : (start.X > 0 ? 0 : end.X), end.Y > 10 ? (start.Y < pictureBox1.Height && end.Y < pictureBox1.Height - 10 ? end.Y : pictureBox1.Height) : (start.Y > 0 ? 0 : end.Y));
                    start = ConvertPointToOriginal(start);
                    end = ConvertPointToOriginal(end);
                    var p = new Point(Math.Min(start.X, end.X), Math.Min(start.Y, end.Y));
                    var s = new Size(Math.Abs(start.X - end.X), Math.Abs(start.Y - end.Y));
                    displayManager.CopyImage(p, s);
                }
                else {
                    displayManager.CopyImage();
                }
            }
            if (e.KeyData == Keys.Tab) {
                SetTopMost(!TopMost);
            }

            if (code == Keys.Right || code == Keys.D) {
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
            if (e.KeyData == Keys.D1) {
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
                if (e.KeyData == (Keys.C | Keys.Control | Keys.Shift)) {
                    var lineData = displayManager.LineSettings;
                    var line = lineData[debugIndex % lineData.Count];
                    if (line != null) {
                        if (line.PointName != "") {
                            Clipboard.SetText($"\n{line.TrackName}\tS\t列番位置x\t列番位置y\t{line.PointName}\t{(line.Reversed ? "True" : "False")}");
                        }
                        else {
                            Clipboard.SetText($"\n{line.TrackName}\tS\t列番位置x\t列番位置y\t\t");
                        }
                    }
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

        private void TIDWindow_KeyUp(object sender, KeyEventArgs e) {
            var mod = e.KeyData & Keys.Modifiers;
            UpdateMouseCursor();
            if ((mod & Keys.Shift) != Keys.Shift && (mod & Keys.Control) != Keys.Control) {
                if ((MouseButtons & MouseButtons.Left) == MouseButtons.Left) {
                    mouseLoc = pictureBox1.PointToClient(Cursor.Position);
                    selectionStarting = null;
                    lock (pictureBox3) {
                        pictureBox3.Location = new Point(-300, -300);
                        pictureBox3.Size = new Size(100, 100);
                    }
                }
            }

        }

        private void PictureBox1_MouseWheel(object sender, MouseEventArgs e) {
            if (ModifierKeys.HasFlag(Keys.Control)) {
                if (TIDScale > 0) {
                    var i = -1;
                    if (e.Delta > 0) {
                        i = Math.Min(Array.IndexOf(scaleArray, TIDScale) + 1, scaleArray.Length - 1);
                    }
                    else {
                        i = Math.Max(Array.IndexOf(scaleArray, TIDScale) - 1, 0);
                    }
                    if (i >= 0) {
                        SetScale(scaleArray[i]);
                    }
                }
                else {
                    if (FixedScale) {
                        SetFixedScale(false);
                    }
                    lock (pictureBox1.Image) {
                        var size = Size;
                        var dp = e.Location;
                        var point = ConvertPointToOriginal(dp.X, dp.Y);
                        var rate = (pictureBox1.Image.Width + e.Delta * 0.2) / displayManager.OriginalWidth;
                        var width = Size.Width - ClientSize.Width + (int)(displayManager.OriginalWidth * rate);
                        var height = Size.Height - ClientSize.Height + panel1.Location.Y + (int)(displayManager.OriginalHeight * rate);
                        var screenSize = Screen.FromControl(this).Bounds;
                        screenSize = new Rectangle(screenSize.Location, new Size(screenSize.Width + 20, screenSize.Height + 20));
                        if (width <= screenSize.Width && height <= screenSize.Height) {
                            Size = new Size(width, height);
                            var np = ConvertPointToScreen(point);
                            if (size != Size) {
                                Location = new Point(Location.X + dp.X - np.X, Location.Y + dp.Y - np.Y);
                            }
                        }
                        else if (width > screenSize.Width) {
                            width = screenSize.Width;
                            height = Size.Height - ClientSize.Height + panel1.Location.Y + displayManager.OriginalHeight * (screenSize.Width - Size.Width + ClientSize.Width) / displayManager.OriginalWidth;
                            Size = new Size(width, height);
                            var np = ConvertPointToScreen(point);
                            if (size != Size) {
                                Location = new Point(Location.X + dp.X - np.X, Location.Y + dp.Y - np.Y);
                            }
                        }
                        else {
                            height = screenSize.Height;
                            width = Size.Width - ClientSize.Width + displayManager.OriginalWidth * (screenSize.Height - Size.Height + ClientSize.Height - panel1.Location.Y) / displayManager.OriginalHeight;
                            Size = new Size(width, height);
                            var np = ConvertPointToScreen(point);
                            if (size != Size) {
                                Location = new Point(Location.X + dp.X - np.X, Location.Y + dp.Y - np.Y);
                            }
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
            if (displayManager != null && DetectResize) {
                DetectResize = false;
                if (WindowState != FormWindowState.Minimized) {
                    if (TIDScale == -1 && !FixedScale) {
                        if (WindowState != FormWindowState.Minimized) {
                            displayManager.ChangeScale();
                            labelScale.Text = $"Scale：{(int)((double)pictureBox1.Image.Width / displayManager.OriginalWidth * 100 + 0.5)}%";
                        }
                    }
                    else {
                        ChangeDefaultCursor();
                    }
                }
                DetectResize = true;
            }
        }
        private void TIDWindow_SizeChanged(object sender, EventArgs e) {
            if (WindowState == FormWindowState.Minimized) {
                if (!windowMinimized) {
                    LogManager.AddInfoLog("ウィンドウが最小化されました");
                    windowMinimized = true;
                }
            }
            else if (windowMinimized) {
                LogManager.AddInfoLog("ウィンドウの最小化が解除されました");
                windowMinimized = false;
            }
        }

        private void ChangeDefaultCursor() {
            defaultCursor = TIDScale == -1 ? Cursors.Default : (pictureBox1.Width < panel1.Width && pictureBox1.Height < panel1.Height ? Cursors.Default : Cursors.SizeAll);
            UpdateMouseCursor();
        }

        private void PictureBox1_MouseDown(object sender, MouseEventArgs e) {
            if (e.Button == MouseButtons.Middle && pictureBox1.Width < displayManager.OriginalWidth) {
                if (toggleMagnifyingGlass && usingMagnifyingGlass) {
                    usingMagnifyingGlass = false;

                    pictureBox2.Location = new Point(-300, -300);
                    pictureBox2.Size = new Size(240, 240);
                    UpdateMouseCursor();
                }
                else {
                    usingMagnifyingGlass = true;
                    var width = pictureBox1.Width - e.X + magnifyingGlassSize / 2;
                    var height = pictureBox1.Height - e.Y + magnifyingGlassSize / 2;
                    if (width <= 1 || height <= 1) {
                        pictureBox2.Location = new Point(-300, -300);
                        pictureBox2.Size = new Size(240, 240);
                    }
                    else {
                        pictureBox2.Location = new Point(e.X - magnifyingGlassSize / 2, e.Y - magnifyingGlassSize / 2);
                        pictureBox2.Size = new Size(Math.Min(magnifyingGlassSize, Math.Max(0, pictureBox1.Width - e.X + magnifyingGlassSize / 2)), Math.Min(magnifyingGlassSize, Math.Max(0, pictureBox1.Height - e.Y + magnifyingGlassSize / 2)));
                    }

                    SetMagnifyingGlass(e.X, e.Y);

                    UpdateMouseCursor();
                }

            }
            else if (usingMagnifyingGlass) {
                usingMagnifyingGlass = false;

                pictureBox2.Location = new Point(-300, -300);
                pictureBox2.Size = new Size(240, 240);
                UpdateMouseCursor();
            }
            if ((e.Button & MouseButtons.Left) == MouseButtons.Left) {
                if (ModifierKeys.HasFlag(Keys.Shift)) {
                    foreach (var w in displayManager.NumberWindowDict.Values) {
                        var t = w.Train;
                        if (t != null && IsInArea(e.Location, w.PosX, w.PosY, w.GetSize(), 1)/* && trainDataDict.TryGetValue(t, out var td)*/) {
                            SetTrainMarkup(t);
                            /*td.Markup = !td.Markup;
                            trainMenuDict[t].CheckState = td.Markup ? CheckState.Checked : CheckState.Unchecked;
                            ReservedUpdate = true;*/
                        }
                    }
                }
                else if (ModifierKeys.HasFlag(Keys.Control)) {
                    selectionStarting = e.Location;
                }
                else {
                    mouseLoc = e.Location;
                }
            }
            if ((e.Button & MouseButtons.Right) == MouseButtons.Right) {
                pictureBox1.Cursor = defaultCursor;
                selectionStarting = null;
                lock (pictureBox3) {
                    pictureBox3.Location = new Point(-300, -300);
                    pictureBox3.Size = new Size(100, 100);
                }
            }
        }

        private void PictureBox2_MouseDown(object sender, MouseEventArgs e) {
            if (e.Button == MouseButtons.Middle && pictureBox1.Width < displayManager.OriginalWidth) {
                if (toggleMagnifyingGlass && usingMagnifyingGlass) {
                    usingMagnifyingGlass = false;

                    pictureBox2.Location = new Point(-300, -300);
                    pictureBox2.Size = new Size(240, 240);
                    UpdateMouseCursor();
                }
            }
            if ((e.Button & MouseButtons.Right) == MouseButtons.Right) {
                usingMagnifyingGlass = false;

                pictureBox2.Location = new Point(-300, -300);
                pictureBox2.Size = new Size(240, 240);
                UpdateMouseCursor();
            }
            if (toggleMagnifyingGlass && (e.Button & MouseButtons.Left) == MouseButtons.Left) {
                if (ModifierKeys.HasFlag(Keys.Shift)) {
                    foreach (var w in displayManager.NumberWindowDict.Values) {
                        var t = w.Train;
                        if (t != null && IsInArea(pictureBox1.PointToClient(Cursor.Position), w.PosX, w.PosY, w.GetSize(), 1)/* && trainDataDict.TryGetValue(t, out var td)*/) {
                            SetTrainMarkup(t);
                            /*td.Markup = !td.Markup;
                            trainMenuDict[t].CheckState = td.Markup ? CheckState.Checked : CheckState.Unchecked;
                            ReservedUpdate = true;*/
                        }
                    }
                }
                else if (ModifierKeys.HasFlag(Keys.Control)) {
                    usingMagnifyingGlass = false;

                    pictureBox2.Location = new Point(-300, -300);
                    pictureBox2.Size = new Size(240, 240);
                    UpdateMouseCursor();
                    selectionStarting = pictureBox1.PointToClient(Cursor.Position);
                }
            }
        }

        private void PictureBox2_MouseUp(object sender, MouseEventArgs e) {
            if ((e.Button & MouseButtons.Left) == MouseButtons.Left) {
                mouseLoc = null;
                if (selectionStarting.HasValue) {
                    var start = selectionStarting.Value;
                    selectionStarting = null;
                    lock (pictureBox3) {
                        pictureBox3.Location = new Point(-300, -300);
                        pictureBox3.Size = new Size(100, 100);
                    }
                    var end = pictureBox1.PointToClient(Cursor.Position);
                    var center = new Point((start.X + end.X) / 2 - end.X + Cursor.Position.X, (start.Y + end.Y) / 2 - end.Y + Cursor.Position.Y);
                    end = new Point(end.X > 16 ? (start.X >= pictureBox1.Width || end.X < pictureBox1.Width - 16 ? end.X : pictureBox1.Width) : (start.X > 0 ? 0 : end.X), end.Y > 16 ? (start.Y >= pictureBox1.Height || end.Y < pictureBox1.Height - 16 ? end.Y : pictureBox1.Height) : (start.Y > 0 ? 0 : end.Y));
                    start = ConvertPointToOriginal(start);
                    end = ConvertPointToOriginal(end);
                    var p = new Point(Math.Min(start.X, end.X), Math.Min(start.Y, end.Y));
                    var s = new Size(Math.Abs(start.X - end.X), Math.Abs(start.Y - end.Y));
                    var screenSize = Screen.FromControl(this).Bounds;
                    screenSize = new Rectangle(screenSize.Location, new Size(screenSize.Width + 20, screenSize.Height + 20));
                    if (s.Width > 120 && s.Width <= screenSize.Width && s.Height > 100 && s.Height <= screenSize.Height - pictureBox1.Location.Y) {
                        var sub = new SubWindow(p, s, displayManager, menuItemTrainMarkup.DropDownItems);
                        sub.Icon = Icon;
                        pictureBox1.Cursor = defaultCursor;
                        sub.SetMarkup9999(trainDataDict["9999"].Markup);
                        sub.Show();
                        var border = (Size.Width - ClientSize.Width) / 2;
                        sub.Location = new Point(center.X - s.Width / 2 - border, center.Y - s.Height / 2 - Size.Height + ClientSize.Height - panel1.Location.Y / 2 - border);
                        sub.SetTopMost(TopMost);
                        sub.SetSilent(Silent);
                        sub.SetClockColor(serverCommunication == null || UseServerTime ? Color.White : Color.Yellow);
                        displayManager.AddSubWindow(sub);
                    }
                }
            }
        }

        private void PictureBox1_MouseMove(object sender, MouseEventArgs e) {
            if (usingMagnifyingGlass) {
                if (!toggleMagnifyingGlass) {
                    if (e.Button == MouseButtons.Middle) {
                        var width = pictureBox1.Width - e.X + magnifyingGlassSize / 2;
                        var height = pictureBox1.Height - e.Y + magnifyingGlassSize / 2;
                        var mouseX = e.X/* - panel1.HorizontalScroll.Value*/;
                        var mouseY = e.Y/* - panel1.VerticalScroll.Value*/;
                        if (width <= 1 || height <= 1) {
                            pictureBox2.Location = new Point(-300, -300);
                            pictureBox2.Size = new Size(240, 240);
                        }
                        else {
                            pictureBox2.Location = new Point(mouseX - magnifyingGlassSize / 2, mouseY - magnifyingGlassSize / 2);
                            pictureBox2.Size = new Size(Math.Min(magnifyingGlassSize, Math.Max(0, pictureBox1.Width - e.X + magnifyingGlassSize / 2)), Math.Min(magnifyingGlassSize, Math.Max(0, pictureBox1.Height - e.Y + magnifyingGlassSize / 2)));
                        }

                        SetMagnifyingGlass(e.X, e.Y);


                    }
                    else {
                        usingMagnifyingGlass = false;

                        pictureBox2.Location = new Point(-300, -300);
                        pictureBox2.Size = new Size(240, 240);
                    }
                }
                else {
                    var width = pictureBox1.Width - e.X + magnifyingGlassSize / 2;
                    var height = pictureBox1.Height - e.Y + magnifyingGlassSize / 2;
                    var mouseX = e.X;
                    var mouseY = e.Y;
                    if (width <= 1 || height <= 1) {
                        pictureBox2.Location = new Point(-300, -300);
                        pictureBox2.Size = new Size(240, 240);
                    }
                    else {
                        pictureBox2.Location = new Point(mouseX - magnifyingGlassSize / 2, mouseY - magnifyingGlassSize / 2);
                        pictureBox2.Size = new Size(Math.Min(magnifyingGlassSize, Math.Max(0, pictureBox1.Width - e.X + magnifyingGlassSize / 2)), Math.Min(magnifyingGlassSize, Math.Max(0, pictureBox1.Height - e.Y + magnifyingGlassSize / 2)));
                    }

                    SetMagnifyingGlass(e.X, e.Y);
                }


            }
            if (mouseLoc.HasValue && !selectionStarting.HasValue && !ModifierKeys.HasFlag(Keys.Shift) && (e.Button & MouseButtons.Left) == MouseButtons.Left) {
                panel1.AutoScrollPosition = new Point(panel1.HorizontalScroll.Value - e.Location.X + mouseLoc.Value.X, panel1.VerticalScroll.Value - e.Location.Y + mouseLoc.Value.Y);
            }
            if (selectionStarting.HasValue) {
                var s = selectionStarting.Value;
                selectionStarting = new Point(s.X > 16 ? (s.X < pictureBox1.Width - 16 ? s.X : pictureBox1.Width) : 0, s.Y > 16 ? (s.Y < pictureBox1.Height - 16 ? s.Y : pictureBox1.Height) : 0);
                var start = selectionStarting.Value;
                var end = e.Location;
                var center = new Point((start.X + end.X) / 2 - end.X + Cursor.Position.X, (start.Y + end.Y) / 2 - end.Y + Cursor.Position.Y);
                end = new Point(end.X > 16 ? (start.X >= pictureBox1.Width || end.X < pictureBox1.Width - 16 ? end.X : pictureBox1.Width) : (start.X > 0 ? 0 : end.X), end.Y > 16 ? (start.Y >= pictureBox1.Height || end.Y < pictureBox1.Height - 16 ? end.Y : pictureBox1.Height) : (start.Y > 0 ? 0 : end.Y));
                var startOrig = ConvertPointToOriginal(start);
                var endOrig = ConvertPointToOriginal(end);
                var pos = new Point(Math.Min(start.X, end.X), Math.Min(start.Y, end.Y));
                var size = new Size(Math.Abs(start.X - end.X), Math.Abs(start.Y - end.Y));
                var sizeOrig = new Size(Math.Abs(startOrig.X - endOrig.X), Math.Abs(startOrig.Y - endOrig.Y));
                if (size.Width > 1 && size.Height > 1) {
                    lock (pictureBox3) {
                        var screenSize = Screen.FromControl(this).Bounds;
                        screenSize = new Rectangle(screenSize.Location, new Size(screenSize.Width + 20, screenSize.Height + 20));
                        var old = pictureBox3.Image;
                        var b = new Bitmap(size.Width, size.Height);
                        using var g = Graphics.FromImage(b);
                        g.Clear(Color.Transparent);
                        g.DrawRectangle(sizeOrig.Width > 120 && sizeOrig.Width <= screenSize.Width && sizeOrig.Height > 100 && sizeOrig.Height <= screenSize.Height ? Pens.LimeGreen : Pens.DarkRed, 0, 0, size.Width - 1, size.Height - 1);
                        pictureBox3.Image = b;
                        pictureBox3.Location = pos;
                        pictureBox3.Size = size;
                        old?.Dispose();
                    }
                }

            }
        }

        private void PictureBox2_MouseMove(object sender, MouseEventArgs e) {
            if (usingMagnifyingGlass) {

                if (toggleMagnifyingGlass) {
                    var cp = pictureBox1.PointToClient(Cursor.Position);
                    var width = pictureBox1.Width - cp.X + magnifyingGlassSize / 2;
                    var height = pictureBox1.Height - cp.Y + magnifyingGlassSize / 2;
                    var mouseX = cp.X;
                    var mouseY = cp.Y;
                    if (width <= 1 || height <= 1) {
                        pictureBox2.Location = new Point(-300, -300);
                        pictureBox2.Size = new Size(240, 240);
                    }
                    else {
                        pictureBox2.Location = new Point(mouseX - magnifyingGlassSize / 2, mouseY - magnifyingGlassSize / 2);
                        pictureBox2.Size = new Size(Math.Min(magnifyingGlassSize, Math.Max(0, pictureBox1.Width - cp.X + magnifyingGlassSize / 2)), Math.Min(magnifyingGlassSize, Math.Max(0, pictureBox1.Height - cp.Y + magnifyingGlassSize / 2)));
                    }
                    UpdateMouseCursor();
                    SetMagnifyingGlass(cp.X, cp.Y);
                }
            }
            else if (selectionStarting.HasValue) {
                var s = selectionStarting.Value;
                selectionStarting = new Point(s.X > 16 ? (s.X < pictureBox1.Width - 16 ? s.X : pictureBox1.Width) : 0, s.Y > 16 ? (s.Y < pictureBox1.Height - 16 ? s.Y : pictureBox1.Height) : 0);
                var start = selectionStarting.Value;
                var end = pictureBox1.PointToClient(Cursor.Position);
                var center = new Point((start.X + end.X) / 2 - end.X + Cursor.Position.X, (start.Y + end.Y) / 2 - end.Y + Cursor.Position.Y);
                end = new Point(end.X > 16 ? (start.X >= pictureBox1.Width || end.X < pictureBox1.Width - 16 ? end.X : pictureBox1.Width) : (start.X > 0 ? 0 : end.X), end.Y > 16 ? (start.Y >= pictureBox1.Height || end.Y < pictureBox1.Height - 16 ? end.Y : pictureBox1.Height) : (start.Y > 0 ? 0 : end.Y));
                var startOrig = ConvertPointToOriginal(start);
                var endOrig = ConvertPointToOriginal(end);
                var pos = new Point(Math.Min(start.X, end.X), Math.Min(start.Y, end.Y));
                var size = new Size(Math.Abs(start.X - end.X), Math.Abs(start.Y - end.Y));
                var sizeOrig = new Size(Math.Abs(startOrig.X - endOrig.X), Math.Abs(startOrig.Y - endOrig.Y));
                if (size.Width > 1 && size.Height > 1) {
                    lock (pictureBox3) {
                        var screenSize = Screen.FromControl(this).Bounds;
                        screenSize = new Rectangle(screenSize.Location, new Size(screenSize.Width + 20, screenSize.Height + 20));
                        var old = pictureBox3.Image;
                        var b = new Bitmap(size.Width, size.Height);
                        using var g = Graphics.FromImage(b);
                        g.Clear(Color.Transparent);
                        g.DrawRectangle(sizeOrig.Width > 120 && sizeOrig.Width <= screenSize.Width && sizeOrig.Height > 100 && sizeOrig.Height <= screenSize.Height ? Pens.LimeGreen : Pens.DarkRed, 0, 0, size.Width - 1, size.Height - 1);
                        pictureBox3.Image = b;
                        pictureBox3.Location = pos;
                        pictureBox3.Size = size;
                        old?.Dispose();
                    }
                }

            }
        }

        private void PictureBox1_MouseUp(object sender, MouseEventArgs e) {
            if (!toggleMagnifyingGlass && usingMagnifyingGlass) {
                usingMagnifyingGlass = false;

                pictureBox2.Location = new Point(-300, -300);
                pictureBox2.Size = new Size(240, 240);
                UpdateMouseCursor();
            }
            if ((e.Button & MouseButtons.Left) == MouseButtons.Left) {
                mouseLoc = null;
                if (selectionStarting.HasValue) {
                    var start = selectionStarting.Value;
                    selectionStarting = null;
                    lock (pictureBox3) {
                        pictureBox3.Location = new Point(-300, -300);
                        pictureBox3.Size = new Size(100, 100);
                    }
                    var end = e.Location;
                    var center = new Point((start.X + end.X) / 2 - end.X + Cursor.Position.X, (start.Y + end.Y) / 2 - end.Y + Cursor.Position.Y);
                    end = new Point(end.X > 16 ? (start.X >= pictureBox1.Width || end.X < pictureBox1.Width - 16 ? end.X : pictureBox1.Width) : (start.X > 0 ? 0 : end.X), end.Y > 16 ? (start.Y >= pictureBox1.Height || end.Y < pictureBox1.Height - 16 ? end.Y : pictureBox1.Height) : (start.Y > 0 ? 0 : end.Y));
                    start = ConvertPointToOriginal(start);
                    end = ConvertPointToOriginal(end);
                    var p = new Point(Math.Min(start.X, end.X), Math.Min(start.Y, end.Y));
                    var s = new Size(Math.Abs(start.X - end.X), Math.Abs(start.Y - end.Y));
                    var screenSize = Screen.FromControl(this).Bounds;
                    screenSize = new Rectangle(screenSize.Location, new Size(screenSize.Width + 20, screenSize.Height + 20));
                    if (s.Width > 120 && s.Width <= screenSize.Width && s.Height > 100 && s.Height <= screenSize.Height - pictureBox1.Location.Y) {
                        var sub = new SubWindow(p, s, displayManager, menuItemTrainMarkup.DropDownItems);
                        sub.Icon = Icon;
                        pictureBox1.Cursor = defaultCursor;
                        sub.SetMarkup9999(trainDataDict["9999"].Markup);
                        sub.Show();
                        var border = (Size.Width - ClientSize.Width) / 2;
                        sub.Location = new Point(center.X - s.Width / 2 - border, center.Y - s.Height / 2 - Size.Height + ClientSize.Height - panel1.Location.Y / 2 - border);
                        sub.SetTopMost(TopMost);
                        sub.SetSilent(Silent);
                        sub.SetClockColor(serverCommunication == null || UseServerTime ? Color.White : Color.Yellow);
                        displayManager.AddSubWindow(sub);
                    }
                }
            }
        }

        public void SetMagnifyingGlass(int x, int y) {
            if (usingMagnifyingGlass) {
                lock (pictureBox2) {
                    var posX = magnifyingGlassSize / 2 - x * displayManager.OriginalWidth / pictureBox1.Width;
                    var posY = magnifyingGlassSize / 2 - y * displayManager.OriginalHeight / pictureBox1.Height;
                    posX = posX > magnifyingGlassSize / 2 + 5 ? magnifyingGlassSize / 2 - x : (posX < magnifyingGlassSize / 2 - displayManager.OriginalWidth ? pictureBox1.Width - x + magnifyingGlassSize / 2 - displayManager.OriginalWidth : posX);
                    posY = posY > magnifyingGlassSize / 2 + 5 ? magnifyingGlassSize / 2 - y : (posY < magnifyingGlassSize / 2 - displayManager.OriginalHeight ? pictureBox1.Height - y + magnifyingGlassSize / 2 - displayManager.OriginalHeight : posY);

                    var b = new Bitmap(magnifyingGlassSize, magnifyingGlassSize);
                    var old = pictureBox2.Image;
                    pictureBox2.Image = b;
                    old?.Dispose();
                    using var g = Graphics.FromImage(pictureBox2.Image);
                    GraphicsPath gp = new();
                    gp.AddEllipse(g.VisibleClipBounds);
                    g.Clip = new Region(gp);

                    lock (displayManager.OriginalBitmap) {
                        g.DrawImage(displayManager.OriginalBitmap, posX, posY);
                    }
                    g.DrawEllipse(new Pen(Color.DarkGray, 2), 0, 0, magnifyingGlassSize, magnifyingGlassSize);
                }
            }
        }

        public void SetMagnifyingGlass() {

            if (InvokeRequired) {
                Invoke(() => {
                    var cp = pictureBox1.PointToClient(Cursor.Position);
                    SetMagnifyingGlass(cp.X, cp.Y);
                });
            }
            else {
                var cp = pictureBox1.PointToClient(Cursor.Position);
                SetMagnifyingGlass(cp.X, cp.Y);
            }
        }

        private void menuItemPushToZoom_Click(object sender, EventArgs e) {
            toggleMagnifyingGlass = false;
            menuItemPushToZoom.CheckState = CheckState.Indeterminate;
            menuItemToggle.CheckState = CheckState.Unchecked;
        }

        private void menuItemToggle_Click(object sender, EventArgs e) {
            toggleMagnifyingGlass = true;
            menuItemPushToZoom.CheckState = CheckState.Unchecked;
            menuItemToggle.CheckState = CheckState.Indeterminate;
        }

        private void TIDWindow_Closing(object sender, EventArgs e) {
            if (LogManager.Output && LogManager.NeededWarning) {
                TaskDialog.ShowDialog(this, new TaskDialogPage {
                    Caption = "エラーログ出力 | TID - ダイヤ運転会",
                    Heading = "エラーログ出力",
                    Icon = TaskDialogIcon.Information,
                    Text =
                        $"エラーログが出力されました。\n本ソフトの製作担当者にお問い合わせのうえ、\n必要な場合はErrorLog.txtをお送りください。\n（ErrorLog.txtは次回起動後に削除される場合があります）"
                });
            }
        }

        private void menuItemSilent_Click(object sender, EventArgs e) {
            SetSilent(!Silent);
        }

        private void menuItemTopMost_Click(object sender, EventArgs e) {
            SetTopMost(!TopMost);
        }

        private void menuItemMarkupType1_Click(object sender, EventArgs e) {
            SetMarkupType(0);
        }

        private void menuItemMarkupType2_Click(object sender, EventArgs e) {
            SetMarkupType(1);
        }

        private void menuItemMarkupType3_Click(object sender, EventArgs e) {
            SetMarkupType(2);
        }

        private void menuItemMarkupDuplication_Click(object sender, EventArgs e) {
            SwitchMarkupDuplication();
        }

        public void SwitchMarkupDuplication() {
            MarkupDuplication = !MarkupDuplication;
            menuItemMarkupDuplication.CheckState = MarkupDuplication ? CheckState.Checked : CheckState.Unchecked;
            if (displayManager != null) {
                foreach (var w in displayManager.SubWindows) {
                    w.SetMarkupDuplication(MarkupDuplication);
                }
            }

            ReservedUpdate = true;
        }

        private void menuItemMarkupFillZero_Click(object sender, EventArgs e) {
            SwitchMarkupFillZero();
        }

        public void SwitchMarkupFillZero() {
            MarkupFillZero = !MarkupFillZero;
            menuItemMarkupFillZero.CheckState = MarkupFillZero ? CheckState.Checked : CheckState.Unchecked;
            if (displayManager != null) {
                foreach (var w in displayManager.SubWindows) {
                    w.SetMarkupFillZero(MarkupFillZero);
                }
            }
            ReservedUpdate = true;
        }

        private void menuItemMarkup9999_Click(object sender, EventArgs e) {
            SwitchMarkup9999();
        }

        public void SwitchMarkup9999() {
            var td = trainDataDict["9999"];
            td.Markup = !td.Markup;
            menuItemMarkup9999.CheckState = td.Markup ? CheckState.Checked : CheckState.Unchecked;
            if (displayManager != null) {
                foreach (var w in displayManager.SubWindows) {
                    w.SetMarkup9999(td.Markup);
                }
            }
            ReservedUpdate = true;
        }

        private void menuItemMarkupNotTrain_Click(object sender, EventArgs e) {
            SwitchMarkupNotTrain();
        }

        public void SwitchMarkupNotTrain() {
            MarkupNotTrain = !MarkupNotTrain;
            menuItemMarkupNotTrain.CheckState = MarkupNotTrain ? CheckState.Checked : CheckState.Unchecked;
            if (displayManager != null) {
                foreach (var w in displayManager.SubWindows) {
                    w.SetMarkupNotTrain(MarkupNotTrain);
                }
            }
            ReservedUpdate = true;
        }

        private void menuItemMarkupSpawned_Click(object sender, EventArgs e) {
            SwitchMarkupSpawned();
        }

        public void SwitchMarkupSpawned() {
            MarkupSpawned = !MarkupSpawned;
            menuItemMarkupSpawned.CheckState = MarkupSpawned ? CheckState.Checked : CheckState.Unchecked;
            if (displayManager != null) {
                foreach (var w in displayManager.SubWindows) {
                    w.SetMarkupSpawned(MarkupSpawned);
                }
            }
        }

        private void menuItemMarkupHandover_Click(object sender, EventArgs e) {
            SwitchMarkupHandover();
        }

        public void SwitchMarkupHandover() {
            MarkupHandover = !MarkupHandover;
            menuItemMarkupHandover.CheckState = MarkupHandover ? CheckState.Checked : CheckState.Unchecked;
            if (displayManager != null) {
                foreach (var w in displayManager.SubWindows) {
                    w.SetMarkupHandover(MarkupHandover);
                }
            }
            if (MarkupHandover) {
                foreach (var t in trainDataDict.Values) {
                    if (t.Markup && int.TryParse(Regex.Replace(t.Number, @"[^0-9]", ""), out var numBody)) {
                        var umban = numBody / 3000 * 100 + numBody / 2 * 2 % 100;
                        if (!markupUmban.Contains(umban)) {
                            markupUmban.Add(umban);
                        }
                    }
                }
            }
            else {
                markupUmban.Clear();
            }
        }

        public void SetMarkupDelayed(int minutes) {
            if (minutes > 10) {
                minutes = 20;
            }
            else if (minutes > 5) {
                minutes = 10;
            }
            else if (minutes > 1) {
                minutes = 5;
            }
            else if (minutes < 0) {
                minutes = 0;
            }
            MarkupDelayed = minutes;
            menuItemMarkupDelayed0.CheckState = minutes == 0 ? CheckState.Indeterminate : CheckState.Unchecked;
            menuItemMarkupDelayed1.CheckState = minutes == 1 ? CheckState.Indeterminate : CheckState.Unchecked;
            menuItemMarkupDelayed5.CheckState = minutes == 5 ? CheckState.Indeterminate : CheckState.Unchecked;
            menuItemMarkupDelayed10.CheckState = minutes == 10 ? CheckState.Indeterminate : CheckState.Unchecked;
            menuItemMarkupDelayed20.CheckState = minutes == 20 ? CheckState.Indeterminate : CheckState.Unchecked;

            if (displayManager != null) {
                foreach (var w in displayManager.SubWindows) {
                    w.SetMarkupDelayed(minutes);
                }
            }
            ReservedUpdate = true;
        }

        private void menuItemMarkupDelayed0_Click(object sender, EventArgs e) {
            SetMarkupDelayed(0);
        }

        private void menuItemMarkupDelayed1_Click(object sender, EventArgs e) {
            SetMarkupDelayed(1);
        }

        private void menuItemMarkupDelayed5_Click(object sender, EventArgs e) {
            SetMarkupDelayed(5);
        }

        private void menuItemMarkupDelayed10_Click(object sender, EventArgs e) {
            SetMarkupDelayed(10);
        }

        private void menuItemMarkupDelayed20_Click(object sender, EventArgs e) {
            SetMarkupDelayed(20);
        }

        private void menuItemMarkupAll_Click(object sender, EventArgs e) {
            MarkupAll();
        }

        public void MarkupAll() {
            foreach (var t in trainDataDict.Keys) {
                trainDataDict[t].Markup = true;
                trainMenuDict[t].CheckState = CheckState.Checked;
                foreach (var w in displayManager.SubWindows) {
                    w.SetMarkupTrain(t, true);
                }
                if (int.TryParse(Regex.Replace(t, @"[^0-9]", ""), out var numBody)) {
                    var umban = numBody / 3000 * 100 + numBody / 2 * 2 % 100;
                    if (!markupUmban.Contains(umban)) {
                        markupUmban.Add(umban);
                    }
                }
            }
            ReservedUpdate = true;
        }

        private void menuItemMarkupCancel_Click(object sender, EventArgs e) {
            MarkupCancel();
        }

        public void MarkupCancel() {
            foreach (var t in trainDataDict.Keys) {
                trainDataDict[t].Markup = false;
                trainMenuDict[t].CheckState = CheckState.Unchecked;
                foreach (var w in displayManager.SubWindows) {
                    w.SetMarkupTrain(t, false);
                }
            }
            markupUmban.Clear();
            ReservedUpdate = true;
        }

        private void menuItemHideNumber_Click(object sender, EventArgs e) {
            SwitchHideNumber(this);
        }

        public void SwitchHideNumber(Form owner) {
            if (HideNumber) {
                if (LockHideNumber) {
                    OpeningDialog = true;
                    TaskDialog.ShowDialog(owner, new TaskDialogPage {
                        Caption = "残念 | TID - ダイヤ運転会",
                        Heading = "鎖錠されています",
                        Icon = TaskDialogIcon.Error,
                        Text = "列番表示隠しはONで鎖錠されているため、\nあなたは真実を確認することができません。"
                    });
                    OpeningDialog = false;
                    return;
                }
                else {
                    if (OpeningDialog) {
                        return;
                    }
                    OpeningDialog = true;
                    var result = TaskDialog.ShowDialog(owner, new TaskDialogPage {
                        Caption = "確認 | TID - ダイヤ運転会",
                        Icon = TaskDialogIcon.Warning,
                        Text = "あなたは真実を受け入れる覚悟ができていますか？",
                        Buttons = { TaskDialogButton.Yes, TaskDialogButton.No },
                        DefaultButton = TaskDialogButton.No

                    });
                    /*DialogResult result = MessageBox.Show($"あなたは真実を受け入れる覚悟ができていますか？", "確認 | TID - ダイヤ運転会",
                        MessageBoxButtons.YesNo, MessageBoxIcon.Question);*/
                    OpeningDialog = false;
                    if (result != TaskDialogButton.Yes) {
                        return;
                    }
                }
            }
            HideNumber = !HideNumber;
            menuItemHideNumber.CheckState = HideNumber ? CheckState.Checked : CheckState.Unchecked;
            foreach (var m in trainMenuDict.Values) {
                m.Text = HideNumber ? "??????" : m.Name;
            }
            if (displayManager != null) {
                foreach (var w in displayManager.SubWindows) {
                    w.SetHideNumber(HideNumber);
                }
            }
            ReservedUpdate = true;
        }

        private void menuItemServerTime_Click(object sender, EventArgs e) {
            SetUseServerTime(!UseServerTime);

        }

        public void SetUseServerTime(bool value) {
            if (serverCommunication != null) {
                UseServerTime = value;
                menuItemServerTime.CheckState = UseServerTime ? CheckState.Checked : CheckState.Unchecked;
                Color color;
                if (UseServerTime) {
                    color = Color.White;
                }
                else {
                    color = Color.Yellow;
                }
                labelClock.ForeColor = color;
                foreach(var w in displayManager.SubWindows) {
                    w.SetClockColor(color);
                }
            }
        }

        private Point ConvertPointToOriginal(int x, int y) {
            return new Point(x * displayManager.OriginalWidth / pictureBox1.Width, y * displayManager.OriginalHeight / pictureBox1.Height);
        }

        private Point ConvertPointToOriginal(Point p) {
            return ConvertPointToOriginal(p.X, p.Y);
        }

        private Point ConvertPointToScreen(int x, int y) {
            return new Point(x * pictureBox1.Width / displayManager.OriginalWidth, y * pictureBox1.Height / displayManager.OriginalHeight);
        }

        private Point ConvertPointToScreen(Point p) {
            return ConvertPointToScreen(p.X, p.Y);
        }

        private bool IsInArea(Point point, int areaX, int areaY, Size areaSize, int padding = 0) {
            var p = ConvertPointToOriginal(point);
            return p.X >= areaX - padding && p.X < (areaX + areaSize.Width + padding) && p.Y >= areaY - padding && p.Y < (areaY + areaSize.Height + padding);
        }

        private void UpdateMouseCursor() {
            if (ModifierKeys.HasFlag(Keys.Shift)) {
                pictureBox1.Cursor = Cursors.Hand;
                pictureBox2.Cursor = Cursors.Hand;
            }
            else if (ModifierKeys.HasFlag(Keys.Control)) {
                pictureBox1.Cursor = Cursors.Cross;
                pictureBox2.Cursor = Cursors.Cross;
            }
            else if (usingMagnifyingGlass) {
                pictureBox1.Cursor = Cursors.Cross;
                pictureBox2.Cursor = Cursors.Cross;
            }
            else {
                pictureBox1.Cursor = defaultCursor;
                pictureBox2.Cursor = Cursors.Cross;
            }
        }

        private void menuItemVersion_Click(object sender, EventArgs e) {
            var form = new VersionWindow();
            form.Icon = Icon;
            var bitmap = Icon != null ? new Icon(Icon, 256, 256).ToBitmap() : new Bitmap(10, 10);
            form.PictureIcon.Image = bitmap;
            form.PictureIcon.Size = new Size(bitmap.Width, bitmap.Height);
            form.LabelVersion.Text = $"TrainCrewTIDWindow\nVer. {ServerAddress.Version.Replace("TrainCrewTIDWindow_", "")}";
            if (TopMost) {
                form.TopMost = true;
            }
            OpeningDialog = true;
            form.ShowDialog();
            OpeningDialog = false;

        }

        public void UpdateTrainCheck(TrainData td) {
            trainMenuDict[td.Number].CheckState = td.Markup ? CheckState.Checked : CheckState.Unchecked;
            foreach (var w in displayManager.SubWindows) {
                w.UpdateTrainCheck(td);
            }
        }

        public void SetTrainMarkup(string trainNumber) {
            if (trainDataDict.TryGetValue(trainNumber, out var td)) {
                td.Markup = !td.Markup;
                UpdateTrainCheck(td);
                if (MarkupHandover && int.TryParse(Regex.Replace(trainNumber, @"[^0-9]", ""), out var numBody)) {
                    var umban = numBody / 3000 * 100 + numBody / 2 * 2 % 100;
                    if (td.Markup) {
                        if (!markupUmban.Contains(umban)) {
                            markupUmban.Add(umban);
                        }
                    }
                    else if (markupUmban.Contains(umban)) {
                        markupUmban.Remove(umban);
                    }
                }
                ReservedUpdate = true;
            }
        }


        public void SetStatusSubWindow(string text, Color color) {
            SubWindow.SetStatus(text, color);
            foreach (var w in displayManager.SubWindows) {
                w.UpdateStatus();
            }
        }
    }
}

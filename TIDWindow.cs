using Dapplo.Microsoft.Extensions.Hosting.WinForms;
using System.Collections.ObjectModel;
using OpenIddict.Client;
using TrainCrewTIDWindow.Communications;
using TrainCrewTIDWindow.Manager;
using TrainCrewTIDWindow.Models;
using System.Diagnostics;
using System.Text;
using System.Drawing.Drawing2D;
using TrainCrewTIDWindow.Settings;
using System.Media;

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

        private SoundPlayer? warningSound = null;

        public void PlayWarningSound() {
            if (warningSound != null) {
                warningSound.Play();
            }
            else {
                SystemSounds.Hand.Play();
            }
        }

        public bool Silent { get; private set; } = false;

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

        public ReadOnlyDictionary<string, PointData> PointDataDict => pointDataDict.AsReadOnly();

        public ReadOnlyDictionary<string, LCR> DirectionDataDict => directionDataDict.AsReadOnly();

        public TrackManager TrackManager => trackManager;

        public TIDWindow(OpenIddictClientService service) {
            this.service = service;
            InitializeComponent();
            LogManager.AddInfoLog("起動");

            pictureBox2.Parent = pictureBox1;

            var loaded = false;

            loaded |= LoadSetting(Environment.GetFolderPath(Environment.SpecialFolder.Personal) + @"\TRAIN CREW Tool\TrainCrewTIDWindow\setting.txt");

            loaded |= LoadSetting(".\\setting\\setting.txt");

            if (!loaded) {
                using (StreamWriter w = new(".\\setting\\setting.txt", false, new UTF8Encoding(false))) {
                    w.Write("source=select\ntopMost=true\nscale=100\ntimeOffset=14\nzoomMode=pushtozoom\nzoomSize=240\nsilent=false");
                }
            }

            if (File.Exists(".\\sound\\warning.wav")) {
                warningSound = new SoundPlayer(".\\sound\\warning.wav");
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

            menuItemHour0.Click += (sender, e) => { SetHourQuick(0); };
            menuItemHour1.Click += (sender, e) => { SetHourQuick(1); };
            menuItemHour2.Click += (sender, e) => { SetHourQuick(2); };
            menuItemHour3.Click += (sender, e) => { SetHourQuick(3); };
            menuItemHour4.Click += (sender, e) => { SetHourQuick(4); };
            menuItemHour5.Click += (sender, e) => { SetHourQuick(5); };
            menuItemHour6.Click += (sender, e) => { SetHourQuick(6); };
            menuItemHour7.Click += (sender, e) => { SetHourQuick(7); };
            menuItemHour8.Click += (sender, e) => { SetHourQuick(8); };
            menuItemHour9.Click += (sender, e) => { SetHourQuick(9); };
            menuItemHour10.Click += (sender, e) => { SetHourQuick(10); };
            menuItemHour11.Click += (sender, e) => { SetHourQuick(11); };
            menuItemHour12.Click += (sender, e) => { SetHourQuick(12); };
            menuItemHour13.Click += (sender, e) => { SetHourQuick(13); };
            menuItemHour14.Click += (sender, e) => { SetHourQuick(14); };
            menuItemHour15.Click += (sender, e) => { SetHourQuick(15); };
            menuItemHour16.Click += (sender, e) => { SetHourQuick(16); };
            menuItemHour17.Click += (sender, e) => { SetHourQuick(17); };
            menuItemHour18.Click += (sender, e) => { SetHourQuick(18); };
            menuItemHour19.Click += (sender, e) => { SetHourQuick(19); };
            menuItemHour20.Click += (sender, e) => { SetHourQuick(20); };
            menuItemHour21.Click += (sender, e) => { SetHourQuick(21); };
            menuItemHour22.Click += (sender, e) => { SetHourQuick(22); };
            menuItemHour23.Click += (sender, e) => { SetHourQuick(23); };
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
                            topMostSetting = texts[1].Replace(" ", "").ToLower() == "true";
                            break;
                        case "scale":
                            menuItemScale50.CheckState = CheckState.Unchecked;
                            menuItemScale75.CheckState = CheckState.Unchecked;
                            menuItemScale90.CheckState = CheckState.Unchecked;
                            menuItemScale100.CheckState = CheckState.Unchecked;
                            menuItemScale110.CheckState = CheckState.Unchecked;
                            menuItemScale125.CheckState = CheckState.Unchecked;
                            menuItemScale150.CheckState = CheckState.Unchecked;
                            menuItemScale175.CheckState = CheckState.Unchecked;
                            menuItemScale200.CheckState = CheckState.Unchecked;
                            menuItemScaleFit.CheckState = CheckState.Unchecked;

                            if (texts[1].Replace(" ", "").ToLower() == "fit") {
                                TIDScale = -1;
                                menuItemScaleFit.CheckState = CheckState.Indeterminate;
                                break;
                            }
                            switch (texts[1]) {
                                case "50":
                                    TIDScale = 50;
                                    menuItemScale50.CheckState = CheckState.Indeterminate;
                                    break;
                                case "75":
                                    TIDScale = 75;
                                    menuItemScale75.CheckState = CheckState.Indeterminate;
                                    break;
                                case "90":
                                    TIDScale = 90;
                                    menuItemScale90.CheckState = CheckState.Indeterminate;
                                    break;
                                case "100":
                                    TIDScale = 100;
                                    menuItemScale100.CheckState = CheckState.Indeterminate;
                                    break;
                                case "110":
                                    TIDScale = 110;
                                    menuItemScale110.CheckState = CheckState.Indeterminate;
                                    break;
                                case "125":
                                    TIDScale = 125;
                                    menuItemScale125.CheckState = CheckState.Indeterminate;
                                    break;
                                case "150":
                                    TIDScale = 150;
                                    menuItemScale150.CheckState = CheckState.Indeterminate;
                                    break;
                                case "175":
                                    TIDScale = 175;
                                    menuItemScale175.CheckState = CheckState.Indeterminate;
                                    break;
                                case "200":
                                    TIDScale = 200;
                                    menuItemScale200.CheckState = CheckState.Indeterminate;
                                    break;
                            }
                            break;
                        case "timeOffset":
                            if (int.TryParse(texts[1], out var hours)) {
                                TimeOffset = new TimeSpan(((hours % 24) + 24) % 24, 0, 0);
                            }
                            break;
                        case "zoomMode":
                            toggleMagnifyingGlass = texts[1].Replace(" ", "").ToLower() == "toggle";
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
                            SetSilent(texts[1].Replace(" ", "").ToLower() == "true");
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
                    LogManager.AddInfoLog("TRAIN CREWに接続します");
                    await TryConnectTrainCrew();
                    break;
                case "debug":
                    LogManager.AddInfoLog("デバッグモードを開始します");
                    debugIndex = 0;
                    break;
                default:
                    /*trackManager.CountStart = 0;*/

                    //デフォルトのサーバへの接続処理
                    serverCommunication = new(this, ServerAddress.SignalAddress, service);
                    serverCommunication.DataUpdated += UpdateServerData;
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
                if (showOffset <= 0) {
                    labelClock.Text = (Clock + TimeOffset).ToString("H:mm:ss");
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

            var updated = tcList != null && trackManager.UpdateTCData(tcList);
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
                if (!ServerCommunication.Error) {
                    ServerCommunication.Error = true;
                    LogManager.AddWarningLog("サーバからの受信が10秒以上ありません");
                    LabelStatusText = $"データ受信不能(最終受信：{updatedTime?.ToString("H:mm:ss")})";
                    Debug.WriteLine($"データ受信不能: {delaySeconds}");
                    if (!Silent) {
                        TaskDialog.ShowDialog(new TaskDialogPage {
                            Caption = "データ受信不能 | TID - ダイヤ運転会",
                            Heading = "データ受信不能",
                            Icon = TaskDialogIcon.Error,
                            Text = "サーバ側からのデータ受信が10秒以上ありませんでした。\n復旧を試みますが、しばらく経っても復旧しない場合はアプリケーションの再起動をおすすめします。"
                        });
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
                    trackManager.UpdateTCData([new TrackCircuitData() { Name = line.TrackName, Last = debugIndex < lineData.Count ? "1111" : "1112", On = true }]);
                    if (line.PointName != "") {
                        UpdatePointData([new SwitchData() { Name = line.PointName, State = line.Reversed ? NRC.Reversed : NRC.Normal }]);
                        LabelStatusText = $"デバッグモード（{(debugIndex < lineData.Count ? "下り" : "上り")}） track: {line.TrackName}  switch: {line.PointName} {(line.Reversed ? "R" : "N")}";
                    }
                    else {
                        LabelStatusText = $"デバッグモード（{(debugIndex < lineData.Count ? "下り" : "上り")}） track: {line.TrackName}";
                    }
                    trackManager.UpdateNumberWindow();
                    displayManager.UpdateTID();
                    debugCount = debugCount == -10000 ? -100 : 100;
                }
                if (debugCount > 0) {
                    debugCount--;
                }
            }
        }

        private void labelClock_MouseDown(object sender, MouseEventArgs e) {
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

        private void SetSilent(bool silent) {
            Silent = silent;
            menuItemSilent.CheckState = silent ? CheckState.Checked : CheckState.Unchecked;
            labelSilent.Text = $"サイレント：{(silent ? "ON" : "OFF")}";
            labelSilent.ForeColor = silent ? Color.Gray : Color.White;
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
            LogManager.AddInfoLog($"拡大率変更：{(scale > 0 ? $"{scale}%" : "fit")}");

            menuItemScale50.CheckState = CheckState.Unchecked;
            menuItemScale75.CheckState = CheckState.Unchecked;
            menuItemScale90.CheckState = CheckState.Unchecked;
            menuItemScale100.CheckState = CheckState.Unchecked;
            menuItemScale110.CheckState = CheckState.Unchecked;
            menuItemScale125.CheckState = CheckState.Unchecked;
            menuItemScale150.CheckState = CheckState.Unchecked;
            menuItemScale175.CheckState = CheckState.Unchecked;
            menuItemScale200.CheckState = CheckState.Unchecked;
            menuItemScaleFit.CheckState = CheckState.Unchecked;

            switch (scale) {
                case 50:
                    menuItemScale50.CheckState = CheckState.Indeterminate;
                    break;
                case 75:
                    menuItemScale75.CheckState = CheckState.Indeterminate;
                    break;
                case 90:
                    menuItemScale90.CheckState = CheckState.Indeterminate;
                    break;
                case 100:
                    menuItemScale100.CheckState = CheckState.Indeterminate;
                    break;
                case 110:
                    menuItemScale110.CheckState = CheckState.Indeterminate;
                    break;
                case 125:
                    menuItemScale125.CheckState = CheckState.Indeterminate;
                    break;
                case 150:
                    menuItemScale150.CheckState = CheckState.Indeterminate;
                    break;
                case 175:
                    menuItemScale175.CheckState = CheckState.Indeterminate;
                    break;
                case 200:
                    menuItemScale200.CheckState = CheckState.Indeterminate;
                    break;
                case -1:
                    menuItemScaleFit.CheckState = CheckState.Indeterminate;
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

        private void SetHourQuick(int hour) {
            TimeOffset = new TimeSpan((hour + 24 - Clock.Hour) % 24, TimeOffset.Minutes, TimeOffset.Seconds);
        }




        private void labelScale_MouseDown(object sender, MouseEventArgs e) {
            if (TIDScale > 0) {
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
                displayManager.CopyImage();
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

        private void PictureBox1_MouseWheel(object sender, MouseEventArgs e) {
            if (ModifierKeys.HasFlag(Keys.Control)) {
                if (TIDScale > 0) {
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
            if (displayManager != null && TIDScale == -1) {
                displayManager.ChangeScale();
                labelScale.Text = $"Scale：{(int)((double)pictureBox1.Image.Width / displayManager.OriginalBitmap.Width * 100 + 0.5)}%";
            }
        }

        private void PictureBox1_MouseDown(object sender, MouseEventArgs e) {
            if (e.Button == MouseButtons.Middle && pictureBox1.Width < displayManager.OriginalBitmap.Width) {
                if (toggleMagnifyingGlass && usingMagnifyingGlass) {
                    usingMagnifyingGlass = false;

                    pictureBox2.Location = new Point(-300, -300);
                    pictureBox2.Size = new Size(240, 240);
                    Cursor.Show();
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
                        pictureBox2.Location = new Point(e.X - panel1.HorizontalScroll.Value - magnifyingGlassSize / 2, e.Y - panel1.VerticalScroll.Value - magnifyingGlassSize / 2);
                        pictureBox2.Size = new Size(Math.Min(magnifyingGlassSize, Math.Max(0, pictureBox1.Width - e.X + magnifyingGlassSize / 2)), Math.Min(magnifyingGlassSize, Math.Max(0, pictureBox1.Height - e.Y + magnifyingGlassSize / 2)));
                    }

                    SetMagnifyingGlass(e.X, e.Y);

                    Cursor.Hide();
                }

            }
            else if (usingMagnifyingGlass) {
                usingMagnifyingGlass = false;

                pictureBox2.Location = new Point(-300, -300);
                pictureBox2.Size = new Size(240, 240);
                Cursor.Show();
            }
            if ((e.Button & MouseButtons.Left) == MouseButtons.Left) {
                mouseLoc = e.Location;
            }
        }

        private void PictureBox2_MouseDown(object sender, MouseEventArgs e) {
            if (e.Button == MouseButtons.Middle && pictureBox1.Width < displayManager.OriginalBitmap.Width) {
                if (toggleMagnifyingGlass && usingMagnifyingGlass) {
                    usingMagnifyingGlass = false;

                    pictureBox2.Location = new Point(-300, -300);
                    pictureBox2.Size = new Size(240, 240);
                    Cursor.Show();
                }
            }
            if ((e.Button & MouseButtons.Right) == MouseButtons.Right) {
                usingMagnifyingGlass = false;

                pictureBox2.Location = new Point(-300, -300);
                pictureBox2.Size = new Size(240, 240);
                Cursor.Show();
            }
        }

        private void PictureBox1_MouseMove(object sender, MouseEventArgs e) {
            if (usingMagnifyingGlass) {
                if (!toggleMagnifyingGlass) {
                    if (e.Button == MouseButtons.Middle) {
                        var width = pictureBox1.Width - e.X + magnifyingGlassSize / 2;
                        var height = pictureBox1.Height - e.Y + magnifyingGlassSize / 2;
                        var mouseX = e.X - panel1.HorizontalScroll.Value;
                        var mouseY = e.Y - panel1.VerticalScroll.Value;
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
                        Cursor.Show();
                    }
                }
                else {
                    var width = pictureBox1.Width - e.X + magnifyingGlassSize / 2;
                    var height = pictureBox1.Height - e.Y + magnifyingGlassSize / 2;
                    var mouseX = e.X - panel1.HorizontalScroll.Value;
                    var mouseY = e.Y - panel1.VerticalScroll.Value;
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
            if ((e.Button & MouseButtons.Left) == MouseButtons.Left) {
                panel1.AutoScrollPosition = new Point(panel1.HorizontalScroll.Value - e.Location.X + mouseLoc.X, panel1.VerticalScroll.Value - e.Location.Y + mouseLoc.Y);
            }
        }

        private void PictureBox2_MouseMove(object sender, MouseEventArgs e) {
            if (usingMagnifyingGlass) {

                if (toggleMagnifyingGlass) {
                    var cp = pictureBox1.PointToClient(Cursor.Position);
                    var width = pictureBox1.Width - cp.X + magnifyingGlassSize / 2;
                    var height = pictureBox1.Height - cp.Y + magnifyingGlassSize / 2;
                    var mouseX = cp.X - panel1.HorizontalScroll.Value;
                    var mouseY = cp.Y - panel1.VerticalScroll.Value;
                    if (width <= 1 || height <= 1) {
                        pictureBox2.Location = new Point(-300, -300);
                        pictureBox2.Size = new Size(240, 240);
                    }
                    else {
                        pictureBox2.Location = new Point(mouseX - magnifyingGlassSize / 2, mouseY - magnifyingGlassSize / 2);
                        pictureBox2.Size = new Size(Math.Min(magnifyingGlassSize, Math.Max(0, pictureBox1.Width - cp.X + magnifyingGlassSize / 2)), Math.Min(magnifyingGlassSize, Math.Max(0, pictureBox1.Height - cp.Y + magnifyingGlassSize / 2)));
                    }

                    SetMagnifyingGlass(cp.X, cp.Y);
                }
            }
        }

        private void PictureBox1_MouseUp(object sender, MouseEventArgs e) {
            if (!toggleMagnifyingGlass && usingMagnifyingGlass) {
                usingMagnifyingGlass = false;

                pictureBox2.Location = new Point(-300, -300);
                pictureBox2.Size = new Size(240, 240);
                Cursor.Show();
            }
            if ((e.Button & MouseButtons.Left) == MouseButtons.Left) {
                mouseLoc = Point.Empty;
            }
        }

        public void SetMagnifyingGlass(int x, int y) {
            if (usingMagnifyingGlass) {
                lock (displayManager.OriginalBitmap)
                    lock (pictureBox2) {
                        var posX = magnifyingGlassSize / 2 - x * displayManager.OriginalBitmap.Width / pictureBox1.Width;
                        var posY = magnifyingGlassSize / 2 - y * displayManager.OriginalBitmap.Height / pictureBox1.Height;
                        posX = posX > magnifyingGlassSize / 2 + 5 ? magnifyingGlassSize / 2 - x : (posX < magnifyingGlassSize / 2 - displayManager.OriginalBitmap.Width ? pictureBox1.Width - x + magnifyingGlassSize / 2 - displayManager.OriginalBitmap.Width : posX);
                        posY = posY > magnifyingGlassSize / 2 + 5 ? magnifyingGlassSize / 2 - y : (posY < magnifyingGlassSize / 2 - displayManager.OriginalBitmap.Height ? pictureBox1.Height - y + magnifyingGlassSize / 2 - displayManager.OriginalBitmap.Height : posY);

                        var b = new Bitmap(magnifyingGlassSize, magnifyingGlassSize);
                        var old = pictureBox2.Image;
                        pictureBox2.Image = b;
                        old?.Dispose();
                        using var g = Graphics.FromImage(pictureBox2.Image);
                        GraphicsPath gp = new();
                        gp.AddEllipse(g.VisibleClipBounds);
                        g.Clip = new Region(gp);
                        g.DrawImage(displayManager.OriginalBitmap, posX, posY);
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
                TaskDialog.ShowDialog(new TaskDialogPage {
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
    }
}

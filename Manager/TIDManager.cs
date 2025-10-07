using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.Text.RegularExpressions;
using TrainCrewTIDWindow.Communications;
using TrainCrewTIDWindow.Forms;
using TrainCrewTIDWindow.Models;
using TrainCrewTIDWindow.Settings;

namespace TrainCrewTIDWindow.Manager {
    /// <summary>
    /// TID画面管理用
    /// </summary>
    public class TIDManager {
        /// <summary>
        /// TID画面表示用のPictureBox
        /// </summary>
        private PictureBox pictureBox;

        /// <summary>
        /// TIDWindowオブジェクト
        /// </summary>
        private TIDWindow window;

        /// <summary>
        /// 各トラックの線の位置やファイル名などのデータ
        /// </summary>
        private readonly List<LineSetting> lineSettings;

        /// <summary>
        /// 各トラックの列車番号の位置などのデータ（下り列車用）
        /// </summary>
        private readonly List<NumberWindowSetting> numSettingsDown;

        /// <summary>
        /// 各トラックの列車番号の位置などのデータ（上り列車用）
        /// </summary>
        private readonly List<NumberWindowSetting> numSettingsUp;

        private readonly Dictionary<Point, NumberWindowData> numberWindowDict = [];

        /// <summary>
        /// 踏切の位置やファイル名などのデータ
        /// </summary>
        private readonly List<CrossingSetting> crossingSettings = [];

        /// <summary>
        /// 単線区間の方向てこ状態を示す矢印の位置やファイル名などのデータ
        /// </summary>
        private readonly List<ArrowSetting> arrowSettings = [];

        /// <summary>
        /// 隣接軌道回路のデータ（転轍機状態も考慮）
        /// </summary>
        private readonly List<TrackConnectionSetting> trackConnections = [];

        /// <summary>
        /// 列車番号の色
        /// </summary>
        private readonly Dictionary<string, Color> numColor = [];

        /// <summary>
        /// 列車番号以外の色
        /// </summary>
        private readonly Dictionary<string, Color> colorDict = [];

        /// <summary>
        /// 列車番号の文字の画像内座標とサイズ
        /// </summary>
        private readonly Dictionary<char, NumberImageSetting> alphaIndexDict = [];

        /// <summary>
        /// 特殊な列車番号の画像内座標とサイズ
        /// </summary>
        private readonly List<NumberImageSetting> numIndexList = [];

        /// <summary>
        /// 画像
        /// </summary>
        private readonly Dictionary<string, Image> images = [];

        /// <summary>
        /// 起動時背景画像
        /// </summary>
        private Image backgroundDefault;

        /// <summary>
        /// 通常時背景画像
        /// </summary>
        private Image backgroundImage;

        /// <summary>
        /// 列車番号下線（遅延表示あり）
        /// </summary>
        private Image numLineL;

        /// <summary>
        /// 列車番号下線（遅延表示なし）
        /// </summary>
        private Image numLineM;

        /// <summary>
        /// 運行番号下線
        /// </summary>
        private Image numLineS;

        /// <summary>
        /// 番号フォント画像
        /// </summary>
        private Image numberImage;

        /// <summary>
        /// TID画像の元画像（リサイズ前）
        /// </summary>
        private Bitmap originalBitmap;

        private readonly Dictionary<string, bool> markupClassesData = [];

        private readonly List<SubWindow> subWindows = [];

        public bool Markuped => markupClassesData.Values.Any(d => d);


        public bool Started {
            get;
            private set;
        } = false;

        /// <summary>
        /// TID画面表示用のPictureBox
        /// </summary>
        public PictureBox PictureBox => pictureBox;

        /// <summary>
        /// TID画像の元画像（リサイズ前）
        /// </summary>
        public Bitmap OriginalBitmap => originalBitmap;


        /// <summary>
        /// 各トラックの線の位置やファイル名などのデータ
        /// </summary>
        public ReadOnlyCollection<LineSetting> LineSettings { get; init; }

        /// <summary>
        /// 各トラックの列車番号の位置などのデータ（下り列車用）
        /// </summary>
        public ReadOnlyCollection<NumberWindowSetting> NumSettingsDown { get; init; }

        /// <summary>
        /// 各トラックの列車番号の位置などのデータ（上り列車用）
        /// </summary>
        public ReadOnlyCollection<NumberWindowSetting> NumSettingsUp { get; init; }

        public ReadOnlyCollection<TrackConnectionSetting> TrackConnections { get; init; }

        public ReadOnlyDictionary<Point, NumberWindowData> NumberWindowDict { get; init; }

        public ReadOnlyCollection<SubWindow> SubWindows { get; init; }

        public TIDWindow Window => window;

        public bool IsActiveForm{
            get {
                var v = false;
                lock (subWindows) {
                    v = subWindows.Any(w => Form.ActiveForm == w || w.OpeningDialog);
                }
                return v;
            }
        }

        /// <summary>
        /// TID画面管理用
        /// </summary>
        /// <param name="pictureBox">TID画面表示用のPictureBox</param>
        /// <param name="window">TIDWindowオブジェクト</param>
        public TIDManager(PictureBox pictureBox, TIDWindow window) {
            this.pictureBox = pictureBox;
            this.window = window;

            backgroundDefault = Image.FromFile(".\\png\\Background-1.png");
            backgroundImage = Image.FromFile(".\\png\\Background.png");
            numLineL = Image.FromFile(".\\png\\TID_Retsuban_W_L.png");
            numLineM = Image.FromFile(".\\png\\TID_Retsuban_W_M.png");
            numLineS = Image.FromFile(".\\png\\TID_Retsuban_W_S.png");
            numberImage = Image.FromFile(".\\png\\Number.png");

            lineSettings = LoadLineSetting("linedata.tsv");
            numSettingsDown = LoadNumberSetting("number_down.tsv", numberWindowDict);
            numSettingsUp = LoadNumberSetting("number_up.tsv", numberWindowDict);


            LineSettings = lineSettings.AsReadOnly();
            TrackConnections = trackConnections.AsReadOnly();
            NumSettingsDown = numSettingsDown.AsReadOnly();
            NumSettingsUp = numSettingsUp.AsReadOnly();
            NumberWindowDict = numberWindowDict.AsReadOnly();
            SubWindows = subWindows.AsReadOnly();

            void AddNewClass(string key, string name) {
                markupClassesData.Add(key, false);
                var menu = new ToolStripMenuItem();
                window.MenuItemMarkupClass.DropDownItems.Add(menu);
                menu.Name = key;
                menu.Size = new Size(110, 22);
                menu.Text = name;
                menu.Click += (sender, e) => {
                    SetMarkupClass(key, window.MenuItemMarkupClass.DropDownItems.IndexOf(menu));
                };
            }

            try {
                using var sr = new StreamReader(".\\setting\\color_setting.tsv");
                sr.ReadLine();
                var line = sr.ReadLine();
                while (line != null) {
                    if (line.StartsWith('#')) {
                        line = sr.ReadLine();
                        continue;
                    }
                    var texts = line.Split('\t');
                    line = sr.ReadLine();

                    var i = 0;
                    for (; i < texts.Length; i++) {
                        if (texts[i] == "") {
                            break;
                        }
                    }
                    if (i < 4) {
                        continue;
                    }

                    var s = texts[0];

                    if (s.Length < 6) {
                        var color = Color.FromArgb(int.Parse(texts[1]), int.Parse(texts[2]), int.Parse(texts[3]));
                        numColor.Add(s, color);
                        if (texts.Length > 4 && texts[4].Length > 1) {
                            AddNewClass(s, texts[4]);
                        }
                    }
                    else {
                        colorDict.Add(texts[0], Color.FromArgb(int.Parse(texts[1]), int.Parse(texts[2]), int.Parse(texts[3])));
                    }
                }
            }
            catch {
            }


            AddNewClass("local", "普通");
            AddNewClass("rinji", "臨時");
            var sep = new ToolStripSeparator();
            window.MenuItemMarkupClass.DropDownItems.Add(sep);
            sep.Name = "sep";
            sep.Size = new Size(177, 6);
            AddNewClass("illegalZ", "Z");
            AddNewClass("superIllegalZ", "臨Z");

            try {
                using var sr = new StreamReader(".\\setting\\alpha_index.tsv");
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
                    if (texts[0].Length != 1) {
                        numIndexList.Add(new(texts[0], int.Parse(texts[1]), int.Parse(texts[2]), int.Parse(texts[3])));
                    }
                    else {
                        alphaIndexDict.Add(texts[0][0], new(texts[0], int.Parse(texts[1]), int.Parse(texts[2]), int.Parse(texts[3])));
                    }

                }
            }
            catch {
            }

            try {
                using var sr = new StreamReader(".\\setting\\crossing.tsv");
                sr.ReadLine();
                var line = sr.ReadLine();
                var name = "";
                while (line != null) {
                    if (line.StartsWith('#')) {
                        line = sr.ReadLine();
                        continue;
                    }
                    var texts = line.Split('\t');
                    line = sr.ReadLine();
                    var i = 1;
                    for (; i < texts.Length; i++) {
                        if (texts[i] == "") {
                            break;
                        }
                    }
                    if (i < 4) {
                        continue;
                    }
                    if (texts[0] != "") {
                        name = texts[0];
                    }
                    if (name == "") {
                        continue;
                    }

                    var imageName = texts[1];
                    crossingSettings.Add(new CrossingSetting(texts[0], imageName, int.Parse(texts[2]), int.Parse(texts[3])));

                    if (!images.ContainsKey($"{imageName}_R")) {
                        images[$"{imageName}_R"] = Image.FromFile($".\\png\\{imageName}_R.png");
                        images[$"{imageName}_G"] = Image.FromFile($".\\png\\{imageName}_G.png");
                    }
                }
            }
            catch {
            }


            try {
                using var sr = new StreamReader(".\\setting\\arrow.tsv");
                sr.ReadLine();
                var line = sr.ReadLine();
                var name = "";
                while (line != null) {
                    if (line.StartsWith('#')) {
                        line = sr.ReadLine();
                        continue;
                    }
                    var texts = line.Split('\t');
                    line = sr.ReadLine();
                    var i = 1;
                    for (; i < texts.Length; i++) {
                        if (texts[i] == "") {
                            break;
                        }
                    }
                    if (i < 7) {
                        continue;
                    }
                    if (texts[0] != "") {
                        name = texts[0];
                    }
                    if (name == "") {
                        continue;
                    }

                    var imageName = texts[2];
                    arrowSettings.Add(new ArrowSetting(texts[0], texts[1] == "R" ? LCR.Right : LCR.Left, imageName, int.Parse(texts[3]), int.Parse(texts[4]), texts[5], texts[6]));

                    if (!images.ContainsKey(imageName)) {
                        images[imageName] = Image.FromFile($".\\png\\{imageName}.png");
                    }
                }
            }
            catch {
            }


            try {
                using var sr = new StreamReader(".\\setting\\track_connection.tsv");
                sr.ReadLine();
                var line = sr.ReadLine();
                TrackConnectionSetting? track = null;
                var switches = new List<IfSwitch>();
                while (line != null) {
                    if (line.StartsWith('#')) {
                        line = sr.ReadLine();
                        continue;
                    }
                    var texts = line.Split('\t');
                    line = sr.ReadLine();
                    
                    if (texts[0] != "") {
                        track = new(texts[0]);
                    }
                    if (track == null) {
                        continue;
                    }
                    var lastText = 0;
                    for (var i = 1; i < texts.Length; i++) {
                        if (texts[i] != "") {
                            lastText = i;
                        }
                    }

                    if(lastText < 3) {
                        track.AddTarget(texts[1]);
                        if (!trackConnections.Contains(track)) {
                            trackConnections.Add(track);
                        }
                        switches = [];
                    }
                    else if (texts[2] != "" && texts[3] != "") {
                        switches.Add(new(texts[2], texts[3] == bool.TrueString));
                        if (texts[1] != "") {
                            track.AddTarget(texts[1], switches);
                            if (!trackConnections.Contains(track)) {
                                trackConnections.Add(track);
                            }
                            switches = [];
                        }
                    }

                }
            }
            catch {
            }

            window.Panel1.Size = new Size(window.ClientSize.Width, window.ClientSize.Height - window.Panel1.Location.Y);

            var width = backgroundDefault.Width * window.TIDScale / 100;
            var height = backgroundDefault.Height * window.TIDScale / 100;

            if (window.TIDScale < 0) {
                width = backgroundDefault.Width * 2;
                height = backgroundDefault.Height * 2;
            }

            window.MaximumSize = new Size(Math.Max(width, backgroundDefault.Width) + window.Size.Width - window.ClientSize.Width, Math.Max(height, backgroundDefault.Height) + window.Panel1.Location.Y + window.Size.Height - window.ClientSize.Height);


            lock (pictureBox) {
                if (window.TIDScale < 0) {
                    pictureBox.Width = window.Size.Width - 16;
                    pictureBox.Height = window.Size.Height - 39 - window.Panel1.Location.Y;
                    pictureBox.Cursor = Cursors.Default;
                }
                else {
                    pictureBox.Width = width;
                    pictureBox.Height = height;
                }
            }


            pictureBox.Image = new Bitmap(backgroundDefault);

            window.Size = new Size(Math.Max(backgroundDefault.Width * window.TIDScale / 100, backgroundDefault.Width) + window.Size.Width - window.ClientSize.Width, Math.Max(backgroundDefault.Height * window.TIDScale / 100, backgroundDefault.Height) + window.Panel1.Location.Y + window.Size.Height - window.ClientSize.Height);
            window.TopMost = true;

            // 試験表示
            {
                using var g = Graphics.FromImage(pictureBox.Image);
                foreach (var lineData in lineSettings) {
                    if (lineData != null && lineData.IsDefault) {
                        AddImage(g, images[lineData.FileNameR], lineData.PosX, lineData.PosY);
                    }
                }

                var ia = new ImageAttributes();
                ia.SetRemapTable([new ColorMap {
                    OldColor = Color.White,
                    NewColor = Color.Red
                }]);
                foreach (var numData in numberWindowDict.Values) {
                    if (numData == null || numData.NotDraw) {
                        continue;
                    }
                    Image image = numData.Size switch {
                        NumberSize.L => new Bitmap(numLineL),
                        NumberSize.S => new Bitmap(numLineS),
                        _ => new Bitmap(numLineM),
                    };
                    AddImage(g, image, numData.PosX, numData.PosY, ia);
                }


                foreach (var crossing in crossingSettings) {
                    if (crossing == null) {
                        continue;
                    }
                    AddImage(g, images[crossing.FileNameR], crossing.PosX, crossing.PosY);
                }
                foreach (var arrow in arrowSettings) {
                    if (arrow == null) {
                        continue;
                    }
                    AddImage(g, images[arrow.FileName], arrow.PosX, arrow.PosY);
                }
            }
            originalBitmap = new Bitmap(pictureBox.Image);
            ChangeScale();
            window.DetectResize = true;

        }



        /// <summary>
        /// 各トラックの線の位置やファイル名などのデータを読み込む
        /// </summary>
        /// <param name="fileName">ファイル名</param>
        /// <returns>読み込んだデータのリスト</returns>
        private List<LineSetting> LoadLineSetting(string fileName) {
            List<LineSetting> list = [];
            try {
                using var sr = new StreamReader($".\\setting\\{fileName}");
                sr.ReadLine();
                var line = sr.ReadLine();
                var trackName = "";
                while (line != null) {
                    if (line.StartsWith('#')) {
                        line = sr.ReadLine();
                        continue;
                    }
                    var texts = line.Split('\t');
                    line = sr.ReadLine();
                    var i = 1;
                    for (; i < texts.Length; i++) {
                        if (texts[i] == "") {
                            break;
                        }
                    }
                    if (i < 4) {
                        continue;
                    }
                    if (texts[0] != "") {
                        trackName = texts[0];
                    }
                    if (trackName == "") {
                        continue;
                    }
                    var imageName = texts[1];

                    if (i > 5) {
                        list.Add(new LineSetting(trackName, imageName, int.Parse(texts[2]), int.Parse(texts[3]), texts[4], texts[5] == bool.TrueString));
                    }
                    else {
                        list.Add(new LineSetting(trackName, imageName, int.Parse(texts[2]), int.Parse(texts[3])));
                    }
                    if (!images.ContainsKey($"{imageName}_R")) {
                        images[$"{imageName}_R"] = Image.FromFile($".\\png\\{imageName}_R.png");
                        images[$"{imageName}_Y"] = Image.FromFile($".\\png\\{imageName}_Y.png");
                    }
                }
            }
            catch {
            }
            return list;
        }

        /// <summary>
        /// 各トラックの列車番号の位置などのデータを読み込む
        /// </summary>
        /// <param name="fileName">ファイル名</param>
        /// <returns>読み込んだデータのリスト</returns>
        private List<NumberWindowSetting> LoadNumberSetting(string fileName) {
            return LoadNumberSetting(fileName, []);
        }

        /// <summary>
        /// 各トラックの列車番号の位置などのデータを読み込む
        /// </summary>
        /// <param name="fileName">ファイル名</param>
        /// <param name="dict">列番表示位置の辞書</param>
        /// <returns>読み込んだデータのリスト</returns>
        private List<NumberWindowSetting> LoadNumberSetting(string fileName, Dictionary<Point, NumberWindowData> dict) {
            List<NumberWindowSetting> list = [];

            try {
                using var sr = new StreamReader($".\\setting\\{fileName}");
                sr.ReadLine();
                var line = sr.ReadLine();
                var trackName = "";
                NumberWindowSetting? numSet = null;
                while (line != null) {
                    if (line.StartsWith('#')) {
                        line = sr.ReadLine();
                        continue;
                    }
                    var texts = line.Split('\t');
                    line = sr.ReadLine();
                    var i = 1;
                    for (; i < texts.Length; i++) {
                        if (texts[i] == "") {
                            break;
                        }
                    }
                    if (i < 4) {
                        continue;
                    }
                    var emptyName = false;
                    if (texts[0] != "") {
                        trackName = texts[0];
                    }
                    else {
                        emptyName = true;
                    }
                    if (trackName == "") {
                        continue;
                    }

                    var size = texts[1] switch {
                        "S" => NumberSize.S,
                        "M" => NumberSize.M,
                        _ => NumberSize.L,
                    };
                    var posX = int.Parse(texts[2]);
                    var posY = int.Parse(texts[3]);

                    var point = new Point(posX, posY);
                    var nwd = new NumberWindowData(size, posX, posY);
                    if(!dict.TryAdd(point, nwd)) {
                        nwd = dict[point];
                    }

                    if (!emptyName) {
                        if (i > 5) {
                            numSet = new NumberWindowSetting(trackName, nwd, texts[4], texts[5] == bool.TrueString);
                        }
                        else {
                            numSet = new NumberWindowSetting(trackName, nwd);
                        }
                        list.Add(numSet);
                    }
                    else if(numSet != null) {
                        numSet.AddNumberWindow(nwd);
                    }
                }
            }
            catch {
            }
            return list;

        }



        /// <summary>
        /// 必要であればTIDの在線表示を更新する
        /// データが更新された際はとりあえずこれを呼ぶ
        /// </summary>
        public void UpdateTID() {
            var trackDataDict = window.TrackDataDict;
            var pointDataDict = window.PointDataDict;
            var directionDataDict = window.DirectionDataDict;
            var trainDataDict = window.TrainDataDict;

            Bitmap? newPic = null;
            lock (backgroundImage) {
                newPic = new Bitmap(backgroundImage);
            }

            using var g = Graphics.FromImage(newPic);

            foreach (var track in trackDataDict.Values) {
                if (!track.OnTrain && !track.IsReserved) {
                    continue;
                }

                // トラックの在線、進路開通状態表示

                var rule = "";
                foreach (var line in track.LineSettings) {
                    if (line == null) {
                        continue;
                    }

                    // 転轍器の状態で表示条件を判定
                    var r = line.PointName != "" ? $"{line.PointName}/{line.Reversed}" : "";
                    if (r != "" && rule == "" && pointDataDict.ContainsKey(line.PointName)) {

                        var point = pointDataDict[line.PointName];
                        if (point.IsLocked && point.IsReversed == line.Reversed) {
                            rule = r;
                        }
                    }

                    // 表示条件を満たさない場合は表示しない
                    if (rule != r) {
                        continue;
                    }
                    AddImage(g, images[track.OnTrain ? line.FileNameR : line.FileNameY], line.PosX, line.PosY);
                }
                if (!track.OnTrain) {
                    continue;
                }

                // 列番表示

                string train = track.Train ?? "";


                var numHeader = Regex.Replace(train, @"[0-9a-zA-Z]", "");  // 列番の頭の文字（回、試など）
                var numBodyStr = Regex.Replace(train, @"[^0-9]", "");
                var isTrain = int.TryParse(numBodyStr, out var numBody);  // 列番本体（数字部分）
                var numFooter = Regex.Replace(train, @"[^a-zA-Z]", "");  // 列番の末尾の文字

                if(isTrain && window.TrackManager.GetTrackForNum(train) != track.Name) {
                    continue;
                }
                
            }

            var duplicatingTrains = window.TrackManager.DuplicatingTrains;

            foreach (var numWindow in numberWindowDict.Values) {
                if(numWindow.Train == null) {
                    continue;
                }
                _ = trainDataDict.TryGetValue(numWindow.Train, out var trainData);

                var numHeader = Regex.Replace(numWindow.Train, @"[0-9a-zA-Z]", "");  // 列番の頭の文字（回、試など）
                var numBodyStr = Regex.Replace(numWindow.Train, @"[^0-9]", "");
                var isTrain = int.TryParse(numBodyStr, out var numBody);  // 列番本体（数字部分）
                var numFooter = Regex.Replace(numWindow.Train, @"[^a-zA-Z]", "");  // 列番の末尾の文字

                // 遅延時分
                var delayMinute = trainData != null ? trainData.DelayMinutes : 0;
                var markUp = trainData != null && trainData.Markup;

                markUp |= window.MarkupDelayed > 0 && delayMinute >= window.MarkupDelayed;


                var iaType = new ImageAttributes();

                // 運番
                if (numWindow.Size == NumberSize.S) {
                    var hf = $"{numHeader}{numFooter}";
                    foreach (var k in numColor.Keys) {
                        if (hf.Contains(k)) {
                            if (markupClassesData.ContainsKey(k)) {
                                markUp |= markupClassesData[k];
                            }
                            break;
                        }
                    }
                    if(hf != "臨Z" && (numHeader == "" || numHeader == "臨") && (numFooter == "" || numFooter == "X" || numFooter == "Y" || numFooter == "Z")) {
                        markUp |= markupClassesData["local"];
                    }
                    if (numHeader == "臨") {
                        markUp |= markupClassesData["rinji"];
                    }
                    if (numFooter.Contains('Z')) {
                        markUp |= markupClassesData["illegalZ"];
                    }
                    if (numHeader == "臨" && numFooter.Contains('Z')) {
                        markUp |= markupClassesData["superIllegalZ"];
                    }

                    Color? color = null;
                    // 0埋め列番への警告色として不明色に
                    if (isTrain && numBodyStr[0] == '0') {
                        markUp |= window.MarkupFillZero;
                        if (colorDict.ContainsKey("UNKNOWN")) {
                            color = colorDict["UNKNOWN"];
                        }
                    }
                    // 列番被りへの警告色として不明色に
                    if (isTrain && duplicatingTrains.Contains(numWindow.Train)) {
                        markUp |= window.MarkupDuplication;
                        if (colorDict.ContainsKey("UNKNOWN")) {
                            color = colorDict["UNKNOWN"];
                        }
                    }
                    markUp |= window.MarkupNotTrain && !isTrain;

                    var numIndex = numIndexList.FirstOrDefault(i => i.Text == numWindow.Train && i.Width == 5);
                    if(numIndex != null) {
                        //色を取得
                        var colorKey = numColor.Keys.FirstOrDefault(numWindow.Train.Contains);
                        if (colorKey != null && numColor.TryGetValue(colorKey, out var newColor)) {
                            color = numColor[colorKey];
                        }
                        // 色が見つからなければとりあえず不明色に
                        if (color == null) {
                            if (colorDict.ContainsKey("UNKNOWN")) {
                                color = colorDict["UNKNOWN"];
                            }
                            else {
                                color = Color.White;
                            }
                        }

                        
                        if (trainData == null && !markUp) {
                            iaType.SetRemapTable([new ColorMap { OldColor = Color.White, NewColor = (Color)color }]);
                        }
                        else if (markUp && window.MarkupType > 0 && (window.MarkupType == 2 || window.FlashState)) {
                            iaType.SetRemapTable([new ColorMap { OldColor = Color.Black, NewColor = (Color)color }, new ColorMap { OldColor = Color.White, NewColor = Color.Black }]);
                        }
                        else if (markUp && window.MarkupType == 0 && !window.FlashState) {
                            iaType.SetRemapTable([new ColorMap { OldColor = Color.White, NewColor = Color.FromArgb(40, 40, 40) }]);
                        }
                        else {
                            iaType.SetRemapTable([new ColorMap { OldColor = Color.White, NewColor = (Color)color }]);
                        }
                        var iaLine = new ImageAttributes();
                        if (!markUp || trainData == null || window.MarkupType == 0 || (window.MarkupType != 2 && !window.FlashState)) {
                            iaLine.SetRemapTable([new ColorMap { OldColor = Color.White, NewColor = (Color)color }]);
                        }
                        else {
                            iaLine.SetRemapTable([new ColorMap { OldColor = Color.Black, NewColor = (Color)color }, new ColorMap { OldColor = Color.White, NewColor = (Color)color }]);
                        }


                        // 下線設置
                        AddImage(g, numLineS, numWindow.PosX, numWindow.PosY, iaLine);
                        // 列番設置
                        AddNumImage(g, numIndex.Width, numIndex.X, numIndex.Y, numWindow.PosX, numWindow.PosY, iaType);





                    }
                    else if (isTrain) {

                        // 遅延時分表示
                        var iaDelay = new ImageAttributes();
                        var delayColor = Color.White;
                        if (delayMinute >= 10 && colorDict.ContainsKey("delayTime10")) {
                            delayColor = colorDict["delayTime10"];
                        }
                        else if (delayMinute >= 5 && colorDict.ContainsKey("delayTime5")) {
                            delayColor = colorDict["delayTime5"];
                        }
                        else if (delayMinute >= 1 && colorDict.ContainsKey("delayTime1")) {
                            delayColor = colorDict["delayTime1"];
                        }

                        color ??= Color.White;
                        if (!markUp || trainData == null || window.MarkupType == 0 || (window.MarkupType != 2 && !window.FlashState)) {
                            iaDelay.SetRemapTable([new ColorMap { OldColor = Color.White, NewColor = delayColor }]);
                        }
                        else {
                            iaDelay.SetRemapTable([new ColorMap { OldColor = Color.Black, NewColor = (Color)color }, new ColorMap { OldColor = Color.White, NewColor = delayColor }]);
                        }


                        AddImage(g, numLineS, numWindow.PosX, numWindow.PosY, iaDelay);

                        if (trainData == null && !markUp) {
                            iaType.SetRemapTable([new ColorMap { OldColor = Color.White, NewColor = (Color)color }]);
                        }
                        else if (markUp && window.MarkupType > 0 && (window.MarkupType == 2 || window.FlashState)) {
                            iaType.SetRemapTable([new ColorMap { OldColor = Color.Black, NewColor = (Color)color }, new ColorMap { OldColor = Color.White, NewColor = Color.Black }]);
                        }
                        else if (markUp && window.MarkupType == 0 && !window.FlashState) {
                            iaType.SetRemapTable([new ColorMap { OldColor = Color.White, NewColor = Color.FromArgb(40, 40, 40) }]);
                        }
                        else {
                            iaType.SetRemapTable([new ColorMap { OldColor = Color.White, NewColor = (Color)color }]);
                        }




                        var umban = numBody / 3000 * 100 + numBody % 100;

                        // 運番を偶数にする・矢印設置
                        if (umban % 2 != 0) {
                            umban -= 1;
                            var index = alphaIndexDict['←'];
                            AddNumImage(g, index.X, index.Y, numWindow.PosX, numWindow.PosY, iaType);
                        }
                        else {
                            var index = alphaIndexDict['→'];
                            AddNumImage(g, index.X, index.Y, numWindow.PosX + 24, numWindow.PosY, iaType);
                        }

                        // 運番設置
                        for (var i = 2; i >= 0; i--) {
                            if (umban <= 0) {
                                if (numBodyStr[0] != '0') {
                                    break;
                                }
                                AddNumImage(g, 0, numWindow.PosX + 6 + i * 6, numWindow.PosY, iaType);
                            }
                            var num = umban % 10;
                            AddNumImage(g, num, numWindow.PosX + 6 + i * 6, numWindow.PosY, iaType);
                            umban /= 10;
                        }

                    }
                }
                // 列番
                else {
                    var retsuban = numBody;

                    // 種別色
                    Color? classColor = null;
                    var hf = $"{numHeader}{numFooter}";
                    foreach (var k in numColor.Keys) {
                        if (hf.Contains(k)) {
                            classColor = numColor[k];

                            if (markupClassesData.ContainsKey(k)) {
                                markUp |= markupClassesData[k];
                            }
                            break;
                        }
                    }
                    if (hf != "臨Z" && (numHeader == "" || numHeader == "臨") && (numFooter == "" || numFooter == "X" || numFooter == "Y" || numFooter == "Z")) {
                        markUp |= markupClassesData["local"];
                    }
                    if (numHeader == "臨") {
                        markUp |= markupClassesData["rinji"];
                    }
                    if (numFooter.Contains('Z')) {
                        markUp |= markupClassesData["illegalZ"];
                    }
                    if (numHeader == "臨" && numFooter.Contains('Z')) {
                        markUp |= markupClassesData["superIllegalZ"];
                    }
                    // 0埋め列番への警告色として不明色に
                    if (isTrain && numBodyStr[0] == '0') {
                        markUp |= window.MarkupFillZero;
                        if (colorDict.ContainsKey("UNKNOWN")) {
                            classColor = colorDict["UNKNOWN"];
                        }
                    }
                    // 列番被りへの警告色として不明色に
                    if (isTrain && duplicatingTrains.Contains(numWindow.Train)) {
                        markUp |= window.MarkupDuplication;
                        if (colorDict.ContainsKey("UNKNOWN")) {
                            classColor = colorDict["UNKNOWN"];
                        }
                    }
                    // 種別色無しかつ数字なしであれば不明色に
                    if (classColor == null) {
                        if (!isTrain && colorDict.ContainsKey("UNKNOWN")) {
                            classColor = colorDict["UNKNOWN"];
                        }
                        else {
                            classColor = Color.White;
                        }
                    }

                    markUp |= window.MarkupNotTrain && !isTrain;





                    // 遅延時分
                    var iaLine = new ImageAttributes();
                    var iaDelay = new ImageAttributes();
                    Color delayColor = Color.White;
                    if (delayMinute >= 10 && colorDict.ContainsKey("delayTime10")) {
                        delayColor = colorDict["delayTime10"];
                    }
                    else if (delayMinute >= 5 && colorDict.ContainsKey("delayTime5")) {
                        delayColor = colorDict["delayTime5"];
                    }
                    else if (delayMinute >= 1 && colorDict.ContainsKey("delayTime1")) {
                        delayColor = colorDict["delayTime1"];
                    }

                    if (trainData == null && !markUp) {
                        iaType.SetRemapTable([new ColorMap { OldColor = Color.White, NewColor = (Color)classColor }]);
                    }
                    else if (markUp && window.MarkupType > 0 && (window.MarkupType == 2 || window.FlashState)) {
                        iaType.SetRemapTable([new ColorMap { OldColor = Color.Black, NewColor = (Color)classColor }, new ColorMap { OldColor = Color.White, NewColor = Color.Black }]);
                    }
                    else if (markUp && window.MarkupType == 0 && !window.FlashState) {
                        iaType.SetRemapTable([new ColorMap { OldColor = Color.White, NewColor = Color.FromArgb(40, 40, 40) }]);
                    }
                    else {
                        iaType.SetRemapTable([new ColorMap { OldColor = Color.White, NewColor = (Color)classColor }]);
                    }

                    var numIndex = numIndexList.FirstOrDefault(i => i.Text == numWindow.Train && i.Width == 7);

                    if (!markUp || trainData == null || window.MarkupType == 0 || (window.MarkupType != 2 && !window.FlashState)) {
                        iaLine.SetRemapTable([new ColorMap { OldColor = Color.White, NewColor = numIndex != null ? (Color)classColor : delayColor }, new ColorMap { OldColor = Color.FromArgb(0, 255, 0), NewColor = Color.Black }, new ColorMap { OldColor = Color.Red, NewColor = Color.Black }]);
                        iaDelay.SetRemapTable([new ColorMap { OldColor = Color.White, NewColor = delayColor }]);
                    }
                    else {
                        if(numIndex != null) {
                            iaLine.SetRemapTable([new ColorMap { OldColor = Color.Black, NewColor = (Color)classColor }, new ColorMap { OldColor = Color.White, NewColor = (Color)classColor }, new ColorMap { OldColor = Color.FromArgb(0, 255, 0), NewColor = (Color)classColor }, new ColorMap { OldColor = Color.Red, NewColor = (Color)classColor }]);
                        }
                        else {
                            iaLine.SetRemapTable([new ColorMap { OldColor = Color.Black, NewColor = (Color)classColor }, new ColorMap { OldColor = Color.White, NewColor = delayColor }, new ColorMap { OldColor = Color.FromArgb(0, 255, 0), NewColor = Color.Black }, new ColorMap { OldColor = Color.Red, NewColor = delayColor }]);
                        }
                        iaDelay.SetRemapTable([new ColorMap { OldColor = Color.Black, NewColor = delayColor }, new ColorMap { OldColor = Color.White, NewColor = Color.Black }]);
                    }


                    if (numIndex != null) {
                        // 下線設置
                        if (numWindow.Size == NumberSize.L) {
                            AddImage(g, numLineL, numWindow.PosX, numWindow.PosY, iaLine);
                        }
                        else {
                            AddImage(g, numLineM, numWindow.PosX, numWindow.PosY, iaLine);
                        }

                        // 列番設置
                        AddNumImage(g, numIndex.Width, numIndex.X, numIndex.Y, numWindow.PosX, numWindow.PosY, iaType);




                    }
                    else if (isTrain) {


                        // 遅延時分表示
                        if (numWindow.Size == NumberSize.L) {
                            AddImage(g, numLineL, numWindow.PosX, numWindow.PosY, iaLine);
                        }
                        else {
                            AddImage(g, numLineM, numWindow.PosX, numWindow.PosY, iaLine);
                        }


                        if (numWindow.Size == NumberSize.L) {
                            if(delayMinute / 10 > 0) {
                                AddNumImage(g, delayMinute % 100 / 10, numWindow.PosX + 48, numWindow.PosY, iaDelay);
                            }
                            AddNumImage(g, delayMinute % 10, numWindow.PosX + 54, numWindow.PosY, iaDelay);
                        }

                        // 列番の頭の文字設置
                        if (numHeader.Length > 0 && alphaIndexDict.TryGetValue(numHeader[0], out var nh)) {
                            AddNumImage(g, nh.Width, nh.X, nh.Y, numWindow.PosX, numWindow.PosY, iaType);
                        }

                        // 列番本体設置
                        for (var i = 0; i < 4 && i < numBodyStr.Length; i++) {
                            var num = numBodyStr[numBodyStr.Length - 1 - i] - '0';
                            AddNumImage(g, num, numWindow.PosX + 12 + (3 - i) * 6, numWindow.PosY, iaType);
                        }


                        // 列番の末尾の文字設置
                        if (numFooter.Length > 0) {
                            var p = alphaIndexDict[numFooter[0]];
                            if (p.X < 55 && p.Y < 55) {
                                AddNumImage(g, p.X, p.Y, numWindow.PosX + 36, numWindow.PosY, iaType);
                            }
                        }
                        if (numFooter.Length > 1) {
                            var p = alphaIndexDict[numFooter[1]];
                            if (p.X < 55 && p.Y < 55) {
                                AddNumImage(g, p.X, p.Y, numWindow.PosX + 42, numWindow.PosY, iaType);
                            }
                        }
                    }
                }
            }

            // 単線区間の運行方向の矢印
            foreach(var a in arrowSettings) {
                if (directionDataDict.ContainsKey(a.Lever1Name) && directionDataDict[a.Lever1Name] == a.Type && directionDataDict.ContainsKey(a.Lever2Name) && directionDataDict[a.Lever2Name] == a.Type) {
                    AddImage(g, images[a.FileName], a.PosX, a.PosY);

                }
            }

            lock(originalBitmap)
            lock (pictureBox) {
                var oldPic = pictureBox.Image;
                var oldOriginal = originalBitmap;


                if (window.TIDScale < 0) {
                    var aspectRatio = (double)newPic.Width / newPic.Height;
                    if (aspectRatio < (double)pictureBox.Width / pictureBox.Height) {
                        pictureBox.Image = new Bitmap(newPic, (int)(pictureBox.Height * aspectRatio), pictureBox.Height);
                    }
                    else {
                        pictureBox.Image = new Bitmap(newPic, pictureBox.Width, (int)(pictureBox.Width / aspectRatio));
                    }
                }
                else {
                    pictureBox.Image = new Bitmap(newPic, newPic.Width * window.TIDScale / 100, newPic.Height * window.TIDScale / 100);
                }

                originalBitmap = newPic;
                lock (subWindows) {
                    foreach (var sw in subWindows) {
                        sw.UpdateImage(originalBitmap);
                    }
                }
                oldPic?.Dispose();
                oldOriginal.Dispose();
            }

            window.SetMagnifyingGlass();

            Started = true;
        }



        /// <summary>
        /// 座標を指定して画像を貼り付ける
        /// </summary>
        /// <param name="g">TID画像のGraphics</param>
        /// <param name="image">貼り付ける画像</param>
        /// <param name="x">貼り付けるx座標</param>
        /// <param name="y">貼り付けるy座標</param>
        private void AddImage(Graphics g, Image image, int x, int y) {
            lock (image) {
                g.DrawImage(image, x, y, image.Width, image.Height);
            }
        }

        /// <summary>
        /// 座標と色を指定して画像を貼り付ける
        /// </summary>
        /// <param name="g">TID画像のGraphics</param>
        /// <param name="image">貼り付ける画像</param>
        /// <param name="x">貼り付けるx座標</param>
        /// <param name="y">貼り付けるy座標</param>
        /// <param name="ia">色の置き換えを指定したImageAttributes</param>
        private void AddImage(Graphics g, Image image, int x, int y, ImageAttributes ia) {
            lock (image) {
                g.DrawImage(image, new Rectangle(x, y, image.Width, image.Height), 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, ia);
            }
        }

        /// <summary>
        /// 座標と色を指定して列車番号フォント画像を貼り付ける（全角可）
        /// </summary>
        /// <param name="g">TID画像のGraphics</param>
        /// <param name="fontSize">フォントの横幅</param>
        /// <param name="numX">画像中に文字がある列</param>
        /// <param name="numY">画像中に文字がある行</param>
        /// <param name="x">貼り付けるx座標</param>
        /// <param name="y">貼り付けるy座標</param>
        /// <param name="ia">色の置き換えを指定したImageAttributes</param>
        private void AddNumImage(Graphics g, int fontSize, int numX, int numY, int x, int y, ImageAttributes ia) {
            lock (numberImage) {
                g.DrawImage(numberImage, new Rectangle(x, y, fontSize * 6 - 1, 9), 1 + numX * 6, 1 + numY * 10, fontSize * 6 - 1, 9, GraphicsUnit.Pixel, ia);
            }
        }

        /// <summary>
        /// 座標と色を指定して列車番号フォント画像を貼り付ける
        /// </summary>
        /// <param name="g">TID画像のGraphics</param>
        /// <param name="numX">画像中に文字がある列</param>
        /// <param name="numY">画像中に文字がある行</param>
        /// <param name="x">貼り付けるx座標</param>
        /// <param name="y">貼り付けるy座標</param>
        /// <param name="ia">色の置き換えを指定したImageAttributes</param>
        private void AddNumImage(Graphics g, int numX, int numY, int x, int y, ImageAttributes ia) {
            AddNumImage(g, 1, numX, numY, x, y, ia);
        }

        /// <summary>
        /// 座標と色を指定して列車番号フォント画像を貼り付ける（数字のみ）
        /// </summary>
        /// <param name="g">TID画像のGraphics</param>
        /// <param name="num">数字</param>
        /// <param name="x">貼り付けるx座標</param>
        /// <param name="y">貼り付けるy座標</param>
        /// <param name="ia">色の置き換えを指定したImageAttributes</param>
        private void AddNumImage(Graphics g, int num, int x, int y, ImageAttributes ia) {
            AddNumImage(g, num, 0, x, y, ia);
        }

        /// <summary>
        /// 座標を指定して列車番号フォント画像を貼り付ける（全角可）
        /// </summary>
        /// <param name="g">TID画像のGraphics</param>
        /// <param name="isFullWidth">全角であるか</param>
        /// <param name="numX">画像中に文字がある列</param>
        /// <param name="numY">画像中に文字がある行</param>
        /// <param name="x">貼り付けるx座標</param>
        /// <param name="y">貼り付けるy座標</param>
        private void AddNumImage(Graphics g, bool isFullWidth, int numX, int numY, int x, int y) {
            lock (numberImage) {
                g.DrawImage(numberImage, new Rectangle(x, y, isFullWidth ? 11 : 5, 9), 1 + numX * 6, 1 + numY * 10, isFullWidth ? 11 : 5, 9, GraphicsUnit.Pixel);
            }
        }

        /// <summary>
        /// 座標を指定して列車番号フォント画像を貼り付ける
        /// </summary>
        /// <param name="g">TID画像のGraphics</param>
        /// <param name="numX">画像中に文字がある列</param>
        /// <param name="numY">画像中に文字がある行</param>
        /// <param name="x">貼り付けるx座標</param>
        /// <param name="y">貼り付けるy座標</param>
        private void AddNumImage(Graphics g, int numX, int numY, int x, int y) {
            AddNumImage(g, false, numX, numY, x, y);
        }

        /// <summary>
        /// 座標を指定して列車番号フォント画像を貼り付ける（全角可）
        /// </summary>
        /// <param name="g">TID画像のGraphics</param>
        /// <param name="num">数字</param>
        /// <param name="x">貼り付けるx座標</param>
        /// <param name="y">貼り付けるy座標</param>
        private void AddNumImage(Graphics g, int num, int x, int y) {
            AddNumImage(g, num, 0, x, y);
        }

        public void ChangeScale() {

            try {
                PrepareChangeScale();

                lock (originalBitmap)
                lock (pictureBox) {

                    var oldPic = pictureBox.Image;
                    if (oldPic != null) {
                        if (window.TIDScale < 0) {
                            var aspectRatio = (double)originalBitmap.Width / originalBitmap.Height;
                            if (aspectRatio < (double)pictureBox.Width / pictureBox.Height) {
                                var width = (int)(pictureBox.Height * aspectRatio);
                                pictureBox.Image = new Bitmap(originalBitmap, width, pictureBox.Height);
                                pictureBox.Width = width;
                            }
                            else {
                                var height = (int)(pictureBox.Width / aspectRatio);
                                pictureBox.Image = new Bitmap(originalBitmap, pictureBox.Width, height);
                                pictureBox.Height = height;
                            }
                        }
                        else {
                            pictureBox.Image = new Bitmap(originalBitmap, originalBitmap.Width * window.TIDScale / 100, originalBitmap.Height * window.TIDScale / 100);
                        }
                        oldPic.Dispose();
                    }
                }
            }
            catch(Exception e) {
                LogManager.AddExceptionLog(e);
                LogManager.OutputLog(true);
                Debug.WriteLine($"Server send failed: {e.Message}\n{e.StackTrace}");
                if (!ServerCommunication.Error) {
                    ServerCommunication.Error = true;
                    window.Invoke(new Action(() => { window.LabelStatusText = "データ受信失敗"; }));
                    TaskDialog.ShowDialog(new TaskDialogPage {
                        Caption = "描画エラー | TID - ダイヤ運転会",
                        Heading = "描画エラー",
                        Icon = TaskDialogIcon.Error,
                        Text = "TID画面の描画に失敗しました。\nTID製作者に状況を報告願います。"
                    });
                }
            }

        }

        private void PrepareChangeScale() {
            if (window.WindowState == FormWindowState.Minimized) {
                return;
            }
            var dr = window.DetectResize;
            window.DetectResize = false;
            int width, height;
            lock (originalBitmap) {
                width = originalBitmap.Width * window.TIDScale / 100;
                height = originalBitmap.Height * window.TIDScale / 100;

                if (window.TIDScale < 0) {
                    width = originalBitmap.Width * 2;
                    height = originalBitmap.Height * 2;
                }

                window.MaximumSize = new Size(Math.Max(width, originalBitmap.Width) + window.Size.Width - window.ClientSize.Width, Math.Max(height, originalBitmap.Height) + window.Panel1.Location.Y + window.Size.Height - window.ClientSize.Height);

                if (-window.Location.X > window.Size.Width - 60) {
                    window.Location = new Point(0, 80);
                }
            }

            lock (pictureBox) {
                if (window.TIDScale < 0) {
                    pictureBox.Width = window.Size.Width - 16;
                    pictureBox.Height = window.Size.Height - 39 - window.Panel1.Location.Y;
                }
                else {
                    pictureBox.Width = width;
                    pictureBox.Height = height;
                }
            }
            window.DetectResize = dr;

        }

        public void CopyImage() {
            lock (originalBitmap) {
                var i = new Bitmap(originalBitmap);
                using (var g = Graphics.FromImage(i)) {
                    g.DrawString((window.Clock + window.TimeOffset).ToString("H:mm:ss"), new Font("ＭＳ ゴシック", 9), Brushes.White, originalBitmap.Width - 51, 0);
                }
                Clipboard.SetImage(i);
                i.Dispose(); 
                
            }
        }

        public void CopyImage(int x, int y, int width, int height) {
            lock (originalBitmap) {
                var i = new Bitmap(width, height + 13);
                using (var g = Graphics.FromImage(i)) {
                    g.Clear(Color.FromArgb(10, 10, 10));
                    g.DrawImage(originalBitmap, new Rectangle(0, 13, width, height), x, y, width, height, GraphicsUnit.Pixel);
                    g.DrawString((window.Clock + window.TimeOffset).ToString("H:mm:ss"), new Font("ＭＳ ゴシック", 9), Brushes.White, width - 51, 0);
                }
                Clipboard.SetImage(i);
                i.Dispose();
            }
        }

        public void CopyImage(Point location, Size size) {
            CopyImage(location.X, location.Y, size.Width, size.Height);
        }

        public bool UpdateNumWindow() {
            var v = false;
            foreach(var n in numberWindowDict.Values) {
                v |= n.UpdateWindow();
            }
            return v;
        }

        public void AddSubWindow(SubWindow subWindow) {
            lock (subWindows) {
                subWindows.Add(subWindow);
            }
        }

        public bool RemoveSubWindow(SubWindow subWindow) {
            lock (subWindows) {
                return subWindows.Remove(subWindow);
            }
        }

        public void SetClockSubWindows(DateTime time) {
            lock (subWindows) {
                foreach (var sw in subWindows) {
                    sw.SetClock(time);
                }
            }
        }

        public void SetMarkupClass(string key, int index) {
            var v = !markupClassesData[key];
            markupClassesData[key] = v;
            ((ToolStripMenuItem)window.MenuItemMarkupClass.DropDownItems[index]).CheckState = v ? CheckState.Checked : CheckState.Unchecked;
            foreach (var w in subWindows) { 
                w.SetMarkupClass(index, v);
            }
            window.ReservedUpdate = true;
        }

    }

}

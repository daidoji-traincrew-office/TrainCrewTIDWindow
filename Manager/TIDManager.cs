using System.Collections.ObjectModel;
using System.Drawing.Imaging;
using System.Text.RegularExpressions;
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
        private readonly List<NumberSetting> numSettingsDown;

        /// <summary>
        /// 各トラックの列車番号の位置などのデータ（上り列車用）
        /// </summary>
        private readonly List<NumberSetting> numSettingsUp;

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
        /// 列車番号の色
        /// </summary>
        private readonly Dictionary<string, Color> numColor = [];

        /// <summary>
        /// 列車番号以外の色
        /// </summary>
        private readonly Dictionary<string, Color> dicColor = [];

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
        public ReadOnlyCollection<LineSetting> LineSettings => lineSettings.AsReadOnly();

        /// <summary>
        /// 各トラックの列車番号の位置などのデータ（下り列車用）
        /// </summary>
        public ReadOnlyCollection<NumberSetting> NumSettingsDown => numSettingsDown.AsReadOnly();

        /// <summary>
        /// 各トラックの列車番号の位置などのデータ（上り列車用）
        /// </summary>
        public ReadOnlyCollection<NumberSetting> NumSettingsUp => numSettingsUp.AsReadOnly();

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

                    if (texts.Length < 4 || texts.Any(t => t == "")) {
                        continue;
                    }

                    if (texts[0].Length < 6) {
                        numColor.Add(texts[0], Color.FromArgb(int.Parse(texts[1]), int.Parse(texts[2]), int.Parse(texts[3])));
                    }
                    else {
                        dicColor.Add(texts[0], Color.FromArgb(int.Parse(texts[1]), int.Parse(texts[2]), int.Parse(texts[3])));
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



            var width = backgroundDefault.Width * window.TIDScale / 100;
            var height = backgroundDefault.Height * window.TIDScale / 100;

            if (window.TIDScale < 0) {
                width = backgroundDefault.Width * 2;
                height = backgroundDefault.Height * 2;
            }

            window.MaximumSize = new Size(Math.Max(width, backgroundDefault.Width) + 16, Math.Max(height, backgroundDefault.Height) + 39 + 24);


            lock (pictureBox) {
                if (window.TIDScale < 0) {
                    pictureBox.Width = window.Size.Width - 16;
                    pictureBox.Height = window.Size.Height - 39 - 24;
                    pictureBox.Cursor = Cursors.Default;
                }
                else {
                    pictureBox.Width = /*width > backgroundDefault.Width ?*/ width /*: backgroundDefault.Width*/;
                    pictureBox.Height = /*height > backgroundDefault.Height ?*/ height /*: backgroundDefault.Height*/;
                }
            }


            pictureBox.Image = new Bitmap(backgroundDefault);
            /*pictureBox.Width = backgroundDefault.Width;
            pictureBox.Height = backgroundDefault.Height;

            window.MaximumSize = new Size(backgroundDefault.Width + 16, backgroundDefault.Height + 39 + 24);*/
            window.Size = new Size(Math.Max(backgroundDefault.Width * window.TIDScale / 100, backgroundDefault.Width) + 16, Math.Max(backgroundDefault.Height * window.TIDScale / 100, backgroundDefault.Height) + 39 + 24);
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
                    AddImage(g, image, numData.PosX, numData.PosY + 10, ia);
                }

                /*foreach (var numData in numSettingsDown) {
                    if (numData == null || numData.NotDraw) {
                        continue;
                    }
                    Image image = numData.Size switch {
                        NumberSize.L => new Bitmap(numLineL),
                        NumberSize.S => new Bitmap(numLineS),
                        _ => new Bitmap(numLineM),
                    };
                    AddImage(g, image, numData.PosX, numData.PosY + 10, ia);
                }

                foreach (var numData in numSettingsUp) {
                    if (numData == null || numData.NotDraw) {
                        continue;
                    }

                    Image image = numData.Size switch {
                        NumberSize.L => new Bitmap(numLineL),
                        NumberSize.S => new Bitmap(numLineS),
                        _ => new Bitmap(numLineM),
                    };
                    AddImage(g, image, numData.PosX, numData.PosY + 10, ia);
                }*/

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
        private List<NumberSetting> LoadNumberSetting(string fileName) {
            return LoadNumberSetting(fileName, []);
        }

        /// <summary>
        /// 各トラックの列車番号の位置などのデータを読み込む
        /// </summary>
        /// <param name="fileName">ファイル名</param>
        /// <param name="dict">列番表示位置の辞書</param>
        /// <returns>読み込んだデータのリスト</returns>
        private List<NumberSetting> LoadNumberSetting(string fileName, Dictionary<Point, NumberWindowData> dict) {
            List<NumberSetting> list = [];

            try {
                using var sr = new StreamReader($".\\setting\\{fileName}");
                sr.ReadLine();
                var line = sr.ReadLine();
                var trackName = "";
                NumberSetting? numSet = null;
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
                            numSet = new NumberSetting(trackName, nwd, texts[4], texts[5] == bool.TrueString);
                        }
                        else {
                            numSet = new NumberSetting(trackName, nwd);
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


                if (!Regex.IsMatch(numHeader, @"^([溝月レイルﾚｲﾙ]+|[回試臨]?)$") || !Regex.IsMatch(numFooter, @"^([ABCKST]?[XYZ]?)$")) {
                    continue;
                }

                rule = "";
                foreach (var numData in isTrain ? (numBody % 2 == 1 ? track.NumSettingsDown : track.NumSettingsUp) : track.NumSettingsDown.Union(track.NumSettingsUp)) {
                    var numWindow = numData.WindowDataList.FirstOrDefault(n => n.Train == train);
                    if (numData == null || numWindow == null) {
                        continue;
                    }

                    // 転轍器の状態で表示条件を判定
                    var r = numData.PointName != "" ? $"{numData.PointName}/{numData.Reversed}" : "";
                    if (r != "" && rule == "" && pointDataDict.ContainsKey(numData.PointName)) {
                        var point = pointDataDict[numData.PointName];
                        if (point.IsLocked && point.IsReversed == numData.Reversed) {
                            rule = r;
                        }
                    }

                    // 表示条件を満たさない場合は表示しない
                    if (rule != r) {
                        continue;
                    }

                    // 運番
                    if (numWindow.Size == NumberSize.S) {
                        if (isTrain) {
                            var umban = numBody / 3000 * 100 + numBody % 100;

                            // 運番を偶数にする・矢印設置
                            if (umban % 2 != 0) {
                                umban -= 1;
                                AddNumImage(g, 8, 0, numWindow.PosX, numWindow.PosY);
                            }
                            else {
                                AddNumImage(g, 9, 0, numWindow.PosX + 24, numWindow.PosY);
                            }

                            // 運番設置
                            for (var i = 2; i >= 0; i--) {
                                if(umban <= 0) {
                                    if(numBodyStr[0] != '0') {
                                        break;
                                    }
                                    AddNumImage(g, 0, numWindow.PosX + 6 + i * 6, numWindow.PosY);
                                }
                                var num = umban % 10;
                                AddNumImage(g, num, numWindow.PosX + 6 + i * 6, numWindow.PosY);
                                umban /= 10;
                            }
                            // 下線設置
                            AddImage(g, numLineS, numWindow.PosX, numWindow.PosY + 10);

                        }
                        else if(numHeader.StartsWith("溝月")) {
                            // みぞつき色
                            ImageAttributes? iaType = null;
                            if (numColor.TryGetValue("溝月", out var newColor)) {
                                iaType = new ImageAttributes();
                                iaType.SetRemapTable([new ColorMap { OldColor = Color.White, NewColor = newColor }]);
                            }
                            // 0埋め列番への警告色として不明色に
                            if (isTrain && numBodyStr[0] == '0') {
                                iaType = new ImageAttributes();
                                if (dicColor.TryGetValue("UNKNOWN", out newColor)) {
                                    iaType.SetRemapTable([new ColorMap { OldColor = Color.White, NewColor = newColor }]);
                                }
                            }
                            // みぞつき色が見つからなければとりあえず不明色に
                            if (iaType == null) {
                                iaType = new ImageAttributes();
                                if (dicColor.TryGetValue("UNKNOWN", out newColor)) {
                                    iaType.SetRemapTable([new ColorMap { OldColor = Color.White, NewColor = newColor }]);
                                }
                            }

                            // 溝月ﾚｲﾙ設置
                            AddNumImage(g, 5, 0, 4, numWindow.PosX, numWindow.PosY, iaType);
                            // 下線設置
                            AddImage(g, numLineS, numWindow.PosX, numWindow.PosY + 10, iaType);

                        }
                    }
                    // 列番
                    else {
                        var retsuban = numBody;

                        // 種別色
                        ImageAttributes? iaType = null;
                        foreach (var k in numColor.Keys) {
                            if ($"{numHeader}{numFooter}".Contains(k)) {
                                iaType = new ImageAttributes();
                                iaType.SetRemapTable([new ColorMap { OldColor = Color.White, NewColor = numColor[k] }]);
                                break;
                            }
                        }
                        // 0埋め列番への警告色として不明色に
                        if (isTrain && numBodyStr[0] == '0') {
                            iaType = new ImageAttributes();
                            if (dicColor.TryGetValue("UNKNOWN", out var newColor)) {
                                iaType.SetRemapTable([new ColorMap { OldColor = Color.White, NewColor = newColor }]);
                            }
                        }
                        // 種別色無しかつ数字なしであれば不明色に
                        if (iaType == null) {
                            iaType = new ImageAttributes();
                            if (!isTrain && dicColor.TryGetValue("UNKNOWN", out var newColor)) {
                                iaType.SetRemapTable([new ColorMap { OldColor = Color.White, NewColor = newColor }]);
                            }
                        }

                        if (isTrain) {
                            // 列番の頭の文字設置
                            switch (numHeader) {
                                case "回":
                                    AddNumImage(g, 2, 0, 0, numWindow.PosX, numWindow.PosY, iaType);
                                    break;
                                case "試":
                                    AddNumImage(g, 2, 2, 0, numWindow.PosX, numWindow.PosY, iaType);
                                    break;
                                case "臨":
                                    AddNumImage(g, 2, 4, 0, numWindow.PosX, numWindow.PosY, iaType);
                                    break;
                            }

                            // 列番本体設置
                            for (var i = 0; i < 4 && i < numBodyStr.Length; i++) {
                                var num = numBodyStr[numBodyStr.Length - 1 - i] - '0';
                                AddNumImage(g, num, numWindow.PosX + 12 + (3 - i) * 6, numWindow.PosY, iaType);
                            }


                            // 列番の末尾の文字設置
                            if (numFooter.Length > 0) {
                                var x = GetAlphaX(numFooter[0]);
                                if (x < 55) {
                                    AddNumImage(g, x, 2, numWindow.PosX + 36, numWindow.PosY, iaType);
                                }
                            }
                            if (numFooter.Length > 1) {
                                var x = GetAlphaX(numFooter[1]);
                                if (x < 55) {
                                    AddNumImage(g, x, 2, numWindow.PosX + 42, numWindow.PosY, iaType);
                                }
                            }


                            // 遅延時分表示（未実装のため必ず0・白色）
                            // ↑が未実装なので一時的に代わりに受信精度確認用カウントダウン（-1になると在線が消える）を表示

                            /*var cm = new ColorMap();
                            cm.OldColor = Color.White;
                            cm.NewColor = Color.FromArgb(255, 0, 0);*/
                            var iaDelay = new ImageAttributes();
                            /*iaDelay.SetRemapTable([cm]);*/
                            if (numWindow.Size == NumberSize.L) {
                                AddNumImage(g, track.DeeCount - 1, numWindow.PosX + 54, numWindow.PosY, iaDelay);
                            }

                            if (numWindow.Size == NumberSize.L) {
                                AddImage(g, numLineL, numWindow.PosX, numWindow.PosY + 10, iaDelay);
                            }
                            else {
                                AddImage(g, numLineM, numWindow.PosX, numWindow.PosY + 10, iaDelay);
                            }

                        }
                        else if (numHeader.StartsWith("溝月")) {

                            // 溝月ﾚｲﾙ設置
                            if(numWindow.Size == NumberSize.L) {
                                AddNumImage(g, 7, 0, 3, numWindow.PosX, numWindow.PosY, iaType);
                            }
                            else {
                                AddNumImage(g, 5, 0, 4, numWindow.PosX, numWindow.PosY, iaType);
                            }
                            // 下線設置
                            if(numWindow.Size == NumberSize.L) {
                                AddImage(g, numLineL, numWindow.PosX, numWindow.PosY + 10, iaType);
                            }
                            else {
                                AddImage(g, numLineM, numWindow.PosX, numWindow.PosY + 10, iaType);
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
                oldPic?.Dispose();
                oldOriginal.Dispose();
            }

            window.SetMagnifyingGlass();


        }

        /// <summary>
        /// 列番画像内のアルファベットの列座標を取得する
        /// </summary>
        /// <param name="alpha">アルファベット</param>
        /// <returns>列の座標</returns>
        public int GetAlphaX(char alpha) {
            switch (alpha) {
                case 'A':
                    return 0;
                case 'B':
                    return 1;
                case 'C':
                    return 2;
                case 'K':
                    return 3;
                case 'S':
                    return 4;
                case 'T':
                    return 5;
                case 'X':
                    return 6;
                case 'Y':
                    return 7;
                case 'Z':
                    return 8;
                default:
                    return 9;
            }
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
            AddNumImage(g, num, 1, x, y, ia);
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
            AddNumImage(g, num, 1, x, y);
        }

        public void ChangeScale() {

            PrepareChangeScale();

            lock (pictureBox) {

                var oldPic = pictureBox.Image;
                if(oldPic != null) {
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

        private void PrepareChangeScale() {
            var width = originalBitmap.Width * window.TIDScale / 100;
            var height = originalBitmap.Height * window.TIDScale / 100;

            if(window.TIDScale < 0) {
                width = originalBitmap.Width * 2;
                height = originalBitmap.Height * 2;
            }

            window.MaximumSize = new Size(Math.Max(width, originalBitmap.Width) + 16, Math.Max(height, originalBitmap.Height) + 39 + 24);

            if(-window.Location.X > window.Size.Width - 60) {
                window.Location = new Point(0, 80);
            }

            lock (pictureBox) {
                if (window.TIDScale < 0) {
                    pictureBox.Width = window.Size.Width - 16;
                    pictureBox.Height = window.Size.Height - 39 - 24;
                }
                else {
                    pictureBox.Width = /*width > originalBitmap.Width ?*/ width /*: originalBitmap.Width*/;
                    pictureBox.Height = /*height > originalBitmap.Height ?*/ height /*: originalBitmap.Height*/;
                }
            }

        }

        public void CopyImage() {
            var i = new Bitmap(originalBitmap);
            using var g = Graphics.FromImage(i);
            g.DrawString((window.Clock + window.TimeOffset).ToString("H:mm:ss"), new Font("ＭＳ ゴシック", 9), Brushes.White, 1869, 0);
            Clipboard.SetImage(i);
        }

        public bool UpdateNumWindow() {
            var v = false;
            foreach(var n in numberWindowDict.Values) {
                v |= n.UpdateWindow();
            }
            return v;
        }

    }

}

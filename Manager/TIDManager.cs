using System.Collections.ObjectModel;
using System.Drawing.Imaging;
using System.Text.RegularExpressions;
using TrainCrewTIDWindow.Settings;

namespace TrainCrewTIDWindow.Manager
{
    /// <summary>
    /// TID画面管理用
    /// </summary>
    public class TIDManager
    {
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
        private readonly List<NumberSetting> numSettingsOut;

        /// <summary>
        /// 各トラックの列車番号の位置などのデータ（上り列車用）
        /// </summary>
        private readonly List<NumberSetting> numSettingsIn;

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
        /// TID画面表示用のPictureBox
        /// </summary>
        public PictureBox PictureBox => pictureBox;


        /// <summary>
        /// 各トラックの線の位置やファイル名などのデータ
        /// </summary>
        public ReadOnlyCollection<LineSetting> LineSettings => lineSettings.AsReadOnly();

        /// <summary>
        /// 各トラックの列車番号の位置などのデータ（下り列車用）
        /// </summary>
        public ReadOnlyCollection<NumberSetting> NumSettingsOut => numSettingsOut.AsReadOnly();

        /// <summary>
        /// 各トラックの列車番号の位置などのデータ（上り列車用）
        /// </summary>
        public ReadOnlyCollection<NumberSetting> NumSettingsIn => numSettingsIn.AsReadOnly();

        /// <summary>
        /// TID画面管理用
        /// </summary>
        /// <param name="pictureBox">TID画面表示用のPictureBox</param>
        /// <param name="window">TIDWindowオブジェクト</param>
        public TIDManager(PictureBox pictureBox, TIDWindow window)
        {
            this.pictureBox = pictureBox;
            this.window = window;

            backgroundDefault = Image.FromFile(".\\png\\Background-1.png");
            backgroundImage = Image.FromFile(".\\png\\Background.png");
            numLineL = Image.FromFile(".\\png\\TID_Retsuban_W_L.png");
            numLineM = Image.FromFile(".\\png\\TID_Retsuban_W_M.png");
            numLineS = Image.FromFile(".\\png\\TID_Retsuban_W_S.png");
            numberImage = Image.FromFile(".\\png\\Number.png");

            lineSettings = LoadLineSetting("linedata.tsv");
            numSettingsOut = LoadNumberSetting("number_outbound.tsv");
            numSettingsIn = LoadNumberSetting("number_inbound.tsv");


            try
            {
                using var sr = new StreamReader(".\\setting\\color_setting.tsv");
                sr.ReadLine();
                var line = sr.ReadLine();
                while (line != null)
                {
                    var texts = line.Split('\t');
                    line = sr.ReadLine();

                    if (texts.Length < 4 || texts.Any(t => t == ""))
                    {
                        continue;
                    }

                    if (texts[0].Length < 6)
                    {
                        numColor.Add(texts[0], Color.FromArgb(int.Parse(texts[1]), int.Parse(texts[2]), int.Parse(texts[3])));
                    }
                    else
                    {
                        dicColor.Add(texts[0], Color.FromArgb(int.Parse(texts[1]), int.Parse(texts[2]), int.Parse(texts[3])));
                    }
                }
            }
            catch
            {
            }

            try
            {
                using var sr = new StreamReader(".\\setting\\crossing.tsv");
                sr.ReadLine();
                var line = sr.ReadLine();
                var name = "";
                while (line != null)
                {
                    var texts = line.Split('\t');
                    line = sr.ReadLine();
                    if (texts.Length < 4 || texts.Any(t => t == ""))
                    {
                        continue;
                    }
                    if (texts[0] != "")
                    {
                        name = texts[0];
                    }
                    if (name == "")
                    {
                        continue;
                    }

                    var imageName = texts[1];
                    crossingSettings.Add(new CrossingSetting(texts[0], imageName, int.Parse(texts[2]), int.Parse(texts[3])));

                    if (!images.ContainsKey($"{imageName}_R"))
                    {
                        images[$"{imageName}_R"] = Image.FromFile($".\\png\\{imageName}_R.png");
                        images[$"{imageName}_G"] = Image.FromFile($".\\png\\{imageName}_G.png");
                    }
                }
            }
            catch
            {
            }


            try
            {
                using var sr = new StreamReader(".\\setting\\arrow.tsv");
                sr.ReadLine();
                var line = sr.ReadLine();
                var name = "";
                while (line != null)
                {
                    var texts = line.Split('\t');
                    line = sr.ReadLine();
                    if (texts.Length < 5 || texts.Any(t => t == ""))
                    {
                        continue;
                    }
                    if (texts[0] != "")
                    {
                        name = texts[0];
                    }
                    if (name == "")
                    {
                        continue;
                    }

                    var imageName = texts[2];
                    arrowSettings.Add(new ArrowSetting(texts[0], texts[1] == "R" ? ArrowType.R : ArrowType.L, imageName, int.Parse(texts[3]), int.Parse(texts[4])));

                    if (!images.ContainsKey(imageName))
                    {
                        images[imageName] = Image.FromFile($".\\png\\{imageName}.png");
                    }
                }
            }
            catch
            {
            }



            pictureBox.Image = new Bitmap(backgroundDefault);
            pictureBox.Width = backgroundDefault.Width;
            pictureBox.Height = backgroundDefault.Height;


            window.MaximumSize = new Size(backgroundDefault.Width + 16, backgroundDefault.Height + 39 + 24);
            window.Size = window.MaximumSize;

            // 試験表示
            {
                using var g = Graphics.FromImage(pictureBox.Image);
                foreach (var lineData in lineSettings)
                {
                    if (lineData != null && lineData.IsDefault)
                    {
                        AddImage(g, images[lineData.FileNameR], lineData.PosX, lineData.PosY);
                    }
                }

                foreach (var numData in numSettingsOut)
                {
                    if (numData == null || numData.NotDraw)
                    {
                        continue;
                    }
                    Image image = numData.Size switch
                    {
                        NumberSize.L => new Bitmap(numLineL),
                        NumberSize.S => new Bitmap(numLineS),
                        _ => new Bitmap(numLineM),
                    };
                    var cm = new ColorMap
                    {
                        OldColor = Color.White,
                        NewColor = Color.Red
                    };
                    var ia = new ImageAttributes();
                    ia.SetRemapTable([cm]);
                    AddImage(g, image, numData.PosX, numData.PosY + 10, ia);
                }

                foreach (var numData in numSettingsIn)
                {
                    if (numData == null || numData.NotDraw)
                    {
                        continue;
                    }
                    Image image;
                    switch (numData.Size)
                    {
                        case NumberSize.L:
                            image = new Bitmap(numLineL);
                            break;
                        case NumberSize.S:
                            image = new Bitmap(numLineS);
                            break;
                        default:
                            image = new Bitmap(numLineM);
                            break;
                    }
                    var cm = new ColorMap
                    {
                        OldColor = Color.White,
                        NewColor = Color.Red
                    };
                    var ia = new ImageAttributes();
                    ia.SetRemapTable([cm]);
                    AddImage(g, image, numData.PosX, numData.PosY + 10, ia);
                }

                foreach (var crossing in crossingSettings)
                {
                    if (crossing == null)
                    {
                        continue;
                    }
                    AddImage(g, images[crossing.FileNameR], crossing.PosX, crossing.PosY);
                }
                foreach (var arrow in arrowSettings)
                {
                    if (arrow == null)
                    {
                        continue;
                    }
                    AddImage(g, images[arrow.FileName], arrow.PosX, arrow.PosY);
                }
            }




        }



        /// <summary>
        /// 各トラックの線の位置やファイル名などのデータを読み込む
        /// </summary>
        /// <param name="fileName">ファイル名</param>
        /// <returns>読み込んだデータのリスト</returns>
        private List<LineSetting> LoadLineSetting(string fileName)
        {
            List<LineSetting> list = [];
            try
            {
                using var sr = new StreamReader($".\\setting\\{fileName}");
                sr.ReadLine();
                var line = sr.ReadLine();
                var trackName = "";
                while (line != null)
                {
                    var texts = line.Split('\t');
                    line = sr.ReadLine();
                    var i = 1;
                    for (; i < texts.Length; i++)
                    {
                        if (texts[i] == "")
                        {
                            break;
                        }
                    }
                    if (i < 4)
                    {
                        continue;
                    }
                    if (texts[0] != "")
                    {
                        trackName = texts[0];
                    }
                    if (trackName == "")
                    {
                        continue;
                    }
                    var imageName = texts[1];

                    if (i > 5)
                    {
                        list.Add(new LineSetting(trackName, imageName, int.Parse(texts[2]), int.Parse(texts[3]), texts[4], texts[5] == bool.TrueString));
                    }
                    else
                    {
                        list.Add(new LineSetting(trackName, imageName, int.Parse(texts[2]), int.Parse(texts[3])));
                    }
                    if (!images.ContainsKey($"{imageName}_R"))
                    {
                        images[$"{imageName}_R"] = Image.FromFile($".\\png\\{imageName}_R.png");
                        images[$"{imageName}_Y"] = Image.FromFile($".\\png\\{imageName}_Y.png");
                    }
                }
            }
            catch
            {
            }
            return list;
        }

        /// <summary>
        /// 各トラックの列車番号の位置などのデータを読み込む
        /// </summary>
        /// <param name="fileName">ファイル名</param>
        /// <returns>読み込んだデータのリスト</returns>
        private List<NumberSetting> LoadNumberSetting(string fileName)
        {
            List<NumberSetting> list = [];

            try
            {
                using var sr = new StreamReader($".\\setting\\{fileName}");
                sr.ReadLine();
                var line = sr.ReadLine();
                var trackName = "";
                while (line != null)
                {
                    var texts = line.Split('\t');
                    line = sr.ReadLine();
                    var i = 1;
                    for (; i < texts.Length; i++)
                    {
                        if (texts[i] == "")
                        {
                            break;
                        }
                    }
                    if (i < 4)
                    {
                        continue;
                    }
                    if (texts[0] != "")
                    {
                        trackName = texts[0];
                    }
                    if (trackName == "")
                    {
                        continue;
                    }
                    NumberSize size;
                    switch (texts[1])
                    {
                        case "S":
                            size = NumberSize.S;
                            break;
                        case "M":
                            size = NumberSize.M;
                            break;
                        default:
                            size = NumberSize.L;
                            break;
                    }


                    if (i > 5)
                    {
                        list.Add(new NumberSetting(trackName, size, int.Parse(texts[2]), int.Parse(texts[3]), texts[4], texts[5] == bool.TrueString));
                    }
                    else
                    {
                        list.Add(new NumberSetting(trackName, size, int.Parse(texts[2]), int.Parse(texts[3])));
                    }
                }
            }
            catch
            {
            }
            return list;

        }



        /// <summary>
        /// 必要であればTIDの在線表示を更新する
        /// データが更新された際はとりあえずこれを呼ぶ
        /// </summary>
        public void UpdateTID()
        {
            var trackDataDict = window.TrackDataDict;
            var pointDataDict = window.PointDataDict;


            if (pictureBox.Image != null)
            {
                pictureBox.Image.Dispose();
            }
            pictureBox.Image = new Bitmap(backgroundImage);
            using var g = Graphics.FromImage(pictureBox.Image);

            foreach (var track in trackDataDict.Values)
            {
                if (!track.OnTrain && !track.IsReserved)
                {
                    continue;
                }

                // トラックの在線、進路開通状態表示

                var rule = "";
                foreach (var line in track.LineSettings)
                {
                    if (line == null)
                    {
                        continue;
                    }

                    // 転轍器の状態で表示条件を判定
                    var r = line.PointName != "" ? $"{line.PointName}/{line.Reversed}" : "";
                    if (r != "" && rule == "" && pointDataDict.ContainsKey(line.PointName))
                    {
                        var point = pointDataDict[line.PointName];
                        if (point.IsLocked && point.IsReversed == line.Reversed)
                        {
                            rule = r;
                        }
                    }

                    // 表示条件を満たさない場合は表示しない
                    if (rule != r)
                    {
                        continue;
                    }
                    AddImage(g, images[track.OnTrain ? line.FileNameR : line.FileNameY], line.PosX, line.PosY);
                }
                if (!track.OnTrain)
                {
                    continue;
                }

                // 列番表示

                var numHeader = Regex.Replace(track.Train, @"[0-9a-zA-Z]", "");  // 列番の頭の文字（回、試など）
                _ = int.TryParse(Regex.Replace(track.Train, @"[^0-9]", ""), out var numBody);  // 列番本体（数字部分）
                var numFooter = Regex.Replace(track.Train, @"[^a-zA-Z]", "");  // 列番の末尾の文字

                var numSettingList = (numBody % 2 == 1 ? numSettingsOut : numSettingsIn).Where(d => d.TrackName == track.Name && !d.NotDraw && !d.ExistPoint);

                rule = "";
                foreach (var numData in numBody % 2 == 1 ? track.NumSettingsOut : track.NumSettingsIn)
                {
                    if (numData == null)
                    {
                        continue;
                    }

                    // 転轍器の状態で表示条件を判定
                    var r = numData.PointName != "" ? $"{numData.PointName}/{numData.Reversed}" : "";
                    if (r != "" && rule == "" && pointDataDict.ContainsKey(numData.PointName))
                    {
                        var point = pointDataDict[numData.PointName];
                        if (point.IsLocked && point.IsReversed == numData.Reversed)
                        {
                            rule = r;
                        }
                    }

                    // 表示条件を満たさない場合は表示しない
                    if (rule != r)
                    {
                        continue;
                    }

                    // 運番
                    if (numData.Size == NumberSize.S)
                    {
                        var umban = numBody / 3000 * 100 + numBody % 100;

                        // 運番を偶数にする・矢印設置
                        if (umban % 2 != 0)
                        {
                            umban -= 1;
                            AddNumImage(g, 8, 0, numData.PosX, numData.PosY);
                        }
                        else
                        {
                            AddNumImage(g, 9, 0, numData.PosX + 24, numData.PosY);
                        }

                        // 運番設置
                        for (var i = 2; i >= 0 && umban > 0; i--)
                        {
                            var num = umban % 10;
                            AddNumImage(g, num, numData.PosX + 6 + i * 6, numData.PosY);
                            umban /= 10;
                        }
                        // 下線設置
                        AddImage(g, numLineS, numData.PosX, numData.PosY + 10);
                    }
                    // 列番
                    else
                    {
                        var retsuban = numBody;

                        // 種別色
                        ImageAttributes? iaType = null;
                        foreach (var k in numColor.Keys)
                        {
                            if ($"{numHeader}{numFooter}".Contains(k))
                            {
                                iaType = new ImageAttributes();
                                iaType.SetRemapTable([new ColorMap { OldColor = Color.White, NewColor = numColor[k] }]);
                                break;
                            }
                        }
                        // 種別色無しかつ数字なしであれば不明色に
                        if (iaType == null)
                        {
                            iaType = new ImageAttributes();
                            if (retsuban <= 0 && dicColor.TryGetValue("UNKNOWN", out var newColor))
                            {
                                iaType.SetRemapTable([new ColorMap { OldColor = Color.White, NewColor = newColor }]);
                            }
                        }

                        // 列番の頭の文字設置
                        switch (numHeader)
                        {
                            case "回":
                                AddNumImage(g, true, 0, 0, numData.PosX, numData.PosY, iaType);
                                break;
                            case "試":
                                AddNumImage(g, true, 2, 0, numData.PosX, numData.PosY, iaType);
                                break;
                            case "臨":
                                AddNumImage(g, true, 4, 0, numData.PosX, numData.PosY, iaType);
                                break;
                        }

                        // 列番本体設置
                        for (var i = 3; i >= 0 && retsuban > 0; i--)
                        {
                            var num = retsuban % 10;
                            AddNumImage(g, num, numData.PosX + 12 + i * 6, numData.PosY, iaType);
                            retsuban /= 10;
                        }


                        // 列番の末尾の文字設置
                        if (numFooter.Length > 0)
                        {
                            var x = GetAlphaX(numFooter[0]);
                            if (x < 55)
                            {
                                AddNumImage(g, x, 2, numData.PosX + 36, numData.PosY, iaType);
                            }
                        }
                        if (numFooter.Length > 1)
                        {
                            var x = GetAlphaX(numFooter[1]);
                            if (x < 55)
                            {
                                AddNumImage(g, x, 2, numData.PosX + 42, numData.PosY, iaType);
                            }
                        }


                        // 遅延時分表示（未実装のため必ず0・白色）

                        /*var cm = new ColorMap();
                        cm.OldColor = Color.White;
                        cm.NewColor = Color.FromArgb(255, 0, 0);*/
                        var iaDelay = new ImageAttributes();
                        /*iaDelay.SetRemapTable([cm]);*/
                        if (numData.Size == NumberSize.L)
                        {
                            AddNumImage(g, 0, numData.PosX + 54, numData.PosY, iaDelay);
                        }
                        Image numLineImage = numData.Size == NumberSize.L ? new Bitmap(numLineL) : new Bitmap(numLineM);
                        AddImage(g, numLineImage, numData.PosX, numData.PosY + 10, iaDelay);
                    }
                }
            }
        }

        /// <summary>
        /// 列番画像内のアルファベットの列座標を取得する
        /// </summary>
        /// <param name="alpha">アルファベット</param>
        /// <returns>列の座標</returns>
        public int GetAlphaX(char alpha)
        {
            switch (alpha)
            {
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
        private void AddImage(Graphics g, Image image, int x, int y)
        {
            g.DrawImage(image, x, y, image.Width, image.Height);
        }

        /// <summary>
        /// 座標と色を指定して画像を貼り付ける
        /// </summary>
        /// <param name="g">TID画像のGraphics</param>
        /// <param name="image">貼り付ける画像</param>
        /// <param name="x">貼り付けるx座標</param>
        /// <param name="y">貼り付けるy座標</param>
        /// <param name="ia">色の置き換えを指定したImageAttributes</param>
        private void AddImage(Graphics g, Image image, int x, int y, ImageAttributes ia)
        {
            g.DrawImage(image, new Rectangle(x, y, image.Width, image.Height), 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, ia);
        }

        /// <summary>
        /// 座標と色を指定して列車番号フォント画像を貼り付ける（全角可）
        /// </summary>
        /// <param name="g">TID画像のGraphics</param>
        /// <param name="isFullWidth">全角であるか</param>
        /// <param name="numX">画像中に文字がある列</param>
        /// <param name="numY">画像中に文字がある行</param>
        /// <param name="x">貼り付けるx座標</param>
        /// <param name="y">貼り付けるy座標</param>
        /// <param name="ia">色の置き換えを指定したImageAttributes</param>
        private void AddNumImage(Graphics g, bool isFullWidth, int numX, int numY, int x, int y, ImageAttributes ia)
        {
            g.DrawImage(numberImage, new Rectangle(x, y, isFullWidth ? 11 : 5, 9), 1 + numX * 6, 1 + numY * 10, isFullWidth ? 11 : 5, 9, GraphicsUnit.Pixel, ia);
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
        private void AddNumImage(Graphics g, int numX, int numY, int x, int y, ImageAttributes ia)
        {
            AddNumImage(g, false, numX, numY, x, y, ia);
        }

        /// <summary>
        /// 座標と色を指定して列車番号フォント画像を貼り付ける（数字のみ）
        /// </summary>
        /// <param name="g">TID画像のGraphics</param>
        /// <param name="num">数字</param>
        /// <param name="x">貼り付けるx座標</param>
        /// <param name="y">貼り付けるy座標</param>
        /// <param name="ia">色の置き換えを指定したImageAttributes</param>
        private void AddNumImage(Graphics g, int num, int x, int y, ImageAttributes ia)
        {
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
        private void AddNumImage(Graphics g, bool isFullWidth, int numX, int numY, int x, int y)
        {
            g.DrawImage(numberImage, new Rectangle(x, y, isFullWidth ? 11 : 5, 9), 1 + numX * 6, 1 + numY * 10, isFullWidth ? 11 : 5, 9, GraphicsUnit.Pixel);
        }

        /// <summary>
        /// 座標を指定して列車番号フォント画像を貼り付ける
        /// </summary>
        /// <param name="g">TID画像のGraphics</param>
        /// <param name="numX">画像中に文字がある列</param>
        /// <param name="numY">画像中に文字がある行</param>
        /// <param name="x">貼り付けるx座標</param>
        /// <param name="y">貼り付けるy座標</param>
        private void AddNumImage(Graphics g, int numX, int numY, int x, int y)
        {
            AddNumImage(g, false, numX, numY, x, y);
        }

        /// <summary>
        /// 座標を指定して列車番号フォント画像を貼り付ける（全角可）
        /// </summary>
        /// <param name="g">TID画像のGraphics</param>
        /// <param name="num">数字</param>
        /// <param name="x">貼り付けるx座標</param>
        /// <param name="y">貼り付けるy座標</param>
        private void AddNumImage(Graphics g, int num, int x, int y)
        {
            AddNumImage(g, num, 1, x, y);
        }

    }

}

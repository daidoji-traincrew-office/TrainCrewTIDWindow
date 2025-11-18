using TrainCrewTIDWindow.Manager;
using TrainCrewTIDWindow.Models;

namespace TrainCrewTIDWindow.Forms {
    public partial class SubWindow : Form {

        public static string StatusText {
            get;
            private set;
        } = "";

        public static Color StatusColor {
            get;
            private set;
        } = Color.White;

        public Point StartLocation {
            get;
            init;
        }

        public Size DisplaySize {
            get;
            init;
        }

        private bool DetectResize {
            get; set;
        } = false;

        private TIDManager displayManager;

        private Bitmap original = new Bitmap(1, 1);

        private static int counter = 0;

        private Size windowSize;

        /// <summary>
        /// WASDキーなど使用時の移動量
        /// </summary>
        private int scrollDelta = 15;

        /// <summary>
        /// 右クリックメニューの列車ボタン
        /// </summary>
        private readonly Dictionary<string, ToolStripMenuItem> trainMenuDict = [];

        public bool OpeningDialog {
            get;
            private set;
        } = false;

        /// <summary>
        /// マウス位置（ドラッグ操作対応用）
        /// </summary>
        private Point mouseLoc = Point.Empty;

        public SubWindow(Point location, Size size, TIDManager displayManager, ToolStripItemCollection menuTrains) {
            StartLocation = location;
            DisplaySize = size;
            this.displayManager = displayManager;
            InitializeComponent();

            Text = $"サブモニタ{++counter} | TID - ダイヤ運転会";

            Size = new Size(Size.Width - ClientSize.Width + DisplaySize.Width, Size.Height - ClientSize.Height + pictureBox1.Location.Y + DisplaySize.Height);
            MinimumSize = new Size(Size.Width - ClientSize.Width + DisplaySize.Width / 2, Size.Height - ClientSize.Height + pictureBox1.Location.Y + DisplaySize.Height / 2);

            windowSize = Size;

            pictureBox1.Size = size;


            SetMarkupType(displayManager.Window.MarkupType);
            SetMarkupDelayed(displayManager.Window.MarkupDelayed);
            SetMarkupDuplication(displayManager.Window.MarkupDuplication);
            SetMarkupFillZero(displayManager.Window.MarkupFillZero);
            SetMarkupNotTrain(displayManager.Window.MarkupNotTrain);
            SetMarkupSpawned(displayManager.Window.MarkupSpawned);
            SetMarkupHandover(displayManager.Window.MarkupHandover);

            for (var i = 6; i < menuTrains.Count; i++) {
                var trainNumber = menuTrains[i].Name;
                if (trainNumber == null) {
                    continue;
                }
                var menu = new ToolStripMenuItem();
                trainMenuDict.Add(trainNumber, menu);
                menuItemTrainMarkup.DropDownItems.Add(menu);
                menu.Name = trainNumber;
                menu.Size = new Size(110, 22);
                menu.Text = trainNumber;
                menu.Click += (sender, e) => {
                    displayManager.Window.SetTrainMarkup(trainNumber);
                };
                menu.CheckState = ((ToolStripMenuItem)menuTrains[i]).CheckState;
            }

            foreach (ToolStripItem item in displayManager.Window.MenuItemMarkupClass.DropDownItems) {
                if (item is ToolStripSeparator) {
                    var sep = new ToolStripSeparator();
                    menuItemMarkupClass.DropDownItems.Add(sep);
                    sep.Name = "sep";
                    sep.Size = new Size(177, 6);
                    continue;
                }
                var key = item.Name;
                if (key == null) {
                    continue;
                }
                var menu = new ToolStripMenuItem();
                menuItemMarkupClass.DropDownItems.Add(menu);
                menu.Name = item.Name;
                menu.Size = new Size(110, 22);
                menu.Text = item.Text;
                menu.CheckState = ((ToolStripMenuItem)item).CheckState;
                menu.Click += (sender, e) => {
                    displayManager.SetMarkupClass(key, menuItemMarkupClass.DropDownItems.IndexOf(menu));
                };
            }
            trainMenuDict.Add("9999", menuItemMarkup9999);

            /*flowLayoutPanel1.Location = new Point(flowLayoutPanel1.Location.X - Size.Width + ClientSize.Width + 16, flowLayoutPanel1.Location.Y);*/

            DetectResize = true;

            lock (displayManager.OriginalBitmap) {
                UpdateImage(displayManager.OriginalBitmap);
            }
            UpdateStatus();
        }

        public void UpdateImage(Image image) {
            lock (pictureBox1) {
                var old = pictureBox1.Image;
                var b = new Bitmap(DisplaySize.Width, DisplaySize.Height);
                using var g = Graphics.FromImage(b);
                g.DrawImage(image, new Rectangle(0, 0, DisplaySize.Width, DisplaySize.Height), StartLocation.X, StartLocation.Y, DisplaySize.Width, DisplaySize.Height, GraphicsUnit.Pixel);
                lock (original) {
                    var origOld = original;
                    original = b;
                    origOld.Dispose();
                }
                if (WindowState != FormWindowState.Minimized) {
                    pictureBox1.Image = new Bitmap(original, pictureBox1.Width, pictureBox1.Height);
                    old?.Dispose();
                }
            }
        }


        private void SubWindow_Closing(object sender, FormClosingEventArgs e) {
            displayManager.RemoveSubWindow(this);
        }

        private void PictureBox1_MouseWheel(object sender, MouseEventArgs e) {
            if (ModifierKeys.HasFlag(Keys.Control)) {
                lock (pictureBox1.Image) {
                    var size = Size;
                    var dp = e.Location;
                    var point = ConvertPointToOriginal(dp.X, dp.Y);
                    var rate = (pictureBox1.Image.Width + e.Delta * 0.2) / DisplaySize.Width;
                    var width = Size.Width - ClientSize.Width + (int)(DisplaySize.Width * rate);
                    var height = Size.Height - ClientSize.Height + pictureBox1.Location.Y + (int)(DisplaySize.Height * rate);
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
                        height = Size.Height - ClientSize.Height + pictureBox1.Location.Y + DisplaySize.Height * (screenSize.Width - Size.Width + ClientSize.Width) / DisplaySize.Width;
                        Size = new Size(width, height);
                        var np = ConvertPointToScreen(point);
                        if (size != Size) {
                            Location = new Point(Location.X + dp.X - np.X, Location.Y + dp.Y - np.Y);
                        }
                    }
                    else {
                        height = screenSize.Height;
                        width = Size.Width - ClientSize.Width + DisplaySize.Width * (screenSize.Height - Size.Height + ClientSize.Height - pictureBox1.Location.Y) / DisplaySize.Height;
                        Size = new Size(width, height);
                        var np = ConvertPointToScreen(point);
                        if (size != Size) {
                            Location = new Point(Location.X + dp.X - np.X, Location.Y + dp.Y - np.Y);
                        }
                    }
                }
            }
            ((HandledMouseEventArgs)e).Handled = true;
        }

        private void PictureBox1_MouseDown(object sender, MouseEventArgs e) {
            if ((e.Button & MouseButtons.Left) == MouseButtons.Left) {
                if (ModifierKeys.HasFlag(Keys.Shift)) {
                    foreach (var w in displayManager.NumberWindowDict.Values) {
                        var t = w.Train;
                        if (t != null && IsInArea(e.Location, w.PosX, w.PosY, w.GetSize(), 1) && displayManager.Window.TrainDataDict.TryGetValue(t, out var td)) {
                            displayManager.Window.SetTrainMarkup(t);
                            /*td.Markup = !td.Markup;
                            displayManager.Window.UpdateTrainCheck(td);
                            displayManager.Window.ReservedUpdate = true;*/
                        }
                    }
                }
                else {
                    mouseLoc = Cursor.Position;
                }
            }
        }

        private void labelTopMost_Click(object sender, EventArgs e) {
            SetTopMost(!TopMost);
        }

        public void SetTopMost(bool topMost) {
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

        private void labelScale_MouseDown(object sender, MouseEventArgs e) {
            if (pictureBox1.Width / (double)DisplaySize.Width != 1) {
                DetectResize = false;
                Size = new Size(Size.Width - ClientSize.Width + DisplaySize.Width, Size.Height - ClientSize.Height + pictureBox1.Location.Y + DisplaySize.Height);
                if (Location.X < 0) {
                    Location = new Point(0, Location.Y);
                }
                lock (pictureBox1) {
                    var old = pictureBox1.Image;
                    pictureBox1.Image = new Bitmap(original, pictureBox1.Width, pictureBox1.Height);
                    old?.Dispose();
                }


                labelScale.Text = $"Scale：100%";
                labelScale.ForeColor = Color.White;
                labelScale.Cursor = Cursors.Default;
                DetectResize = true;
            }
        }

        private void PictureBox1_MouseMove(object sender, MouseEventArgs e) {
            if (!ModifierKeys.HasFlag(Keys.Shift) && (e.Button & MouseButtons.Left) == MouseButtons.Left) {
                var pos = Cursor.Position;
                Location = new Point(Location.X + pos.X - mouseLoc.X, Location.Y + pos.Y - mouseLoc.Y);
                mouseLoc = pos;
            }
        }

        private void PictureBox1_MouseUp(object sender, MouseEventArgs e) {
            if ((e.Button & MouseButtons.Left) == MouseButtons.Left) {
                mouseLoc = Point.Empty;
            }
        }

        private void SubWindow_ResizeBegin(object sender, EventArgs e) {
            var screenSize = Screen.FromControl(this).Bounds;
            screenSize = new Rectangle(screenSize.Location, new Size(screenSize.Width + 20, screenSize.Height + 20));
            var mw = Size.Width - ClientSize.Width + DisplaySize.Width * (screenSize.Height - Size.Height + ClientSize.Height - pictureBox1.Location.Y) / DisplaySize.Height;
            var mh = Size.Height - ClientSize.Height + pictureBox1.Location.Y + DisplaySize.Height * (screenSize.Width - Size.Width + ClientSize.Width) / DisplaySize.Width;
            if (screenSize.Height < mh) {
                MaximumSize = new Size(mw, screenSize.Height + Size.Height - ClientSize.Height);
            }
            else {
                MaximumSize = new Size(screenSize.Width + Size.Width - ClientSize.Width, mh);
            }

        }

        private void SubWindow_Resize(object sender, EventArgs e) {
            if (!DetectResize || WindowState == FormWindowState.Minimized) {
                return;
            }
            DetectResize = false;
            if (Size.Width == windowSize.Width && Size.Height != windowSize.Height) {
                Size = new Size(Size.Width - ClientSize.Width + (DisplaySize.Width * pictureBox1.Height / DisplaySize.Height), Size.Height);
            }
            else {
                Size = new Size(Size.Width, Size.Height - ClientSize.Height + pictureBox1.Location.Y + (DisplaySize.Height * pictureBox1.Width / DisplaySize.Width));
            }
            lock (pictureBox1) {
                var old = pictureBox1.Image;
                pictureBox1.Image = new Bitmap(original, pictureBox1.Width, pictureBox1.Height);
                old?.Dispose();
                var ratio = pictureBox1.Width * 100 / (double)DisplaySize.Width;
                labelScale.Text = $"Scale：{(int)ratio}%";
                if (ratio == 100) {
                    labelScale.Cursor = Cursors.Default;
                    labelScale.ForeColor = Color.White;
                }
                else {
                    labelScale.Cursor = Cursors.Hand;
                    labelScale.ForeColor = Color.LightGreen;
                }
            }
            DetectResize = true;
        }

        private void SubWindow_ResizeEnd(object sender, EventArgs e) {
            windowSize = Size;
        }

        private void SubWindow_KeyDown(object sender, KeyEventArgs e) {
            var code = e.KeyData & Keys.KeyCode;
            var mod = e.KeyData & Keys.Modifiers;
            if ((mod & Keys.Shift) == Keys.Shift) {
                pictureBox1.Cursor = Cursors.Hand;
            }
            if (e.KeyData == (Keys.C | Keys.Control)) {
                CopyImage();
            }
            if (e.KeyData == Keys.Tab) {
                SetTopMost(!TopMost);
            }

            if (code == Keys.Right || code == Keys.D) {
                Location = new Point(Location.X + scrollDelta * (mod == Keys.Shift ? 1 : 3), Location.Y);
            }
            if (code == Keys.Left || code == Keys.A) {
                Location = new Point(Location.X - scrollDelta * (mod == Keys.Shift ? 1 : 3), Location.Y);
            }
            if (code == Keys.Up || code == Keys.W) {
                Location = new Point(Location.X, Location.Y - scrollDelta * (mod == Keys.Shift ? 1 : 3));
            }
            if (code == Keys.Down || code == Keys.S) {
                Location = new Point(Location.X, Location.Y + scrollDelta * (mod == Keys.Shift ? 1 : 3));
            }
        }

        private void SubWindow_KeyUp(object sender, KeyEventArgs e) {
            UpdateMouseCursor();

        }

        public void SetClock(DateTime time) {
            labelClock.Text = time.ToString("H:mm:ss");
        }

        private Point ConvertPointToOriginal(int x, int y) {
            return new Point(StartLocation.X + x * DisplaySize.Width / pictureBox1.Width, StartLocation.Y + y * DisplaySize.Height / pictureBox1.Height);
        }

        private Point ConvertPointToOriginal(Point p) {
            return ConvertPointToOriginal(p.X, p.Y);
        }

        private Point ConvertPointToScreen(int x, int y) {
            return new Point((x - StartLocation.X) * pictureBox1.Width / DisplaySize.Width, (y - StartLocation.Y) * pictureBox1.Height / DisplaySize.Height);
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
            }
            else {
                pictureBox1.Cursor = Cursors.SizeAll;
            }
        }

        public void CopyImage() {
            lock (original) {
                var i = new Bitmap(original.Width, original.Height + 13);
                using (var g = Graphics.FromImage(i)) {
                    g.Clear(Color.FromArgb(10, 10, 10));
                    g.DrawImage(original, 0, 13);
                    g.DrawString(labelClock.Text, new Font("ＭＳ ゴシック", 9), Brushes.White, original.Width - 51, 0);
                }
                Clipboard.SetImage(i);
                i.Dispose();
            }
        }

        private void menuItemCopy_Click(object sender, EventArgs e) {
            CopyImage();
        }

        private void menuItemTopMost_Click(object sender, EventArgs e) {
            SetTopMost(!TopMost);
        }

        private void menuItemSilent_Click(object sender, EventArgs e) {
            displayManager.Window.SetSilent(!displayManager.Window.Silent);
        }

        public void SetSilent(bool silent) {
            menuItemSilent.CheckState = silent ? CheckState.Checked : CheckState.Unchecked;
        }

        public void SetWindowName(string name) {
            Text = $"{name} | TID - ダイヤ運転会";
        }

        private void menuItemRename_Click(object sender, EventArgs e) {
            var d = new SubWindowName(this);
            d.TopMost = TopMost;
            OpeningDialog = true;
            d.ShowDialog();
            OpeningDialog = false;
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

        private void menuItemMarkupDelayed0_Click(object sender, EventArgs e) {
            displayManager.Window.SetMarkupDelayed(0);
        }

        private void menuItemMarkupDelayed1_Click(object sender, EventArgs e) {
            displayManager.Window.SetMarkupDelayed(1);
        }

        private void menuItemMarkupDelayed5_Click(object sender, EventArgs e) {
            displayManager.Window.SetMarkupDelayed(5);
        }

        private void menuItemMarkupDelayed10_Click(object sender, EventArgs e) {
            displayManager.Window.SetMarkupDelayed(10);
        }

        private void menuItemMarkupDelayed20_Click(object sender, EventArgs e) {
            displayManager.Window.SetMarkupDelayed(20);
        }

        public void SetMarkupDelayed(int minutes) {
            menuItemMarkupDelayed0.CheckState = minutes == 0 ? CheckState.Indeterminate : CheckState.Unchecked;
            menuItemMarkupDelayed1.CheckState = minutes == 1 ? CheckState.Indeterminate : CheckState.Unchecked;
            menuItemMarkupDelayed5.CheckState = minutes == 5 ? CheckState.Indeterminate : CheckState.Unchecked;
            menuItemMarkupDelayed10.CheckState = minutes == 10 ? CheckState.Indeterminate : CheckState.Unchecked;
            menuItemMarkupDelayed20.CheckState = minutes == 20 ? CheckState.Indeterminate : CheckState.Unchecked;
        }

        private void menuItemMarkupDuplication_Click(object sender, EventArgs e) {
            displayManager.Window.SwitchMarkupDuplication();
        }

        public void SetMarkupDuplication(bool value) {
            menuItemMarkupDuplication.CheckState = value ? CheckState.Checked : CheckState.Unchecked;
        }

        private void menuItemMarkupFillZero_Click(object sender, EventArgs e) {
            displayManager.Window.SwitchMarkupFillZero();
        }

        public void SetMarkupFillZero(bool value) {
            menuItemMarkupFillZero.CheckState = value ? CheckState.Checked : CheckState.Unchecked;
        }

        private void menuItemMarkup9999_Click(object sender, EventArgs e) {
            displayManager.Window.SwitchMarkup9999();
        }

        public void SetMarkup9999(bool value) {
            menuItemMarkup9999.CheckState = value ? CheckState.Checked : CheckState.Unchecked;
        }

        private void menuItemMarkupNotTrain_Click(object sender, EventArgs e) {
            displayManager.Window.SwitchMarkupNotTrain();
        }

        public void SetMarkupNotTrain(bool value) {
            menuItemMarkupNotTrain.CheckState = value ? CheckState.Checked : CheckState.Unchecked;
        }

        private void menuItemMarkupSpawned_Click(object sender, EventArgs e) {
            displayManager.Window.SwitchMarkupSpawned();
        }

        public void SetMarkupSpawned(bool value) {
            menuItemMarkupSpawned.CheckState = value ? CheckState.Checked : CheckState.Unchecked;
        }


        private void menuItemMarkupHandover_Click(object sender, EventArgs e) {
            displayManager.Window.SwitchMarkupHandover();
        }

        public void SetMarkupHandover(bool value) {
            menuItemMarkupHandover.CheckState = value ? CheckState.Checked : CheckState.Unchecked;
        }

        private void menuItemMarkupAll_Click(object sender, EventArgs e) {
            displayManager.Window.MarkupAll();
        }

        private void menuItemMarkupCancel_Click(object sender, EventArgs e) {
            displayManager.Window.MarkupCancel();
        }

        private void menuItemMarkupType1_Click(object sender, EventArgs e) {
            displayManager.Window.SetMarkupType(0);
        }

        private void menuItemMarkupType2_Click(object sender, EventArgs e) {
            displayManager.Window.SetMarkupType(1);
        }

        private void menuItemMarkupType3_Click(object sender, EventArgs e) {
            displayManager.Window.SetMarkupType(2);
        }

        public void SetMarkupType(int type) {
            menuItemMarkupType1.CheckState = type == 0 ? CheckState.Indeterminate : CheckState.Unchecked;
            menuItemMarkupType2.CheckState = type == 1 ? CheckState.Indeterminate : CheckState.Unchecked;
            menuItemMarkupType3.CheckState = type == 2 ? CheckState.Indeterminate : CheckState.Unchecked;
        }

        public void SetMarkupClass(int index, bool value) {
            ((ToolStripMenuItem)menuItemMarkupClass.DropDownItems[index]).CheckState = value ? CheckState.Checked : CheckState.Unchecked;
        }

        public void UpdateTrainCheck(TrainData td) {
            trainMenuDict[td.Number].CheckState = td.Markup ? CheckState.Checked : CheckState.Unchecked;
        }

        public void AddTrain(string trainNumber) {
            var menu = new ToolStripMenuItem();
            trainMenuDict.Add(trainNumber, menu);
            for (var i = 6; i <= menuItemTrainMarkup.DropDownItems.Count; i++) {
                if (menuItemTrainMarkup.DropDownItems.Count == i) {
                    menuItemTrainMarkup.DropDownItems.Add(menu);
                    break;
                }
                if (menuItemTrainMarkup.DropDownItems[i].Name?.CompareTo(trainNumber) >= 0) {
                    menuItemTrainMarkup.DropDownItems.Insert(i, menu);
                    break;
                }
            }
            menu.Name = trainNumber;
            menu.Size = new Size(110, 22);
            menu.Text = trainNumber;
            menu.Click += (sender, e) => {
                displayManager.Window.SetTrainMarkup(trainNumber);
            };
            menu.CheckState = displayManager.Window.MarkupSpawned ? CheckState.Checked : CheckState.Unchecked;
        }

        public void RemoveTrain(string trainNumber) {
            var menu = trainMenuDict[trainNumber];
            trainMenuDict.Remove(trainNumber);
            menuItemTrainMarkup.DropDownItems.Remove(menu);
        }

        public void SetMarkupTrain(string trainNumber, bool value) {
            if (trainMenuDict.TryGetValue(trainNumber, out var menu)) {
                menu.CheckState = value ? CheckState.Checked : CheckState.Unchecked;
            }
        }

        public void UpdateStatus() {
            if (InvokeRequired) {
                Invoke(() => {
                    labelStatus.Text = StatusText;
                    labelStatus.ForeColor = StatusColor;
                });
            }
            else {
                labelStatus.Text = StatusText;
                labelStatus.ForeColor = StatusColor;
            }
        }

        public static void SetStatus(string text, Color color) {
            StatusText = text;
            StatusColor = color;
        }
    }
}

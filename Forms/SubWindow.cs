using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Forms;
using TrainCrewTIDWindow.Manager;
using TrainCrewTIDWindow.Models;

namespace TrainCrewTIDWindow.Forms {
    public partial class SubWindow : Form {

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

        public bool OpeningDialog {
            get;
            private set;
        }

        /// <summary>
        /// マウス位置（ドラッグ操作対応用）
        /// </summary>
        private Point mouseLoc = Point.Empty;

        public SubWindow(Point location, Size size, TIDManager displayManager) {
            StartLocation = location;
            DisplaySize = size;
            this.displayManager = displayManager;
            InitializeComponent();

            Text = $"サブモニタ{++counter} | TID - ダイヤ運転会";

            Size = new Size(Size.Width - ClientSize.Width + DisplaySize.Width, Size.Height - ClientSize.Height + pictureBox1.Location.Y + DisplaySize.Height);
            MinimumSize = new Size(Size.Width - ClientSize.Width + DisplaySize.Width / 2, Size.Height - ClientSize.Height + pictureBox1.Location.Y + DisplaySize.Height / 2);

            windowSize = Size;

            pictureBox1.Size = size;

            /*flowLayoutPanel1.Location = new Point(flowLayoutPanel1.Location.X - Size.Width + ClientSize.Width + 16, flowLayoutPanel1.Location.Y);*/

            DetectResize = true;

            UpdateImage(displayManager.OriginalBitmap);
        }

        public void UpdateImage(Image image) {
            Debug.WriteLine($"updated");
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
                pictureBox1.Image = new Bitmap(original, pictureBox1.Width, pictureBox1.Height);
                old?.Dispose();
            }
        }


        private void SubWindow_Closing(object sender, FormClosingEventArgs e) {
            displayManager.RemoveSubWindow(this);
        }

        private void PictureBox1_MouseDown(object sender, MouseEventArgs e) {
            if ((e.Button & MouseButtons.Left) == MouseButtons.Left) {
                if (ModifierKeys.HasFlag(Keys.Shift)) {
                    foreach (var w in displayManager.NumberWindowDict.Values) {
                        var t = w.Train;
                        if (t != null && IsInArea(e.Location, w.PosX, w.PosY, w.GetSize(), 1) && displayManager.Window.TrainDataDict.TryGetValue(t, out var td)) {
                            td.Markup = !td.Markup;
                            displayManager.Window.UpdateTrainCheck(td);
                            displayManager.Window.ReservedUpdate = true;
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
            DetectResize = false;
            Size = new Size(Size.Width - ClientSize.Width + DisplaySize.Width, Size.Height - ClientSize.Height + pictureBox1.Location.Y + DisplaySize.Height);
            lock (pictureBox1) {
                var old = pictureBox1.Image;
                pictureBox1.Image = new Bitmap(original, pictureBox1.Width, pictureBox1.Height);
                old?.Dispose();
            }


            labelScale.Text = $"Scale：100%";
            labelScale.ForeColor = Color.White;
            DetectResize = true;
        }

        private void PictureBox1_MouseMove(object sender, MouseEventArgs e) {
            if (!ModifierKeys.HasFlag(Keys.Shift) && (e.Button & MouseButtons.Left) == MouseButtons.Left) {
                var pos = Cursor.Position;
                Debug.WriteLine($"{pos.X - mouseLoc.X} {pos.Y - mouseLoc.Y}");
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
            var mw = Size.Width - ClientSize.Width + DisplaySize.Width * (screenSize.Height - Size.Height + ClientSize.Height - pictureBox1.Location.Y) / DisplaySize.Height;
            var mh = Size.Height - ClientSize.Height + pictureBox1.Location.Y + DisplaySize.Height * (screenSize.Width - Size.Width + ClientSize.Width) / DisplaySize.Width;
            if (screenSize.Height < mh) {
                MaximumSize = new Size(mw, screenSize.Height);
            }
            else {
                MaximumSize = new Size(screenSize.Width, mh);
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
                if (ratio != 100) {
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
        }

        private void SubWindow_KeyUp(object sender, KeyEventArgs e) {
            UpdateMouseCursor();

        }

        public void SetClock(DateTime time) {
            labelClock.Text = time.ToString("H:mm:ss");
        }

        private Point ConvertPoint(int x, int y) {
            return new Point(StartLocation.X + x * DisplaySize.Width / pictureBox1.Width, StartLocation.Y + y * DisplaySize.Height / pictureBox1.Height);
        }

        private Point ConvertPoint(Point p) {
            return ConvertPoint(p.X, p.Y);
        }

        private bool IsInArea(Point point, int areaX, int areaY, Size areaSize, int padding = 0) {
            var p = ConvertPoint(point);
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
                using var g = Graphics.FromImage(i);
                g.Clear(Color.FromArgb(10, 10, 10));
                g.DrawImage(original, 0, 13);
                g.DrawString(labelClock.Text, new Font("ＭＳ ゴシック", 9), Brushes.White, original.Width - 51, 0);
                Clipboard.SetImage(i);
            }
        }

        private void menuItemCopy_Click(object sender, EventArgs e) {
            CopyImage();
        }

        private void menuItemTopMost_Click(object sender, EventArgs e) {
            SetTopMost(!TopMost);
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
    }
}

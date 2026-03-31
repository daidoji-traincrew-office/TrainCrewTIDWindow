
using TrainCrewTIDWindow.Manager;
using TrainCrewTIDWindow.Settings;

namespace TrainCrewTIDWindow.Forms {
    public partial class NavigationWindow : Form {
        public static NavigationWindow? Instance { get; private set; } = null;

        private readonly TIDManager displayManager;

        private readonly List<Panel> panelStations = [];

        private readonly List<Label> labelStations = [];

        private readonly List<CheckBox> checkBoxStations = [];

        private int selectedStationIndex = -1;

        public NavigationWindow(TIDManager displayManager) {
            Instance = this;
            InitializeComponent();
            this.displayManager = displayManager;
            Text = $"ナビゲーション | TID - ダイヤ運転会";
            /*PlaceStations();
            UpdateNotification();
            UpdateAlert();*/
            trackBarVolumeMaster.Value = TIDWindow.VolumeMaster;
            buttonMuteMaster.Text = TIDWindow.MuteMaster ? "ﾐｭｰﾄ解除" : "ミュート";
            labelVolumeMaster.ForeColor = TIDWindow.MuteMaster ? Color.Gray : Color.Black;
            trackBarVolumeWarning.Value = TIDWindow.VolumeWarning;
            buttonMuteWarning.Text = TIDWindow.MuteWarningSound ? "ﾐｭｰﾄ解除" : "ミュート";
            labelVolumeWarning.ForeColor = TIDWindow.MuteWarningSound ? Color.Gray : Color.Black;

            labelVolumeWarning.Text = $"警告・{displayManager.Window.ErrorText}音";

            checkBoxTopMost.Checked = displayManager.Window.TopMost;



        }

        private void PlaceStations() {
            /*var stations = displayManager.StationSettings;
            for (var i = 0; i < stations.Count; i++) {
                var s = stations[i];
                var p = new Panel();
                var l = new Label();
                var c = new CheckBox();
                var b1 = new Button();
                var b2 = new Button();
                panelStations.Add(p);
                labelStations.Add(l);
                checkBoxStations.Add(c);
                tabHavingStation.Controls.Add(p);
                p.SuspendLayout();
                p.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
                p.BackColor = s.Active ? Color.LightBlue : SystemColors.ControlLight;
                p.Controls.Add(l);
                p.Controls.Add(c);
                p.Controls.Add(b1);
                p.Controls.Add(b2);
                p.Location = new Point(8, label1.Location.Y + label1.Size.Height + 5 + i * 36);
                p.Name = $"panel{s.Code}";
                p.Size = new Size(*//*360*//*tabHavingStation.Size.Width - 16, 35);
                *//*p.Cursor = Cursors.Hand;*//*
                p.TabIndex = 0;


                b2.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Right;
                b2.Name = $"b2{s.Code}";
                b2.Size = new Size(50, 31);
                b2.AutoSize = false;
                b2.Text = "開く";
                b2.Location = new Point(p.Size.Width - b2.Size.Width - 2, (p.Size.Height - b2.Size.Height) / 2);
                b2.TabIndex = 1;
                b2.Click += (sender, e) => {
                    foreach (var w in displayManager.SubWindows) {
                        if (w.StartLocation.X < s.AreaLocation.X && w.StartLocation.Y < s.AreaLocation.Y && w.StartLocation.X + w.DisplaySize.Width > s.AreaLocation.X + s.AreaSize.Width && w.StartLocation.Y + w.DisplaySize.Height > s.AreaLocation.Y + s.AreaSize.Height) {
                            w.Activate();
                            return;
                        }
                    }

                    var sub = new SubWindow(new Point(Math.Max(0, s.AreaLocation.X - 16), Math.Max(0, s.AreaLocation.Y - 16)), new Size(s.AreaSize.Width + 32, s.AreaSize.Height + 32), displayManager, s.FullName);
                    sub.Icon = Icon;
                    sub.Show();
                    var border = (Size.Width - ClientSize.Width) / 2;
                    sub.SetTopMost(TopMost);
                    sub.SetClockColor(displayManager.Window.UseServerTime ? Color.White : Color.Yellow);
                    displayManager.AddSubWindow(sub);
                };
                b2.UseVisualStyleBackColor = true;


                b1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Right;
                b1.Name = $"b1{s.Code}";
                b1.Size = new Size(50, 31);
                b1.AutoSize = false;
                b1.Text = "移動";
                b1.Location = new Point(p.Size.Width - b1.Size.Width - b2.Size.Width - 4, (p.Size.Height - b1.Size.Height) / 2);
                b1.TabIndex = 1;
                b1.Click += (sender, e) => {
                    displayManager.Window.Activate();

                    displayManager.Window.MoveScroll(Math.Max(0, s.AreaLocation.X - 16), Math.Max(0, s.AreaLocation.Y - 16));
                };
                b1.UseVisualStyleBackColor = true;


                l.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
                l.BackColor = Color.Transparent;
                l.Font = new Font("Yu Gothic UI", 11F);
                l.Location = new Point(32, 0);
                l.Name = $"label{s.Code}";
                l.Size = new Size(p.Size.Width - 34 - 8 - b1.Size.Width - b2.Size.Width, 35);
                l.TabIndex = 1;
                l.Text = s.FullName;
                l.TextAlign = ContentAlignment.MiddleLeft;
                l.Click += (sender, e) => {
                    var i = labelStations.IndexOf(l);
                    if (selectedStationIndex < 0) {
                        selectedStationIndex = i;
                        panelStations[i].BackColor = Color.LightSkyBlue;
                    }
                    else {
                        var start = Math.Min(i, selectedStationIndex);
                        var end = Math.Max(i, selectedStationIndex);
                        for (var j = 0; j < checkBoxStations.Count; j++) {
                            panelStations[j].BackColor = j >= start && j <= end ? Color.LightBlue : SystemColors.ControlLight;
                            checkBoxStations[j].Checked = j >= start && j <= end;
                        }
                    }
                };
                l.DoubleClick += (sender, e) => {
                    var i = labelStations.IndexOf(l);
                    for (var j = 0; j < checkBoxStations.Count; j++) {
                        panelStations[j].BackColor = j == i ? Color.LightBlue : SystemColors.ControlLight;
                        checkBoxStations[j].Checked = j == i;
                    }
                };
                l.Cursor = Cursors.Hand;
                c.AutoSize = false;
                c.Location = new Point(9, 0);
                c.Name = $"checkBox{s.Code}";
                c.Size = new Size(23, 35);
                c.Checked = s.Active;
                c.CheckedChanged += (sender, e) => {
                    if (selectedStationIndex >= 0) {
                        panelStations[selectedStationIndex].BackColor = checkBoxStations[selectedStationIndex].Checked ? Color.LightBlue : SystemColors.ControlLight;
                        selectedStationIndex = -1;
                    }
                    s.SetActive(c.Checked);
                    p.BackColor = s.Active ? Color.LightBlue : SystemColors.ControlLight;
                    displayManager.Window.ReservedUpdate = 1;
                };
                c.TabIndex = 0;
                c.UseVisualStyleBackColor = true;

                p.ResumeLayout(false);
                p.PerformLayout();
            }*/
        }
        private void NavigationWindow_Closing(object sender, FormClosingEventArgs e) {
            Instance = null;
        }

        public void UpdateNotification() {
            /*if (InvokeRequired) {
                Invoke(() => {
                    labelNotifications.Text = NotificationManager.IsEmpty ? "通知がありません" : NotificationManager.GetNotification();
                });
            }
            else {
                labelNotifications.Text = NotificationManager.IsEmpty ? "通知がありません" : NotificationManager.GetNotification();
            }*/
        }

        public void UpdateAlert() {
            /*if (InvokeRequired) {
                Invoke(() => {
                    tabTrain.Controls.Clear();
                    var alerts = TrainAlertManager.TrainAlerts;
                    var width = Size.Width - 36;
                    tabTrain.AutoScroll = false;
                    for (var i = 0; i < alerts.Count; i++) {
                        var p = new Panel();
                        var l1 = new Label();
                        var l2 = new Label();
                        tabTrain.Controls.Add(p);

                        var a = alerts[i];
                        p.BackColor = Color.WhiteSmoke;
                        p.Controls.Add(l2);
                        p.Controls.Add(l1);
                        p.Location = new Point(6, 3 + i * 100);
                        p.Name = "panel2";
                        p.Size = new Size(width, 96);
                        p.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
                        p.TabIndex = 3;
                        p.Cursor = Cursors.Hand;
                        p.Click += (sender, e) => { MoveToStation(a.Station); };


                        l1.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
                        l1.BackColor = a.Important ? Color.OrangeRed : Color.FromArgb(0, 192, 192);
                        l1.Font = new Font("Yu Gothic UI", 10F, FontStyle.Bold);
                        l1.ForeColor = Color.White;
                        l1.Location = new Point(3, 0);
                        l1.Name = "label2";
                        l1.Text = a.Title;
                        l1.Size = new Size(p.Size.Width - 6, 30);
                        l1.TabIndex = 0;
                        l1.TextAlign = ContentAlignment.MiddleLeft;
                        l1.Cursor = Cursors.Hand;
                        l1.Click += (sender, e) => { MoveToStation(a.Station); };

                        l2.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
                        l2.BackColor = Color.WhiteSmoke;
                        l2.Font = new Font("Yu Gothic UI", 9F);
                        l2.Location = new Point(3, 32);
                        l2.Name = "richTextBox1";
                        l2.Text = a.Text;
                        l2.Size = new Size(p.Size.Width - 6, 61);
                        l2.TabIndex = 1;
                        l2.Cursor = Cursors.Hand;
                        l2.Click += (sender, e) => { MoveToStation(a.Station); };


                    }
                    tabTrain.AutoScroll = true;
                });
            }
            else {
                tabTrain.Controls.Clear();
                var alerts = TrainAlertManager.TrainAlerts;
                var width = Size.Width - 36;
                tabTrain.AutoScroll = false;
                for (var i = 0; i < alerts.Count; i++) {
                    var p = new Panel();
                    var l1 = new Label();
                    var l2 = new Label();
                    tabTrain.Controls.Add(p);

                    var a = alerts[i];
                    p.BackColor = Color.WhiteSmoke;
                    p.Controls.Add(l2);
                    p.Controls.Add(l1);
                    p.Location = new Point(6, 3 + i * 100);
                    p.Name = "panel2";
                    p.Size = new Size(width, 96);
                    p.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
                    p.TabIndex = 3;
                    p.Cursor = Cursors.Hand;
                    p.Click += (sender, e) => { MoveToStation(a.Station); };


                    l1.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
                    l1.BackColor = a.Important ? Color.OrangeRed : Color.FromArgb(0, 192, 192);
                    l1.Font = new Font("Yu Gothic UI", 10F, FontStyle.Bold);
                    l1.ForeColor = Color.White;
                    l1.Location = new Point(3, 0);
                    l1.Name = "label2";
                    l1.Text = a.Title;
                    l1.Size = new Size(p.Size.Width - 6, 30);
                    l1.TabIndex = 0;
                    l1.TextAlign = ContentAlignment.MiddleLeft;
                    l1.Cursor = Cursors.Hand;
                    l1.Click += (sender, e) => { MoveToStation(a.Station); };

                    l2.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
                    l2.BackColor = Color.WhiteSmoke;
                    l2.Font = new Font("Yu Gothic UI", 9F);
                    l2.Location = new Point(3, 32);
                    l2.Name = "richTextBox1";
                    l2.Text = a.Text;
                    l2.Size = new Size(p.Size.Width - 6, 61);
                    l2.TabIndex = 1;
                    l2.Cursor = Cursors.Hand;
                    l2.Click += (sender, e) => { MoveToStation(a.Station); };


                }
                tabTrain.AutoScroll = true;
            }*/
        }
/*
        public void MoveToStation(StationSetting s) {
            foreach (var w in displayManager.SubWindows) {
                if (w.StartLocation.X < s.AreaLocation.X && w.StartLocation.Y < s.AreaLocation.Y && w.StartLocation.X + w.DisplaySize.Width > s.AreaLocation.X + s.AreaSize.Width && w.StartLocation.Y + w.DisplaySize.Height > s.AreaLocation.Y + s.AreaSize.Height) {
                    w.Activate();
                    return;
                }
            }

            displayManager.Window.Activate();

            displayManager.Window.MoveScroll(Math.Max(0, s.AreaLocation.X - 16), Math.Max(0, s.AreaLocation.Y - 16));

        }*/
/*
        public void SelectTabTrainAlert() {
            tabControl1.SelectedTab = tabTrain;
        }*/

        private void trackBarVolumeMaster_ValueChanged(object sender, EventArgs e) {
            toolTip1.SetToolTip(trackBarVolumeMaster, trackBarVolumeMaster.Value.ToString());
            TIDWindow.VolumeMaster = trackBarVolumeMaster.Value;
        }

        private void trackBarVolumeMaster_MouseUp(object sender, MouseEventArgs e) {

        }

        private void buttonMuteMaster_Click(object sender, EventArgs e) {
            TIDWindow.MuteMaster = !TIDWindow.MuteMaster;
            buttonMuteMaster.Text = TIDWindow.MuteMaster ? "ﾐｭｰﾄ解除" : "ミュート";
            labelVolumeMaster.ForeColor = TIDWindow.MuteMaster ? Color.Gray : Color.Black;
        }

        private void trackBarVolumeWarning_ValueChanged(object sender, EventArgs e) {
            toolTip1.SetToolTip(trackBarVolumeWarning, trackBarVolumeWarning.Value.ToString());
            TIDWindow.VolumeWarning = trackBarVolumeWarning.Value;
        }

        private void trackBarVolumeWarning_MouseUp(object sender, MouseEventArgs e) {
            TIDWindow.PlayWarningSound();
        }

        private void buttonMuteWarning_Click(object sender, EventArgs e) {
            TIDWindow.MuteWarningSound = !TIDWindow.MuteWarningSound;
            buttonMuteWarning.Text = TIDWindow.MuteWarningSound ? "ﾐｭｰﾄ解除" : "ミュート";
            labelVolumeWarning.ForeColor = TIDWindow.MuteWarningSound ? Color.Gray : Color.Black;
        }

        private void checkBoxTopMost_CheckedChanged(object sender, EventArgs e) {
            SetTopMost(!TopMost);
        }

        public void SetTopMost(bool topMost) {
            TopMost = topMost;
            checkBoxTopMost.Checked = topMost;
        }
    }
}

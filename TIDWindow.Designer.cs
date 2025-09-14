namespace TrainCrewTIDWindow {
    partial class TIDWindow {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            components = new System.ComponentModel.Container();
            pictureBox1 = new PictureBox();
            contextMenuStrip1 = new ContextMenuStrip(components);
            menuItemCopy = new ToolStripMenuItem();
            toolStripSeparator2 = new ToolStripSeparator();
            menuItemTrainMarkup = new ToolStripMenuItem();
            menuItemMarkupClass = new ToolStripMenuItem();
            menuItemMarkupDelayed = new ToolStripMenuItem();
            menuItemMarkupDelayed0 = new ToolStripMenuItem();
            toolStripSeparator5 = new ToolStripSeparator();
            menuItemMarkupDelayed1 = new ToolStripMenuItem();
            menuItemMarkupDelayed5 = new ToolStripMenuItem();
            menuItemMarkupDelayed10 = new ToolStripMenuItem();
            menuItemMarkupDelayed20 = new ToolStripMenuItem();
            menuItemMarkupOther = new ToolStripMenuItem();
            menuItemMarkupDuplication = new ToolStripMenuItem();
            menuItemMarkupFillZero = new ToolStripMenuItem();
            menuItemMarkup9999 = new ToolStripMenuItem();
            menuItemMarkupNotTrain = new ToolStripMenuItem();
            toolStripSeparator4 = new ToolStripSeparator();
            menuItemMarkupSpawned = new ToolStripMenuItem();
            menuItemMarkupAll = new ToolStripMenuItem();
            menuItemMarkupCancel = new ToolStripMenuItem();
            toolStripSeparator3 = new ToolStripSeparator();
            menuItemMarkupType = new ToolStripMenuItem();
            menuItemMarkupType1 = new ToolStripMenuItem();
            menuItemMarkupType2 = new ToolStripMenuItem();
            menuItemMarkupType3 = new ToolStripMenuItem();
            toolStripSeparator1 = new ToolStripSeparator();
            menuItemScale = new ToolStripMenuItem();
            menuItemScaleFit = new ToolStripMenuItem();
            menuItemMagnifyingGlass = new ToolStripMenuItem();
            menuItemPushToZoom = new ToolStripMenuItem();
            menuItemToggle = new ToolStripMenuItem();
            menuItemTopMost = new ToolStripMenuItem();
            menuItemSilent = new ToolStripMenuItem();
            menuItemQuickTimeSetting = new ToolStripMenuItem();
            menuItemHour0 = new ToolStripMenuItem();
            menuItemHour1 = new ToolStripMenuItem();
            menuItemHour2 = new ToolStripMenuItem();
            menuItemHour3 = new ToolStripMenuItem();
            menuItemHour4 = new ToolStripMenuItem();
            menuItemHour5 = new ToolStripMenuItem();
            menuItemHour6 = new ToolStripMenuItem();
            menuItemHour7 = new ToolStripMenuItem();
            menuItemHour8 = new ToolStripMenuItem();
            menuItemHour9 = new ToolStripMenuItem();
            menuItemHour10 = new ToolStripMenuItem();
            menuItemHour11 = new ToolStripMenuItem();
            menuItemHour12 = new ToolStripMenuItem();
            menuItemHour13 = new ToolStripMenuItem();
            menuItemHour14 = new ToolStripMenuItem();
            menuItemHour15 = new ToolStripMenuItem();
            menuItemHour16 = new ToolStripMenuItem();
            menuItemHour17 = new ToolStripMenuItem();
            menuItemHour18 = new ToolStripMenuItem();
            menuItemHour19 = new ToolStripMenuItem();
            menuItemHour20 = new ToolStripMenuItem();
            menuItemHour21 = new ToolStripMenuItem();
            menuItemHour22 = new ToolStripMenuItem();
            menuItemHour23 = new ToolStripMenuItem();
            panel1 = new Panel();
            pictureBox2 = new PictureBox();
            labelStatus = new Label();
            labelClock = new Label();
            labelTopMost = new Label();
            labelScale = new Label();
            labelSilent = new Label();
            toolStripSeparator6 = new ToolStripSeparator();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
            contextMenuStrip1.SuspendLayout();
            panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox2).BeginInit();
            SuspendLayout();
            // 
            // pictureBox1
            // 
            pictureBox1.ContextMenuStrip = contextMenuStrip1;
            pictureBox1.Cursor = Cursors.SizeAll;
            pictureBox1.Location = new Point(0, 0);
            pictureBox1.Name = "pictureBox1";
            pictureBox1.Size = new Size(984, 537);
            pictureBox1.TabIndex = 0;
            pictureBox1.TabStop = false;
            pictureBox1.MouseDown += PictureBox1_MouseDown;
            pictureBox1.MouseMove += PictureBox1_MouseMove;
            pictureBox1.MouseUp += PictureBox1_MouseUp;
            pictureBox1.MouseWheel += PictureBox1_MouseWheel;
            // 
            // contextMenuStrip1
            // 
            contextMenuStrip1.Items.AddRange(new ToolStripItem[] { menuItemCopy, toolStripSeparator2, menuItemTrainMarkup, menuItemMarkupType, toolStripSeparator1, menuItemScale, menuItemMagnifyingGlass, menuItemTopMost, menuItemSilent, menuItemQuickTimeSetting });
            contextMenuStrip1.Name = "contextMenuStrip1";
            contextMenuStrip1.Size = new Size(181, 214);
            // 
            // menuItemCopy
            // 
            menuItemCopy.Name = "menuItemCopy";
            menuItemCopy.Size = new Size(180, 22);
            menuItemCopy.Text = "TID画面をコピー";
            menuItemCopy.Click += menuItemCopy_Click;
            // 
            // toolStripSeparator2
            // 
            toolStripSeparator2.Name = "toolStripSeparator2";
            toolStripSeparator2.Size = new Size(177, 6);
            // 
            // menuItemTrainMarkup
            // 
            menuItemTrainMarkup.DropDownItems.AddRange(new ToolStripItem[] { menuItemMarkupClass, menuItemMarkupDelayed, menuItemMarkupOther, menuItemMarkupAll, menuItemMarkupCancel, toolStripSeparator3 });
            menuItemTrainMarkup.Name = "menuItemTrainMarkup";
            menuItemTrainMarkup.Size = new Size(180, 22);
            menuItemTrainMarkup.Text = "列車番号強調表示";
            // 
            // menuItemMarkupClass
            // 
            menuItemMarkupClass.Name = "menuItemMarkupClass";
            menuItemMarkupClass.Size = new Size(134, 22);
            menuItemMarkupClass.Text = "列車種別";
            // 
            // menuItemMarkupDelayed
            // 
            menuItemMarkupDelayed.DropDownItems.AddRange(new ToolStripItem[] { menuItemMarkupDelayed0, toolStripSeparator5, menuItemMarkupDelayed1, menuItemMarkupDelayed5, menuItemMarkupDelayed10, menuItemMarkupDelayed20 });
            menuItemMarkupDelayed.Name = "menuItemMarkupDelayed";
            menuItemMarkupDelayed.Size = new Size(134, 22);
            menuItemMarkupDelayed.Text = "遅延列車";
            // 
            // menuItemMarkupDelayed0
            // 
            menuItemMarkupDelayed0.Checked = true;
            menuItemMarkupDelayed0.CheckState = CheckState.Indeterminate;
            menuItemMarkupDelayed0.Name = "menuItemMarkupDelayed0";
            menuItemMarkupDelayed0.Size = new Size(122, 22);
            menuItemMarkupDelayed0.Text = "無効";
            menuItemMarkupDelayed0.Click += menuItemMarkupDelayed0_Click;
            // 
            // toolStripSeparator5
            // 
            toolStripSeparator5.Name = "toolStripSeparator5";
            toolStripSeparator5.Size = new Size(119, 6);
            // 
            // menuItemMarkupDelayed1
            // 
            menuItemMarkupDelayed1.Name = "menuItemMarkupDelayed1";
            menuItemMarkupDelayed1.Size = new Size(122, 22);
            menuItemMarkupDelayed1.Text = "1分以上";
            menuItemMarkupDelayed1.Click += menuItemMarkupDelayed1_Click;
            // 
            // menuItemMarkupDelayed5
            // 
            menuItemMarkupDelayed5.Name = "menuItemMarkupDelayed5";
            menuItemMarkupDelayed5.Size = new Size(122, 22);
            menuItemMarkupDelayed5.Text = "5分以上";
            menuItemMarkupDelayed5.Click += menuItemMarkupDelayed5_Click;
            // 
            // menuItemMarkupDelayed10
            // 
            menuItemMarkupDelayed10.Name = "menuItemMarkupDelayed10";
            menuItemMarkupDelayed10.Size = new Size(122, 22);
            menuItemMarkupDelayed10.Text = "10分以上";
            menuItemMarkupDelayed10.Click += menuItemMarkupDelayed10_Click;
            // 
            // menuItemMarkupDelayed20
            // 
            menuItemMarkupDelayed20.Name = "menuItemMarkupDelayed20";
            menuItemMarkupDelayed20.Size = new Size(122, 22);
            menuItemMarkupDelayed20.Text = "20分以上";
            menuItemMarkupDelayed20.Click += menuItemMarkupDelayed20_Click;
            // 
            // menuItemMarkupOther
            // 
            menuItemMarkupOther.DropDownItems.AddRange(new ToolStripItem[] { menuItemMarkupDuplication, menuItemMarkupFillZero, menuItemMarkup9999, menuItemMarkupNotTrain, toolStripSeparator4, menuItemMarkupSpawned });
            menuItemMarkupOther.Name = "menuItemMarkupOther";
            menuItemMarkupOther.Size = new Size(134, 22);
            menuItemMarkupOther.Text = "その他";
            // 
            // menuItemMarkupDuplication
            // 
            menuItemMarkupDuplication.Name = "menuItemMarkupDuplication";
            menuItemMarkupDuplication.Size = new Size(163, 22);
            menuItemMarkupDuplication.Text = "重複列車番号";
            menuItemMarkupDuplication.Click += menuItemMarkupDuplication_Click;
            // 
            // menuItemMarkupFillZero
            // 
            menuItemMarkupFillZero.Name = "menuItemMarkupFillZero";
            menuItemMarkupFillZero.Size = new Size(163, 22);
            menuItemMarkupFillZero.Text = "ゼロ埋め列車番号";
            menuItemMarkupFillZero.Click += menuItemMarkupFillZero_Click;
            // 
            // menuItemMarkup9999
            // 
            menuItemMarkup9999.Name = "menuItemMarkup9999";
            menuItemMarkup9999.Size = new Size(163, 22);
            menuItemMarkup9999.Text = "9999";
            menuItemMarkup9999.Click += menuItemMarkup9999_Click;
            // 
            // menuItemMarkupNotTrain
            // 
            menuItemMarkupNotTrain.Name = "menuItemMarkupNotTrain";
            menuItemMarkupNotTrain.Size = new Size(163, 22);
            menuItemMarkupNotTrain.Text = "列車以外";
            menuItemMarkupNotTrain.Click += menuItemMarkupNotTrain_Click;
            // 
            // toolStripSeparator4
            // 
            toolStripSeparator4.Name = "toolStripSeparator4";
            toolStripSeparator4.Size = new Size(160, 6);
            // 
            // menuItemMarkupSpawned
            // 
            menuItemMarkupSpawned.Name = "menuItemMarkupSpawned";
            menuItemMarkupSpawned.Size = new Size(163, 22);
            menuItemMarkupSpawned.Text = "新規スポーン";
            menuItemMarkupSpawned.Click += menuItemMarkupSpawned_Click;
            // 
            // menuItemMarkupAll
            // 
            menuItemMarkupAll.Name = "menuItemMarkupAll";
            menuItemMarkupAll.Size = new Size(134, 22);
            menuItemMarkupAll.Text = "列番全選択";
            menuItemMarkupAll.Click += menuItemMarkupAll_Click;
            // 
            // menuItemMarkupCancel
            // 
            menuItemMarkupCancel.Name = "menuItemMarkupCancel";
            menuItemMarkupCancel.Size = new Size(134, 22);
            menuItemMarkupCancel.Text = "列番全解除";
            menuItemMarkupCancel.Click += menuItemMarkupCancel_Click;
            // 
            // toolStripSeparator3
            // 
            toolStripSeparator3.Name = "toolStripSeparator3";
            toolStripSeparator3.Size = new Size(131, 6);
            // 
            // menuItemMarkupType
            // 
            menuItemMarkupType.DropDownItems.AddRange(new ToolStripItem[] { menuItemMarkupType1, menuItemMarkupType2, menuItemMarkupType3 });
            menuItemMarkupType.Name = "menuItemMarkupType";
            menuItemMarkupType.Size = new Size(180, 22);
            menuItemMarkupType.Text = "強調表示タイプ";
            // 
            // menuItemMarkupType1
            // 
            menuItemMarkupType1.Checked = true;
            menuItemMarkupType1.CheckState = CheckState.Indeterminate;
            menuItemMarkupType1.Name = "menuItemMarkupType1";
            menuItemMarkupType1.Size = new Size(191, 22);
            menuItemMarkupType1.Text = "タイプ1（点滅）";
            menuItemMarkupType1.Click += menuItemMarkupType1_Click;
            // 
            // menuItemMarkupType2
            // 
            menuItemMarkupType2.Name = "menuItemMarkupType2";
            menuItemMarkupType2.Size = new Size(191, 22);
            menuItemMarkupType2.Text = "タイプ2（色逆転点滅）";
            menuItemMarkupType2.Click += menuItemMarkupType2_Click;
            // 
            // menuItemMarkupType3
            // 
            menuItemMarkupType3.Name = "menuItemMarkupType3";
            menuItemMarkupType3.Size = new Size(191, 22);
            menuItemMarkupType3.Text = "タイプ3（色逆転固定）";
            menuItemMarkupType3.Click += menuItemMarkupType3_Click;
            // 
            // toolStripSeparator1
            // 
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new Size(177, 6);
            // 
            // menuItemScale
            // 
            menuItemScale.DropDownItems.AddRange(new ToolStripItem[] { toolStripSeparator6, menuItemScaleFit });
            menuItemScale.Name = "menuItemScale";
            menuItemScale.Size = new Size(180, 22);
            menuItemScale.Text = "拡大率";
            // 
            // menuItemScaleFit
            // 
            menuItemScaleFit.Name = "menuItemScaleFit";
            menuItemScaleFit.Size = new Size(180, 22);
            menuItemScaleFit.Text = "フィット表示";
            // 
            // menuItemMagnifyingGlass
            // 
            menuItemMagnifyingGlass.DropDownItems.AddRange(new ToolStripItem[] { menuItemPushToZoom, menuItemToggle });
            menuItemMagnifyingGlass.Name = "menuItemMagnifyingGlass";
            menuItemMagnifyingGlass.Size = new Size(180, 22);
            menuItemMagnifyingGlass.Text = "拡大鏡モード";
            // 
            // menuItemPushToZoom
            // 
            menuItemPushToZoom.Checked = true;
            menuItemPushToZoom.CheckState = CheckState.Indeterminate;
            menuItemPushToZoom.Name = "menuItemPushToZoom";
            menuItemPushToZoom.Size = new Size(231, 22);
            menuItemPushToZoom.Text = "プッシュトゥズーム（押下中のみ）";
            menuItemPushToZoom.Click += menuItemPushToZoom_Click;
            // 
            // menuItemToggle
            // 
            menuItemToggle.Name = "menuItemToggle";
            menuItemToggle.Size = new Size(231, 22);
            menuItemToggle.Text = "トグル（切替式）";
            menuItemToggle.Click += menuItemToggle_Click;
            // 
            // menuItemTopMost
            // 
            menuItemTopMost.Name = "menuItemTopMost";
            menuItemTopMost.Size = new Size(180, 22);
            menuItemTopMost.Text = "最前面表示";
            menuItemTopMost.Click += menuItemTopMost_Click;
            // 
            // menuItemSilent
            // 
            menuItemSilent.Name = "menuItemSilent";
            menuItemSilent.Size = new Size(180, 22);
            menuItemSilent.Text = "サイレントモード";
            menuItemSilent.Click += menuItemSilent_Click;
            // 
            // menuItemQuickTimeSetting
            // 
            menuItemQuickTimeSetting.DropDownItems.AddRange(new ToolStripItem[] { menuItemHour0, menuItemHour1, menuItemHour2, menuItemHour3, menuItemHour4, menuItemHour5, menuItemHour6, menuItemHour7, menuItemHour8, menuItemHour9, menuItemHour10, menuItemHour11, menuItemHour12, menuItemHour13, menuItemHour14, menuItemHour15, menuItemHour16, menuItemHour17, menuItemHour18, menuItemHour19, menuItemHour20, menuItemHour21, menuItemHour22, menuItemHour23 });
            menuItemQuickTimeSetting.Name = "menuItemQuickTimeSetting";
            menuItemQuickTimeSetting.Size = new Size(180, 22);
            menuItemQuickTimeSetting.Text = "クイック時刻設定";
            // 
            // menuItemHour0
            // 
            menuItemHour0.Name = "menuItemHour0";
            menuItemHour0.Size = new Size(110, 22);
            menuItemHour0.Text = "0時台";
            // 
            // menuItemHour1
            // 
            menuItemHour1.Name = "menuItemHour1";
            menuItemHour1.Size = new Size(110, 22);
            menuItemHour1.Text = "1時台";
            // 
            // menuItemHour2
            // 
            menuItemHour2.Name = "menuItemHour2";
            menuItemHour2.Size = new Size(110, 22);
            menuItemHour2.Text = "2時台";
            // 
            // menuItemHour3
            // 
            menuItemHour3.Name = "menuItemHour3";
            menuItemHour3.Size = new Size(110, 22);
            menuItemHour3.Text = "3時台";
            // 
            // menuItemHour4
            // 
            menuItemHour4.Name = "menuItemHour4";
            menuItemHour4.Size = new Size(110, 22);
            menuItemHour4.Text = "4時台";
            // 
            // menuItemHour5
            // 
            menuItemHour5.Name = "menuItemHour5";
            menuItemHour5.Size = new Size(110, 22);
            menuItemHour5.Text = "5時台";
            // 
            // menuItemHour6
            // 
            menuItemHour6.Name = "menuItemHour6";
            menuItemHour6.Size = new Size(110, 22);
            menuItemHour6.Text = "6時台";
            // 
            // menuItemHour7
            // 
            menuItemHour7.Name = "menuItemHour7";
            menuItemHour7.Size = new Size(110, 22);
            menuItemHour7.Text = "7時台";
            // 
            // menuItemHour8
            // 
            menuItemHour8.Name = "menuItemHour8";
            menuItemHour8.Size = new Size(110, 22);
            menuItemHour8.Text = "8時台";
            // 
            // menuItemHour9
            // 
            menuItemHour9.Name = "menuItemHour9";
            menuItemHour9.Size = new Size(110, 22);
            menuItemHour9.Text = "9時台";
            // 
            // menuItemHour10
            // 
            menuItemHour10.Name = "menuItemHour10";
            menuItemHour10.Size = new Size(110, 22);
            menuItemHour10.Text = "10時台";
            // 
            // menuItemHour11
            // 
            menuItemHour11.Name = "menuItemHour11";
            menuItemHour11.Size = new Size(110, 22);
            menuItemHour11.Text = "11時台";
            // 
            // menuItemHour12
            // 
            menuItemHour12.Name = "menuItemHour12";
            menuItemHour12.Size = new Size(110, 22);
            menuItemHour12.Text = "12時台";
            // 
            // menuItemHour13
            // 
            menuItemHour13.Name = "menuItemHour13";
            menuItemHour13.Size = new Size(110, 22);
            menuItemHour13.Text = "13時台";
            // 
            // menuItemHour14
            // 
            menuItemHour14.Name = "menuItemHour14";
            menuItemHour14.Size = new Size(110, 22);
            menuItemHour14.Text = "14時台";
            // 
            // menuItemHour15
            // 
            menuItemHour15.Name = "menuItemHour15";
            menuItemHour15.Size = new Size(110, 22);
            menuItemHour15.Text = "15時台";
            // 
            // menuItemHour16
            // 
            menuItemHour16.Name = "menuItemHour16";
            menuItemHour16.Size = new Size(110, 22);
            menuItemHour16.Text = "16時台";
            // 
            // menuItemHour17
            // 
            menuItemHour17.Name = "menuItemHour17";
            menuItemHour17.Size = new Size(110, 22);
            menuItemHour17.Text = "17時台";
            // 
            // menuItemHour18
            // 
            menuItemHour18.Name = "menuItemHour18";
            menuItemHour18.Size = new Size(110, 22);
            menuItemHour18.Text = "18時台";
            // 
            // menuItemHour19
            // 
            menuItemHour19.Name = "menuItemHour19";
            menuItemHour19.Size = new Size(110, 22);
            menuItemHour19.Text = "19時台";
            // 
            // menuItemHour20
            // 
            menuItemHour20.Name = "menuItemHour20";
            menuItemHour20.Size = new Size(110, 22);
            menuItemHour20.Text = "20時台";
            // 
            // menuItemHour21
            // 
            menuItemHour21.Name = "menuItemHour21";
            menuItemHour21.Size = new Size(110, 22);
            menuItemHour21.Text = "21時台";
            // 
            // menuItemHour22
            // 
            menuItemHour22.Name = "menuItemHour22";
            menuItemHour22.Size = new Size(110, 22);
            menuItemHour22.Text = "22時台";
            // 
            // menuItemHour23
            // 
            menuItemHour23.Name = "menuItemHour23";
            menuItemHour23.Size = new Size(110, 22);
            menuItemHour23.Text = "23時台";
            // 
            // panel1
            // 
            panel1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            panel1.AutoScroll = true;
            panel1.Controls.Add(pictureBox2);
            panel1.Controls.Add(pictureBox1);
            panel1.Location = new Point(0, 24);
            panel1.Margin = new Padding(0);
            panel1.Name = "panel1";
            panel1.Size = new Size(984, 537);
            panel1.TabIndex = 2;
            // 
            // pictureBox2
            // 
            pictureBox2.BackColor = Color.Transparent;
            pictureBox2.Cursor = Cursors.Cross;
            pictureBox2.Location = new Point(-300, -300);
            pictureBox2.Name = "pictureBox2";
            pictureBox2.Size = new Size(240, 240);
            pictureBox2.TabIndex = 6;
            pictureBox2.TabStop = false;
            pictureBox2.MouseDown += PictureBox2_MouseDown;
            pictureBox2.MouseMove += PictureBox2_MouseMove;
            // 
            // labelStatus
            // 
            labelStatus.AutoSize = true;
            labelStatus.BackColor = Color.Transparent;
            labelStatus.Font = new Font("ＭＳ ゴシック", 9F, FontStyle.Regular, GraphicsUnit.Point, 128);
            labelStatus.ForeColor = Color.White;
            labelStatus.Location = new Point(3, 3);
            labelStatus.Name = "labelStatus";
            labelStatus.Size = new Size(107, 12);
            labelStatus.TabIndex = 1;
            labelStatus.Text = "Status：起動中...";
            // 
            // labelClock
            // 
            labelClock.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            labelClock.BackColor = Color.Transparent;
            labelClock.Font = new Font("ＭＳ ゴシック", 9F, FontStyle.Regular, GraphicsUnit.Point, 128);
            labelClock.ForeColor = Color.White;
            labelClock.Location = new Point(918, 3);
            labelClock.Name = "labelClock";
            labelClock.Size = new Size(67, 12);
            labelClock.TabIndex = 3;
            labelClock.Text = "00:00:00";
            labelClock.TextAlign = ContentAlignment.TopRight;
            labelClock.MouseDown += labelClock_MouseDown;
            // 
            // labelTopMost
            // 
            labelTopMost.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            labelTopMost.BackColor = Color.FromArgb(30, 30, 30);
            labelTopMost.Font = new Font("ＭＳ ゴシック", 9F, FontStyle.Regular, GraphicsUnit.Point, 128);
            labelTopMost.ForeColor = Color.Gray;
            labelTopMost.Location = new Point(840, 3);
            labelTopMost.Name = "labelTopMost";
            labelTopMost.Size = new Size(78, 12);
            labelTopMost.TabIndex = 4;
            labelTopMost.Text = "最前面：OFF";
            labelTopMost.Click += labelTopMost_Click;
            labelTopMost.MouseLeave += labelTopMost_Leave;
            labelTopMost.MouseHover += labelTopMost_Hover;
            // 
            // labelScale
            // 
            labelScale.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            labelScale.BackColor = Color.Transparent;
            labelScale.Font = new Font("ＭＳ ゴシック", 9F, FontStyle.Regular, GraphicsUnit.Point, 128);
            labelScale.ForeColor = Color.White;
            labelScale.Location = new Point(750, 3);
            labelScale.Name = "labelScale";
            labelScale.Size = new Size(80, 12);
            labelScale.TabIndex = 5;
            labelScale.Text = "Scale：100%";
            labelScale.TextAlign = ContentAlignment.TopRight;
            labelScale.MouseDown += labelScale_MouseDown;
            // 
            // labelSilent
            // 
            labelSilent.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            labelSilent.BackColor = Color.FromArgb(30, 30, 30);
            labelSilent.Font = new Font("ＭＳ ゴシック", 9F, FontStyle.Regular, GraphicsUnit.Point, 128);
            labelSilent.ForeColor = Color.White;
            labelSilent.Location = new Point(650, 3);
            labelSilent.Name = "labelSilent";
            labelSilent.Size = new Size(95, 12);
            labelSilent.TabIndex = 6;
            labelSilent.Text = "サイレント：OFF";
            labelSilent.Click += labelSilent_Click;
            labelSilent.MouseLeave += labelSilent_Leave;
            labelSilent.MouseHover += labelSilent_Hover;
            // 
            // toolStripSeparator6
            // 
            toolStripSeparator6.Name = "toolStripSeparator6";
            toolStripSeparator6.Size = new Size(177, 6);
            // 
            // TIDWindow
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            AutoScroll = true;
            BackColor = Color.FromArgb(5, 5, 5);
            ClientSize = new Size(984, 561);
            Controls.Add(labelSilent);
            Controls.Add(labelScale);
            Controls.Add(labelTopMost);
            Controls.Add(labelClock);
            Controls.Add(panel1);
            Controls.Add(labelStatus);
            MaximumSize = new Size(1000, 600);
            MinimumSize = new Size(540, 300);
            Name = "TIDWindow";
            Text = "全線TID | TID - ダイヤ運転会";
            TopMost = true;
            FormClosing += TIDWindow_Closing;
            SizeChanged += TIDWindow_SizeChanged;
            KeyDown += TIDWindow_KeyDown;
            KeyUp += TIDWindow_KeyUp;
            Resize += TIDWindow_Resize;
            ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
            contextMenuStrip1.ResumeLayout(false);
            panel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)pictureBox2).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private PictureBox pictureBox1;
        private Label labelStatus;
        private Panel panel1;
        private Label labelClock;
        private Label labelTopMost;
        private ContextMenuStrip contextMenuStrip1;
        private ToolStripMenuItem menuItemCopy;
        private ToolStripMenuItem menuItemScale;
        private ToolStripMenuItem menuItemScaleFit;
        private Label labelScale;
        private PictureBox pictureBox2;
        private ToolStripMenuItem menuItemMagnifyingGlass;
        private ToolStripMenuItem menuItemPushToZoom;
        private ToolStripMenuItem menuItemToggle;
        private ToolStripSeparator toolStripSeparator1;
        private ToolStripMenuItem menuItemSilent;
        private Label labelSilent;
        private ToolStripMenuItem menuItemTopMost;
        private ToolStripMenuItem menuItemQuickTimeSetting;
        private ToolStripMenuItem menuItemHour0;
        private ToolStripMenuItem menuItemHour1;
        private ToolStripMenuItem menuItemHour2;
        private ToolStripMenuItem menuItemHour3;
        private ToolStripMenuItem menuItemHour4;
        private ToolStripMenuItem menuItemHour5;
        private ToolStripMenuItem menuItemHour6;
        private ToolStripMenuItem menuItemHour7;
        private ToolStripMenuItem menuItemHour8;
        private ToolStripMenuItem menuItemHour9;
        private ToolStripMenuItem menuItemHour10;
        private ToolStripMenuItem menuItemHour11;
        private ToolStripMenuItem menuItemHour12;
        private ToolStripMenuItem menuItemHour13;
        private ToolStripMenuItem menuItemHour14;
        private ToolStripMenuItem menuItemHour15;
        private ToolStripMenuItem menuItemHour16;
        private ToolStripMenuItem menuItemHour17;
        private ToolStripMenuItem menuItemHour18;
        private ToolStripMenuItem menuItemHour19;
        private ToolStripMenuItem menuItemHour20;
        private ToolStripMenuItem menuItemHour21;
        private ToolStripMenuItem menuItemHour22;
        private ToolStripMenuItem menuItemHour23;
        private ToolStripSeparator toolStripSeparator2;
        private ToolStripMenuItem menuItemTrainMarkup;
        private ToolStripMenuItem menuItemMarkupType;
        private ToolStripMenuItem menuItemMarkupType1;
        private ToolStripMenuItem menuItemMarkupType2;
        private ToolStripMenuItem menuItemMarkupType3;
        private ToolStripMenuItem menuItemMarkupDuplication;
        private ToolStripMenuItem menuItemMarkupFillZero;
        private ToolStripMenuItem menuItemMarkup9999;
        private ToolStripSeparator toolStripSeparator3;
        private ToolStripMenuItem menuItemMarkupNotTrain;
        private ToolStripMenuItem menuItemMarkupSpawned;
        private ToolStripMenuItem menuItemMarkupDelayed;
        private ToolStripMenuItem menuItemMarkupDelayed0;
        private ToolStripMenuItem menuItemMarkupDelayed1;
        private ToolStripMenuItem menuItemMarkupDelayed5;
        private ToolStripMenuItem menuItemMarkupDelayed10;
        private ToolStripMenuItem menuItemMarkupDelayed20;
        private ToolStripMenuItem menuItemMarkupClass;
        private ToolStripMenuItem menuItemMarkupOther;
        private ToolStripSeparator toolStripSeparator4;
        private ToolStripSeparator toolStripSeparator5;
        private ToolStripMenuItem menuItemMarkupAll;
        private ToolStripMenuItem menuItemMarkupCancel;
        private ToolStripSeparator toolStripSeparator6;
    }
}

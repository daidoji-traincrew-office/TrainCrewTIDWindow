using System.Windows.Forms;

namespace TrainCrewTIDWindow.Forms {
    partial class SubWindow {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            components = new System.ComponentModel.Container();
            flowLayoutPanel1 = new FlowLayoutPanel();
            labelClock = new Label();
            labelTopMost = new Label();
            labelScale = new Label();
            pictureBox1 = new PictureBox();
            contextMenuStrip1 = new ContextMenuStrip(components);
            menuItemCopy = new ToolStripMenuItem();
            toolStripSeparator1 = new ToolStripSeparator();
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
            toolStripSeparator6 = new ToolStripSeparator();
            menuItemMarkupSpawned = new ToolStripMenuItem();
            menuItemMarkupAll = new ToolStripMenuItem();
            menuItemMarkupCancel = new ToolStripMenuItem();
            toolStripSeparator4 = new ToolStripSeparator();
            menuItemMarkupType = new ToolStripMenuItem();
            menuItemMarkupType1 = new ToolStripMenuItem();
            menuItemMarkupType2 = new ToolStripMenuItem();
            menuItemMarkupType3 = new ToolStripMenuItem();
            toolStripSeparator2 = new ToolStripSeparator();
            menuItemTopMost = new ToolStripMenuItem();
            menuItemSilent = new ToolStripMenuItem();
            menuItemRename = new ToolStripMenuItem();
            toolStripSeparator3 = new ToolStripSeparator();
            menuItemVersion = new ToolStripMenuItem();
            labelStatus = new Label();
            menuItemMarkupHandover = new ToolStripMenuItem();
            flowLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
            contextMenuStrip1.SuspendLayout();
            SuspendLayout();
            // 
            // flowLayoutPanel1
            // 
            flowLayoutPanel1.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            flowLayoutPanel1.AutoSize = true;
            flowLayoutPanel1.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            flowLayoutPanel1.BackColor = Color.Transparent;
            flowLayoutPanel1.Controls.Add(labelClock);
            flowLayoutPanel1.Controls.Add(labelTopMost);
            flowLayoutPanel1.Controls.Add(labelScale);
            flowLayoutPanel1.FlowDirection = FlowDirection.RightToLeft;
            flowLayoutPanel1.Location = new Point(549, 3);
            flowLayoutPanel1.Name = "flowLayoutPanel1";
            flowLayoutPanel1.Size = new Size(235, 14);
            flowLayoutPanel1.TabIndex = 8;
            flowLayoutPanel1.WrapContents = false;
            // 
            // labelClock
            // 
            labelClock.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            labelClock.AutoSize = true;
            labelClock.BackColor = Color.Transparent;
            labelClock.Font = new Font("ＭＳ ゴシック", 9F, FontStyle.Regular, GraphicsUnit.Point, 128);
            labelClock.ForeColor = Color.White;
            labelClock.Location = new Point(177, 0);
            labelClock.Name = "labelClock";
            labelClock.Padding = new Padding(1);
            labelClock.Size = new Size(55, 14);
            labelClock.TabIndex = 3;
            labelClock.Text = "00:00:00";
            labelClock.TextAlign = ContentAlignment.TopRight;
            // 
            // labelTopMost
            // 
            labelTopMost.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            labelTopMost.AutoSize = true;
            labelTopMost.BackColor = Color.FromArgb(30, 30, 30);
            labelTopMost.Cursor = Cursors.Hand;
            labelTopMost.Font = new Font("ＭＳ ゴシック", 9F, FontStyle.Regular, GraphicsUnit.Point, 128);
            labelTopMost.ForeColor = Color.Gray;
            labelTopMost.Location = new Point(90, 0);
            labelTopMost.Name = "labelTopMost";
            labelTopMost.Padding = new Padding(5, 1, 5, 1);
            labelTopMost.Size = new Size(81, 14);
            labelTopMost.TabIndex = 4;
            labelTopMost.Text = "最前面：OFF";
            labelTopMost.Click += labelTopMost_Click;
            labelTopMost.MouseLeave += labelTopMost_Leave;
            labelTopMost.MouseHover += labelTopMost_Hover;
            // 
            // labelScale
            // 
            labelScale.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            labelScale.AutoSize = true;
            labelScale.BackColor = Color.Transparent;
            labelScale.Font = new Font("ＭＳ ゴシック", 9F, FontStyle.Regular, GraphicsUnit.Point, 128);
            labelScale.ForeColor = Color.White;
            labelScale.Location = new Point(3, 0);
            labelScale.Name = "labelScale";
            labelScale.Padding = new Padding(5, 1, 5, 1);
            labelScale.Size = new Size(81, 14);
            labelScale.TabIndex = 5;
            labelScale.Text = "Scale：100%";
            labelScale.TextAlign = ContentAlignment.TopRight;
            labelScale.MouseDown += labelScale_MouseDown;
            // 
            // pictureBox1
            // 
            pictureBox1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            pictureBox1.BackColor = Color.Black;
            pictureBox1.ContextMenuStrip = contextMenuStrip1;
            pictureBox1.Cursor = Cursors.SizeAll;
            pictureBox1.Location = new Point(0, 24);
            pictureBox1.Name = "pictureBox1";
            pictureBox1.Size = new Size(784, 437);
            pictureBox1.TabIndex = 9;
            pictureBox1.TabStop = false;
            pictureBox1.MouseDown += PictureBox1_MouseDown;
            pictureBox1.MouseMove += PictureBox1_MouseMove;
            pictureBox1.MouseUp += PictureBox1_MouseUp;
            pictureBox1.MouseWheel += PictureBox1_MouseWheel;
            // 
            // contextMenuStrip1
            // 
            contextMenuStrip1.Items.AddRange(new ToolStripItem[] { menuItemCopy, toolStripSeparator1, menuItemTrainMarkup, menuItemMarkupType, toolStripSeparator2, menuItemTopMost, menuItemSilent, menuItemRename, toolStripSeparator3, menuItemVersion });
            contextMenuStrip1.Name = "contextMenuStrip1";
            contextMenuStrip1.Size = new Size(181, 198);
            // 
            // menuItemCopy
            // 
            menuItemCopy.Name = "menuItemCopy";
            menuItemCopy.Size = new Size(180, 22);
            menuItemCopy.Text = "TID画面をコピー";
            menuItemCopy.Click += menuItemCopy_Click;
            // 
            // toolStripSeparator1
            // 
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new Size(177, 6);
            // 
            // menuItemTrainMarkup
            // 
            menuItemTrainMarkup.DropDownItems.AddRange(new ToolStripItem[] { menuItemMarkupClass, menuItemMarkupDelayed, menuItemMarkupOther, menuItemMarkupAll, menuItemMarkupCancel, toolStripSeparator4 });
            menuItemTrainMarkup.Name = "menuItemTrainMarkup";
            menuItemTrainMarkup.Size = new Size(180, 22);
            menuItemTrainMarkup.Text = "列車番号強調表示";
            // 
            // menuItemMarkupClass
            // 
            menuItemMarkupClass.Name = "menuItemMarkupClass";
            menuItemMarkupClass.Size = new Size(180, 22);
            menuItemMarkupClass.Text = "列車種別";
            // 
            // menuItemMarkupDelayed
            // 
            menuItemMarkupDelayed.DropDownItems.AddRange(new ToolStripItem[] { menuItemMarkupDelayed0, toolStripSeparator5, menuItemMarkupDelayed1, menuItemMarkupDelayed5, menuItemMarkupDelayed10, menuItemMarkupDelayed20 });
            menuItemMarkupDelayed.Name = "menuItemMarkupDelayed";
            menuItemMarkupDelayed.Size = new Size(180, 22);
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
            menuItemMarkupOther.DropDownItems.AddRange(new ToolStripItem[] { menuItemMarkupDuplication, menuItemMarkupFillZero, menuItemMarkup9999, menuItemMarkupNotTrain, toolStripSeparator6, menuItemMarkupSpawned, menuItemMarkupHandover });
            menuItemMarkupOther.Name = "menuItemMarkupOther";
            menuItemMarkupOther.Size = new Size(180, 22);
            menuItemMarkupOther.Text = "その他";
            // 
            // menuItemMarkupDuplication
            // 
            menuItemMarkupDuplication.Name = "menuItemMarkupDuplication";
            menuItemMarkupDuplication.Size = new Size(180, 22);
            menuItemMarkupDuplication.Text = "重複列車番号";
            menuItemMarkupDuplication.Click += menuItemMarkupDuplication_Click;
            // 
            // menuItemMarkupFillZero
            // 
            menuItemMarkupFillZero.Name = "menuItemMarkupFillZero";
            menuItemMarkupFillZero.Size = new Size(180, 22);
            menuItemMarkupFillZero.Text = "ゼロ埋め列車番号";
            menuItemMarkupFillZero.Click += menuItemMarkupFillZero_Click;
            // 
            // menuItemMarkup9999
            // 
            menuItemMarkup9999.Name = "menuItemMarkup9999";
            menuItemMarkup9999.Size = new Size(180, 22);
            menuItemMarkup9999.Text = "9999";
            menuItemMarkup9999.Click += menuItemMarkup9999_Click;
            // 
            // menuItemMarkupNotTrain
            // 
            menuItemMarkupNotTrain.Name = "menuItemMarkupNotTrain";
            menuItemMarkupNotTrain.Size = new Size(180, 22);
            menuItemMarkupNotTrain.Text = "列車以外";
            menuItemMarkupNotTrain.Click += menuItemMarkupNotTrain_Click;
            // 
            // toolStripSeparator6
            // 
            toolStripSeparator6.Name = "toolStripSeparator6";
            toolStripSeparator6.Size = new Size(177, 6);
            // 
            // menuItemMarkupSpawned
            // 
            menuItemMarkupSpawned.Name = "menuItemMarkupSpawned";
            menuItemMarkupSpawned.Size = new Size(180, 22);
            menuItemMarkupSpawned.Text = "新規スポーン";
            menuItemMarkupSpawned.Click += menuItemMarkupSpawned_Click;
            // 
            // menuItemMarkupAll
            // 
            menuItemMarkupAll.Name = "menuItemMarkupAll";
            menuItemMarkupAll.Size = new Size(180, 22);
            menuItemMarkupAll.Text = "列番全選択";
            menuItemMarkupAll.Click += menuItemMarkupAll_Click;
            // 
            // menuItemMarkupCancel
            // 
            menuItemMarkupCancel.Name = "menuItemMarkupCancel";
            menuItemMarkupCancel.Size = new Size(180, 22);
            menuItemMarkupCancel.Text = "列番全解除";
            menuItemMarkupCancel.Click += menuItemMarkupCancel_Click;
            // 
            // toolStripSeparator4
            // 
            toolStripSeparator4.Name = "toolStripSeparator4";
            toolStripSeparator4.Size = new Size(177, 6);
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
            // toolStripSeparator2
            // 
            toolStripSeparator2.Name = "toolStripSeparator2";
            toolStripSeparator2.Size = new Size(177, 6);
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
            // menuItemRename
            // 
            menuItemRename.Name = "menuItemRename";
            menuItemRename.Size = new Size(180, 22);
            menuItemRename.Text = "ウィンドウ名の変更";
            menuItemRename.Click += menuItemRename_Click;
            // 
            // toolStripSeparator3
            // 
            toolStripSeparator3.Name = "toolStripSeparator3";
            toolStripSeparator3.Size = new Size(177, 6);
            // 
            // menuItemVersion
            // 
            menuItemVersion.Name = "menuItemVersion";
            menuItemVersion.Size = new Size(180, 22);
            menuItemVersion.Text = "バージョン情報";
            menuItemVersion.Click += menuItemVersion_Click;
            // 
            // labelStatus
            // 
            labelStatus.AutoSize = true;
            labelStatus.BackColor = Color.Transparent;
            labelStatus.Font = new Font("ＭＳ ゴシック", 9F, FontStyle.Regular, GraphicsUnit.Point, 128);
            labelStatus.ForeColor = Color.White;
            labelStatus.Location = new Point(3, 3);
            labelStatus.Name = "labelStatus";
            labelStatus.Padding = new Padding(0, 1, 0, 1);
            labelStatus.Size = new Size(11, 14);
            labelStatus.TabIndex = 10;
            labelStatus.Text = " ";
            // 
            // menuItemMarkupHandover
            // 
            menuItemMarkupHandover.Name = "menuItemMarkupHandover";
            menuItemMarkupHandover.Size = new Size(180, 22);
            menuItemMarkupHandover.Text = "同一運番に引継ぎ";
            menuItemMarkupHandover.Click += menuItemMarkupHandover_Click;
            // 
            // SubWindow
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(20, 20, 20);
            ClientSize = new Size(784, 461);
            Controls.Add(labelStatus);
            Controls.Add(pictureBox1);
            Controls.Add(flowLayoutPanel1);
            MaximizeBox = false;
            Name = "SubWindow";
            Text = "TID | TID - ダイヤ運転会";
            FormClosing += SubWindow_Closing;
            ResizeBegin += SubWindow_ResizeBegin;
            ResizeEnd += SubWindow_ResizeEnd;
            KeyDown += SubWindow_KeyDown;
            KeyUp += SubWindow_KeyUp;
            Resize += SubWindow_Resize;
            flowLayoutPanel1.ResumeLayout(false);
            flowLayoutPanel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
            contextMenuStrip1.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private FlowLayoutPanel flowLayoutPanel1;
        private Label labelClock;
        private Label labelTopMost;
        private Label labelScale;
        private PictureBox pictureBox1;
        private ContextMenuStrip contextMenuStrip1;
        private ToolStripMenuItem menuItemCopy;
        private ToolStripMenuItem menuItemTopMost;
        private ToolStripMenuItem menuItemRename;
        private ToolStripSeparator toolStripSeparator1;
        private ToolStripSeparator toolStripSeparator2;
        private ToolStripMenuItem menuItemTrainMarkup;
        private ToolStripMenuItem menuItemMarkupType;
        private ToolStripMenuItem menuItemMarkupType1;
        private ToolStripMenuItem menuItemMarkupType2;
        private ToolStripMenuItem menuItemMarkupType3;
        private ToolStripMenuItem menuItemMarkupDuplication;
        private ToolStripMenuItem menuItemMarkupFillZero;
        private ToolStripMenuItem menuItemMarkup9999;
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
        private ToolStripMenuItem menuItemMarkupAll;
        private ToolStripMenuItem menuItemMarkupCancel;
        private ToolStripSeparator toolStripSeparator3;
        private ToolStripSeparator toolStripSeparator4;
        private ToolStripSeparator toolStripSeparator5;
        private ToolStripSeparator toolStripSeparator6;
        private ToolStripMenuItem menuItemVersion;
        private ToolStripMenuItem menuItemSilent;
        private Label labelStatus;
        private ToolStripMenuItem menuItemMarkupHandover;
    }
}
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
            menuItemScale = new ToolStripMenuItem();
            menuItemScale50 = new ToolStripMenuItem();
            menuItemScale75 = new ToolStripMenuItem();
            menuItemScale90 = new ToolStripMenuItem();
            menuItemScale100 = new ToolStripMenuItem();
            menuItemScale110 = new ToolStripMenuItem();
            menuItemScale125 = new ToolStripMenuItem();
            menuItemScale150 = new ToolStripMenuItem();
            menuItemScale175 = new ToolStripMenuItem();
            menuItemScale200 = new ToolStripMenuItem();
            menuItemScaleFit = new ToolStripMenuItem();
            panel1 = new Panel();
            labelStatus = new Label();
            labelClock = new Label();
            labelTopMost = new Label();
            labelScale = new Label();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
            contextMenuStrip1.SuspendLayout();
            panel1.SuspendLayout();
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
            contextMenuStrip1.Items.AddRange(new ToolStripItem[] { menuItemCopy, menuItemScale });
            contextMenuStrip1.Name = "contextMenuStrip1";
            contextMenuStrip1.Size = new Size(181, 70);
            // 
            // menuItemCopy
            // 
            menuItemCopy.Name = "menuItemCopy";
            menuItemCopy.Size = new Size(180, 22);
            menuItemCopy.Text = "TID画面をコピー";
            menuItemCopy.Click += menuItemCopy_Click;
            // 
            // menuItemScale
            // 
            menuItemScale.DropDownItems.AddRange(new ToolStripItem[] { menuItemScale50, menuItemScale75, menuItemScale90, menuItemScale100, menuItemScale110, menuItemScale125, menuItemScale150, menuItemScale175, menuItemScale200, menuItemScaleFit });
            menuItemScale.Name = "menuItemScale";
            menuItemScale.Size = new Size(180, 22);
            menuItemScale.Text = "拡大率";
            // 
            // menuItemScale50
            // 
            menuItemScale50.Name = "menuItemScale50";
            menuItemScale50.Size = new Size(180, 22);
            menuItemScale50.Text = "50%";
            // 
            // menuItemScale75
            // 
            menuItemScale75.Name = "menuItemScale75";
            menuItemScale75.Size = new Size(180, 22);
            menuItemScale75.Text = "75%";
            // 
            // menuItemScale90
            // 
            menuItemScale90.Name = "menuItemScale90";
            menuItemScale90.Size = new Size(180, 22);
            menuItemScale90.Text = "90%";
            // 
            // menuItemScale100
            // 
            menuItemScale100.Name = "menuItemScale100";
            menuItemScale100.Size = new Size(180, 22);
            menuItemScale100.Text = "100%（現在）";
            // 
            // menuItemScale110
            // 
            menuItemScale110.Name = "menuItemScale110";
            menuItemScale110.Size = new Size(180, 22);
            menuItemScale110.Text = "110%";
            // 
            // menuItemScale125
            // 
            menuItemScale125.Name = "menuItemScale125";
            menuItemScale125.Size = new Size(180, 22);
            menuItemScale125.Text = "125%";
            // 
            // menuItemScale150
            // 
            menuItemScale150.Name = "menuItemScale150";
            menuItemScale150.Size = new Size(180, 22);
            menuItemScale150.Text = "150%";
            // 
            // menuItemScale175
            // 
            menuItemScale175.Name = "menuItemScale175";
            menuItemScale175.Size = new Size(180, 22);
            menuItemScale175.Text = "175%";
            // 
            // menuItemScale200
            // 
            menuItemScale200.Name = "menuItemScale200";
            menuItemScale200.Size = new Size(180, 22);
            menuItemScale200.Text = "200%";
            // 
            // menuItemScaleFit
            // 
            menuItemScaleFit.Name = "menuItemScaleFit";
            menuItemScaleFit.Size = new Size(180, 22);
            menuItemScaleFit.Text = "フィット表示";
            // 
            // panel1
            // 
            panel1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            panel1.AutoScroll = true;
            panel1.Controls.Add(pictureBox1);
            panel1.Location = new Point(0, 24);
            panel1.Margin = new Padding(0);
            panel1.Name = "panel1";
            panel1.Size = new Size(984, 537);
            panel1.TabIndex = 2;
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
            // TIDWindow
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            AutoScroll = true;
            BackColor = Color.FromArgb(5, 5, 5);
            ClientSize = new Size(984, 561);
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
            KeyDown += TIDWindow_KeyDown;
            Resize += TIDWindow_Resize;
            ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
            contextMenuStrip1.ResumeLayout(false);
            panel1.ResumeLayout(false);
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
        private ToolStripMenuItem menuItemScale50;
        private ToolStripMenuItem menuItemScale75;
        private ToolStripMenuItem menuItemScale100;
        private ToolStripMenuItem menuItemScale125;
        private ToolStripMenuItem menuItemScale150;
        private ToolStripMenuItem menuItemScale175;
        private ToolStripMenuItem menuItemScale200;
        private ToolStripMenuItem menuItemScaleFit;
        private Label labelScale;
        private ToolStripMenuItem menuItemScale90;
        private ToolStripMenuItem menuItemScale110;
    }
}

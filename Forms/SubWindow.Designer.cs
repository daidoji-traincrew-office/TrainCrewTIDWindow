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
            menuItemTopMost = new ToolStripMenuItem();
            menuItemRename = new ToolStripMenuItem();
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
            // 
            // contextMenuStrip1
            // 
            contextMenuStrip1.Items.AddRange(new ToolStripItem[] { menuItemCopy, menuItemTopMost, menuItemRename });
            contextMenuStrip1.Name = "contextMenuStrip1";
            contextMenuStrip1.Size = new Size(181, 92);
            // 
            // menuItemCopy
            // 
            menuItemCopy.Name = "menuItemCopy";
            menuItemCopy.Size = new Size(180, 22);
            menuItemCopy.Text = "TID画面をコピー";
            menuItemCopy.Click += menuItemCopy_Click;
            // 
            // menuItemTopMost
            // 
            menuItemTopMost.Name = "menuItemTopMost";
            menuItemTopMost.Size = new Size(180, 22);
            menuItemTopMost.Text = "最前面表示";
            menuItemTopMost.Click += menuItemTopMost_Click;
            // 
            // menuItemRename
            // 
            menuItemRename.Name = "menuItemRename";
            menuItemRename.Size = new Size(180, 22);
            menuItemRename.Text = "ウィンドウ名の変更";
            menuItemRename.Click += menuItemRename_Click;
            // 
            // SubWindow
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(20, 20, 20);
            ClientSize = new Size(784, 461);
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
    }
}
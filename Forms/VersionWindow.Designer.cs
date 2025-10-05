namespace TrainCrewTIDWindow.Forms {
    partial class VersionWindow {
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
            tableLayoutPanel1 = new TableLayoutPanel();
            icon = new PictureBox();
            labelVersion = new Label();
            labelCredit1 = new Label();
            labelCredit2 = new Label();
            tableLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)icon).BeginInit();
            SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            tableLayoutPanel1.ColumnCount = 2;
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 55F));
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 45F));
            tableLayoutPanel1.Controls.Add(icon, 0, 0);
            tableLayoutPanel1.Controls.Add(labelVersion, 0, 1);
            tableLayoutPanel1.Controls.Add(labelCredit1, 0, 3);
            tableLayoutPanel1.Controls.Add(labelCredit2, 1, 3);
            tableLayoutPanel1.Location = new Point(0, 0);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 4;
            tableLayoutPanel1.RowStyles.Add(new RowStyle());
            tableLayoutPanel1.RowStyles.Add(new RowStyle());
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 50F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle());
            tableLayoutPanel1.Size = new Size(284, 421);
            tableLayoutPanel1.TabIndex = 0;
            // 
            // icon
            // 
            icon.Anchor = AnchorStyles.None;
            tableLayoutPanel1.SetColumnSpan(icon, 2);
            icon.Location = new Point(92, 33);
            icon.Margin = new Padding(3, 33, 3, 3);
            icon.Name = "icon";
            icon.Size = new Size(100, 50);
            icon.TabIndex = 0;
            icon.TabStop = false;
            // 
            // labelVersion
            // 
            labelVersion.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            labelVersion.AutoSize = true;
            labelVersion.BackColor = SystemColors.Control;
            tableLayoutPanel1.SetColumnSpan(labelVersion, 2);
            labelVersion.Location = new Point(3, 86);
            labelVersion.Name = "labelVersion";
            labelVersion.Size = new Size(278, 15);
            labelVersion.TabIndex = 1;
            labelVersion.Text = "label1";
            labelVersion.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // labelCredit1
            // 
            labelCredit1.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            labelCredit1.AutoSize = true;
            labelCredit1.ImageAlign = ContentAlignment.TopLeft;
            labelCredit1.Location = new Point(55, 151);
            labelCredit1.Name = "labelCredit1";
            labelCredit1.Size = new Size(98, 105);
            labelCredit1.TabIndex = 2;
            labelCredit1.Text = "メインプログラム\r\nプログラム協力\r\n\r\n画面設計・デザイン\r\nアイコンデザイン\r\n\r\n監修";
            // 
            // labelCredit2
            // 
            labelCredit2.AutoSize = true;
            labelCredit2.Location = new Point(159, 151);
            labelCredit2.Name = "labelCredit2";
            labelCredit2.Size = new Size(70, 105);
            labelCredit2.TabIndex = 3;
            labelCredit2.Text = "鍋立山 儀明\r\nケシゴモン\r\n匠手 津道\r\n匠手 津道\r\nHortensia\r\n\r\n匠手 津道";
            // 
            // VersionWindow
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(284, 421);
            Controls.Add(tableLayoutPanel1);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "VersionWindow";
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = "バージョン情報 | TID - ダイヤ運転会";
            tableLayoutPanel1.ResumeLayout(false);
            tableLayoutPanel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)icon).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private TableLayoutPanel tableLayoutPanel1;
        private PictureBox icon;
        private Label labelVersion;
        private Label labelCredit1;
        private Label labelCredit2;
    }
}
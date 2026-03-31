namespace TrainCrewTIDWindow.Forms {
    partial class NavigationWindow {
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
            tabControl1 = new TabControl();
            tabVolume = new TabPage();
            labelVolumeWarning = new Label();
            buttonMuteWarning = new Button();
            trackBarVolumeWarning = new TrackBar();
            labelVolumeMaster = new Label();
            buttonMuteMaster = new Button();
            trackBarVolumeMaster = new TrackBar();
            toolTip1 = new ToolTip(components);
            checkBoxTopMost = new CheckBox();
            tabControl1.SuspendLayout();
            tabVolume.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)trackBarVolumeWarning).BeginInit();
            ((System.ComponentModel.ISupportInitialize)trackBarVolumeMaster).BeginInit();
            SuspendLayout();
            // 
            // tabControl1
            // 
            tabControl1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            tabControl1.Controls.Add(tabVolume);
            tabControl1.Location = new Point(0, 24);
            tabControl1.Name = "tabControl1";
            tabControl1.SelectedIndex = 0;
            tabControl1.Size = new Size(384, 537);
            tabControl1.TabIndex = 0;
            // 
            // tabVolume
            // 
            tabVolume.AutoScroll = true;
            tabVolume.BackColor = SystemColors.Control;
            tabVolume.Controls.Add(labelVolumeWarning);
            tabVolume.Controls.Add(buttonMuteWarning);
            tabVolume.Controls.Add(trackBarVolumeWarning);
            tabVolume.Controls.Add(labelVolumeMaster);
            tabVolume.Controls.Add(buttonMuteMaster);
            tabVolume.Controls.Add(trackBarVolumeMaster);
            tabVolume.Location = new Point(4, 24);
            tabVolume.Name = "tabVolume";
            tabVolume.Padding = new Padding(3);
            tabVolume.Size = new Size(376, 509);
            tabVolume.TabIndex = 3;
            tabVolume.Text = "音量設定";
            // 
            // labelVolumeWarning
            // 
            labelVolumeWarning.AutoSize = true;
            labelVolumeWarning.ForeColor = Color.Black;
            labelVolumeWarning.Location = new Point(30, 105);
            labelVolumeWarning.Name = "labelVolumeWarning";
            labelVolumeWarning.Size = new Size(74, 15);
            labelVolumeWarning.TabIndex = 5;
            labelVolumeWarning.Text = "警告・エラー音";
            // 
            // buttonMuteWarning
            // 
            buttonMuteWarning.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            buttonMuteWarning.Location = new Point(275, 98);
            buttonMuteWarning.Name = "buttonMuteWarning";
            buttonMuteWarning.Size = new Size(70, 28);
            buttonMuteWarning.TabIndex = 4;
            buttonMuteWarning.Text = "ミュート";
            buttonMuteWarning.UseVisualStyleBackColor = true;
            buttonMuteWarning.Click += buttonMuteWarning_Click;
            // 
            // trackBarVolumeWarning
            // 
            trackBarVolumeWarning.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            trackBarVolumeWarning.LargeChange = 2;
            trackBarVolumeWarning.Location = new Point(25, 130);
            trackBarVolumeWarning.Maximum = 100;
            trackBarVolumeWarning.Name = "trackBarVolumeWarning";
            trackBarVolumeWarning.Size = new Size(327, 45);
            trackBarVolumeWarning.TabIndex = 3;
            trackBarVolumeWarning.TickFrequency = 5;
            toolTip1.SetToolTip(trackBarVolumeWarning, "100");
            trackBarVolumeWarning.Value = 100;
            trackBarVolumeWarning.ValueChanged += trackBarVolumeWarning_ValueChanged;
            trackBarVolumeWarning.MouseUp += trackBarVolumeWarning_MouseUp;
            // 
            // labelVolumeMaster
            // 
            labelVolumeMaster.AutoSize = true;
            labelVolumeMaster.ForeColor = Color.Black;
            labelVolumeMaster.Location = new Point(30, 25);
            labelVolumeMaster.Name = "labelVolumeMaster";
            labelVolumeMaster.Size = new Size(66, 15);
            labelVolumeMaster.TabIndex = 2;
            labelVolumeMaster.Text = "マスター音量";
            // 
            // buttonMuteMaster
            // 
            buttonMuteMaster.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            buttonMuteMaster.Location = new Point(275, 18);
            buttonMuteMaster.Name = "buttonMuteMaster";
            buttonMuteMaster.Size = new Size(70, 28);
            buttonMuteMaster.TabIndex = 1;
            buttonMuteMaster.Text = "ミュート";
            buttonMuteMaster.UseVisualStyleBackColor = true;
            buttonMuteMaster.Click += buttonMuteMaster_Click;
            // 
            // trackBarVolumeMaster
            // 
            trackBarVolumeMaster.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            trackBarVolumeMaster.LargeChange = 2;
            trackBarVolumeMaster.Location = new Point(25, 50);
            trackBarVolumeMaster.Maximum = 100;
            trackBarVolumeMaster.Name = "trackBarVolumeMaster";
            trackBarVolumeMaster.Size = new Size(327, 45);
            trackBarVolumeMaster.TabIndex = 0;
            trackBarVolumeMaster.TickFrequency = 5;
            toolTip1.SetToolTip(trackBarVolumeMaster, "100");
            trackBarVolumeMaster.Value = 100;
            trackBarVolumeMaster.ValueChanged += trackBarVolumeMaster_ValueChanged;
            trackBarVolumeMaster.MouseUp += trackBarVolumeMaster_MouseUp;
            // 
            // toolTip1
            // 
            toolTip1.AutoPopDelay = 5000;
            toolTip1.InitialDelay = 100;
            toolTip1.ReshowDelay = 100;
            // 
            // checkBoxTopMost
            // 
            checkBoxTopMost.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            checkBoxTopMost.AutoSize = true;
            checkBoxTopMost.Location = new Point(320, 1);
            checkBoxTopMost.Name = "checkBoxTopMost";
            checkBoxTopMost.Size = new Size(62, 19);
            checkBoxTopMost.TabIndex = 1;
            checkBoxTopMost.Text = "最前面";
            checkBoxTopMost.UseVisualStyleBackColor = true;
            checkBoxTopMost.CheckedChanged += checkBoxTopMost_CheckedChanged;
            // 
            // NavigationWindow
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(384, 561);
            Controls.Add(checkBoxTopMost);
            Controls.Add(tabControl1);
            MaximizeBox = false;
            MaximumSize = new Size(1000, 800);
            MinimumSize = new Size(200, 200);
            Name = "NavigationWindow";
            StartPosition = FormStartPosition.CenterParent;
            Text = "ナビゲーション | TID - ダイヤ運転会";
            FormClosing += NavigationWindow_Closing;
            tabControl1.ResumeLayout(false);
            tabVolume.ResumeLayout(false);
            tabVolume.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)trackBarVolumeWarning).EndInit();
            ((System.ComponentModel.ISupportInitialize)trackBarVolumeMaster).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private TabControl tabControl1;
        private TabPage tabVolume;
        private TrackBar trackBarVolumeMaster;
        private ToolTip toolTip1;
        private Button buttonMuteMaster;
        private Label labelVolumeMaster;
        private Label labelVolumeWarning;
        private Button buttonMuteWarning;
        private TrackBar trackBarVolumeWarning;
        private CheckBox checkBoxTopMost;

    }
}
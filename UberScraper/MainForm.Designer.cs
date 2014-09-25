namespace UberScraper {
    partial class MainForm {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose( bool disposing ) {
            if ( disposing && ( components != null ) ) {
                components.Dispose();
            }
            base.Dispose( disposing );
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            this.components = new System.ComponentModel.Container();
            this.webBrowser1 = new Awesomium.Windows.Forms.WebControl(this.components);
            this.labelBrowserSource = new System.Windows.Forms.Label();
            this.labelNavigationStatus = new System.Windows.Forms.Label();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.checkBoxAIBrainDotOrg = new System.Windows.Forms.CheckBox();
            this.checkBoxBitChestDotMe = new System.Windows.Forms.CheckBox();
            this.checkBoxVisitLandOfBitcoin = new System.Windows.Forms.CheckBox();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.numericThrottle = new System.Windows.Forms.NumericUpDown();
            this.label1 = new System.Windows.Forms.Label();
            this.pictureBoxChallenge = new System.Windows.Forms.PictureBox();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.buttonSolveChallenge = new System.Windows.Forms.Button();
            this.buttonStart = new System.Windows.Forms.Button();
            this.buttonStop = new System.Windows.Forms.Button();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.tabPage2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericThrottle)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxChallenge)).BeginInit();
            this.SuspendLayout();
            // 
            // webBrowser1
            // 
            this.webBrowser1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.webBrowser1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.webBrowser1.Location = new System.Drawing.Point(612, 58);
            this.webBrowser1.Size = new System.Drawing.Size(929, 471);
            this.webBrowser1.TabIndex = 0;
            this.webBrowser1.AddressChanged += new Awesomium.Core.UrlEventHandler(this.Awesomium_Windows_Forms_WebControl_AddressChanged);
            this.webBrowser1.DocumentReady += new Awesomium.Core.UrlEventHandler(this.Awesomium_Windows_Forms_WebControl_DocumentReady);
            this.webBrowser1.LoadingFrame += new Awesomium.Core.LoadingFrameEventHandler(this.Awesomium_Windows_Forms_WebControl_LoadingFrame);
            this.webBrowser1.LoadingFrameComplete += new Awesomium.Core.FrameEventHandler(this.Awesomium_Windows_Forms_WebControl_LoadingFrameComplete);
            this.webBrowser1.Crashed += new Awesomium.Core.CrashedEventHandler(this.Awesomium_Windows_Forms_WebControl_Crashed);
            // 
            // labelBrowserSource
            // 
            this.labelBrowserSource.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.labelBrowserSource.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.labelBrowserSource.Location = new System.Drawing.Point(612, 9);
            this.labelBrowserSource.Name = "labelBrowserSource";
            this.labelBrowserSource.Size = new System.Drawing.Size(929, 23);
            this.labelBrowserSource.TabIndex = 1;
            this.labelBrowserSource.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // labelNavigationStatus
            // 
            this.labelNavigationStatus.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.labelNavigationStatus.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.labelNavigationStatus.Location = new System.Drawing.Point(612, 32);
            this.labelNavigationStatus.Name = "labelNavigationStatus";
            this.labelNavigationStatus.Size = new System.Drawing.Size(929, 23);
            this.labelNavigationStatus.TabIndex = 2;
            this.labelNavigationStatus.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Location = new System.Drawing.Point(164, 12);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(146, 102);
            this.tabControl1.TabIndex = 3;
            // 
            // tabPage1
            // 
            this.tabPage1.AutoScroll = true;
            this.tabPage1.Controls.Add(this.checkBoxAIBrainDotOrg);
            this.tabPage1.Controls.Add(this.checkBoxBitChestDotMe);
            this.tabPage1.Controls.Add(this.checkBoxVisitLandOfBitcoin);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(138, 76);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Visit";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // checkBoxAIBrainDotOrg
            // 
            this.checkBoxAIBrainDotOrg.AutoSize = true;
            this.checkBoxAIBrainDotOrg.Checked = global::UberScraper.Properties.Settings.Default.checkedAIBrainDotOrg;
            this.checkBoxAIBrainDotOrg.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxAIBrainDotOrg.DataBindings.Add(new System.Windows.Forms.Binding("Checked", global::UberScraper.Properties.Settings.Default, "checkedAIBrainDotOrg", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.checkBoxAIBrainDotOrg.Location = new System.Drawing.Point(6, 6);
            this.checkBoxAIBrainDotOrg.Name = "checkBoxAIBrainDotOrg";
            this.checkBoxAIBrainDotOrg.Size = new System.Drawing.Size(60, 17);
            this.checkBoxAIBrainDotOrg.TabIndex = 2;
            this.checkBoxAIBrainDotOrg.Text = "AIBrain";
            this.checkBoxAIBrainDotOrg.UseVisualStyleBackColor = true;
            // 
            // checkBoxBitChestDotMe
            // 
            this.checkBoxBitChestDotMe.AutoSize = true;
            this.checkBoxBitChestDotMe.Checked = global::UberScraper.Properties.Settings.Default.checkedBitChest;
            this.checkBoxBitChestDotMe.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxBitChestDotMe.DataBindings.Add(new System.Windows.Forms.Binding("Checked", global::UberScraper.Properties.Settings.Default, "checkedBitChest", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.checkBoxBitChestDotMe.Location = new System.Drawing.Point(6, 29);
            this.checkBoxBitChestDotMe.Name = "checkBoxBitChestDotMe";
            this.checkBoxBitChestDotMe.Size = new System.Drawing.Size(68, 17);
            this.checkBoxBitChestDotMe.TabIndex = 1;
            this.checkBoxBitChestDotMe.Text = "Bit Chest";
            this.checkBoxBitChestDotMe.UseVisualStyleBackColor = true;
            // 
            // checkBoxVisitLandOfBitcoin
            // 
            this.checkBoxVisitLandOfBitcoin.AutoSize = true;
            this.checkBoxVisitLandOfBitcoin.Checked = global::UberScraper.Properties.Settings.Default.checkedLandOfBitcoin;
            this.checkBoxVisitLandOfBitcoin.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxVisitLandOfBitcoin.DataBindings.Add(new System.Windows.Forms.Binding("Checked", global::UberScraper.Properties.Settings.Default, "checkedLandOfBitcoin", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.checkBoxVisitLandOfBitcoin.Location = new System.Drawing.Point(6, 52);
            this.checkBoxVisitLandOfBitcoin.Name = "checkBoxVisitLandOfBitcoin";
            this.checkBoxVisitLandOfBitcoin.Size = new System.Drawing.Size(99, 17);
            this.checkBoxVisitLandOfBitcoin.TabIndex = 0;
            this.checkBoxVisitLandOfBitcoin.Text = "Land Of Bitcoin";
            this.checkBoxVisitLandOfBitcoin.UseVisualStyleBackColor = true;
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.numericThrottle);
            this.tabPage2.Controls.Add(this.label1);
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(332, 255);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Options";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // numericThrottle
            // 
            this.numericThrottle.DataBindings.Add(new System.Windows.Forms.Binding("Value", global::UberScraper.Properties.Settings.Default, "numericThrottle", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.numericThrottle.DecimalPlaces = 1;
            this.numericThrottle.Location = new System.Drawing.Point(106, 6);
            this.numericThrottle.Maximum = new decimal(new int[] {
            1000000000,
            0,
            0,
            0});
            this.numericThrottle.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            131072});
            this.numericThrottle.Name = "numericThrottle";
            this.numericThrottle.Size = new System.Drawing.Size(69, 20);
            this.numericThrottle.TabIndex = 2;
            this.numericThrottle.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.numericThrottle.ThousandsSeparator = true;
            this.numericThrottle.Value = global::UberScraper.Properties.Settings.Default.numericThrottle;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 8);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(94, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Throttle (Seconds)";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // pictureBoxChallenge
            // 
            this.pictureBoxChallenge.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pictureBoxChallenge.Location = new System.Drawing.Point(12, 120);
            this.pictureBoxChallenge.Name = "pictureBoxChallenge";
            this.pictureBoxChallenge.Size = new System.Drawing.Size(594, 382);
            this.pictureBoxChallenge.TabIndex = 4;
            this.pictureBoxChallenge.TabStop = false;
            this.pictureBoxChallenge.WaitOnLoad = true;
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(12, 508);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(237, 20);
            this.textBox1.TabIndex = 5;
            // 
            // buttonSolveChallenge
            // 
            this.buttonSolveChallenge.Location = new System.Drawing.Point(255, 506);
            this.buttonSolveChallenge.Name = "buttonSolveChallenge";
            this.buttonSolveChallenge.Size = new System.Drawing.Size(97, 23);
            this.buttonSolveChallenge.TabIndex = 6;
            this.buttonSolveChallenge.Text = "Solve";
            this.buttonSolveChallenge.UseVisualStyleBackColor = true;
            // 
            // buttonStart
            // 
            this.buttonStart.Location = new System.Drawing.Point(12, 12);
            this.buttonStart.Name = "buttonStart";
            this.buttonStart.Size = new System.Drawing.Size(70, 23);
            this.buttonStart.TabIndex = 7;
            this.buttonStart.Text = "Start";
            this.buttonStart.UseVisualStyleBackColor = true;
            this.buttonStart.Click += new System.EventHandler(this.buttonStart_Click);
            // 
            // buttonStop
            // 
            this.buttonStop.Location = new System.Drawing.Point(88, 12);
            this.buttonStop.Name = "buttonStop";
            this.buttonStop.Size = new System.Drawing.Size(70, 23);
            this.buttonStop.TabIndex = 8;
            this.buttonStop.Text = "Stop";
            this.buttonStop.UseVisualStyleBackColor = true;
            this.buttonStop.Visible = false;
            this.buttonStop.Click += new System.EventHandler(this.buttonStop_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1553, 541);
            this.Controls.Add(this.buttonStop);
            this.Controls.Add(this.buttonStart);
            this.Controls.Add(this.buttonSolveChallenge);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.pictureBoxChallenge);
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.labelNavigationStatus);
            this.Controls.Add(this.labelBrowserSource);
            this.Controls.Add(this.webBrowser1);
            this.DataBindings.Add(new System.Windows.Forms.Binding("Location", global::UberScraper.Properties.Settings.Default, "MainFormLocation", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.Location = global::UberScraper.Properties.Settings.Default.MainFormLocation;
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Main Form";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.MainForm_FormClosed);
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.Shown += new System.EventHandler(this.MainForm_Shown);
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage1.PerformLayout();
            this.tabPage2.ResumeLayout(false);
            this.tabPage2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericThrottle)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxChallenge)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private Awesomium.Windows.Forms.WebControl webBrowser1;
        private System.Windows.Forms.Label labelBrowserSource;
        private System.Windows.Forms.Label labelNavigationStatus;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.CheckBox checkBoxVisitLandOfBitcoin;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.NumericUpDown numericThrottle;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.CheckBox checkBoxAIBrainDotOrg;
        private System.Windows.Forms.CheckBox checkBoxBitChestDotMe;
        private System.Windows.Forms.PictureBox pictureBoxChallenge;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Button buttonSolveChallenge;
        private System.Windows.Forms.Button buttonStart;
        private System.Windows.Forms.Button buttonStop;
    }
}


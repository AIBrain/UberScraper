namespace UberScraper {
    using System.ComponentModel;
    using System.Windows.Forms;
    using Awesomium.Windows.Forms;

    partial class MainForm {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private IContainer components = null;

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
			this.pictureBoxChallenge = new System.Windows.Forms.PictureBox();
			this.textBox1 = new System.Windows.Forms.TextBox();
			this.buttonSolveChallenge = new System.Windows.Forms.Button();
			this.buttonStop = new System.Windows.Forms.Button();
			this.buttonSiteEditor = new System.Windows.Forms.Button();
			this.tabControls = new System.Windows.Forms.TabControl();
			((System.ComponentModel.ISupportInitialize)(this.pictureBoxChallenge)).BeginInit();
			this.SuspendLayout();
			// 
			// pictureBoxChallenge
			// 
			this.pictureBoxChallenge.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.pictureBoxChallenge.Location = new System.Drawing.Point(12, 363);
			this.pictureBoxChallenge.Name = "pictureBoxChallenge";
			this.pictureBoxChallenge.Size = new System.Drawing.Size(402, 139);
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
			// buttonStop
			// 
			this.buttonStop.Location = new System.Drawing.Point(434, 506);
			this.buttonStop.Name = "buttonStop";
			this.buttonStop.Size = new System.Drawing.Size(70, 23);
			this.buttonStop.TabIndex = 8;
			this.buttonStop.Text = "Stop";
			this.buttonStop.UseVisualStyleBackColor = true;
			this.buttonStop.Visible = false;
			this.buttonStop.Click += new System.EventHandler(this.buttonStop_Click);
			// 
			// buttonSiteEditor
			// 
			this.buttonSiteEditor.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
			this.buttonSiteEditor.Location = new System.Drawing.Point(510, 506);
			this.buttonSiteEditor.Name = "buttonSiteEditor";
			this.buttonSiteEditor.Size = new System.Drawing.Size(70, 23);
			this.buttonSiteEditor.TabIndex = 9;
			this.buttonSiteEditor.Text = "Sites";
			this.buttonSiteEditor.UseVisualStyleBackColor = true;
			this.buttonSiteEditor.Click += new System.EventHandler(this.buttonSiteEditor_Click);
			// 
			// tabControls
			// 
			this.tabControls.Location = new System.Drawing.Point(612, 12);
			this.tabControls.Name = "tabControls";
			this.tabControls.SelectedIndex = 0;
			this.tabControls.Size = new System.Drawing.Size(929, 517);
			this.tabControls.TabIndex = 10;
			// 
			// MainForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(1553, 541);
			this.Controls.Add(this.tabControls);
			this.Controls.Add(this.buttonSiteEditor);
			this.Controls.Add(this.buttonStop);
			this.Controls.Add(this.buttonSolveChallenge);
			this.Controls.Add(this.textBox1);
			this.Controls.Add(this.pictureBoxChallenge);
			this.DataBindings.Add(new System.Windows.Forms.Binding("Location", global::UberScraper.Properties.Settings.Default, "MainFormLocation", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
			this.DoubleBuffered = true;
			this.Location = global::UberScraper.Properties.Settings.Default.MainFormLocation;
			this.Name = "MainForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Main Form";
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
			this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.MainForm_FormClosed);
			this.Shown += new System.EventHandler(this.MainForm_Shown);
			((System.ComponentModel.ISupportInitialize)(this.pictureBoxChallenge)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

        }

        #endregion
        private PictureBox pictureBoxChallenge;
        private TextBox textBox1;
        private Button buttonSolveChallenge;
        private Button buttonStop;
        private Button buttonSiteEditor;
        private TabControl tabControls;
    }
}


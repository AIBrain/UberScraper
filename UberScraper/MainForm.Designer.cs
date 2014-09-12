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
            this.webBrowser = new Awesomium.Windows.Forms.WebControl(this.components);
            this.labelBrowserSource = new System.Windows.Forms.Label();
            this.labelNavigationStatus = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // webBrowser
            // 
            this.webBrowser.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.webBrowser.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.webBrowser.Location = new System.Drawing.Point(12, 58);
            this.webBrowser.Size = new System.Drawing.Size(1068, 471);
            this.webBrowser.TabIndex = 0;
            this.webBrowser.AddressChanged += new Awesomium.Core.UrlEventHandler(this.Awesomium_Windows_Forms_WebControl_AddressChanged);
            this.webBrowser.DocumentReady += new Awesomium.Core.UrlEventHandler(this.Awesomium_Windows_Forms_WebControl_DocumentReady);
            this.webBrowser.LoadingFrame += new Awesomium.Core.LoadingFrameEventHandler(this.Awesomium_Windows_Forms_WebControl_LoadingFrame);
            this.webBrowser.LoadingFrameComplete += new Awesomium.Core.FrameEventHandler(this.Awesomium_Windows_Forms_WebControl_LoadingFrameComplete);
            this.webBrowser.Crashed += new Awesomium.Core.CrashedEventHandler(this.Awesomium_Windows_Forms_WebControl_Crashed);
            // 
            // labelBrowserSource
            // 
            this.labelBrowserSource.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.labelBrowserSource.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.labelBrowserSource.Location = new System.Drawing.Point(12, 9);
            this.labelBrowserSource.Name = "labelBrowserSource";
            this.labelBrowserSource.Size = new System.Drawing.Size(1068, 23);
            this.labelBrowserSource.TabIndex = 1;
            this.labelBrowserSource.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // labelNavigationStatus
            // 
            this.labelNavigationStatus.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.labelNavigationStatus.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.labelNavigationStatus.Location = new System.Drawing.Point(12, 32);
            this.labelNavigationStatus.Name = "labelNavigationStatus";
            this.labelNavigationStatus.Size = new System.Drawing.Size(1068, 23);
            this.labelNavigationStatus.TabIndex = 2;
            this.labelNavigationStatus.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1092, 541);
            this.Controls.Add(this.labelNavigationStatus);
            this.Controls.Add(this.labelBrowserSource);
            this.Controls.Add(this.webBrowser);
            this.Name = "MainForm";
            this.Text = "Form1";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.Shown += new System.EventHandler(this.MainForm_Shown);
            this.ResumeLayout(false);

        }

        #endregion

        private Awesomium.Windows.Forms.WebControl webBrowser;
        private System.Windows.Forms.Label labelBrowserSource;
        private System.Windows.Forms.Label labelNavigationStatus;
    }
}


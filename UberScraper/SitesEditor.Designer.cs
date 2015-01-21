namespace UberScraper {
    partial class SitesEditor {
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
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.SiteName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Wait = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.UserName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Password = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.LastAttempt = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.NextAttempt = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Address = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.sitesDatabaseDataSetBindingSource = new System.Windows.Forms.BindingSource(this.components);
            this.sitesDatabaseDataSet = new UberScraper.SitesDatabaseDataSet();
            this.button1 = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.sitesDatabaseDataSetBindingSource)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.sitesDatabaseDataSet)).BeginInit();
            this.SuspendLayout();
            // 
            // dataGridView1
            // 
            this.dataGridView1.AllowUserToOrderColumns = true;
            this.dataGridView1.AutoGenerateColumns = false;
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView1.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.SiteName,
            this.Wait,
            this.UserName,
            this.Password,
            this.LastAttempt,
            this.NextAttempt,
            this.Address});
            this.dataGridView1.DataSource = this.sitesDatabaseDataSet;
            this.dataGridView1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataGridView1.Location = new System.Drawing.Point(0, 0);
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.Size = new System.Drawing.Size(934, 376);
            this.dataGridView1.TabIndex = 0;
            // 
            // SiteName
            // 
            this.SiteName.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.SiteName.HeaderText = "Name";
            this.SiteName.Name = "SiteName";
            this.SiteName.ToolTipText = "Name of the website";
            this.SiteName.Width = 60;
            // 
            // Wait
            // 
            this.Wait.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.Wait.HeaderText = "Wait";
            this.Wait.Name = "Wait";
            this.Wait.Width = 54;
            // 
            // UserName
            // 
            this.UserName.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.UserName.HeaderText = "User Name";
            this.UserName.Name = "UserName";
            this.UserName.Width = 85;
            // 
            // Password
            // 
            this.Password.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.ColumnHeader;
            this.Password.HeaderText = "Password";
            this.Password.Name = "Password";
            this.Password.Width = 78;
            // 
            // LastAttempt
            // 
            this.LastAttempt.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.LastAttempt.HeaderText = "Last Attempt";
            this.LastAttempt.Name = "LastAttempt";
            this.LastAttempt.Width = 91;
            // 
            // NextAttempt
            // 
            this.NextAttempt.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.NextAttempt.HeaderText = "Next Attempt";
            this.NextAttempt.Name = "NextAttempt";
            this.NextAttempt.Width = 93;
            // 
            // Address
            // 
            this.Address.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.DisplayedCells;
            this.Address.HeaderText = "Address";
            this.Address.Name = "Address";
            this.Address.Width = 70;
            // 
            // sitesDatabaseDataSetBindingSource
            // 
            this.sitesDatabaseDataSetBindingSource.DataSource = this.sitesDatabaseDataSet;
            this.sitesDatabaseDataSetBindingSource.Position = 0;
            // 
            // sitesDatabaseDataSet
            // 
            this.sitesDatabaseDataSet.DataSetName = "SitesDatabaseDataSet";
            this.sitesDatabaseDataSet.SchemaSerializationMode = System.Data.SchemaSerializationMode.IncludeSchema;
            // 
            // button1
            // 
            this.button1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.button1.Location = new System.Drawing.Point(0, 353);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(934, 23);
            this.button1.TabIndex = 1;
            this.button1.Text = "Done";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // SitesEditor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoScroll = true;
            this.ClientSize = new System.Drawing.Size(934, 376);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.dataGridView1);
            this.DoubleBuffered = true;
            this.Name = "SitesEditor";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Sites Editor";
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.sitesDatabaseDataSetBindingSource)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.sitesDatabaseDataSet)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.DataGridView dataGridView1;
        private System.Windows.Forms.BindingSource sitesDatabaseDataSetBindingSource;
        private SitesDatabaseDataSet sitesDatabaseDataSet;
        private System.Windows.Forms.DataGridViewTextBoxColumn SiteName;
        private System.Windows.Forms.DataGridViewTextBoxColumn Wait;
        private System.Windows.Forms.DataGridViewTextBoxColumn UserName;
        private System.Windows.Forms.DataGridViewTextBoxColumn Password;
        private System.Windows.Forms.DataGridViewTextBoxColumn LastAttempt;
        private System.Windows.Forms.DataGridViewTextBoxColumn NextAttempt;
        private System.Windows.Forms.DataGridViewTextBoxColumn Address;
        private System.Windows.Forms.Button button1;
    }
}
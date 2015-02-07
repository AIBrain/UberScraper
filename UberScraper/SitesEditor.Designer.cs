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
			this.buttonDone = new System.Windows.Forms.Button();
			this.buttonCancel = new System.Windows.Forms.Button();
			this.dataGridViewMain = new System.Windows.Forms.DataGridView();
			this.dataGridViewTextBoxColumn1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.dataGridViewTextBoxColumn2 = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.userNameDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.passwordDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.addressDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.lastAttemptDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.siteEditorDataTableBindingSource = new System.Windows.Forms.BindingSource(this.components);
			this.gvDatabaseDataSetBindingSource = new System.Windows.Forms.BindingSource(this.components);
			this.gvDatabaseDataSet = new UberScraper.gvDatabaseDataSet();
			this.labelStatus = new System.Windows.Forms.Label();
			this.progressBarMain = new System.Windows.Forms.ProgressBar();
			this.buttonSave = new System.Windows.Forms.Button();
			((System.ComponentModel.ISupportInitialize)(this.dataGridViewMain)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.siteEditorDataTableBindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.gvDatabaseDataSetBindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.gvDatabaseDataSet)).BeginInit();
			this.SuspendLayout();
			// 
			// buttonDone
			// 
			this.buttonDone.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonDone.Location = new System.Drawing.Point(870, 341);
			this.buttonDone.Name = "buttonDone";
			this.buttonDone.Size = new System.Drawing.Size(52, 23);
			this.buttonDone.TabIndex = 1;
			this.buttonDone.Text = "Close";
			this.buttonDone.UseVisualStyleBackColor = true;
			this.buttonDone.Click += new System.EventHandler(this.buttonDone_Click);
			// 
			// buttonCancel
			// 
			this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonCancel.Location = new System.Drawing.Point(812, 341);
			this.buttonCancel.Name = "buttonCancel";
			this.buttonCancel.Size = new System.Drawing.Size(52, 23);
			this.buttonCancel.TabIndex = 2;
			this.buttonCancel.Text = "Cancel";
			this.buttonCancel.UseVisualStyleBackColor = true;
			this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
			// 
			// dataGridViewMain
			// 
			this.dataGridViewMain.AllowUserToOrderColumns = true;
			this.dataGridViewMain.AutoGenerateColumns = false;
			this.dataGridViewMain.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			this.dataGridViewMain.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.dataGridViewTextBoxColumn1,
            this.dataGridViewTextBoxColumn2,
            this.userNameDataGridViewTextBoxColumn,
            this.passwordDataGridViewTextBoxColumn,
            this.addressDataGridViewTextBoxColumn,
            this.lastAttemptDataGridViewTextBoxColumn});
			this.dataGridViewMain.DataSource = this.siteEditorDataTableBindingSource;
			this.dataGridViewMain.Location = new System.Drawing.Point(12, 12);
			this.dataGridViewMain.Name = "dataGridViewMain";
			this.dataGridViewMain.Size = new System.Drawing.Size(910, 323);
			this.dataGridViewMain.TabIndex = 0;
			this.dataGridViewMain.CellContextMenuStripNeeded += new System.Windows.Forms.DataGridViewCellContextMenuStripNeededEventHandler(this.dataGridViewMain_CellContextMenuStripNeeded);
			this.dataGridViewMain.CellMouseDown += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.dataGridViewMain_CellMouseDown);
			// 
			// dataGridViewTextBoxColumn1
			// 
			this.dataGridViewTextBoxColumn1.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
			this.dataGridViewTextBoxColumn1.DataPropertyName = "Name";
			this.dataGridViewTextBoxColumn1.HeaderText = "Name";
			this.dataGridViewTextBoxColumn1.Name = "dataGridViewTextBoxColumn1";
			this.dataGridViewTextBoxColumn1.Width = 60;
			// 
			// dataGridViewTextBoxColumn2
			// 
			this.dataGridViewTextBoxColumn2.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
			this.dataGridViewTextBoxColumn2.DataPropertyName = "Wait";
			this.dataGridViewTextBoxColumn2.HeaderText = "Wait";
			this.dataGridViewTextBoxColumn2.Name = "dataGridViewTextBoxColumn2";
			this.dataGridViewTextBoxColumn2.Width = 54;
			// 
			// userNameDataGridViewTextBoxColumn
			// 
			this.userNameDataGridViewTextBoxColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
			this.userNameDataGridViewTextBoxColumn.DataPropertyName = "User Name";
			this.userNameDataGridViewTextBoxColumn.HeaderText = "User Name";
			this.userNameDataGridViewTextBoxColumn.Name = "userNameDataGridViewTextBoxColumn";
			this.userNameDataGridViewTextBoxColumn.Width = 85;
			// 
			// passwordDataGridViewTextBoxColumn
			// 
			this.passwordDataGridViewTextBoxColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
			this.passwordDataGridViewTextBoxColumn.DataPropertyName = "Password";
			this.passwordDataGridViewTextBoxColumn.HeaderText = "Password";
			this.passwordDataGridViewTextBoxColumn.Name = "passwordDataGridViewTextBoxColumn";
			this.passwordDataGridViewTextBoxColumn.Width = 78;
			// 
			// addressDataGridViewTextBoxColumn
			// 
			this.addressDataGridViewTextBoxColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.DisplayedCells;
			this.addressDataGridViewTextBoxColumn.DataPropertyName = "Address";
			this.addressDataGridViewTextBoxColumn.HeaderText = "Address";
			this.addressDataGridViewTextBoxColumn.Name = "addressDataGridViewTextBoxColumn";
			this.addressDataGridViewTextBoxColumn.Width = 70;
			// 
			// lastAttemptDataGridViewTextBoxColumn
			// 
			this.lastAttemptDataGridViewTextBoxColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
			this.lastAttemptDataGridViewTextBoxColumn.DataPropertyName = "LastAttempt";
			this.lastAttemptDataGridViewTextBoxColumn.HeaderText = "LastAttempt";
			this.lastAttemptDataGridViewTextBoxColumn.Name = "lastAttemptDataGridViewTextBoxColumn";
			this.lastAttemptDataGridViewTextBoxColumn.Width = 88;
			// 
			// siteEditorDataTableBindingSource
			// 
			this.siteEditorDataTableBindingSource.DataMember = "SiteEditorDataTable";
			this.siteEditorDataTableBindingSource.DataSource = this.gvDatabaseDataSetBindingSource;
			// 
			// gvDatabaseDataSetBindingSource
			// 
			this.gvDatabaseDataSetBindingSource.DataSource = this.gvDatabaseDataSet;
			this.gvDatabaseDataSetBindingSource.Position = 0;
			// 
			// gvDatabaseDataSet
			// 
			this.gvDatabaseDataSet.DataSetName = "gvDatabaseDataSet";
			this.gvDatabaseDataSet.SchemaSerializationMode = System.Data.SchemaSerializationMode.IncludeSchema;
			// 
			// labelStatus
			// 
			this.labelStatus.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.labelStatus.Location = new System.Drawing.Point(197, 341);
			this.labelStatus.Name = "labelStatus";
			this.labelStatus.Size = new System.Drawing.Size(551, 23);
			this.labelStatus.TabIndex = 3;
			this.labelStatus.Text = "Status";
			this.labelStatus.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// progressBarMain
			// 
			this.progressBarMain.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.progressBarMain.Location = new System.Drawing.Point(12, 341);
			this.progressBarMain.Name = "progressBarMain";
			this.progressBarMain.Size = new System.Drawing.Size(179, 23);
			this.progressBarMain.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
			this.progressBarMain.TabIndex = 4;
			// 
			// buttonSave
			// 
			this.buttonSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonSave.Location = new System.Drawing.Point(754, 341);
			this.buttonSave.Name = "buttonSave";
			this.buttonSave.Size = new System.Drawing.Size(52, 23);
			this.buttonSave.TabIndex = 5;
			this.buttonSave.Text = "Save";
			this.buttonSave.UseVisualStyleBackColor = true;
			this.buttonSave.Click += new System.EventHandler(this.buttonSave_Click);
			// 
			// SitesEditor
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.AutoScroll = true;
			this.ClientSize = new System.Drawing.Size(934, 376);
			this.Controls.Add(this.dataGridViewMain);
			this.Controls.Add(this.buttonSave);
			this.Controls.Add(this.progressBarMain);
			this.Controls.Add(this.labelStatus);
			this.Controls.Add(this.buttonCancel);
			this.Controls.Add(this.buttonDone);
			this.DoubleBuffered = true;
			this.Name = "SitesEditor";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Sites Editor";
			this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.SitesEditor_FormClosed);
			this.Shown += new System.EventHandler(this.SitesEditor_Shown);
			((System.ComponentModel.ISupportInitialize)(this.dataGridViewMain)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.siteEditorDataTableBindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.gvDatabaseDataSetBindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.gvDatabaseDataSet)).EndInit();
			this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.DataGridView dataGridViewMain;
        private System.Windows.Forms.Button buttonDone;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Label labelStatus;
        private System.Windows.Forms.ProgressBar progressBarMain;
        private System.Windows.Forms.BindingSource gvDatabaseDataSetBindingSource;
        private gvDatabaseDataSet gvDatabaseDataSet;
        private System.Windows.Forms.BindingSource siteEditorDataTableBindingSource;
        private System.Windows.Forms.Button buttonSave;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn1;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn2;
        private System.Windows.Forms.DataGridViewTextBoxColumn userNameDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn passwordDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn addressDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn lastAttemptDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn nextAttemptDataGridViewTextBoxColumn;
    }
}
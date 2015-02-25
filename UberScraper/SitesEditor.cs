// This notice must be kept visible in the source.
// 
// This section of source code belongs to Rick@AIBrain.Org unless otherwise specified, or the
// original license has been overwritten by the automatic formatting of this code. Any unmodified
// sections of source code borrowed from other projects retain their original license and thanks
// goes to the Authors.
// 
// Donations and Royalties can be paid via
// PayPal: paypal@aibrain.org
// bitcoin: 1Mad8TxTqxKnMiHuZxArFvX8BuFEB9nqX2
// bitcoin: 1NzEsF7eegeEWDr5Vr9sSSgtUC4aL6axJu
// litecoin: LeUxdU2w3o6pLZGVys5xpDZvvo8DUrjBp9
// 
// Usage of the source code or compiled binaries is AS-IS. I am not responsible for Anything You Do.
// 
// Contact me by email if you have any questions or helpful criticism.
// 
// "UberScraper 2015/SitesEditor.cs" was last cleaned by Rick on 2015/01/22 at 6:03 AM

namespace UberScraper {
	using System;
	using System.Collections.Generic;
	using System.Data;
	using System.Drawing;
	using System.Linq;
	using System.Runtime.Serialization;
	using System.Threading;
	using System.Threading.Tasks;
	using System.Windows.Forms;
	using System.Xml;
	using JetBrains.Annotations;
	using Librainian.Controls;
	using Librainian.IO;
	using Librainian.Measurement.Time;
	using Librainian.Threading;

	[DataContract]
	public partial class SitesEditor : Form {

		public const String ConfigFileName = "Sites.xml";

		private readonly Folder _dataFolder = new Folder( Environment.SpecialFolder.LocalApplicationData, null, null, "siteEditor" );

		public SitesEditor( MainForm mainForm ) {
			this.MainForm = mainForm;
			InitializeComponent();
		}

		[CanBeNull]
		public MainForm MainForm { get; }

		public class SiteData {
			public String Name;
			public int Wait;
			public String UserName;
			public String Password;
			public Uri Address;
			public DateTime? LastSuccess;
			public DateTime? LastAttempt;
		}

        /// <summary>
        /// Name, Wait, UserName, Password, Address, LastAttempt, IdealNextAttempt
        /// </summary>
		public IEnumerable<SiteData> Data => Enumerable.Select( this.gvDatabaseDataSet.Tables[ 0 ].AsEnumerable(), row => new SiteData {
			Name = row.Field<String>( "Name" ),
			Wait = row.Field<int>( "Wait" ),
			UserName = row.Field<String>( "UserName" ),
			Password = row.Field<String>( "Password" ),
			Address = row.Field<Uri>( "Address" ),
			LastSuccess = row.Field<DateTime>( "LastSuccess" ),
			LastAttempt = row.Field<DateTime>( "LastAttempt" ),
		} );

		private readonly SemaphoreSlim _access = new SemaphoreSlim( initialCount: 1, maxCount: 1 );

		public void LoadData() {
			if ( !this._access.Wait( Seconds.Ten ) ) {
				return;
			}
			try {
				"Loading sites data...".WriteLine();
				this.dataGridViewMain.SelectAll();
				this.dataGridViewMain.ClearSelection();
				//this.progressBarMain.Usable( true );
				//this.progressBarMain.Values( 0, 1, 2 );
				//this.progressBarMain.Style( ProgressBarStyle.Marquee );

				if ( this._dataFolder.Exists() ) {
					var document = new Document( this._dataFolder, ConfigFileName );
					if ( document.Exists() ) {
						//this.progressBarMain.Usable( true );
						//this.progressBarMain.Values( 0, 500, 1000 );
						//this.progressBarMain.Style( ProgressBarStyle.Continuous );

						var size = new Document( document.FullPathWithFileName ).Size;
						if ( size.HasValue ) {
							//this.progressBarMain.Values( 0, 0, ( int ) size );
						}

						using (var reader = XmlReader.Create( document.FullPathWithFileName )) {
							//TODO using ( var ps = new ProgressStream( reader ) ) { }
							this.gvDatabaseDataSet.ReadXml( reader );
						}

						if ( size.HasValue ) {
							//this.progressBarMain.Values( 0, ( int ) size, ( int ) size );
						}

						this.OnThread( () => this.siteEditorDataTableBindingSource.ResetBindings( false ) ); //also adjusts the column header widths

						//this.progressBarMain.Usable( false );
					}
				}
				"Done loading sites data".WriteLine();
			}
			finally {
				this._access.Release();
			}
		}

		public void SaveData() {
			if ( !this._access.Wait( Seconds.Ten ) ) {
				return;
			}
			try {
				"Saving sites data".WriteLine();
				try {
					this._dataFolder.Create();

					var document = new Document( this._dataFolder, ConfigFileName );

					using (var writer = XmlWriter.Create( document.FullPathWithFileName )) {
						this.gvDatabaseDataSet.RemotingFormat = SerializationFormat.Xml;
						this.gvDatabaseDataSet.WriteXml( writer, XmlWriteMode.WriteSchema );
					}
				}
				catch ( XmlException exception ) {
					exception.More();
				}
				"Done saving sites data".WriteLine();
			}
			finally {
				this._access.Release();
			}

		}

		/*
				private void buttonCancel_Click( Object sender, EventArgs e ) {
					this.DialogResult = DialogResult.Cancel;
					this.Close();
				}
		*/

		/*
				private async void buttonDone_Click( Object sender, EventArgs e ) {
					this.DialogResult = DialogResult.OK;
					await Task.Run( () => this.SaveData() );
					this.Close();
				}
		*/

		private async void buttonSave_Click( Object sender, EventArgs e ) => await Task.Run( () => this.SaveData() );

		private void dataGridViewMain_CellContextMenuStripNeeded( Object sender, DataGridViewCellContextMenuStripNeededEventArgs e ) {
			if ( e.ColumnIndex == -1 || e.RowIndex == -1 ) {
				return;
			}
			var contextMenu = new ContextMenu();
			var cell = this.dataGridViewMain[ e.ColumnIndex, e.RowIndex ];

			if ( cell.ValueType == typeof(DateTime) ) {
				var menuItem = new MenuItem( "Insert Now" );
				menuItem.Click += ( o, args ) => cell.Value = DateTime.Now;

				contextMenu.MenuItems.Add( menuItem );

				var relativeMousePosition = dataGridViewMain.PointToClient( Cursor.Position );
				contextMenu.Show( dataGridViewMain, new Point( relativeMousePosition.X, relativeMousePosition.Y ) );
			}
		}

		private void dataGridViewMain_CellMouseDown( Object sender, DataGridViewCellMouseEventArgs e ) {
			var cell = sender as DataGridView;
			if ( null == cell ) {
				return;
			}
			if ( e.ColumnIndex == -1 || e.RowIndex == -1 || e.Button != MouseButtons.Right ) {
				return;
			}
			var c = cell[ e.ColumnIndex, e.RowIndex ];
			if ( c.Selected ) {
				return;
			}
			c.DataGridView.ClearSelection();
			c.DataGridView.CurrentCell = c;
			c.Selected = true;
		}

		private void SitesEditor_FormClosed( Object sender, FormClosedEventArgs e ) {
			var form = this.MainForm;
			if ( form != null ) {
				form.SitesEditor = null;
			}
		}

		private async void SitesEditor_Shown( Object sender, EventArgs e ) => await Task.Run( () => this.LoadData() );

	}
}
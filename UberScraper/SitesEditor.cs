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
	using System.Data;
	using System.Drawing;
	using System.Runtime.Serialization;
	using System.Threading.Tasks;
	using System.Windows.Forms;
	using System.Xml;
	using JetBrains.Annotations;
	using Librainian.Controls;
	using Librainian.IO;
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
        public MainForm MainForm { get; set; }

        public async void LoadData() {
            this.dataGridViewMain.SelectAll();
            this.dataGridViewMain.ClearSelection();
            this.progressBarMain.Usable( true );
            this.progressBarMain.Values( 0, 1, 2 );
            this.progressBarMain.Style( ProgressBarStyle.Marquee );

            await Task.Run( () => {
                if ( !this._dataFolder.Exists() ) {
                    return;
                }

                var document = new Document( this._dataFolder, ConfigFileName );
                if ( !document.Exists() ) {
                    return;
                }

                this.progressBarMain.Usable( true );
                this.progressBarMain.Values( 0, 500, 1000 );
                this.progressBarMain.Style( ProgressBarStyle.Continuous );

                var size = new Document( document.FullPathWithFileName ).Size;
                if ( size.HasValue ) {
                    this.progressBarMain.Values( 0, 0, ( int )size );
                }

                using (var reader = XmlReader.Create( document.FullPathWithFileName )) {

                    //TODO using ( var ps = new ProgressStream( reader ) ) { }
                    this.gvDatabaseDataSet.ReadXml( reader );
                }

                if ( size.HasValue ) {
                    this.progressBarMain.Values( 0, ( int )size, ( int )size );
                }

                this.OnThread( () => this.siteEditorDataTableBindingSource.ResetBindings( false ) ); //also adjusts the column header widths
            } );
            this.progressBarMain.Usable( false );
        }

        public Boolean SaveData() {
            try {
                this._dataFolder.Create();

                var document = new Document( this._dataFolder, ConfigFileName );

                using (var writer = XmlWriter.Create( document.FullPathWithFileName )) {
                    this.gvDatabaseDataSet.RemotingFormat = SerializationFormat.Xml;
                    this.gvDatabaseDataSet.WriteXml( writer, XmlWriteMode.WriteSchema );

                    return true;
                }
            }
            catch ( XmlException exception ) {
                exception.More();
            }
            return false;
        }

        private void buttonCancel_Click( Object sender, EventArgs e ) {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private async void buttonDone_Click( Object sender, EventArgs e ) {
            this.DialogResult = DialogResult.OK;
            await Task.Run( () => { this.SaveData(); } );
            this.Close();
        }

        private async void buttonSave_Click( Object sender, EventArgs e ) {
            this.DialogResult = DialogResult.OK;
            await Task.Run( () => { this.SaveData(); } );
        }

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

        private async void SitesEditor_Shown( Object sender, EventArgs e ) {
            await Task.Run( () => this.LoadData() );

            //this.Invalidate( this.ClientRectangle, true );
            //this.dataGridViewMain.Refresh();
            //this.Refresh();
        }
    }
}
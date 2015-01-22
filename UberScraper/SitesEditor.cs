namespace UberScraper {
    using System;
    using System.Data;
    using System.Runtime.Serialization;
    using System.Threading.Tasks;
    using System.Windows.Forms;
    using System.Xml;
    using JetBrains.Annotations;
    using Librainian.Controls;
    using Librainian.IO;
    using Librainian.Threading;
    using Newtonsoft.Json;

    [DataContract]
    public partial class SitesEditor : Form {

        [CanBeNull]
        public MainForm MainForm { get; set; }

        public SitesEditor( MainForm mainForm ) {
            this.MainForm = mainForm;
            InitializeComponent();
        }

        private void buttonDone_Click( Object sender, EventArgs e ) {
            this.buttonSave.Push();
            this.Close();
        }

        private void buttonSave_Click( Object sender, EventArgs e ) {
            this.DialogResult = DialogResult.OK;
            this.SaveData();
        }

        private void buttonCancel_Click( Object sender, EventArgs e ) {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private async void SitesEditor_Shown( Object sender, EventArgs e ) {
            await Task.Run( () => this.LoadData() );
            //this.Invalidate( this.ClientRectangle, true );
            //this.dataGridViewMain.Refresh();
            //this.Refresh();
        }

        public const String ConfigFileName = "Sites.xml";

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

                this.OnThread( () => this.siteEditorDataTableBindingSource.ResetBindings( false ) );    //also adjusts the column headers
            } );
            this.progressBarMain.Usable( false );
        }

        private readonly Folder _dataFolder = new Folder( Environment.SpecialFolder.LocalApplicationData, null, null, "siteEditor" );

        public Boolean SaveData() {

            try {
                this._dataFolder.Create();

                var document = new Document( this._dataFolder, ConfigFileName );

                using (var writer = XmlWriter.Create( document.FullPathWithFileName )) {

                    this.gvDatabaseDataSet.RemotingFormat = SerializationFormat.Xml;
                    this.gvDatabaseDataSet.WriteXml( writer );

                    return true;
                }

            }
            catch ( JsonSerializationException exception ) {
                exception.More();
            }
            return false;
        }

        private void SitesEditor_FormClosed( Object sender, FormClosedEventArgs e ) {
            var form = this.MainForm;
            if ( form != null ) {
                form.SitesEditor = null;
            }
        }
    }
}

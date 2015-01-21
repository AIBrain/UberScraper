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

        [ CanBeNull ]
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
            this.Invalidate( this.ClientRectangle, true );
            this.dataGridViewMain.Refresh();
            this.Refresh();
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

                //foreach ( var data in File.ReadAllLines( document.FullPathWithFileName ) ) {
                //    //var row = JsonConvert.DeserializeObject<DataGridViewRow>( data, _jSettings );
                //    this.dataGridViewMain.Rows.Add( row );
                //    this.progressBarMain.Value( ++line );
                //}
            } );
            this.progressBarMain.Usable( false );
        }

        private readonly Folder _dataFolder = new Folder( Environment.SpecialFolder.LocalApplicationData, null, null, "siteEditor" );

        /*
                private readonly JsonSerializerSettings _jSettings = new JsonSerializerSettings {
                    NullValueHandling = NullValueHandling.Ignore,
                    ObjectCreationHandling = ObjectCreationHandling.Replace,
                    MissingMemberHandling = MissingMemberHandling.Ignore,
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                };
        */

        //public static void GetClipboardData( Action<Object> onGetData ) {
        //    var t = new Thread( () => {
        //        var data = Clipboard.GetData( DataFormats.Serializable );
        //        if ( onGetData != null ) {
        //            onGetData( data );
        //        }
        //    } );
        //    t.SetApartmentState( ApartmentState.STA );
        //    t.Start();
        //    t.Join();
        //}

        //public static void SetClipboardData( Object data ) {
        //    var t = new Thread( () => Clipboard.SetDataObject( data ) );
        //    t.SetApartmentState( ApartmentState.STA );
        //    t.Start();
        //    t.Join();
        //}

        public Boolean SaveData() {

            try {
                this._dataFolder.Create();

                var document = new Document( this._dataFolder, ConfigFileName );

                //dataGridViewMain.AllowUserToAddRows = false;
                //dataGridViewMain.RowHeadersVisible = false;
                //dataGridViewMain.ClipboardCopyMode = DataGridViewClipboardCopyMode.EnableAlwaysIncludeHeaderText;// Choose whether to write header. You will want to do this for a CSV file.
                //dataGridViewMain.SelectAll(); // Select the cells we want to serialize.

                //var data = dataGridViewMain.GetClipboardContent();
                //dataGridViewMain.GetClipboardContent

                //if ( data != null ) {
                //    SetClipboardData( data );
                //    var text = Clipboard.GetText( TextDataFormat.CommaSeparatedValue );
                //    File.WriteAllText( document.FullPathWithFileName, text );
                //}

                //if ( oldClipboard != null ) {
                //    Clipboard.SetDataObject( oldClipboard );  // Restore the current state of the clipboard so the effect is seamless
                //}


                using (var writer = XmlWriter.Create( document.FullPathWithFileName )) {
                    // If we want to serialize the columns we could do this:
                    //var data = JsonConvert.SerializeObject( this.dataGridViewMain.Columns, Formatting.Indented, jSettings );
                    //streamWriter.WriteLine( data );

                    //var serializer = new NetDataContractSerializer();
                    this.gvDatabaseDataSet.RemotingFormat = SerializationFormat.Xml;
                    this.gvDatabaseDataSet.WriteXml( writer );
                    //serializer.WriteObject( streamWriter, data );

                    //foreach ( DataGridViewRow row in this.dataGridViewMain.Rows ) {
                    //    foreach ( DataGridViewCell cell in row.Cells ) {
                    //        var data = JsonConvert.SerializeObject( cell, Formatting.Indented, this._jSettings );
                    //        streamWriter.WriteLine( data );
                    //    }
                    //    streamWriter.WriteLine( Environment.NewLine );
                    //}

                    return true;
                }

            }
            catch ( JsonSerializationException exception ) {
                exception.More();
            }
            return false;
        }

        private void SitesEditor_FormClosed( Object sender, FormClosedEventArgs e ) {
            this.MainForm.SitesEditor = null;
        }
    }
}

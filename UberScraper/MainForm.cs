namespace UberScraper {

    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows.Forms;
    using Awesomium.Core;
    using FluentAssertions;
    using JetBrains.Annotations;
    using Librainian.Controls;
    using Librainian.Measurement.Time;
    using Librainian.Threading;
    using Properties;

    public partial class MainForm : Form {

        public MainForm() {
            AwesomiumContext = SynchronizationContext.Current;
            this.InitializeComponent();
            AwesomiumContext.Should().NotBeNull();
        }

        public SynchronizationContext AwesomiumContext {
            get;
            private set;
        }

        [NotNull]
        protected readonly CancellationTokenSource CancellationTokenSource = new CancellationTokenSource();

        [CanBeNull]
        public Uber Uber {
            get;
            set;
        }

        private void Awesomium_Windows_Forms_WebControl_AddressChanged( object sender, UrlEventArgs e ) {
            this.labelNavigationStatus.Text( "Address changed" );
        }

        private void Awesomium_Windows_Forms_WebControl_Crashed( object sender, CrashedEventArgs e ) {
            this.labelNavigationStatus.Text( "Browser crashed" );
        }

        private void Awesomium_Windows_Forms_WebControl_DocumentReady( object sender, UrlEventArgs e ) {
            this.labelNavigationStatus.Text( "Document ready" );
        }

        private void Awesomium_Windows_Forms_WebControl_LoadingFrame( object sender, LoadingFrameEventArgs e ) {
            this.labelNavigationStatus.Text( "Loading frame" );
            this.labelBrowserSource.Text( this.webBrowser1.Source.ToString() );
        }

        private void Awesomium_Windows_Forms_WebControl_LoadingFrameComplete( object sender, FrameEventArgs e ) {
            this.labelNavigationStatus.Text( "Frame loaded" );
        }

        private void MainForm_FormClosing( object sender, FormClosingEventArgs e ) {
            this.labelNavigationStatus.Text( "Form closing" );
            Settings.Default.Save();

            var uber = this.Uber;
            if ( uber != null ) {
                uber.Stop();
            }
        }

        private void MainForm_Load( object sender, EventArgs e ) {
            "Loading...".Write();
            this.Uber = new Uber();
            var uber = this.Uber;
            if ( uber == null ) {
                return;
            }
            //uber.WebBrowser1 = this.webBrowser1;
            //uber.WebBrowser1.Source = new Uri( "about:blank" );

            //uber.WebBrowser2 = this.webBrowser2;
            this.buttonStart.Usable( false );
            "loaded.".WriteLine();
        }

        private async void MainForm_Shown( object sender, EventArgs e ) {
            var uber = this.Uber;
            if ( null == uber ) {
                return;
            }
            this.buttonStart.Usable( await uber.Init() );
        }

        private async void buttonStart_Click( object sender, EventArgs e ) {
            Console.WriteLine( "Website visit start requested..." );
            try {
                this.buttonStart.Usable( false );
                this.buttonStop.Usable( true );
                var uber = this.Uber;
                if ( null == uber ) {
                    return;
                }
                await Task.Run( () => {
                    uber.PictureBoxChallenge = this.pictureBoxChallenge;
                    uber.VisitSites( CancellationTokenSource.Token );
                } );
            }
            finally {
                this.buttonStop.Usable( false );
            }
        }

        private async void buttonStop_Click( object sender, EventArgs e ) {
            Console.WriteLine( "Stop button pressed." );
            var uber = this.Uber;
            if ( uber != null ) {
                await Task.Run( () => uber.Stop() );
            }
        }

        private void MainForm_FormClosed( object sender, FormClosedEventArgs e ) {
            using (this) {
                Console.Write( "Disposing of resources..." );
                var uber = this.Uber;
                if ( uber != null ) {
                    using (uber) {
                        uber.Dispose();
                    }
                }
                Console.WriteLine( "Resources disposed." );
#if DEBUG
                Console.WriteLine();

                //Console.WriteLine( "Press any key to exit" );
                Task.Delay( Seconds.Five ).Wait();

                //Console.ReadKey();
#endif
            }
        }

        private void button1_Click( Object sender, EventArgs e ) {
            using (var bob = new SitesEditor()) {
                var result = bob.ShowDialog( this );
                switch ( result ) {
                    case DialogResult.None:
                        break;

                    case DialogResult.OK:
                        break;

                    case DialogResult.Cancel:
                        break;

                    case DialogResult.Abort:
                        break;

                    case DialogResult.Retry:
                        break;

                    case DialogResult.Ignore:
                        break;

                    case DialogResult.Yes:
                        break;

                    case DialogResult.No:
                        break;

                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        private void Awesomium_Windows_Forms_WebControl_ShowCreatedWebView( Object sender, ShowCreatedWebViewEventArgs e ) {

        }

        private void Awesomium_Windows_Forms_WebControl_ShowCreatedWebView_1( Object sender, ShowCreatedWebViewEventArgs e ) {

        }
    }
}
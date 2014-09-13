
namespace UberScraper {
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows.Forms;
    using Awesomium.Core;
    using Librainian.Annotations;
    using Librainian.Controls;
    using Librainian.Magic;
    using Librainian.Threading;

    public partial class MainForm : Form {

        public SynchronizationContext AwesomiumContext {
            get;
            private set;
        }

        public MainForm() {
            this.InitializeComponent();

            var awesomiumThread = new Thread( () => {
                WebCore.Started += ( s, e ) => {
                    this.AwesomiumContext = SynchronizationContext.Current;
                };

                WebCore.Run();
            } );
        }

        [CanBeNull]
        public Uber Uber {
            get;
            set;
        }

        private void MainForm_Load( object sender, EventArgs e ) {
            this.Uber = Ioc.Container.TryGet<Uber>();
            var uber = this.Uber;
            if ( uber != null ) {

                uber.WebBrowser = this.webBrowser;
                WebCore.Started += ( o, coreStartEventArgs ) => {
                    uber.AwesomiumContext = SynchronizationContext.Current;
                };
                uber.Remember( Task.Factory.StartNew( () => {
                    Report.Enter();

                    //uber.AwesomiumContext.



                    Report.Exit();
                }, TaskCreationOptions.LongRunning ) );
            }
        }

        private async void MainForm_Shown( object sender, EventArgs e ) {
            var uber = this.Uber;
            if ( null == uber ) {
                return;
            }
            await uber.Start();
            await Task.Run( () => uber.VisitSites() );
        }

        private void Awesomium_Windows_Forms_WebControl_AddressChanged( object sender, UrlEventArgs e ) {
            this.labelNavigationStatus.Text( "Address changed" );
        }

        private void Awesomium_Windows_Forms_WebControl_Crashed( object sender, CrashedEventArgs e ) {
            this.labelNavigationStatus.Text( "Crashed" );
        }

        private void Awesomium_Windows_Forms_WebControl_DocumentReady( object sender, UrlEventArgs e ) {
            this.labelNavigationStatus.Text( "Document ready" );
        }

        private void Awesomium_Windows_Forms_WebControl_LoadingFrame( object sender, LoadingFrameEventArgs e ) {
            this.labelNavigationStatus.Text( "Loading frame" );
            this.labelBrowserSource.Text( this.webBrowser.Source.ToString() );
        }

        private void Awesomium_Windows_Forms_WebControl_LoadingFrameComplete( object sender, FrameEventArgs e ) {
            this.labelNavigationStatus.Text( "Frame loaded" );
        }

        private void MainForm_FormClosing( object sender, FormClosingEventArgs e ) {
            this.labelNavigationStatus.Text( "Form closing" );
            var uber = this.Uber;
            if ( uber != null ) {
                uber.Stop();
            }
        }
    }
}

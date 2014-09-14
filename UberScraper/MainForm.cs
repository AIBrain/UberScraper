
namespace UberScraper {
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows.Forms;
    using Awesomium.Core;
    using FluentAssertions;
    using Librainian.Annotations;
    using Librainian.Controls;
    using Librainian.Magic;

    public partial class MainForm : Form {

        public SynchronizationContext AwesomiumContext {
            get;
            private set;
        }

        [ CanBeNull ]
        public CancellationTokenSource CancellationTokenSource {
            get;
             set;
        }


        public MainForm() {
            AwesomiumContext = SynchronizationContext.Current;
            this.InitializeComponent();
            AwesomiumContext.Should().NotBeNull();
        }

        [CanBeNull]
        public Uber Uber {
            get;
            set;
        }

        private void MainForm_Load( object sender, EventArgs e ) {
            this.Uber = Ioc.Container.TryGet<Uber>();
            var uber = this.Uber;
            if ( uber == null ) {
                return;
            }
            uber.WebBrowser1 = this.webBrowser1;
            uber.WebBrowser2 = this.webBrowser2;
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
            this.labelBrowserSource.Text( this.webBrowser1.Source.ToString() );
        }

        private void Awesomium_Windows_Forms_WebControl_LoadingFrameComplete( object sender, FrameEventArgs e ) {
            this.labelNavigationStatus.Text( "Frame loaded" );
        }

        private void MainForm_FormClosing( object sender, FormClosingEventArgs e ) {
            this.labelNavigationStatus.Text( "Form closing" );

            var cancellationTokenSource = this.CancellationTokenSource;
            if ( cancellationTokenSource != null ) {
                cancellationTokenSource.Cancel();
            }

            var uber = this.Uber;
            if ( uber != null ) {
                uber.Stop();
            }
        }
    }
}

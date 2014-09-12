
namespace UberScraper {
    using System;
    using System.Windows.Forms;
    using Librainian.Annotations;
    using Librainian.Controls;
    using Librainian.Magic;

    public partial class MainForm : Form {
        public MainForm() {
            InitializeComponent();
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
            }
        }

        private async void MainForm_Shown( object sender, EventArgs e ) {
            var uber = this.Uber;
            if ( null == uber ) {
                return;
            }
            await uber.Start();
#if DEBUG
            await uber.Navigate( new Uri( "http://www.google.com/" ) );

            var bob = uber.GetBrowserHTMLCleaned();

            await uber.Navigate( new Uri( "http://freedoge.co.in/?op=home" ) );
#endif
        }

        private void Awesomium_Windows_Forms_WebControl_AddressChanged( object sender, Awesomium.Core.UrlEventArgs e ) {
            this.labelNavigationStatus.Text( "Address changed" );
        }

        private void Awesomium_Windows_Forms_WebControl_Crashed( object sender, Awesomium.Core.CrashedEventArgs e ) {
            this.labelNavigationStatus.Text( "Crashed" );
        }

        private void Awesomium_Windows_Forms_WebControl_DocumentReady( object sender, Awesomium.Core.UrlEventArgs e ) {
            this.labelNavigationStatus.Text( "Document ready" );
        }

        private void Awesomium_Windows_Forms_WebControl_LoadingFrame( object sender, Awesomium.Core.LoadingFrameEventArgs e ) {
            this.labelNavigationStatus.Text( "Loading frame");
            this.labelBrowserSource.Text( this.webBrowser.Source.ToString() );
        }

        private void Awesomium_Windows_Forms_WebControl_LoadingFrameComplete( object sender, Awesomium.Core.FrameEventArgs e ) {
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

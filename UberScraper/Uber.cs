namespace UberScraper {

    using System;
    using System.Collections.Concurrent;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows.Forms;
    using Awesomium.Core;
    using Awesomium.Windows.Forms;
    using Librainian;
    using Librainian.Annotations;
    using Librainian.Internet;
    using Librainian.Magic;
    using Librainian.Maths;
    using Librainian.Persistence;
    using Librainian.Threading;
    using MarkupSanitizer;

    public class Uber : IUber {
        private readonly ConcurrentDictionary<IDisposable, DateTime> _autoDisposables = new ConcurrentDictionary<IDisposable, DateTime>();

        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

        private WebControl _webBrowser;

        [CanBeNull]
        private PersistTable<String, WebSite> _webSites;

        public Boolean HasWebSitesBeenLoaded {
            get;
            private set;
        }

        public WebControl WebBrowser {
            get {
                return this._webBrowser;
            }

            set {
                if ( value == null ) {
                    return;
                }
                this._webBrowser = value;
                this._webBrowser.LoadingFrameComplete += ( sender, args ) => {
                };
                this._autoDisposables.TryAdd( value, DateTime.Now );
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose() {
            foreach ( var disposable in this._autoDisposables.Where( pair => null != pair.Key ).OrderByDescending( pair => pair.Value ) ) {
                try {
                    Report.Before( String.Format( "Disposing of {0}...", disposable.Key.ToString() ) );
                    disposable.Key.Dispose();
                    Report.After( String.Format( "Disposed.", disposable.Key.ToString() ) );
                }
                catch ( Exception exception ) {
                    exception.Error();
                }
            }
        }

        [CanBeNull]
        public String GetBrowserHTML() {
            try {
                var browser = this.WebBrowser;
                if ( browser != null ) {
                    return browser.HTML;
                }

                //TODO check for non - "http status of 200 OK"
                //var result = this.Browser.Invoke( new Func<string>( () => {
                //    var javascriptWithResult = this.Browser.ExecuteJavascriptWithResult( "document.getElementsByTagName('html')[0].innerText" );
                //    return !javascriptWithResult.IsString ? String.Empty : javascriptWithResult.ToString();
                //} ) );

                //return result as String;
            }
            catch ( Exception exception ) {
                exception.Error();
            }
            return null;
        }

        [CanBeNull]
        public String GetBrowserHTMLCleaned() {
            var html = this.GetBrowserHTML();

            var bob = Sanitizer.SanitizeMarkup( html );

            //var bob = new StringWriter();
            //bob.Write( html  );
            //var reader = Html2Xhtml.RunAsFilter( writer => { }, dosEndOfLine: true );
            var newhtml = bob.ToString();
            return html;
        }

        public Boolean LoadWebsites() {
            try {
                Report.Enter();
                this._webSites = new PersistTable<String, WebSite>( Environment.SpecialFolder.CommonApplicationData, "Websites" );
                if ( null != this._webSites ) {
                    this._autoDisposables.TryAdd( this._webSites, DateTime.Now );
                }
            }
            catch ( InvalidOperationException ) {
                return false;
            }
            catch ( PathTooLongException ) {
                return false;
            }
            catch ( DirectoryNotFoundException ) {
                return false;
            }
            catch ( FileNotFoundException ) {
                return false;
            }
            finally {
                Report.Exit();
            }

            return null != this._webSites;
        }

        /// <summary>
        /// <para>Starts a <see cref="Task"/> to navigate to the specified <paramref name="uri"/>.</para>
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        public Task Navigate( [NotNull] Uri uri ) {
            return Task.Run( () => {
                NavigateTo( uri );
            } );
        }

        /// <summary>
        /// <para>Starts a <see cref="Task"/> to navigate to the specified <paramref name="uri"/>.</para>
        /// </summary>
        /// <param name="uriString"></param>
        /// <returns></returns>
        public Task Navigate( String uriString ) {
            return Task.Run( () => {
                Uri uri;
                if ( Uri.TryCreate( uriString, UriKind.Absolute, out uri ) ) {
                    NavigateTo( uri );
                }
            } );
        }

        /// <summary>
        /// <para>No guarantee that more ajax/javascript can and will fire off after this is 'true'.</para>
        /// <para>Internal while loop blocks.</para>
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        /// <seealso cref="http://answers.awesomium.com/questions/3971/loading-script-complete.html"/>
        public Boolean NavigateTo( [NotNull] Uri uri ) {
            if ( null == uri ) {
                throw new ArgumentNullException( "request" );
            }

            try {
                this.WebBrowser.Invoke( method: new Action( () => {
                    Report.Before( String.Format( "Navigating to {0}...", uri ) );
                    this.WebBrowser.Source = uri;

                    while ( this.WebBrowser.IsLoading || this.WebBrowser.IsNavigating ) {
                        if ( _cancellationTokenSource.IsCancellationRequested ) {
                            break;
                        }
                        WebCore.Update();
                        Application.DoEvents();
                    }
                    Report.After( "done navigating." );
                } ) );

                return this.WebBrowser.IsDocumentReady && this.WebBrowser.IsResponsive;
            }
            catch ( Exception exception ) {
                Debug.WriteLine( exception.Message );
            }
            return false;
        }

        public void SetBrowser( [CanBeNull] WebControl webBrowser ) {
            this.WebBrowser = webBrowser;
        }

        /// <summary>
        /// <para>Load dictionaries.</para>
        /// </summary>
        /// <returns></returns>
        public async Task<Boolean> Start() {
            if ( _cancellationTokenSource.IsCancellationRequested ) {
                return false;
            }

            var tasks = Ioc.Container.TryGet<VotallyI>();
            if ( null == tasks ) {
                return false;
            }

            var result = await this.LoadDictionaries();
            if ( result ) {
                tasks.VoteYes();
            }
            else {
                tasks.VoteNo();
            }

            return tasks.IsYesWinning();
        }

        public void Stop() {
            try {
                this._cancellationTokenSource.Cancel();
                this.WebBrowser.Stop();
            }
            finally {
                this.Dispose();
            }
        }

        private async Task<Boolean> LoadDictionaries() {
            if ( !this.HasWebSitesBeenLoaded ) {
                this.HasWebSitesBeenLoaded = await Task.Run( () => this.LoadWebsites() );
            }
            return this.HasWebSitesBeenLoaded;
        }
    }
}
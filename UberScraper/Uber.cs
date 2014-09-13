namespace UberScraper {

    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows.Forms;
    using System.Windows.Media.Imaging;
    using Awesomium.Core;
    using Awesomium.Windows.Forms;
    using CsQuery;
    using Librainian;
    using Librainian.Annotations;
    using Librainian.Internet;
    using Librainian.Magic;
    using Librainian.Maths;
    using Librainian.Measurement.Time;
    using Librainian.Persistence;
    using Librainian.Threading;
    using Tidy.Core;
    using DocType = CsQuery.DocType;

    public class Uber : IUber {
        private readonly ConcurrentDictionary<IDisposable, DateTime> _autoDisposables = new ConcurrentDictionary<IDisposable, DateTime>();
        private readonly ConcurrentDictionary<Object, DateTime> _otherObjects = new ConcurrentDictionary<Object, DateTime>();

        private WebControl _webBrowser;

        [CanBeNull]
        private PersistTable<String, WebSite> _webSites;
        [CanBeNull]
        private PersistTable<String, CaptchaSite> _captchaSites;

        public Uber() {
            this.CancellationTokenSource = new CancellationTokenSource();
            this.NavigationTimeout = Seconds.Ten;
        }

        public SynchronizationContext AwesomiumContext {
            get;
            set;
        }

        [NotNull]
        public CancellationTokenSource CancellationTokenSource {
            get;
            private set;
        }

        public Boolean HasWebSitesBeenLoaded {
            get;
            private set;
        }

        [CanBeNull]
        public WebControl WebBrowser {
            get {
                return this._webBrowser;
            }

            set {
                this._webBrowser = value;
                if ( value == null ) {
                    return;
                }
                this._webBrowser.LoadingFrameComplete += ( sender, args ) => {
                };
                this._autoDisposables.TryAdd( value, DateTime.Now );
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose() {
            var before = GC.GetTotalMemory( true );
            _otherObjects.Clear();
            var after = GC.GetTotalMemory( false );
            var difference = before - after;

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
                    var result = browser.Invoke( new Func<string>( () => browser.HTML ) );
                    return result as String;
                }
            }
            catch ( Exception exception ) {
                exception.Error();
            }
            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="javascript"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public Boolean GetJavascriptWithResult( String javascript, out JSValue result ) {
            result = default( JSValue );
            var browser = this.WebBrowser;
            if ( browser != null ) {
                try {
                    result = ( JSValue )browser.Invoke( new Func<JSValue>( () => browser.ExecuteJavascriptWithResult( javascript ) ) );
                    return true;
                }
                catch ( Exception exception ) {
                    exception.Error();
                }
            }
            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="javascript"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public Boolean GetJavascriptWithResultArray( String javascript, out JSValue[] result ) {
            result = default( JSValue[] );
            var browser = this.WebBrowser;
            if ( browser != null ) {
                try {
                    var links = new JSValue[] { };
                    //dynamic localresult = null;
                    //dynamic links = null;

                    browser.Invoke( new Action( () => {

                        //var bob = new CQ(

                        //var aresult = (JSObject)browser.ExecuteJavascriptWithResult( javascript );
                        //var aresult = (JSObject)browser.ExecuteJavascriptWithResult( javascript );
                        var aresult = browser.ExecuteJavascriptWithResult( String.Format( "(function(){{ {0} }})()", javascript ) );
                        try {
                            //links = aresult.getElementsByTagName( 'a' );

                            links = ( JSValue[] )aresult;

                        }
                        catch ( InvalidOperationException ) {
                        }
                    } ) );

                    //foreach ( var link in links ) {
                    //    Console.WriteLine( link as object );
                    //}

                    result = links;


                    return true;
                }
                catch ( Exception exception ) {
                    exception.Error();
                }
            }
            return false;
        }

        public IEnumerable<Uri> GetAllLinks() {
            //var allLinks = new List<Uri>( 128 );
            //const string javascript = " var arr = []; var l = document.getElementsByTagName('a'); for ( var i=0; i<l.length; i++) { arr.push(l[i].href); }  ";
            //const string javascript = " var arr = []; var l = document.getElementsByTagName('a'); for ( var i=0; i<l.length; i++) { arr.push(l[i].href); }; return arr; ";
            //const string javascript = " document.getElementsByTagName('a'); ";
            //const string javascript = "document";
            //const string javascript = "document.links";
            //const string javascript = "document.links";

            var cq = new CQ( this.GetBrowserHTML() );

            var anchors = cq[ "a" ];

            foreach ( var domObject in anchors ) {
                var href = domObject[ "href" ];
                Uri uri;

                if ( !Uri.TryCreate( href, UriKind.Absolute, out uri ) ) {
                    continue;
                }
                yield return uri;
            }
        }

        [CanBeNull]
        public String GetBrowserHTMLCleaned() {
            var badhtml = this.GetBrowserHTML();
            if ( null == badhtml ) {
                return null;
            }

            var dan = new Tidy();

            //dan.Options.DocType = DocType.Auto;
            //dan.Options.DropEmptyParas = false;
            //dan.Options.DropFontTags = false;
            //dan.Options.FixComments = true;
            //dan.Options.FixBackslash = true;
            ////dan.Options.MakeClean = true;
            //dan.Options.SmartIndent = true;
            //dan.Options.Xhtml = true;
            dan.Options.XmlOut = true;

            var tidyMessageCollection = new TidyMessageCollection();
            var newhtml = dan.Parse( badhtml, tidyMessageCollection );

            return newhtml;

            //var bob =new SanitizedMarkup( html, false );

            //    //. Sanitizer.SanitizeMarkup( html );

            ////var bob = new StringWriter();
            ////bob.Write( html  );
            ////var reader = Html2Xhtml.RunAsFilter( writer => { }, dosEndOfLine: true );
            //var newhtml = bob.ToString();
            //return html;
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

        public Boolean LoadCaptchas() {
            try {
                Report.Enter();

                this._captchaSites = new PersistTable<String, CaptchaSite>( Environment.SpecialFolder.CommonApplicationData, "Captchas" );

                if ( null != this._captchaSites ) {
                    this._autoDisposables.TryAdd( this._captchaSites, DateTime.Now );
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

            return null != this._captchaSites;
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
                //this.AwesomiumContext.

                var webBrowser = this.WebBrowser;
                if ( webBrowser != null ) {
                    webBrowser.Invoke( method: new Action( () => {
                        Report.Before( String.Format( "Navigating to {0}...", uri ) );
                        if ( HasWebSitesBeenLoaded ) {

                        }
                        this._webSites[ uri.PathAndQuery ] = new WebSite {
                            Location = uri
                        };
                        webBrowser.Source = uri;

                        while ( webBrowser.IsLoading || webBrowser.IsNavigating ) {
                            if ( this.CancellationTokenSource.IsCancellationRequested ) {
                                break;
                            }
                            WebCore.Update();
                            Application.DoEvents();
                        }
                        Report.After( "done navigating." );
                    } ) );

                    return webBrowser.IsDocumentReady && webBrowser.IsResponsive;
                }
            }
            catch ( Exception exception ) {
                Debug.WriteLine( exception.Message );
            }
            return false;
        }

        public void Remember<TKey>( TKey key ) {
            this._otherObjects.TryAdd( key, DateTime.Now );
        }

        public void SetBrowser( [CanBeNull] WebControl webBrowser ) {
            this.WebBrowser = webBrowser;
        }

        /// <summary>
        /// <para>Load dictionaries.</para>
        /// </summary>
        /// <returns></returns>
        public async Task<Boolean> Start() {
            if ( this.CancellationTokenSource.IsCancellationRequested ) {
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
                var cancellationTokenSource = this.CancellationTokenSource;
                //if ( cancellationTokenSource != null ) {
                cancellationTokenSource.Cancel();
                //}
                var webBrowser = this.WebBrowser;
                if ( webBrowser != null ) {
                    webBrowser.Stop();
                }
            }
            finally {
                this.Dispose();
            }
        }

        public void VisitSites() {
            var faucets = ( BitcoinFaucets[] )Enum.GetValues( typeof( BitcoinFaucets ) );
            //var list = new List<BitcoinFaucets>( faucets );
            foreach ( var faucetID in faucets.OrderBy( bitcoinFaucets => ( int )bitcoinFaucets ) ) {
                Visit( faucetID );
            }

            //await uber.Navigate( "http://freedoge.co.in/?op=home" );
        }

        [NotNull]
        private static string GetDescription( BitcoinFaucets faucet ) {
            var fi = faucet.GetType().GetField( faucet.ToString() );
            var attributes = ( DescriptionAttribute[] )fi.GetCustomAttributes( typeof( DescriptionAttribute ), false );
            return attributes.Length > 0 ? attributes[ 0 ].Description : String.Empty;
        }

        private void Throttle( TimeSpan? until = null ) {
            //await Task.Delay( TimeSpan.FromSeconds( 1 ) );
            if ( !until.HasValue ) {
                until = Seconds.Five;
            }
            var watch = Stopwatch.StartNew();
            do {
                Thread.Sleep( Milliseconds.FiveHundred );
                Application.DoEvents();
                if ( this.CancellationTokenSource.IsCancellationRequested ) {
                    break;
                }
            } while ( watch.Elapsed > until.Value );
        }

        private async Task<Boolean> LoadDictionaries() {
            if ( !this.HasWebSitesBeenLoaded ) {
                this.HasWebSitesBeenLoaded = await Task.Run( () => this.LoadWebsites() );
            }
            if ( !HasCaptchasBeenLoaded ) {
                this.HasCaptchasBeenLoaded = await Task.Run( () => this.LoadCaptchas() );
            }
            return this.HasWebSitesBeenLoaded && this.HasCaptchasBeenLoaded;
        }

        public Boolean HasCaptchasBeenLoaded {
            get;
            private set;
        }

        private void Visit( BitcoinFaucets faucetID ) {
            try {
                var uri = new Uri( GetDescription( faucetID ) );
                Debug.WriteLine( "Visiting ({0}) {1}", faucetID, GetDescription( faucetID ) );

                var faucetUri = GetDescription( faucetID );

                if ( String.IsNullOrWhiteSpace( faucetUri ) ) {
                    return;
                }

                Throttle();
                var navigated = this.Navigate( faucetUri ).Wait( this.NavigationTimeout );
                if ( !navigated ) {
                    faucetID = BitcoinFaucets.AboutBlank;
                }

                switch ( faucetID ) {
                    case BitcoinFaucets.BitChestDotMe:
                        Visit_BitChestDotMe( "1MpfkH1vDyGrmtykodJmzBNWi81KqXa8SE" );
                        break;

                    case BitcoinFaucets.LandOfBitCoinDotCom:
                        this.Visit_LandOfBitCoinDotCom();
                        break;

                    default:
                        Visit_AboutBlank();
                        break;
                }
            }
            catch ( Exception exception ) {
                exception.Error();
            }
        }

        public TimeSpan NavigationTimeout {
            get;
            set;
        }

        public void JsFireEvent( string getElementQuery, string eventName ) {
            var browser = this.WebBrowser;
            if ( browser != null ) {
                browser.ExecuteJavascript( @"
                            function fireEvent(element,event) {
                                var evt = document.createEvent('HTMLEvents');
                                evt.initEvent(event, true, true ); // event type,bubbling,cancelable
                                element.dispatchEvent(evt);                                 
                            }
                            " + String.Format( "fireEvent({0}, '{1}');", getElementQuery, eventName ) );
            }
        }

        private void Visit_BitChestDotMe( String bitcoinAddress ) {
            Report.Enter();
            if ( !this.Navigate( String.Format( "http://www.bitchest.me/?a={0}", bitcoinAddress ) ).Wait( NavigationTimeout ) ) {
                return;
            }

            Throttle();

            var alllinks = this.GetAllLinks().ToList();
            var links = alllinks.Where( uri => uri.PathAndQuery.Contains( bitcoinAddress ) ).ToList();

            foreach ( var link in links ) {
                Throttle();
                this.Navigate( link ).Wait( this.CancellationTokenSource.Token );
                Throttle();
                FindCaptcha();  //TODO solve captcha (or try past answers)
                Throttle( Seconds.Ten );

                //TODO submit
            }

            Report.Exit();
        }

        private void FindCaptcha() {
            // 
            var cq = new CQ( this.GetBrowserHTML(), HtmlParsingMode.Auto, HtmlParsingOptions.AllowSelfClosingTags, DocType.HTML5 );

            var captchaChallange = cq[ "recaptcha_challenge_image" ];

            var captchaInputArea = cq[ "recaptcha_response_field" ];

            if ( null != captchaChallange && null != captchaInputArea ) {
                this.GetCaptcha( captchaChallange, captchaInputArea );
            }
            //TODO look for other captcha
        }

        public Boolean UpdateWebsite( [CanBeNull] Uri uri, [CanBeNull] out WebSite webSite ) {
            webSite = default( WebSite );
            if ( null == uri ) {
                return false;
            }
            if ( null == this._webSites ) {
                return false;
            }

            if ( null == this._webSites[ uri.PathAndQuery ] ) {
                this._webSites[ uri.PathAndQuery ] = new WebSite();
            }
            webSite = this._webSites[ uri.PathAndQuery ];
            if ( null == webSite.Location ) {
                webSite.Location = uri;
            }

            return true;
        }

        public CaptchaSite UpdateCaptcha( [CanBeNull] Uri uri ) {
            //captchaSite = default( CaptchaSite );
            if ( null == uri ) {
                return default( CaptchaSite );
            }
            if ( null == this._captchaSites ) {
                return default( CaptchaSite );
            }

            if ( null == this._captchaSites[ uri.PathAndQuery ] ) {
                this._captchaSites[ uri.PathAndQuery ] = new CaptchaSite();
            }
            var captchaSite = this._captchaSites[ uri.PathAndQuery ];
            if ( null == captchaSite.Location ) {
                captchaSite.Location = uri;
            }

            return captchaSite;
        }

        private void GetCaptcha( CQ captchaChallange, CQ captchaInputArea ) {
            try {
                Report.Enter();
                var imageSrc = captchaChallange[ "src" ].Text();
                Uri imageUri;
                if ( !Uri.TryCreate( imageSrc, UriKind.Absolute, out imageUri ) ) {
                    return;
                }
                try {
                    var isImageGood = true;

                    var bitmapImage = new BitmapImage();

                    bitmapImage.DecodeFailed += ( sender, args ) => {
                        isImageGood = false;
                    };
                    bitmapImage.DownloadFailed += ( sender, args ) => {
                        isImageGood = false;
                    };
                    bitmapImage.DownloadCompleted += ( sender, args ) => {
                        isImageGood = true;
                    };
                    bitmapImage.BeginInit();
                    bitmapImage.UriSource = imageUri;
                    bitmapImage.EndInit();

                    if ( isImageGood && bitmapImage.Width > 0 && bitmapImage.Height > 0 ) {
                        UpdateCaptcha( imageUri ).CaptchaStatus = CaptchaStatus.LoadedImage;
                    }


                }
                catch ( Exception exception ) {
                    UpdateCaptcha( imageUri ).CaptchaStatus = CaptchaStatus.ErrorLoadingImage;
                    exception.Error();
                }
            }
            catch ( Exception exception ) {
                exception.Error();
            }
            finally {
                Report.Exit();
            }
        }

        private void Visit_LandOfBitCoinDotCom() {


            if ( !this.Navigate( "https://www.landofbitcoin.com/login" ).Wait( NavigationTimeout ) ) {
                return;
            }

            var bob = new CQ( this.GetBrowserHTML() );
            var text = bob[ "username" ].Text();
            bob[ "username" ].Text( "AIBrain" );

            //while ( !this.CancellationTokenSource.IsCancellationRequested ) {
            Throttle();
            //}
            //go to main page

        }

        private void Visit_AboutBlank() {
            Throttle();
        }
    }

}
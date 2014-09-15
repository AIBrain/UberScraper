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
    using CsQuery.ExtensionMethods.Internal;
    using FluentAssertions;
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


        private WebControl _webBrowser1;

        [NotNull]
        private PersistTable<String, WebSite> _webSites;

        [NotNull]
        private PersistTable<String, Captcha> _captchas;

        private WebControl _webBrowser2;

        public Uber() {
            this.CancellationTokenSource = new CancellationTokenSource();
            this.NavigationTimeout = Seconds.Thirty;
        }

        /*
                public SynchronizationContext AwesomiumContext {
                    get;
                    set;
                }
        */

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
        public WebControl WebBrowser1 {
            get {
                return this._webBrowser1;
            }

            set {
                this._webBrowser1 = value;
                if ( value == null ) {
                    return;
                }
                this._webBrowser1.LoadingFrameComplete += ( sender, args ) => {
                };
                this._autoDisposables.TryAdd( value, DateTime.Now );
            }
        }

        [CanBeNull]
        public WebControl WebBrowser2 {
            get {
                return this._webBrowser2;
            }

            set {
                this._webBrowser2 = value;
                if ( value == null ) {
                    return;
                }
                this._webBrowser2.LoadingFrameComplete += ( sender, args ) => {
                };
                this._autoDisposables.TryAdd( value, DateTime.Now );
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose() {
            try {
                Report.Enter();

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
            finally {
                Report.Exit();
            }
        }

        [CanBeNull]
        public static String GetBrowserHTML( WebControl webBrowser ) {
            try {
                if ( webBrowser != null ) {
                    //var result = webBrowser.Invoke( new Func<string>( () => webBrowser.HTML ) );
                    var result = webBrowser.Invoke( new Func<string>( () => webBrowser.ExecuteJavascriptWithResult( "document.getElementsByTagName('html')[0].innerHTML" ) ) );


                    if ( result is String ) {
                        return result as String;
                    }
                    return result.ToString();
                }
            }
            catch ( Exception exception ) {
                exception.Error();
            }
            return null;
        }

        /// <summary>
        /// Retrieve the <see cref="Uri"/> the <see cref="WebBrowser1"/> is currently at.
        /// </summary>
        /// <returns></returns>
        [NotNull]
        public Uri GetBrowser1Location() {
            try {
                var browser = this.WebBrowser1;
                if ( browser != null ) {
                    var result = browser.Invoke( new Func<Uri>( () => browser.Source ) );
                    return ( Uri )result;
                }
            }
            catch ( Exception exception ) {
                exception.Error();
            }
            return new Uri( "about:blank" );
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="webBrowser"></param>
        /// <param name="javascript"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public static Boolean GetJavascriptWithResult( WebControl webBrowser, String javascript, out JSValue result ) {
            result = default( JSValue );
            if ( webBrowser == null ) {
                return false;
            }
            try {
                result = ( JSValue )webBrowser.Invoke( new Func<JSValue>( () => webBrowser.ExecuteJavascriptWithResult( javascript ) ) );
                return true;
            }
            catch ( Exception exception ) {
                exception.Error();
            }
            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="webBrowser"></param>
        /// <param name="javascript"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public static Boolean GetJavascriptWithResultArray( WebControl webBrowser, String javascript, out JSValue[] result ) {
            result = default( JSValue[] );
            if ( webBrowser == null ) {
                return false;
            }
            try {
                var links = new JSValue[] { };
                //dynamic localresult = null;
                //dynamic links = null;

                webBrowser.Invoke( new Action( () => {

                    //var bob = new CQ(

                    //var aresult = (JSObject)browser.ExecuteJavascriptWithResult( javascript );
                    //var aresult = (JSObject)browser.ExecuteJavascriptWithResult( javascript );
                    var aresult = webBrowser.ExecuteJavascriptWithResult( String.Format( "(function(){{ {0} }})()", javascript ) );
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

            var html = GetBrowserHTML( this.WebBrowser1 );

            var cq = new CQ( html );

            var cq2 = new CQ( GetBrowserHTML( this.WebBrowser1 ), HtmlParsingMode.Auto, HtmlParsingOptions.AllowSelfClosingTags, DocType.HTML5 );


            var anchors = cq[ "a" ];
            //var anchors = cq[ "document.links" ];

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
            var badhtml = GetBrowserHTML( this.WebBrowser1 );
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

                this._captchas = new PersistTable<String, Captcha>( Environment.SpecialFolder.CommonApplicationData, "Captchas" );

                if ( null != this._captchas ) {
                    this._autoDisposables.TryAdd( this._captchas, DateTime.Now );
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

            return null != this._captchas;
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
        /// <para>Starts a <see cref="Task"/> to navigate to the specified <paramref name="uriString"/>.</para>
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
                if ( this.CancellationTokenSource.IsCancellationRequested ) {
                    return false;
                }

                //this.AwesomiumContext.

                var webBrowser = this.WebBrowser1;
                if ( webBrowser != null ) {
                    webBrowser.Invoke( method: new Action( () => {
                        if ( this.CancellationTokenSource.IsCancellationRequested ) {
                            return;
                        }

                        Report.Before( String.Format( "Navigating to {0}...", uri ) );

                        this.EnsureWebsite( uri );

                        webBrowser.Source = uri;

                        while ( webBrowser.IsLoading || webBrowser.IsNavigating ) {
                            //this.Throttle( Milliseconds.Hertz111 );
                            //WebCore.Update();
                            Application.DoEvents();
                            if ( this.CancellationTokenSource.IsCancellationRequested ) {
                                break;
                            }
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

        public void SetBrowser( [CanBeNull] WebControl webBrowser ) {
            this.WebBrowser1 = webBrowser;
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
                var webBrowser = this.WebBrowser1;
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
            if ( !until.HasValue ) {
                until = Milliseconds.Hertz111;
            }
            var watch = Stopwatch.StartNew();
            do {
                Application.DoEvents();
                if ( watch.Elapsed < until.Value ) {
                    Thread.Sleep( Milliseconds.Hertz111 );
                }
                else {
                    break;
                }
                if ( this.CancellationTokenSource.IsCancellationRequested ) {
                    break;
                }
            } while ( watch.Elapsed < until.Value );
            watch.Stop();
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

        public void EnsureWebsite( [CanBeNull] Uri uri ) {
            if ( null == uri ) {
                return;
            }

            this._webSites.Should().NotBeNull();

            if ( null == this._webSites[ uri.PathAndQuery ] ) {
                this._webSites[ uri.PathAndQuery ] = new WebSite();
            }
            this._webSites[ uri.PathAndQuery ].Location = uri;
        }

        /// <summary>
        /// Returns the <see cref="Librainian.Internet.Captcha"/> object. Call <see cref="Captcha(System.Uri,Librainian.Internet.Captcha)"/> to return the update.
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        [NotNull]
        public Captcha Captcha( [CanBeNull] Uri uri ) {
            if ( null == uri ) {
                return new Captcha();
            }

            if ( null == this._captchas[ uri.PathAndQuery ] ) {
                this._captchas[ uri.PathAndQuery ] = new Captcha();
            }

            if ( null == this._captchas[ uri.PathAndQuery ].Uri ) {
                this._captchas[ uri.PathAndQuery ].Uri = uri;
            }

            return this._captchas[ uri.PathAndQuery ];
        }

        /// <summary>
        /// <para>Updates the stored captcha with this new information.</para>
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="captcha"></param>
        public void Captcha( [CanBeNull] Uri uri, [CanBeNull] Captcha captcha ) {
            if ( null == uri || null == captcha ) {
                return;
            }
            this._captchas[ uri.PathAndQuery ] = captcha;
        }

        public Boolean HasCaptchasBeenLoaded {
            get;
            private set;
        }

        private void Visit( BitcoinFaucets faucetID ) {
            try {
                var description = GetDescription( faucetID );
                var uri = new Uri( description );
                Debug.WriteLine( "Visiting ({0}) {1} @ {2}", faucetID, description, uri.PathAndQuery );


                if ( String.IsNullOrWhiteSpace( description ) ) {
                    return;
                }

                //Throttle();
                var navigated = this.Navigate( description ).Wait( this.NavigationTimeout );
                if ( !navigated ) {
                    faucetID = BitcoinFaucets.AboutBlank;
                }
                if ( !description.StartsWith( "about:", StringComparison.OrdinalIgnoreCase ) ) {
                    Throttle();
                }

                switch ( faucetID ) {
                    case BitcoinFaucets.BitChestDotMe:
                        Visit_BitChestDotMe( bitcoinAddress: "1MpfkH1vDyGrmtykodJmzBNWi81KqXa8SE" );
                        break;

                    case BitcoinFaucets.LandOfBitCoinDotCom:
                        this.Visit_LandOfBitCoinDotCom();
                        break;

                    default:
                        //Visit_AboutBlank();
                        break;
                }
            }
            catch ( Exception exception ) {
                exception.Error();
            }
            finally {
                this.Throttle();
            }
        }

        /// <summary>
        /// <para>Defaults to <see cref="Seconds.Thirty"/> in the ctor.</para>
        /// </summary>
        public TimeSpan NavigationTimeout {
            get;
            set;
        }


        public void JsFireEvent( string getElementQuery, string eventName ) {
            var browser = this.WebBrowser1;
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

            Throttle( Seconds.Three );

            var allLinks = this.GetAllLinks().ToList();

            var links = allLinks.Where( uri => uri.PathAndQuery.Contains( bitcoinAddress ) ).ToList();

            foreach ( var link in links ) {
                try {
                    this.Navigate( String.Format( "http://www.bitchest.me/?a={0}", bitcoinAddress ) ).Wait( NavigationTimeout ); 
                    Throttle();
                    this.Navigate( link ).Wait( this.CancellationTokenSource.Token );
                    Throttle();
                    this.StartTheWholeCaptchaThing();
                }
                catch ( Exception exception ) {
                    exception.Error();
                }
                finally {
                    Throttle();
                }


                //TODO submit
            }

            Throttle( Seconds.Ten );

            Report.Exit();
        }

        private void StartTheWholeCaptchaThing() {
            var uri = this.GetBrowser1Location();

            var captcha = this.Captcha( uri );
            captcha.Status = CaptchaStatus.SearchingForChallenge;
            this.Captcha( uri, captcha );

            var cq = new CQ( GetBrowserHTML( this.WebBrowser1 ), HtmlParsingMode.Auto, HtmlParsingOptions.AllowSelfClosingTags, DocType.HTML5 );
            if ( cq.IsNullOrEmpty() ) {
                captcha.Status = CaptchaStatus.ChallengeNotFound;
                this.Captcha( uri, captcha );
                return;
            }

            var captchaChallenge = cq[ "recaptcha_challenge_image" ];

            if ( captchaChallenge.IsNullOrEmpty() ) {
                captchaChallenge = cq[ "adcopy-puzzle-image" ][ "img" ];
            }

            if ( captchaChallenge.IsNullOrEmpty() ) {
                captcha.Status = CaptchaStatus.ChallengeNotFound;
                this.Captcha( uri, captcha );
                return;
            }

            captcha.Status = CaptchaStatus.ChallengeFound;
            this.Captcha( uri, captcha );

            var captchaResponse = cq[ "recaptcha_response_field" ];

            if ( null != captchaChallenge && null != captchaResponse ) {
                this.GetCaptcha( uri, captchaChallenge, captchaResponse );
            }
            //TODO look for other captcha
            //TODO solve captcha (or try past answers)
        }

        /// <summary>
        /// Pull the image locally so we can take a look at it.
        /// </summary>
        /// <param name="challenge"></param>
        /// <param name="captchaChallenge"></param>
        /// <param name="captchaInputArea"></param>
        private void GetCaptcha( Uri challenge, CQ captchaChallenge, CQ captchaInputArea ) {
            try {
                Report.Enter();
                var imageSrc = captchaChallenge[ "src" ].Text();
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
                        var captcha = this.Captcha( imageUri );
                        captcha.Status = CaptchaStatus.LoadedImage;
                        this.Captcha( challenge, captcha );
                    }


                }
                catch ( Exception exception ) {
                    this.Captcha( imageUri ).Status = CaptchaStatus.ErrorLoadingImage;
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

            var bob = new CQ( GetBrowserHTML( this.WebBrowser1 ) );
            var text = bob[ "username" ].Text();
            bob[ "username" ].Text( "AIBrain" );

            //while ( !this.CancellationTokenSource.IsCancellationRequested ) {
            Throttle();
            //}
            //go to main page

        }

        private void Visit_AboutBlank() {
            this.Navigate( "about:blank" ).Wait( this.NavigationTimeout );
            Throttle();
        }
    }

}
#region License & Information
// This notice must be kept visible in the source.
// 
// This section of source code belongs to Rick@AIBrain.Org unless otherwise specified,
// or the original license has been overwritten by the automatic formatting of this code.
// Any unmodified sections of source code borrowed from other projects retain their original license and thanks goes to the Authors.
// 
// Donations and Royalties can be paid via
// PayPal: paypal@aibrain.org
// bitcoin:1Mad8TxTqxKnMiHuZxArFvX8BuFEB9nqX2
// bitcoin:1NzEsF7eegeEWDr5Vr9sSSgtUC4aL6axJu
// litecoin:LeUxdU2w3o6pLZGVys5xpDZvvo8DUrjBp9
// 
// Usage of the source code or compiled binaries is AS-IS.
// I am not responsible for Anything You Do.
// 
// Contact me by email if you have any questions or helpful criticism.
// 
// "UberScraper/Uber.cs" was last cleaned by Rick on 2014/09/15 at 4:50 PM
#endregion

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
    using Librainian.Measurement.Time;
    using Librainian.Persistence;
    using Librainian.Threading;

    public class Uber : IUber {
        private readonly ConcurrentDictionary<IDisposable, DateTime> _autoDisposables = new ConcurrentDictionary<IDisposable, DateTime>();

        [NotNull]
        private PersistTable<String, Captcha> _captchas;

        private WebControl _webBrowser1;

        private WebControl _webBrowser2;

        [NotNull]
        private PersistTable<String, WebSite> _webSites;

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

        public Boolean HasCaptchasBeenLoaded {
            get;
            private set;
        }

        public Boolean HasWebSitesBeenLoaded {
            get;
            private set;
        }

        /// <summary>
        ///     <para>Defaults to <see cref="Seconds.Thirty" /> in the ctor.</para>
        /// </summary>
        public TimeSpan NavigationTimeout {
            get;
            set;
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
                //this._webBrowser1.LoadingFrameComplete += ( sender, args ) => { };
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
                //this._webBrowser2.LoadingFrameComplete += ( sender, args ) => { };
                this._autoDisposables.TryAdd( value, DateTime.Now );
            }
        }

        /// <summary>
        ///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose() {
            try {
                Report.Enter();

                foreach ( var disposable in this._autoDisposables.Where( pair => null != pair.Key ).OrderByDescending( pair => pair.Value ) ) {
                    try {
                        Console.Write( String.Format( "Disposing of {0}...", disposable.Key.ToString() ) );
                        disposable.Key.Dispose();
                        Console.WriteLine( String.Format( "Disposed.", disposable.Key.ToString() ) );
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

        public void SetBrowser( [CanBeNull] WebControl webBrowser ) {
            this.WebBrowser1 = webBrowser;
        }

        /// <summary>
        ///     <para>Load tables.</para>
        /// </summary>
        /// <returns></returns>
        public async Task<Boolean> Init() {
            if ( this.CancellationTokenSource.IsCancellationRequested ) {
                return false;
            }

            await this.LoadDictionaries( this.CancellationTokenSource.Token );

            return true;
        }

        [CanBeNull]
        public static String GetBrowserHTML( WebControl webBrowser ) {
            try {
                if ( webBrowser != null ) {
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

        [CanBeNull]
        public static String GetBrowserText( WebControl webBrowser ) {
            try {
                if ( webBrowser != null ) {
                    var result = webBrowser.Invoke( new Func<string>( () => webBrowser.ExecuteJavascriptWithResult( "document.getElementsByTagName('html')[0].innerText" ) ) );

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

        [CanBeNull]
        public static dynamic GetElementByID( WebControl webBrowser, String id ) {
            try {
                if ( webBrowser != null ) {
                    var answer = webBrowser.Invoke( new Func<JSObject>( () => {
                        dynamic document = ( JSObject )webBrowser.ExecuteJavascriptWithResult( "document" );
                        using ( document ) {
                            try {
                                return document.getElementById( id );
                            }
                            catch ( Exception exception ) {
                                exception.Error();
                            }
                            return null;
                        }
                    } ) );
                    return answer;

                }
            }
            catch ( Exception exception ) {
                exception.Error();
            }
            return null;
        }

        [CanBeNull]
        public static dynamic GetElementsByTagName( WebControl webBrowser, String type ) {
            try {
                if ( webBrowser != null ) {
                    var answer = webBrowser.Invoke( new Func<JSObject>( () => {
                        dynamic document = ( JSObject )webBrowser.ExecuteJavascriptWithResult( "document" );
                        using ( document ) {
                            try {
                                return document.getElementsByTagName( type );
                            }
                            catch ( Exception exception ) {
                                exception.Error();
                            }
                            return null;
                        }
                    } ) );
                    return answer;

                }
            }
            catch ( Exception exception ) {
                exception.Error();
            }
            return null;
        }

        [CanBeNull]
        public static dynamic PushButton( WebControl webBrowser, String type, int index ) {
            try {
                if ( webBrowser != null ) {
                    var answer = webBrowser.Invoke( new Func<JSObject>( () => {
                        dynamic document = ( JSObject )webBrowser.ExecuteJavascriptWithResult( String.Format( "document.getElementsByTagName('submit')[{0}].click();", index ) );
                        return document;
                    } ) );
                    return answer;

                }
            }
            catch ( Exception exception ) {
                exception.Error();
            }
            return null;
        }

        /// <summary>
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

        /*
                /// <summary>
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
        */

        /// <summary>
        ///     <para>Gets and sets a <see cref="Captcha"/>.</para>
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="value"></param>
        public Captcha this[ [CanBeNull] Uri uri ] {
            set {
                if ( null == uri || null == value ) {
                    return;
                }
                this._captchas[ uri.AbsoluteUri ] = value;
            }
            get {
                if ( null == uri ) {
                    return new Captcha();
                }

                if ( null == this._captchas[ uri.AbsoluteUri ] ) {
                    this._captchas[ uri.AbsoluteUri ] = new Captcha();
                }

                if ( null == this._captchas[ uri.AbsoluteUri ].Uri ) {
                    this._captchas[ uri.AbsoluteUri ].Uri = uri;
                }

                return this._captchas[ uri.AbsoluteUri ];
            }
        }

        public void EnsureWebsite( [CanBeNull] Uri uri ) {
            if ( null == uri ) {
                return;
            }

            this._webSites.Should().NotBeNull();

            if ( null == this._webSites[ uri.AbsoluteUri ] ) {
                this._webSites[ uri.AbsoluteUri ] = new WebSite();
            }
            this._webSites[ uri.AbsoluteUri ].Location = uri;
        }

        public static IEnumerable<Uri> GetAllLinks( WebControl webBrowser ) {
            //var allLinks = new List<Uri>( 128 );
            //const string javascript = " var arr = []; var l = document.getElementsByTagName('a'); for ( var i=0; i<l.length; i++) { arr.push(l[i].href); }  ";
            //const string javascript = " var arr = []; var l = document.getElementsByTagName('a'); for ( var i=0; i<l.length; i++) { arr.push(l[i].href); }; return arr; ";
            //const string javascript = " document.getElementsByTagName('a'); ";
            //const string javascript = "document";
            //const string javascript = "document.links";
            //const string javascript = "document.links";

            var html = GetBrowserHTML( webBrowser );

            var cq = new CQ( html, HtmlParsingMode.Auto, HtmlParsingOptions.AllowSelfClosingTags, DocType.HTML5 );

            var anchors = cq[ "a" ].ToList();

            foreach ( var href in anchors.Select( domObject => domObject[ "href" ] ) ) {
                Uri uri;
                if ( !Uri.TryCreate( href, UriKind.Absolute, out uri ) ) {
                    continue;
                }
                yield return uri;
            }
        }

        /// <summary>
        ///     Retrieve the <see cref="Uri" /> the <see cref="WebBrowser1" /> is currently at.
        /// </summary>
        /// <param name="webBrowser"></param>
        /// <returns></returns>
        [NotNull]
        public static Uri GetBrowserLocation( WebControl webBrowser ) {
            try {
                var browser = webBrowser;
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

        /*
                [ CanBeNull ]
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
        */

        public void JsFireEvent( string getElementQuery, string eventName ) {
            var browser = this.WebBrowser1;
            if ( browser != null ) {
                browser.ExecuteJavascript( string.Format( @"
                            function fireEvent(element,event) {{
                                var evt = document.createEvent('HTMLEvents');
                                evt.initEvent(event, true, true ); // event type,bubbling,cancelable
                                element.dispatchEvent(evt);
                            }}
                            {0}", String.Format( "fireEvent({0}, '{1}');", getElementQuery, eventName ) ) );
            }
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

        ///// <summary>
        /////     <para>Starts a <see cref="Task" /> to navigate to the specified <paramref name="uri" />.</para>
        ///// </summary>
        ///// <param name="uri"></param>
        ///// <returns></returns>
        //public Task Navigate( [NotNull] Uri uri ) {
        //    return Task.Run( () => {
        //        this.NavigateTo( uri );
        //    } );
        //}

        ///// <summary>
        /////     <para>Starts a <see cref="Task" /> to navigate to the specified <paramref name="uriString" />.</para>
        ///// </summary>
        ///// <param name="uriString"></param>
        ///// <returns></returns>
        //public Task Navigate( String uriString ) {
        //    return Task.Run( () => {
        //        Uri uri;
        //        if ( Uri.TryCreate( uriString, UriKind.Absolute, out uri ) ) {
        //            this.NavigateTo( uri );
        //        }
        //    } );
        //}

        public Boolean Navigate( [NotNull] String uri ) {
            return this.Navigate( new Uri( uri ) );
        }

        /// <summary>
        ///     <para>No guarantee that more ajax/javascript can and will fire off after this is 'true'.</para>
        ///     <para>Internal while loop blocks.</para>
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        /// <seealso cref="http://answers.awesomium.com/questions/3971/loading-script-complete.html" />
        public Boolean Navigate( [NotNull] Uri uri ) {
            if ( null == uri ) {
                throw new ArgumentNullException( "request" );
            }

            try {
                var watchdog = Stopwatch.StartNew();

                if ( this.CancellationTokenSource.Token.IsCancellationRequested ) {
                    return false;
                }

                var webBrowser = this.WebBrowser1;
                if ( webBrowser == null ) {
                    return false;
                }
                webBrowser.Invoke( method: new Action( () => {

                    Report.Before( String.Format( "Navigating to {0}...", uri ) );

                    this.EnsureWebsite( uri );

                    webBrowser.Source = uri;

                    while ( webBrowser.IsLoading || webBrowser.IsNavigating ) {
                        this.Throttle();
                        WebCore.Update();
                        Application.DoEvents();
                        if ( this.CancellationTokenSource.Token.IsCancellationRequested ) {
                            break;
                        }
                        if ( watchdog.Elapsed >= NavigationTimeout ) {
                            Report.Before( "*navigation^timed->out*" );
                            break;
                        }
                    }

                    Report.After( "done navigating." );
                } ) );

                return webBrowser.IsDocumentReady && webBrowser.IsResponsive;
            }
            catch ( Exception exception ) {
                Debug.WriteLine( exception.Message );
            }
            return false;
        }

        public void Stop() {
            Console.WriteLine( "Requesting source token cancel..." );
            this.CancellationTokenSource.Cancel();

            var webBrowser = this.WebBrowser1;
            if ( webBrowser != null ) {
                Console.WriteLine( "Requesting browser stops..." );
                webBrowser.Stop();
            }
        }

        public void VisitSites( CancellationToken cancellationToken ) {
            var faucets = ( BitcoinFaucets[] )Enum.GetValues( typeof( BitcoinFaucets ) );

            Console.WriteLine( "Visiting websites..." );
            foreach ( var faucetID in faucets.OrderBy( bitcoinFaucets => ( int )bitcoinFaucets ) ) {
                this.Visit( faucetID, cancellationToken );
                if ( cancellationToken.IsCancellationRequested ) {
                    break;
                }
            }
        }

        [NotNull]
        private static string GetDescription( BitcoinFaucets faucet ) {
            var fi = faucet.GetType().GetField( faucet.ToString() );
            var attributes = ( DescriptionAttribute[] )fi.GetCustomAttributes( typeof( DescriptionAttribute ), false );
            return attributes.Length > 0 ? attributes[ 0 ].Description : String.Empty;
        }

        /// <summary>
        ///     Pull the image locally so we can take a look at it.
        /// </summary>
        /// <param name="challenge"></param>
        /// <param name="captchaChallenge"></param>
        /// <param name="captchaInput"></param>
        /// <param name="cancellationToken"></param>
        private void GetCaptcha( Uri challenge, dynamic captchaChallenge, dynamic captchaInput, CancellationToken cancellationToken ) {
            try {
                Report.Enter();

                if ( cancellationToken.IsCancellationRequested ) {
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
                    bitmapImage.UriSource = captchaChallenge;
                    bitmapImage.EndInit();

                    if ( cancellationToken.IsCancellationRequested ) {
                        return;
                    }

                    if ( isImageGood && bitmapImage.Width > 0 && bitmapImage.Height > 0 ) {
                        var captcha = this[ captchaChallenge ];
                        captcha.Status = CaptchaStatus.LoadedImage;
                        this[ challenge ] = captcha;
                    }
                }
                catch ( Exception exception ) {
                    this[ captchaChallenge ].Status = CaptchaStatus.ErrorLoadingImage;
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

        private async Task<bool> LoadDictionaries( CancellationToken token ) {
            if ( !this.HasWebSitesBeenLoaded ) {
                this.HasWebSitesBeenLoaded = await Task.Run( () => this.LoadWebsites(), token );
            }
            if ( !this.HasCaptchasBeenLoaded ) {
                this.HasCaptchasBeenLoaded = await Task.Run( () => this.LoadCaptchas(), token );
            }
            return this.HasWebSitesBeenLoaded && this.HasCaptchasBeenLoaded;
        }

        private void StartTheWholeCaptchaThing( CancellationToken cancellationToken ) {

            var uri = GetBrowserLocation( this.WebBrowser1 );
            var captcha = this[ uri ];

            // 1. check if the page shows any sort of countdown/notreadyyet
            var text = GetBrowserText( this.WebBrowser1 ) ?? String.Empty;
            if ( text.Contains( "You must wait" ) ) {
                return;
            }

            // 2. find the captcha image uri
            // 3. solve (OCR) the captcha image
            // 4. find the captcha response textbox
            // 5. respond (type) the response
            // 6. go onto another page (dont keep retrying on this page until later in the cycles)

            captcha.Status = CaptchaStatus.ChallengeSearchingFor;
            this[ uri ] = captcha;

            dynamic submitButton = null;
            dynamic captchaChallenge = GetElementByID( this.WebBrowser1, "recaptcha_challenge_image" );
            dynamic captchaInput = GetElementByID( this.WebBrowser1, "recaptcha_response_field" );

            dynamic buttons = GetElementsByTagName( this.WebBrowser1, "document.getElementsByTagName('button');" );
            if ( null == buttons ) {
                return;
            }

            for ( var i = 0 ; i < buttons.length ; i++ ) {
                var button = buttons[ i ];
                if ( "submit" == button.getAttribute( "type" ) ) {
                    submitButton = button;
                }
            }


            if ( null != captchaChallenge && null != captchaInput ) {
                captcha.Status = CaptchaStatus.ChallengeFound;
                captcha.ResponseID = "recaptcha_challenge_image";
                captcha.ResponseID = "recaptcha_response_field";
                if ( submitButton != null ) {
                    captcha.SubmitID = submitButton.id;
                }
                this[ uri ] = captcha;

                this.GetCaptcha( challenge: uri, captchaChallenge: captchaChallenge, captchaInput: captchaInput, cancellationToken: cancellationToken );
                return;
            }

            //var html = GetBrowserHTML( this.WebBrowser1 );

            //var cq = new CQ( html, HtmlParsingMode.Auto, HtmlParsingOptions.AllowSelfClosingTags, DocType.HTML5 );

            //if ( cq.IsNullOrEmpty() ) {
            //    captcha.Status = CaptchaStatus.ChallengeNotFound;
            //    this[ uri ] = captcha;
            //    return;
            //}


            //var captchaChallenge = cq[ "recaptcha_challenge_image" ];

            //if ( captchaChallenge.IsNullOrEmpty() ) {
            //    var captchaChallenges = cq[ "adcopy-puzzle-image" ][ "img" ].ToList();
            //}

            //if ( captchaChallenge.IsNullOrEmpty() ) {
            //    captcha.Status = CaptchaStatus.ChallengeNotFound;
            //    this[ uri ] = captcha;
            //    return;
            //}

            //captcha.Status = CaptchaStatus.ChallengeFound;
            //this[ uri ] = captcha;

            //var captchaResponse = cq[ "" ];

            //if ( cancellationToken.IsCancellationRequested ) {
            //    return;
            //}

            //if ( null != captchaChallenge && null != captchaResponse ) {
            //    this.GetCaptcha( uri, captchaChallenge, captchaResponse, cancellationToken );
            //}

            //TODO look for other captcha
            //TODO solve captcha (or try past answers)
        }

        private void Throttle( TimeSpan? until = null ) {
            if ( !until.HasValue ) {
                until = Seconds.One;
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

        private void Visit( BitcoinFaucets faucetID, CancellationToken cancellationToken ) {
            try {
                var description = GetDescription( faucetID );   //in BitcoinFaucets case, the description is a uri
                if ( String.IsNullOrWhiteSpace( description ) ) {
                    return;
                }

                var uri = new Uri( description );
                Console.WriteLine( "Visiting (#{0}) {1} @ {2}", faucetID, description, uri.PathAndQuery );


                var navigated = this.Navigate( description );   //.Wait( ( int )this.NavigationTimeout.TotalMilliseconds, cancellationTokenSource.Token );

                if ( !navigated ) {
                    faucetID = BitcoinFaucets.AboutBlank;
                }
                if ( !description.StartsWith( "about:", StringComparison.OrdinalIgnoreCase ) ) {
                    this.Throttle();
                }

                switch ( faucetID ) {
                    case BitcoinFaucets.BitChestDotMe:
                        this.Visit_BitChestDotMe( "1KEEP1Wd6KKVHJrBaB45cSHXzMJu9VWWAt", cancellationTokenSource.Token );
                        break;

                    case BitcoinFaucets.LandOfBitCoinDotCom:
                        this.Visit_LandOfBitCoinDotCom();
                        break;

                    default:
                        this.Visit_AboutBlank();
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

        private void Visit_AboutBlank() {
            this.Navigate( "about:blank" ); //.Wait( this.NavigationTimeout );
            this.Throttle();
        }

        private void Visit_BitChestDotMe( string bitcoinAddress, CancellationToken cancellationToken ) {
            Report.Enter();
            if ( !this.Navigate( String.Format( "http://www.bitchest.me/?a={0}", bitcoinAddress ) ) ) {   //.Wait( this.NavigationTimeout ) ) 
                return;
            }

            this.Throttle( Seconds.Three );

            if ( cancellationToken.IsCancellationRequested ) {
                return;
            }

            var links = GetAllLinks( this.WebBrowser1 ).Where( uri => uri.PathAndQuery.Contains( bitcoinAddress ) ).ToList();

            this.Throttle( Seconds.Three );

            foreach ( var link in links ) {
                if ( cancellationToken.IsCancellationRequested ) {
                    return;
                }
                try {
                    this.Navigate( String.Format( "http://www.bitchest.me/?a={0}", bitcoinAddress ) );   //.Wait( this.NavigationTimeout );
                    this.Throttle();
                    this.Navigate( link );
                    this.Throttle();
                    try {
                        this.StartTheWholeCaptchaThing( cancellationToken );
                    }
                    catch ( Exception exception ) {
                        exception.Error();
                    }
                }
                catch ( Exception exception ) {
                    exception.Error();
                }
                finally {
                    this.Throttle();
                }

                //TODO submit
            }

            Report.Exit();
        }

        private void Visit_LandOfBitCoinDotCom() {
            if ( !this.Navigate( "https://www.landofbitcoin.com/login" ) ) {    //.Wait( this.NavigationTimeout )
                return;
            }

            var bob = new CQ( GetBrowserHTML( this.WebBrowser1 ) );
            var text = bob[ "username" ].Text();
            bob[ "username" ].Text( "AIBrain" );

            //while ( !this.CancellationTokenSource.IsCancellationRequested ) {
            this.Throttle();

            //}
            //go to main page
        }
    }
}

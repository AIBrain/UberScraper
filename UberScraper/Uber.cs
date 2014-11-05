// This notice must be kept visible in the source.
//
// This section of source code belongs to Rick@AIBrain.Org unless otherwise specified, or the
// original license has been overwritten by the automatic formatting of this code. Any unmodified
// sections of source code borrowed from other projects retain their original license and thanks
// goes to the Authors.
//
// Donations and Royalties can be paid via
// PayPal: paypal@aibrain.org
// bitcoin: 1Mad8TxTqxKnMiHuZxArFvX8BuFEB9nqX2
// bitcoin: 1NzEsF7eegeEWDr5Vr9sSSgtUC4aL6axJu
// litecoin: LeUxdU2w3o6pLZGVys5xpDZvvo8DUrjBp9
//
// Usage of the source code or compiled binaries is AS-IS. I am not responsible for Anything You Do.
//
// Contact me by email if you have any questions or helpful criticism.
//
// "UberScraper/Uber.cs" was last cleaned by Rick on 2014/09/22 at 11:04 AM

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
    using AForge.Imaging.Filters;
    using Awesomium.Core;
    using Awesomium.Windows.Forms;
    using CsQuery;
    using FluentAssertions;
    using Librainian;
    using Librainian.Annotations;
    using Librainian.Collections;
    using Librainian.Controls;
    using Librainian.Internet;
    using Librainian.IO;
    using Librainian.Linguistics;
    using Librainian.Measurement.Time;
    using Librainian.Persistence;
    using Librainian.Threading;
    using Properties;
    using Tesseract;
    using ImageFormat = System.Drawing.Imaging.ImageFormat;

    public class Uber : IUber {

        [CanBeNull]
        public TesseractEngine TesseractEngine;

        private readonly ConcurrentDictionary<IDisposable, DateTime> _autoDisposables = new ConcurrentDictionary<IDisposable, DateTime>();

        [NotNull]
        private PersistTable<String, Captcha> _captchaDatabase;

        [NotNull]
        private PersistTable<String, String> _pastAnswers;

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

        public Boolean HasCaptchaDatabaseBeenConnected {
            get;
            private set;
        }

        public Boolean HasPastAnswersBeenConnected {
            get;
            private set;
        }

        public bool HasTessEngineBeenConnected {
            get;
            set;
        }

        public Boolean HasWebSitesDatabaseBeenConnected {
            get;
            private set;
        }

        /// <summary>
        /// <para>Defaults to <see cref="Seconds.Thirty" /> in the ctor.</para>
        /// </summary>
        public TimeSpan NavigationTimeout {
            get;
            set;
        }

        [NotNull]
        public PictureBox PictureBoxChallenge {
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

        public static Boolean ByIDFunction( WebControl webBrowser, String id, String function ) {
            try {
                if ( webBrowser != null ) {
                    webBrowser.Invoke( new Action( () => webBrowser.ExecuteJavascript( String.Format( "document.getElementById( {0} ).{1}();", id, function ) ) ) );
                    return true;
                }
            }
            catch ( Exception exception ) {
                exception.Error();
            }
            return false;
        }

        public static Boolean ByIDSetValue( WebControl webBrowser, String id, String text ) {
            try {
                if ( webBrowser != null ) {
                    webBrowser.Invoke( new Action( () => webBrowser.ExecuteJavascript( String.Format( "document.getElementById( {0} ).value=\"{1}\";", id, text ) ) ) );
                    return true;
                }
            }
            catch ( Exception exception ) {
                exception.Error();
            }
            return false;
        }

        public static Boolean ClickSubmit( WebControl webBrowser, int index = 0 ) {
            try {
                if ( webBrowser != null ) {
                    webBrowser.Invoke( new Action( () => webBrowser.ExecuteJavascript( String.Format( "document.querySelectorAll(\"button[type='submit']\")[{0}].click();", index ) ) ) );
                    return true;
                }
            }
            catch ( Exception exception ) {
                exception.Error();
            }
            return false;
        }

        public static IEnumerable<Uri> GetAllLinks( WebControl webBrowser ) {
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

        /// <summary>
        /// Retrieve the <see cref="Uri" /> the <see cref="WebBrowser1" /> is currently at.
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

        public Boolean ConnectDatabase_Captchas() {
            try {
                Report.Enter();

                this._captchaDatabase = new PersistTable<String, Captcha>( Environment.SpecialFolder.CommonApplicationData, "Captchas" );

                if ( null != this._captchaDatabase ) {
                    this._autoDisposables.TryAdd( this._captchaDatabase, DateTime.Now );
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

            return null != this._captchaDatabase;
        }

        public Boolean ConnectDatabase_PastAnswers() {
            try {
                Report.Enter();

                this._pastAnswers = new PersistTable<String, String>( Environment.SpecialFolder.CommonApplicationData, "PastAnswers" );

                if ( null != this._pastAnswers ) {
                    this._autoDisposables.TryAdd( this._captchaDatabase, DateTime.Now );
                    return true;
                }
            }
            catch ( InvalidOperationException ) {
            }
            catch ( PathTooLongException ) {
            }
            catch ( DirectoryNotFoundException ) {
            }
            catch ( FileNotFoundException ) {
            }
            finally {
                Report.Exit();
            }

            return false;
        }

        public Boolean ConnectDatabase_Websites() {
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
        /// Performs application-defined tasks associated with freeing, releasing, or resetting
        /// unmanaged resources.
        /// </summary>
        public void Dispose() {
            try {
                Report.Enter();

                foreach ( var disposable in this._autoDisposables.Where( pair => null != pair.Key ).OrderByDescending( pair => pair.Value ) ) {
                    try {
                        Console.Write( String.Format( "Disposing of {0}...", disposable.Key ) );
                        disposable.Key.Dispose();
                        Console.WriteLine( String.Format( "Disposed of {0}.", disposable.Key ) );
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

        /// <summary>
        /// <para>Load tables.</para>
        /// </summary>
        /// <returns></returns>
        public async Task<Boolean> Init() {
            if ( this.CancellationTokenSource.IsCancellationRequested ) {
                return false;
            }

            if ( !HasTessEngineBeenConnected ) {
                HasTessEngineBeenConnected = await Task.Run( () => {
                    try {
                        this.TesseractEngine = new TesseractEngine( @"tessdata", "eng", EngineMode.Default );
                        return null != this.TesseractEngine;
                    }
                    catch ( TesseractException exception ) {
                        exception.Error();
                        return false;
                    }
                }, this.CancellationTokenSource.Token );
            }

            if ( !this.HasWebSitesDatabaseBeenConnected ) {
                this.HasWebSitesDatabaseBeenConnected = await Task.Run( () => this.ConnectDatabase_Websites(), this.CancellationTokenSource.Token );
            }
            if ( !this.HasCaptchaDatabaseBeenConnected ) {
                this.HasCaptchaDatabaseBeenConnected = await Task.Run( () => this.ConnectDatabase_Captchas(), this.CancellationTokenSource.Token );
            }
            if ( !this.HasPastAnswersBeenConnected ) {
                this.HasPastAnswersBeenConnected = await Task.Run( () => this.ConnectDatabase_PastAnswers(), this.CancellationTokenSource.Token );
            }

            return HasTessEngineBeenConnected && this.HasWebSitesDatabaseBeenConnected && this.HasCaptchaDatabaseBeenConnected && this.HasPastAnswersBeenConnected;
        }

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

        public Boolean Navigate( [NotNull] String uri ) {
            return this.Navigate( new Uri( uri ) );
        }

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
                        WebCore.Update();
                        this.Throttle();
                        Application.DoEvents();
                        if ( this.CancellationTokenSource.Token.IsCancellationRequested ) {
                            break;
                        }
                        if ( watchdog.Elapsed < this.NavigationTimeout ) {
                            continue;
                        }
                        Report.Before( "*navigation^timed->out*" );
                        break;
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

        /// <summary>
        /// <para>Gets and sets a <see cref="Captcha" /> .</para>
        /// </summary>
        /// <param name="uri"></param>
        [NotNull]
        public Captcha PullCaptchaData( [NotNull] Uri uri ) {
            if ( uri == null ) {
                throw new ArgumentNullException( "uri" );
            }

            if ( null == this._captchaDatabase[ uri.AbsoluteUri ] ) {
                var captcha = new Captcha {
                    Uri = uri
                };
                this._captchaDatabase[ uri.AbsoluteUri ] = captcha;
            }

            return this._captchaDatabase[ uri.AbsoluteUri ];
        }

        public void SetBrowser( [CanBeNull] WebControl webBrowser ) {
            this.WebBrowser1 = webBrowser;
        }

        public void Stop() {
            Console.WriteLine( "Requesting source token cancel..." );
            this.CancellationTokenSource.Cancel();

            var webBrowser = this.WebBrowser1;
            if ( webBrowser != null ) {
                Console.WriteLine( "Requesting browser stops..." );
                webBrowser.Invoke( new Action( webBrowser.Stop ) );
            }
        }

        /// <summary>
        /// <para>Gets and sets a <see cref="Captcha" /> .</para>
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="captcha"></param>
        public void UpdateCaptchaData( Captcha captcha ) {
            if ( null == captcha || null == captcha.Uri ) {
                return;
            }
            this._captchaDatabase[ captcha.Uri.AbsoluteUri ] = captcha;
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
        /// <para>Pulls the image</para>
        /// <para>Runs the ocr on it</para>
        /// <para>fills in the blanks</para>
        /// <para>submits the page</para>
        /// </summary>
        /// <param name="challenge"></param>
        /// <param name="cancellationToken"></param>
        /// <param name="answer"></param>
        /// <returns></returns>
        private bool SolveCaptcha( Uri challenge, CancellationToken cancellationToken, out String answer ) {
            answer = null;
            var tesseractEngine = this.TesseractEngine;
            if ( null == tesseractEngine ) {
                return false;
            }

            var captchaData = this.PullCaptchaData( challenge );

            if ( captchaData.ImageUri == null ) {
                captchaData.Status = CaptchaStatus.NoImageFoundToBeSolved;
                this.UpdateCaptchaData( captchaData );
                return false;
            }

            Console.WriteLine( Resources.Uber_SolveCaptcha_Attempting_OCR_on__0_, captchaData.ImageUri.AbsolutePath );

            captchaData.Status = CaptchaStatus.SolvingImage;
            this.UpdateCaptchaData( captchaData );

            var folder = new Folder( Path.GetTempPath() );

            Document document;
            folder.TryGetTempDocument( document: out document, extension: "png" );

            this.PictureBoxChallenge.Image.Save( document.FullPathWithFileName, ImageFormat.Png );

            var aforgeImage = AForge.Imaging.Image.FromFile( document.FullPathWithFileName );

            var smoothing = new ConservativeSmoothing();

            var cannyEdgeDetector = new CannyEdgeDetector();

            cannyEdgeDetector.Apply( aforgeImage );

            aforgeImage.Save( document.FullPathWithFileName, ImageFormat.Png );

            this.PictureBoxChallenge.ImageLocation = document.FullPathWithFileName;

            this.PictureBoxChallenge.Load();

            this.Throttle( Seconds.Ten );

            using ( var img = Pix.LoadFromFile( document.FullPathWithFileName ).Deskew() ) {

                using ( var page = tesseractEngine.Process( img, PageSegMode.SingleLine ) ) {

                    answer = page.GetText();

                    var paragraph = new Paragraph( answer );

                    answer = new Sentence( paragraph.ToStrings( " " ) ).ToStrings( " " );

                    FluentTimers.Create( Minutes.One, () => document.Delete() ).AndStart();

                    if ( !String.IsNullOrWhiteSpace( answer ) ) {
                        captchaData.Status = CaptchaStatus.SolvedChallenge;
                        this.UpdateCaptchaData( captchaData );
                        return true;
                    }

                    return false;
                }
            }
        }

        private void StartTheCaptchaStuff( CancellationToken cancellationToken ) {
            var uri = GetBrowserLocation( this.WebBrowser1 );

            // Check if the page shows any sort of countdown/NotReadyYet
            var text = GetBrowserText( this.WebBrowser1 ) ?? String.Empty;
            if ( text.Contains( "You must wait" ) ) {
                return;
            }

            // 2. find the captcha image uri

            var captchaData = this.PullCaptchaData( uri );

            captchaData.Status = CaptchaStatus.SearchingForChallenge;
            this.UpdateCaptchaData( captchaData );

            //method 1
            var captchaChallenge = GetElementByID( this.WebBrowser1, "recaptcha_challenge_image" );
            var captchaInput = GetElementByID( this.WebBrowser1, "recaptcha_response_field" ); // find the captcha response textbox

            if ( null != captchaChallenge && null != captchaInput ) {
                captchaData.Status = CaptchaStatus.ChallengeFound;

                captchaData.ChallengeElementID = captchaChallenge.id;

                captchaData.ResponseElementID = captchaInput.id;

                captchaData.ImageUri = new Uri( captchaChallenge.src );

                captchaData.Status = CaptchaStatus.LoadingImage;
                this.UpdateCaptchaData( captchaData );

                this.PictureBoxChallenge.Flash();
                this.PictureBoxChallenge.Load( captchaChallenge.src );
                this.PictureBoxChallenge.Flash();

                //captchaData.Image = this.PictureBoxChallenge.Image.Clone() as Image;
                captchaData.Status = CaptchaStatus.LoadedImage;
                this.UpdateCaptchaData( captchaData );

                String answer;
                if ( this.SolveCaptcha( uri, cancellationToken, out answer ) ) {

                    ByIDFunction( this.WebBrowser1, captchaData.ResponseElementID, "focus" );
                    ByIDFunction( this.WebBrowser1, captchaData.ResponseElementID, "scrollIntoView" );
                    ByIDFunction( this.WebBrowser1, captchaData.ResponseElementID, "click" );
                    ByIDSetValue( this.WebBrowser1, captchaData.ResponseElementID, answer );

                    this.Throttle( Seconds.Five );

                    ClickSubmit( this.WebBrowser1 );

                    this.PictureBoxChallenge.Image = null;
                    this.PictureBoxChallenge.InitialImage = null;

                    this.Throttle( Seconds.Ten );

                    return;
                }

                captchaData.Status = CaptchaStatus.ChallengeStillNotSolved;
                this.UpdateCaptchaData( captchaData );
            }

            //TODO look for other captcha types (methods to find and solve them)

            //method 2 ....
            if ( false ) {

                return;
            }

            //method 3 ....
            if ( false ) {

                return;
            }
            captchaData.Status = CaptchaStatus.NoChallengesFound;
            this.UpdateCaptchaData( captchaData );
        }

        private void Throttle( TimeSpan? until = null ) {

            //TODO look into that semaphore wategate thing...
            if ( !until.HasValue ) {
                until = Seconds.One;
            }
            var watch = Stopwatch.StartNew();
            do {
                Application.DoEvents();
                if ( watch.Elapsed < until.Value ) {
                    Thread.Sleep( Milliseconds.Hertz111 );
                    Application.DoEvents();
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
                var description = GetDescription( faucetID ); //in BitcoinFaucets case, the description is a uri
                if ( String.IsNullOrWhiteSpace( description ) ) {
                    return;
                }

                var uri = new Uri( description );
                Console.WriteLine( "Visiting (#{0}) {1} @ {2}", faucetID, description, uri.PathAndQuery );

                var navigated = this.Navigate( description );

                if ( !navigated ) {
                    faucetID = BitcoinFaucets.AboutBlank;
                }
                if ( !description.StartsWith( "about:", StringComparison.OrdinalIgnoreCase ) ) {
                    this.Throttle();
                }

                switch ( faucetID ) {
                    case BitcoinFaucets.BitChestDotMe:
                        this.Visit_BitChestDotMe( "1KEEP1Wd6KKVHJrBaB45cSHXzMJu9VWWAt", cancellationToken );
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

            //this.Throttle();
        }

        private void Visit_BitChestDotMe( string bitcoinAddress, CancellationToken cancellationToken ) {
            Report.Enter();
            if ( !this.Navigate( String.Format( "http://www.bitchest.me/?a={0}", bitcoinAddress ) ) ) {

                //.Wait( this.NavigationTimeout ) )
                return;
            }

            this.Throttle();

            if ( cancellationToken.IsCancellationRequested ) {
                return;
            }

            var links = GetAllLinks( this.WebBrowser1 ).Where( uri => uri.PathAndQuery.Contains( bitcoinAddress ) ).ToList();

            this.Throttle();

            foreach ( var link in links ) {
                if ( cancellationToken.IsCancellationRequested ) {
                    return;
                }
                try {
                    this.Navigate( String.Format( "http://www.bitchest.me/?a={0}", bitcoinAddress ) ); //.Wait( this.NavigationTimeout );
                    this.Throttle();
                    this.Navigate( link );

                    //this.Throttle();
                    try {
                        this.StartTheCaptchaStuff( cancellationToken );
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
            if ( !this.Navigate( "https://www.landofbitcoin.com/login" ) ) {

                //.Wait( this.NavigationTimeout )
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
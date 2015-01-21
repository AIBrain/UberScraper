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
// "UberScraper 2015/Uber.cs" was last cleaned by Rick on 2015/01/21 at 5:20 AM
#endregion

namespace UberScraper {
    using System;
    using System.Collections.Concurrent;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows.Forms;
    using AForge.Imaging;
    using AForge.Imaging.Filters;
    using Awesomium.Windows.Forms;
    using FluentAssertions;
    using JetBrains.Annotations;
    using Librainian.Collections;
    using Librainian.Internet;
    using Librainian.Internet.Browser;
    using Librainian.IO;
    using Librainian.Linguistics;
    using Librainian.Measurement.Time;
    using Librainian.Parsing;
    using Librainian.Persistence;
    using Librainian.Threading;
    using Properties;
    using Tesseract;
    using ImageFormat = System.Drawing.Imaging.ImageFormat;

    public class Uber : IDisposable {
        [NotNull]
        private readonly ConcurrentStack<IDisposable> _autoDisposables = new ConcurrentStack<IDisposable>();
        [NotNull]
        private PersistTable<String, Captcha> _captchaDatabase;
        [NotNull]
        private PersistTable<String, String> _pastAnswers;
        [NotNull]
        private PersistTable<String, WebSite> _webSites;
        [CanBeNull]
        public TesseractEngine TesseractEngine;

        public Uber( [NotNull] TabControl tabControls ) {
            if ( tabControls == null ) {
                throw new ArgumentNullException( "tabControls" );
            }
            this.TabControls = tabControls;
            this.CancellationTokenSource = new CancellationTokenSource();
            this.NavigationTimeout = Seconds.Thirty;
        }

        [NotNull]
        public TabControl TabControls { get; private set; }

        [NotNull]
        public CancellationTokenSource CancellationTokenSource { get; private set; }

        public Boolean HasCaptchaDatabaseBeenConnected { get; private set; }
        public Boolean HasPastAnswersBeenConnected { get; private set; }
        public bool HasTessEngineBeenConnected { get; set; }
        public Boolean HasWebSitesDatabaseBeenConnected { get; private set; }

        /// <summary>
        ///     <para>Defaults to <see cref="Seconds.Thirty" /> in the ctor.</para>
        /// </summary>
        public TimeSpan NavigationTimeout { get; set; }

        [NotNull]
        public PictureBox PictureBoxChallenge { get; set; }

        /// <summary>
        ///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose() {
            try {
                Log.Enter();

                foreach ( var disposable in this._autoDisposables ) {
                    try {
                        Console.Write( "Disposing of {0}...", disposable );
                        using (disposable) {
                            disposable.Dispose();
                        }
                        Console.WriteLine( "Disposed of {0}.", disposable );
                    }
                    catch ( Exception exception ) {
                        exception.More();
                    }
                }
            }
            finally {
                Log.Exit();
            }
        }


        public async Task<Boolean> ConnectDatabase_Captchas() {
            try {
                Log.Enter();
                await Task.Run( () => { this._captchaDatabase = new PersistTable<String, Captcha>( Environment.SpecialFolder.CommonApplicationData, "Captchas" ); } );
                if ( null != this._captchaDatabase ) {
                    this._autoDisposables.Push( this._captchaDatabase );
                }
            }
            catch ( InvalidOperationException) {
                return false;
            }
            catch ( PathTooLongException) {
                return false;
            }
            catch ( DirectoryNotFoundException) {
                return false;
            }
            catch ( FileNotFoundException) {
                return false;
            }
            finally {
                Log.Exit();
            }

            return null != this._captchaDatabase;
        }

        public async Task<Boolean> ConnectDatabase_PastAnswers() {
            try {
                Log.Enter();
                await Task.Run( () => { this._pastAnswers = new PersistTable<String, String>( Environment.SpecialFolder.CommonApplicationData, "PastAnswers" ); } );

                if ( null != this._pastAnswers ) {
                    this._autoDisposables.Push( this._pastAnswers );
                    return true;
                }
            }
            catch ( InvalidOperationException) { }
            catch ( PathTooLongException) { }
            catch ( DirectoryNotFoundException) { }
            catch ( FileNotFoundException) { }
            finally {
                Log.Exit();
            }

            return false;
        }

        public async Task<Boolean> ConnectDatabase_Websites() {
            try {
                Log.Enter();
                await Task.Run( () => { this._webSites = new PersistTable<String, WebSite>( Environment.SpecialFolder.CommonApplicationData, "Websites" ); } );
                if ( null != this._webSites ) {
                    this._autoDisposables.Push( this._webSites );
                }
            }
            catch ( InvalidOperationException) {
                return false;
            }
            catch ( PathTooLongException) {
                return false;
            }
            catch ( DirectoryNotFoundException) {
                return false;
            }
            catch ( FileNotFoundException) {
                return false;
            }
            finally {
                Log.Exit();
            }

            return null != this._webSites;
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
        ///     <para>Load tables.</para>
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
                        exception.More();
                        return false;
                    }
                }, this.CancellationTokenSource.Token );
            }

            if ( !HasTessEngineBeenConnected ) {
                return false;
            }

            this.HasWebSitesDatabaseBeenConnected = await this.ConnectDatabase_Websites();

            this.HasCaptchaDatabaseBeenConnected = await this.ConnectDatabase_Captchas();

            this.HasPastAnswersBeenConnected = await this.ConnectDatabase_PastAnswers();

            return this.HasWebSitesDatabaseBeenConnected && this.HasCaptchaDatabaseBeenConnected && this.HasPastAnswersBeenConnected;
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
        /// <param name="uber"></param>
        /// <param name="url"></param>
        /// <returns></returns>
        /// <seealso cref="http://answers.awesomium.com/questions/3971/loading-script-complete.html" />
        public async Task<bool> Navigate( [NotNull] Uri url ) {
            if ( null == url ) {
                throw new ArgumentNullException( "url" );
            }

            var bestBrowser = PickBestBrowser( url );
            EnsureWebsite( url );

            await bestBrowser.Navigate( url );

            return true;
        }

        [NotNull]
        private AwesomiumWrapper PickBestBrowser( Uri uri ) {
            var host = uri.Host;

            foreach ( TabControl.TabPageCollection page in this.TabControls.TabPages ) {
                foreach ( TabPage tabPage in page ) {
                    var tag = tabPage.Tag as string;
                    if ( String.IsNullOrWhiteSpace( tag ) || !Uri.IsWellFormedUriString( tag, UriKind.Absolute ) ) {
                        continue;
                    }

                    Uri pageuri;
                    if ( Uri.TryCreate( tag, UriKind.Absolute, out pageuri ) && pageuri.Host.Like( host ) ) {
                        //we found a match, return the browser
                        return new AwesomiumWrapper( tabPage.Controls.OfType< WebControl >().First(), Minutes.One );
                    }
                }
            }

            var newTabPage = new TabPage( host );

            var newBrowser = new WebControl();
            newTabPage.Controls.Add( newBrowser );
            newTabPage.Tag = uri;

            this.TabControls.Controls.Add( newTabPage );

            return new AwesomiumWrapper( newBrowser, Minutes.One );
        }

        /// <summary>
        ///     <para>Gets and sets a <see cref="Captcha" /> .</para>
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

        public void AllStop() {
            Console.WriteLine( "Requesting source token cancel..." );
            this.CancellationTokenSource.Cancel();

            Log.Info( "Requesting browser stops..." );
            foreach ( TabControl.TabPageCollection page in this.TabControls.TabPages ) {
                foreach ( TabPage tabPage in page ) {
                    var webcontrol = tabPage.Tag as WebControl;
                    if ( null == webcontrol ) {
                        continue;
                    }
                    webcontrol.Invoke( new Action( webcontrol.Stop ) );
                    using (webcontrol) {
                        tabPage.Tag = null;
                    }
                }
            }
        }

        /// <summary>
        ///     <para>Gets and sets a <see cref="Captcha" /> .</para>
        /// </summary>
        /// <param name="captcha"></param>
        public void UpdateCaptchaData( Captcha captcha ) {
            if ( null == captcha || null == captcha.Uri ) {
                return;
            }
            this._captchaDatabase[ captcha.Uri.AbsoluteUri ] = captcha;
        }

        public void VisitSites( CancellationToken cancellationToken ) {
            //var faucets = ( BitcoinFaucets[] )Enum.GetValues( typeof(BitcoinFaucets) );

            //Console.WriteLine( "Visiting websites..." );
            //foreach ( var faucetID in faucets.OrderBy( bitcoinFaucets => ( int )bitcoinFaucets ) ) {
            //    this.Visit( faucetID, cancellationToken );
            //    if ( cancellationToken.IsCancellationRequested ) {
            //        break;
            //    }
            //}
        }

        //[NotNull]
        //private static string GetDescription( BitcoinFaucets faucet ) {
        //    var fi = faucet.GetType().GetField( faucet.ToString() );
        //    var attributes = ( DescriptionAttribute[] )fi.GetCustomAttributes( typeof(DescriptionAttribute), false );
        //    return attributes.Length > 0 ? attributes[ 0 ].Description : String.Empty;
        //}

        /// <summary>
        ///     <para>Pulls the image</para>
        ///     <para>Runs the ocr on it</para>
        ///     <para>fills in the blanks</para>
        ///     <para>submits the page</para>
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

            var aforgeImage = Image.FromFile( document.FullPathWithFileName );

            var smoothing = new ConservativeSmoothing();

            var cannyEdgeDetector = new CannyEdgeDetector();

            cannyEdgeDetector.Apply( aforgeImage );

            aforgeImage.Save( document.FullPathWithFileName, ImageFormat.Png );

            this.PictureBoxChallenge.ImageLocation = document.FullPathWithFileName;

            this.PictureBoxChallenge.Load();


            using (var img = Pix.LoadFromFile( document.FullPathWithFileName ).Deskew()) {
                using (var page = tesseractEngine.Process( img, PageSegMode.SingleLine )) {
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

        private void StartTheCaptchaStuff( AwesomiumWrapper webControl, CancellationToken cancellationToken ) {
            var uri = webControl.GetBrowserLocation();

            // Check if the page shows any sort of countdown/NotReadyYet
            //var text = webControl.GetInnerText() ?? String.Empty;
            //if ( text.Contains( "You must wait" ) ) {
            //    return;
            //}

            // 2. find the captcha image uri

            //var captchaData = this.PullCaptchaData( uri );

            //captchaData.Status = CaptchaStatus.SearchingForChallenge;
            //this.UpdateCaptchaData( captchaData );

            //method 1
            //var captchaChallenge = GetElementByID( this.WebBrowser1, "recaptcha_challenge_image" );
            //var captchaInput = GetElementByID( this.WebBrowser1, "recaptcha_response_field" ); // find the captcha response textbox

            //if ( null != captchaChallenge && null != captchaInput ) {
            //captchaData.Status = CaptchaStatus.ChallengeFound;

            //captchaData.ChallengeElementID = captchaChallenge.id;

            //captchaData.ResponseElementID = captchaInput.id;

            //captchaData.ImageUri = new Uri( captchaChallenge.src );

            //captchaData.Status = CaptchaStatus.LoadingImage;
            //this.UpdateCaptchaData( captchaData );

            //this.PictureBoxChallenge.Flash();
            ////this.PictureBoxChallenge.Load( captchaChallenge.src );
            //this.PictureBoxChallenge.Flash();

            ////captchaData.Image = this.PictureBoxChallenge.Image.Clone() as Image;
            //captchaData.Status = CaptchaStatus.LoadedImage;
            //this.UpdateCaptchaData( captchaData );

            //String answer;
            //if ( this.SolveCaptcha( uri, cancellationToken, out answer ) ) {
            //this.WebBrowser1.by
            //ByIDFunction( this.WebBrowser1, captchaData.ResponseElementID, "focus" );
            //ByIDFunction( this.WebBrowser1, captchaData.ResponseElementID, "scrollIntoView" );
            //ByIDFunction( this.WebBrowser1, captchaData.ResponseElementID, "click" );
            //ByIDSetValue( this.WebBrowser1, captchaData.ResponseElementID, answer );

            //Throttle( Seconds.Five );

            //AwesomiumWrapper.ClickSubmit( webControl );

            this.PictureBoxChallenge.Image = null;
            this.PictureBoxChallenge.InitialImage = null;

            //Throttle( Seconds.Ten );

            //return;
            //}

            //captchaData.Status = CaptchaStatus.ChallengeStillNotSolved;
            //this.UpdateCaptchaData( captchaData );
        }
    }

    //private void Visit( BitcoinFaucets faucetID, CancellationToken cancellationToken ) {
    //    try {
    //        var description = GetDescription( faucetID ); //in BitcoinFaucets case, the description is a uri
    //        if ( String.IsNullOrWhiteSpace( description ) ) {
    //            return;
    //        }

    //        var uri = new Uri( description );
    //        Console.WriteLine( "Visiting (#{0}) {1} @ {2}", faucetID, description, uri.PathAndQuery );

    //        var navigated = this.Navigate( description );

    //        if ( !navigated ) {
    //            faucetID = BitcoinFaucets.AboutBlank;
    //        }
    //        if ( !description.StartsWith( "about:", StringComparison.OrdinalIgnoreCase ) ) {
    //            Throttle();
    //        }

    //        switch ( faucetID ) {
    //            //case BitcoinFaucets.BitChestDotMe:
    //            //    this.Visit_BitChestDotMe( "1KEEP1Wd6KKVHJrBaB45cSHXzMJu9VWWAt", cancellationToken );
    //            //    break;

    //            case BitcoinFaucets.LandOfBitCoinDotCom:
    //                this.Visit_LandOfBitCoinDotCom( "1MpfkH1vDyGrmtykodJmzBNWi81KqXa8SE", cancellationToken );
    //                break;

    //            default:
    //                this.Visit_AboutBlank();
    //                break;
    //        }
    //    }
    //    catch ( Exception exception ) {
    //        exception.More();
    //    }
    //    finally {
    //        Throttle();
    //    }
    //}

    //private void Visit_AboutBlank() {
    //    this.Navigate( "about:blank" ); //.Wait( this.NavigationTimeout );

    //    //this.Throttle();
    //}

    /*
            private void Visit_BitChestDotMe( String bitcoinAddress, CancellationToken cancellationToken ) {
                Log.Enter();

                if ( !this.Navigate( String.Format( "http://www.bitchest.me/?a={0}", bitcoinAddress ) ) ) {
                    return;
                }

                Throttle();

                if ( cancellationToken.IsCancellationRequested ) {
                    return;
                }

                var links = this.WebBrowser1.GetAllLinks().Where( uri => uri.PathAndQuery.Contains( bitcoinAddress ) ).ToList();

                Throttle();

                foreach ( var link in links ) {
                    if ( cancellationToken.IsCancellationRequested ) {
                        return;
                    }
                    try {
                        this.Navigate( String.Format( "http://www.bitchest.me/?a={0}", bitcoinAddress ) ); //.Wait( this.NavigationTimeout );
                        Throttle();
                        Navigate( this, link );

                        //this.Throttle();
                        try {
                            this.StartTheCaptchaStuff( cancellationToken );
                        }
                        catch ( Exception exception ) {
                            exception.More();
                        }
                    }
                    catch ( Exception exception ) {
                        exception.More();
                    }
                    finally {
                        Throttle();
                    }

                    //TODO submit
                }

                Log.Exit();
            }
    */

    //private void Visit_LandOfBitCoinDotCom( String bitcoinAddress, CancellationToken cancellationToken ) {
    //    if ( !this.Navigate( "http://www.landofbitcoin.com/free-bitcoin-faucets" ) ) {
    //        return;
    //    }

    //    Throttle();

    //    if ( cancellationToken.IsCancellationRequested ) {
    //        return;
    //    }

    //    var links = this.WebBrowser1.GetAllLinks().Where( uri => uri.PathAndQuery.Contains( bitcoinAddress ) ).ToList();

    //    Throttle();

    //    foreach ( var link in links ) {
    //        if ( cancellationToken.IsCancellationRequested ) {
    //            return;
    //        }
    //        try {
    //            this.Navigate( String.Format( "http://www.bitchest.me/?a={0}", bitcoinAddress ) ); //.Wait( this.NavigationTimeout );
    //            Throttle();
    //            Navigate( this, link );

    //            //this.Throttle();
    //            try {
    //                this.StartTheCaptchaStuff( cancellationToken );
    //            }
    //            catch ( Exception exception ) {
    //                exception.More();
    //            }
    //        }
    //        catch ( Exception exception ) {
    //            exception.More();
    //        }
    //        finally {
    //            Throttle();
    //        }

    //        //TODO submit
    //    }
    //}
    //}

    //[CanBeNull]
    //public static dynamic GetElementByID( WebControl webBrowser, String id ) {
    //    try {
    //        if ( webBrowser != null ) {
    //            var answer = webBrowser.Invoke( new Func<JSObject>( () => {
    //                dynamic document = ( JSObject )webBrowser.ExecuteJavascriptWithResult( "document" );
    //                using (document) {
    //                    try {
    //                        return document.getElementById( id );
    //                    }
    //                    catch ( Exception exception ) {
    //                        exception.More();
    //                    }
    //                    return null;
    //                }
    //            } ) );
    //            return answer;
    //        }
    //    }
    //    catch ( Exception exception ) {
    //        exception.More();
    //    }
    //    return null;
    //}

    //[CanBeNull]
    //public static dynamic GetElementsByTagName( WebControl webBrowser, String type ) {
    //    try {
    //        if ( webBrowser != null ) {
    //            var answer = webBrowser.Invoke( new Func<JSObject>( () => {
    //                dynamic document = ( JSObject )webBrowser.ExecuteJavascriptWithResult( "document" );
    //                using (document) {
    //                    try {
    //                        return document.getElementsByTagName( type );
    //                    }
    //                    catch ( Exception exception ) {
    //                        exception.More();
    //                    }
    //                    return null;
    //                }
    //            } ) );
    //            return answer;
    //        }
    //    }
    //    catch ( Exception exception ) {
    //        exception.More();
    //    }
    //    return null;
    //}

}

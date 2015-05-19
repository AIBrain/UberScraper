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
// "UberScraper 2015/Uber.cs" was last cleaned by RICK on 2015/02/25 at 4:28 PM
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
    using JetBrains.Annotations;
    using Librainian.Collections;
    using Librainian.Controls;
    using Librainian.Internet;
    using Librainian.Internet.Browser;
    using Librainian.IO;
    using Librainian.Linguistics;
    using Librainian.Magic;
    using Librainian.Measurement.Time;
    using Librainian.Parsing;
    using Librainian.Persistence;
    using Librainian.Threading;
    using Properties;
    using Tesseract;
    using ImageFormat = System.Drawing.Imaging.ImageFormat;

    public sealed class Uber : BetterDisposableClass {

        [CanBeNull]
        private TesseractEngine _tesseractEngine;

        public Uber( [NotNull] TabControl tabControls, [NotNull] PictureBox pictureBox, [NotNull] SitesEditor sitesEditor/*, [NotNull] SynchronizationContext context*/ ) {
            if ( tabControls == null ) {
                throw new ArgumentNullException( nameof( tabControls ) );
            }
            if ( pictureBox == null ) {
                throw new ArgumentNullException( nameof( pictureBox ) );
            }
            if ( sitesEditor == null ) {
                throw new ArgumentNullException( nameof( sitesEditor ) );
            }
            //if ( context == null ) {
            //    throw new ArgumentNullException( nameof( context ) );
            //}

            this.TabControls = tabControls;
            this.PictureBox = pictureBox;
            this.SitesEditor = sitesEditor;
            //this.Context = context;
            this.NavigationTimeout = Seconds.Thirty;

            this.AutoDisposables.Push( this.Answers );
            this.AutoDisposables.Push( this.WebSites );
            this.AutoDisposables.Push( this.CaptchaDatabase );

            this.RunningTask = Task.Run( () => this.Run() );
        }

        //[NotNull]public SynchronizationContext Context { get; }

        public TabControl TabControls { get; }
        public PictureBox PictureBox { get; }
        public SitesEditor SitesEditor { get; set; }

        /// <summary>
        ///     <para>Defaults to <see cref="Seconds.Thirty" /> in the ctor.</para>
        /// </summary>
        public TimeSpan NavigationTimeout { get; set; }

        public Task RunningTask { get; }

        [NotNull]
        public SimpleCancel MainCancel { get; } = new SimpleCancel();

        [NotNull]
        private PersistTable<String, Captcha> CaptchaDatabase { get; } = new PersistTable<String, Captcha>( Environment.SpecialFolder.CommonApplicationData, "Captchas" );

        [NotNull]
        private PersistTable<String, WebSite> WebSites { get; } = new PersistTable<String, WebSite>( Environment.SpecialFolder.CommonApplicationData, "Websites" );

        [NotNull]
        private PersistTable<String, String> Answers { get; } = new PersistTable<String, String>( Environment.SpecialFolder.CommonApplicationData, "Answers" );

        /// <summary>
        /// </summary>
        /// <exception cref="TesseractException"></exception>
        [NotNull]
        public TesseractEngine TesseractEngine {
            get {
                if ( null != this._tesseractEngine ) {
                    return this._tesseractEngine;
                }

                try {
                    this._tesseractEngine = new TesseractEngine( datapath: "tessdata", language: "eng", engineMode: EngineMode.Default );
                    this.AutoDisposables.Push( this._tesseractEngine );
                    return this._tesseractEngine;
                }
                catch ( TesseractException) {
                    throw new TesseractException( "Unable to connect to tesseract engine." );
                }
            }
        }

        [NotNull]
        private ConcurrentStack<IDisposable> AutoDisposables { get; } = new ConcurrentStack<IDisposable>();

        protected override void CleanUpManagedResources() {
            try {
                Log.Enter();

                while ( this.AutoDisposables.Any() ) {
                    try {
                        IDisposable disposable;
                        if ( !this.AutoDisposables.TryPop( out disposable ) ) {
                            continue;
                        }
                        Console.Write( "Disposing of {0}...", disposable );
                        using ( disposable ) {
                            disposable.Dispose();
                        }
                        Console.WriteLine( "Disposed of {0}.", disposable );
                    }
                    catch ( Exception exception ) {
                        exception.More();
                    }
                }

                foreach ( var disposable in this.AutoDisposables ) {
                    
                }
            }
            finally {
                base.CleanUpManagedResources();
                Log.Exit();
            }
        }

        private void Run() {
            Log.Enter();

            try {
                //well, why not?
                Parallel.Invoke( () => this.TabControls.ThrowIfNull(), () => this.PictureBox.ThrowIfNull(), () => this.CaptchaDatabase.ThrowIfNull(), () => this.WebSites.ThrowIfNull(), () => this.Answers.ThrowIfNull(), () => this.SitesEditor.ThrowIfNull() );
            }
            catch ( AggregateException exception ) {
                foreach ( var innerException in exception.InnerExceptions ) {
                    innerException.More();
                }
            }

            foreach ( var task in this.SitesEditor.Data.Select( site => Task.Run( () => this.SpinupWorker( site, this.PickBrowser( site.Address ) ) ) ) ) {
                this.Workers.Add( task );
            }

            while ( this.Workers.Any() ) {
                Task.WaitAny( this.Workers.ToArray(), Seconds.One );
                Application.DoEvents();
            }

            Log.Exit();
        }

        private async void SpinupWorker( SitesEditor.SiteData site, AwesomiumWrapper browser ) {
            String.Format( "Starting worker {0} ", site.Address.AbsoluteUri ).WriteLine();
            // we know the site, and we're running inside a task... so: we no longer have access to any controls.
            //todo create timers

            //TODO	create timers and awesomewrappers based upon rows found in siteEditor
            while ( !this.MainCancel.HaveAnyCancellationsBeenRequested() ) {
                await Task.Delay( Seconds.One );    //be nice to the system.

                if ( !site.LastSuccess.HasValue ) {
                    //we haven't tried the site. go visit it.
                }
            }

            String.Format( "Stopping worker {0} ", site.Address.AbsoluteUri ).WriteLine();
        }

        private ConcurrentList<Task> Workers { get; } = new ConcurrentList<Task>();

        public void RequestStop() {
            "Requesting cancel...".WriteLine();
            this.MainCancel.Cancel();

            "Requesting browser stops...".Info();
            foreach ( TabControl.TabPageCollection tabPages in this.TabControls.TabPages ) {
                foreach ( TabPage tabPage in tabPages ) {
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

        public void EnsureWebsite( [CanBeNull] Uri uri ) {
            if ( null == uri ) {
                return;
            }

            if ( null == this.WebSites[ uri.AbsoluteUri ] ) {
                this.WebSites[ uri.AbsoluteUri ] = new WebSite {
                    Location = uri
                };
            }
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

        /*
                /// <summary>
                ///     <para>No guarantee that more ajax/javascript can and will fire off after this is 'true'.</para>
                ///     <para>Internal while loop blocks.</para>
                /// </summary>
                /// <param name="url"></param>
                /// <returns></returns>
                /// <seealso cref="http://answers.awesomium.com/questions/3971/loading-script-complete.html" />
                public async Task<bool> Navigate( [NotNull] Uri url ) {
                    if ( null == url ) {
                        throw new ArgumentNullException( nameof( url ) );
                    }

                    var bestBrowser = PickBestBrowser( url );
                    EnsureWebsite( url );

                    await bestBrowser.Navigate( url );

                    return true;
                }
        */

        /// <summary>
        /// </summary>
        /// <param name="uri"></param>
        [NotNull]
        public Captcha PullCaptchaData( [NotNull] Uri uri ) {
            if ( uri == null ) {
                throw new ArgumentNullException( nameof( uri ) );
            }

            var captcha = this.CaptchaDatabase[ uri.AbsoluteUri ] ?? ( this.CaptchaDatabase[ uri.AbsoluteUri ] = new Captcha {
                Uri = uri
            } );

            return captcha;
        }

        /// <summary>
        ///     <para>Gets and sets a <see cref="Captcha" /> .</para>
        /// </summary>
        /// <param name="captcha"></param>
        public void PutCaptchaData( [NotNull] Captcha captcha ) {
            if ( captcha == null ) {
                throw new ArgumentNullException( nameof( captcha ) );
            }
            if ( captcha.Uri == null ) {
                return;
            }
            CaptchaDatabase[ captcha.Uri.AbsoluteUri ] = captcha;
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

        [NotNull]
        private AwesomiumWrapper PickBrowser( Uri uri ) {





            var host = uri.Host;

            foreach ( TabControl.TabPageCollection pages in this.TabControls.TabPages ) {
                foreach ( TabPage tabPage in pages ) {
                    var tag = tabPage.Tag as string;
                    if ( String.IsNullOrWhiteSpace( tag ) || !Uri.IsWellFormedUriString( tag, UriKind.Absolute ) ) {
                        continue;
                    }

                    Uri pageuri;
                    if ( Uri.TryCreate( tag, UriKind.Absolute, out pageuri ) && pageuri.Host.Like( host ) ) {
                        //we found a match, return the browser
                        return new AwesomiumWrapper( webControl: tabPage.Controls.OfType<WebControl>().First(), timeout: Minutes.One );
                    }
                }
            }

            TabPage newTabPage;
            WebControl newBrowser = null;

            this.TabControls.InvokeIfRequired( () => {

                newTabPage = new TabPage( host );

                newBrowser = new WebControl();
                newTabPage.Controls.Add( newBrowser );
                newTabPage.Tag = uri;

                this.TabControls.Controls.Add( newTabPage );

            } );

            return new AwesomiumWrapper( newBrowser, Minutes.One );


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
            var tesseractEngine = this._tesseractEngine;
            if ( null == tesseractEngine ) {
                return false;
            }

            var captchaData = this.PullCaptchaData( challenge );

            if ( captchaData.ImageUri == null ) {
                captchaData.Status = CaptchaStatus.NoImageFoundToBeSolved;
                this.PutCaptchaData( captchaData );
                return false;
            }

            Console.WriteLine( Resources.Uber_SolveCaptcha_Attempting_OCR_on__0_, captchaData.ImageUri.AbsolutePath );

            captchaData.Status = CaptchaStatus.SolvingImage;
            this.PutCaptchaData( captchaData );

            var folder = new Folder( Path.GetTempPath() );

            Document document;
            folder.TryGetTempDocument( document: out document, extension: "png" );

            this.PictureBox.Image.Save( document.FullPathWithFileName, ImageFormat.Png );

            var aforgeImage = Image.FromFile( document.FullPathWithFileName );

            var smoothing = new ConservativeSmoothing();

            var cannyEdgeDetector = new CannyEdgeDetector();

            cannyEdgeDetector.Apply( aforgeImage );

            aforgeImage.Save( document.FullPathWithFileName, ImageFormat.Png );

            this.PictureBox.ImageLocation = document.FullPathWithFileName;

            this.PictureBox.Load();

            using (var img = Pix.LoadFromFile( document.FullPathWithFileName ).Deskew()) {
                using (var page = tesseractEngine.Process( img, PageSegMode.SingleLine )) {
                    answer = page.GetText();

                    var paragraph = new Paragraph( answer );

                    answer = new Sentence( paragraph.ToStrings( " " ) ).ToStrings( " " );

                    FluentTimers.Create( Minutes.One, () => document.Delete() ).AndStart();

                    if ( !String.IsNullOrWhiteSpace( answer ) ) {
                        captchaData.Status = CaptchaStatus.SolvedChallenge;
                        this.PutCaptchaData( captchaData );
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

            this.PictureBox.Image = null;
            this.PictureBox.InitialImage = null;

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

    // var uri = new Uri( description ); Console.WriteLine( "Visiting (#{0}) {1} @ {2}", faucetID,
    // description, uri.PathAndQuery );

    // var navigated = this.Navigate( description );

    // if ( !navigated ) { faucetID = BitcoinFaucets.AboutBlank; } if ( !description.StartsWith(
    // "about:", StringComparison.OrdinalIgnoreCase ) ) { Throttle(); }

    // switch ( faucetID ) { //case BitcoinFaucets.BitChestDotMe: // this.Visit_BitChestDotMe(
    // "1KEEP1Wd6KKVHJrBaB45cSHXzMJu9VWWAt", cancellationToken ); // break;

    // case BitcoinFaucets.LandOfBitCoinDotCom: this.Visit_LandOfBitCoinDotCom(
    // "1MpfkH1vDyGrmtykodJmzBNWi81KqXa8SE", cancellationToken ); break;

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

    // Throttle();

    // if ( cancellationToken.IsCancellationRequested ) { return; }

    // var links = this.WebBrowser1.GetAllLinks().Where( uri => uri.PathAndQuery.Contains(
    // bitcoinAddress ) ).ToList();

    // Throttle();

    // foreach ( var link in links ) { if ( cancellationToken.IsCancellationRequested ) { return; }
    // try { this.Navigate( String.Format( "http://www.bitchest.me/?a={0}", bitcoinAddress ) );
    // //.Wait( this.NavigationTimeout ); Throttle(); Navigate( this, link );

    // //this.Throttle(); try { this.StartTheCaptchaStuff( cancellationToken ); } catch ( Exception
    // exception ) { exception.More(); } } catch ( Exception exception ) { exception.More(); }
    // finally { Throttle(); }

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

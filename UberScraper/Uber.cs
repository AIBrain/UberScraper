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
// "UberScraper 2015/Uber.cs" was last cleaned by Rick on 2015/02/07 at 7:32 AM

#endregion License & Information

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

		[CanBeNull]
		private PersistTable<String, String> _answers;

		[CanBeNull]
		private PersistTable<String, Captcha> _captchaDatabase;

		[CanBeNull]
		private TesseractEngine _tesseractEngine;

		[CanBeNull]
		private PersistTable<String, WebSite> _webSites;

		public Uber( [NotNull] TabControl tabControls, [NotNull] PictureBox pictureBox, [NotNull] SitesEditor sitesEditor, [NotNull] SynchronizationContext context ) {
			if ( tabControls == null ) {
				throw new ArgumentNullException( "tabControls" );
			}
			if ( pictureBox == null ) {
				throw new ArgumentNullException( "pictureBox" );
			}
			if ( sitesEditor == null ) {
				throw new ArgumentNullException( "sitesEditor" );
			}
			if ( context == null ) {
				throw new ArgumentNullException( "context" );
			}
			this.TabControls = tabControls;
			this.PictureBox = pictureBox;
			this.SitesEditor = sitesEditor;
			this.Context = context;
			this.NavigationTimeout = Seconds.Thirty;
			this.Running = Task.Run( () => this.Run() );
		}

		[NotNull]
		public SynchronizationContext Context { get; }

		public TabControl TabControls { get; }

		public PictureBox PictureBox { get; }

		public SitesEditor SitesEditor { get; set; }

		/// <summary>
		///     <para>Defaults to <see cref="Seconds.Thirty" /> in the ctor.</para>
		/// </summary>
		public TimeSpan NavigationTimeout { get; set; }

		public Task Running { get; }

		[NotNull]
		public SimpleCancel MainCancel { get; }
		= new SimpleCancel();

		[NotNull]
		public PersistTable<String, Captcha> CaptchaDatabase {
			get {
				if ( null != this._captchaDatabase ) {
					return this._captchaDatabase;
				}

				try {
					"Loading captcha database".Info();
					this._captchaDatabase = new PersistTable<String, Captcha>( Environment. SpecialFolder.CommonApplicationData, "Captchas" );
					this.AutoDisposables.Push( this._captchaDatabase );
					return this._captchaDatabase;
				}
				catch ( InvalidOperationException) {
					throw new InvalidOperationException( "Unable to connect to captcha database: invalid operation." );
				}
				catch ( PathTooLongException) {
					throw new InvalidOperationException( "Unable to connect to captcha database: path too long." );
				}
				catch ( DirectoryNotFoundException) {
					throw new InvalidOperationException( "Unable to connect to captcha database: folder not found." );
				}
				catch ( FileNotFoundException) {
					throw new InvalidOperationException( "Unable to connect to captcha database: file not found." );
				}
			}
		}

		[NotNull]
		public PersistTable<String, WebSite> WebSites {
			get {
				if ( null != this._webSites ) {
					return this._webSites;
				}

				try {
					this._webSites = new PersistTable<String, WebSite>( Environment.SpecialFolder.CommonApplicationData, "Websites" );
					this.AutoDisposables.Push( this._webSites );
					return this._webSites;
				}
				catch ( InvalidOperationException) {
					throw new InvalidOperationException( "Unable to connect to website database: invalid operation." );
				}
				catch ( PathTooLongException) {
					throw new InvalidOperationException( "Unable to connect to website database: path too long." );
				}
				catch ( DirectoryNotFoundException) {
					throw new InvalidOperationException( "Unable to connect to website database: folder not found." );
				}
				catch ( FileNotFoundException) {
					throw new InvalidOperationException( "Unable to connect to website database: file not found." );
				}
			}
		}

		[NotNull]
		public PersistTable<String, String> Answers {
			get {
				if ( null != this._answers ) {
					return this._answers;
				}

				try {
					this._answers = new PersistTable<String, String>( Environment.SpecialFolder.CommonApplicationData, "Answers" );
					this.AutoDisposables.Push( this._answers );
					return this._answers;
				}
				catch ( InvalidOperationException) {
					throw new InvalidOperationException( "Unable to connect to answers database: invalid operation." );
				}
				catch ( PathTooLongException) {
					throw new InvalidOperationException( "Unable to connect to answers database: path too long." );
				}
				catch ( DirectoryNotFoundException) {
					throw new InvalidOperationException( "Unable to connect to answers database: folder not found." );
				}
				catch ( FileNotFoundException) {
					throw new InvalidOperationException( "Unable to connect to answers database: file not found." );
				}
			}
		}

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
		private ConcurrentStack<IDisposable> AutoDisposables { get; }
		= new ConcurrentStack<IDisposable>();

		/// <summary>
		///     Performs application-defined tasks associated with freeing, releasing, or resetting
		///     unmanaged resources.
		/// </summary>
		public void Dispose() {
			try {
				Log.Enter();

				foreach ( var disposable in this.AutoDisposables ) {
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

		private void Run() {
			Log.Enter();

			try {
				Parallel.Invoke( ThreadingExtensions.Parallelism, () => VerifyNotNull( this.TabControls ), () => VerifyNotNull( this.PictureBox ), () => VerifyNotNull( this.CaptchaDatabase ), () => VerifyNotNull( this.WebSites ), () => VerifyNotNull( this.Answers ), () => VerifyNotNull( this.SitesEditor ) );
			}
			catch ( AggregateException exception ) {
				foreach ( var innerException in exception.InnerExceptions ) {
					innerException.More();
				}
			}

			var nextSite =  this.SitesEditor.Data .or


			//TODO	//create timers and awesomewrappers based upon rows found in siteEditor
			Log.Exit();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <typeparam name="TKey"></typeparam>
		/// <param name="obj"></param>
		// ReSharper disable once UnusedParameter.Local
		private static void VerifyNotNull<TKey>( [ CanBeNull ] TKey obj ) {
			if ( null == obj ) {
				throw new ArgumentNullException();
			}
		}

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

			var webSites = this._webSites;

			webSites.Should().NotBeNull();
			if ( webSites == null ) {
				throw new NullReferenceException( "_websites is null" );
			}

			if ( null == webSites[ uri.AbsoluteUri ] ) {
				webSites[ uri.AbsoluteUri ] = new WebSite();
			}
			webSites[ uri.AbsoluteUri ].Location = uri;
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

		/// <summary>
		/// </summary>
		/// <param name="uri"></param>
		[NotNull]
		public Captcha PullCaptchaData( [NotNull] Uri uri ) {
			if ( uri == null ) {
				throw new ArgumentNullException( "uri" );
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
				throw new ArgumentNullException( "captcha" );
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
		private AwesomiumWrapper PickBestBrowser( Uri uri ) {
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

			var newTabPage = new TabPage( host );

			var newBrowser = new WebControl();
			newTabPage.Controls.Add( newBrowser );
			newTabPage.Tag = uri;

			this.TabControls.Controls.Add( newTabPage );

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
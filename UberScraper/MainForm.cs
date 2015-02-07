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
// "UberScraper 2015/MainForm.cs" was last cleaned by Rick on 2015/02/07 at 7:28 AM

#endregion License & Information

namespace UberScraper {

	using System;
	using System.Threading;
	using System.Threading.Tasks;
	using System.Windows.Forms;
	using FluentAssertions;
	using JetBrains.Annotations;
	using Librainian.Controls;
	using Librainian.Measurement.Time;
	using Librainian.Threading;
	using Properties;

	public partial class MainForm : Form {

		public MainForm() {
			AwesomiumContext = SynchronizationContext.Current;
			this.InitializeComponent();
		}

		public SynchronizationContext AwesomiumContext { get; }

		[CanBeNull]
		public Uber Uber { get; set; }

		[CanBeNull]
		public SitesEditor SitesEditor { get; set; }

		private void MainForm_FormClosing( object sender, FormClosingEventArgs e ) {
			Settings.Default.Save();
			this.Uber?.RequestStop();
		}

		/// <summary>
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		/// <exception cref="NullReferenceException"></exception>
		private void MainForm_Shown( object sender, EventArgs e ) {
			this.SitesEditor = new SitesEditor( this );
			this.SitesEditor.OnThread( () => this.SitesEditor.Show() );

			AwesomiumContext.Should().NotBeNull();

			this.Uber = new Uber( this.tabControls, this.pictureBoxChallenge, this.SitesEditor, AwesomiumContext );
			this.Uber.Should().NotBeNull();
            if ( this.Uber == null ) {
				throw new NullReferenceException( "this.Uber" );
			}
			this.buttonSiteEditor.Push();
		}

		private async void buttonStop_Click( object sender, EventArgs e ) {
			"Stop button pressed.".WriteLine();
			var uber = this.Uber;
			if ( uber != null ) {
				await Task.Run( () => uber.RequestStop() );
			}
		}

		private void MainForm_FormClosed( object sender, FormClosedEventArgs e ) {
			using (this) {
				"Disposing of resources...".WriteLine();
				var uber = this.Uber;
				if ( uber != null ) {
					using (uber) {
						uber.Dispose();
					}
				}
				Console.WriteLine( "Resources disposed." );
#if DEBUG
				Console.WriteLine();

				//Console.WriteLine( "Press any key to exit" );
				Task.Delay( Seconds.One ).Wait();

				//Console.ReadKey();
#endif
			}
		}

		private void buttonSiteEditor_Click( Object sender, EventArgs e ) {
			var sitesEditor = this.SitesEditor;
			sitesEditor?.OnThread( () => sitesEditor.Show() );
		}
	}
}
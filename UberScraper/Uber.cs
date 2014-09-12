namespace UberScraper {
    using System;
    using System.IO;
    using System.Threading.Tasks;
    using Librainian.Annotations;
    using Librainian.Internet;
    using Librainian.Persistence;
    using Librainian.Threading;

    /// <summary>
    /// <para>Interface for the <see cref="Uber"/> class.</para>
    /// </summary>
    public interface IUber : IDisposable {
        Task<Boolean> Start();

        Boolean LoadWebsites();
    }

    public class Uber : IUber {

        [CanBeNull]
        private PersistTable<String, WebSite> _webSites;

        public Boolean HasWebSitesBeenLoaded {
            get;
            private set;
        }

        /// <summary>
        /// <para>Load dictionaries.</para>
        /// </summary>
        /// <returns></returns>
        public async Task<Boolean> Start() {

            return await this.LoadDictionaries();
        }

        private async Task<Boolean> LoadDictionaries() {
            if ( !this.HasWebSitesBeenLoaded ) {
                this.HasWebSitesBeenLoaded = await Task.Run( () => this.LoadWebsites() );
            }
            return this.HasWebSitesBeenLoaded;
        }

        public Boolean LoadWebsites() {

            try {
                Report.Enter();
                this._webSites = new PersistTable<String, WebSite>( Environment.SpecialFolder.CommonApplicationData, "Websites" );
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
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose() {
            var webSites = this._webSites;
            if ( null != webSites ) {
                webSites.Dispose();
            }
        }
    }
}
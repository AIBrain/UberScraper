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
    public interface IUber {
        Task<Boolean> Start();

        Boolean LoadWebsites();
    }

    public class Uber : IUber {

        [CanBeNull]
        private PersistTable<String, WebSite> _webSites;

        private Boolean _webSitesLoaded;

        public async Task<Boolean> Start() {


            this._webSitesLoaded = await Task.Run( () => this.LoadWebsites() );

            return true;
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
    }
}
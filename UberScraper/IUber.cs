namespace UberScraper {
    using System;
    using System.Threading.Tasks;
    using Awesomium.Windows.Forms;

    /// <summary>
    /// <para>Interface for the <see cref="Uber"/> class.</para>
    /// </summary>
    public interface IUber : IDisposable {
        Task<Boolean> Start();

        Boolean LoadWebsites();

        /// <summary>
        /// <seealso cref="Uber.WebBrowser1"/>
        /// </summary>
        /// <param name="webBrowser"></param>
        void SetBrowser( WebControl webBrowser );
    }
}
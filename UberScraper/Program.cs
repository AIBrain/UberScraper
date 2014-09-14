
namespace UberScraper {
    using System;
    using System.Windows.Forms;
    using Librainian;
    using Librainian.Magic;

    static class Program {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main() {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault( false );
            using ( var mainForm = Ioc.Container.TryGet<MainForm>() ) {
                try {
                    if ( mainForm != null ) {
                        Application.Run( mainForm );
                    }
                }
                catch ( Exception exception ) {
                    exception.Error();
                }
            }

        }

    }
}

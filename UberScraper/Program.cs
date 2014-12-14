namespace UberScraper {
    using System;
    using System.Windows.Forms;
    using Librainian.Threading;

    internal static class Program {

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        private static void Main() {
            try {
                Log.Startup();

                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault( false );

                using ( var mainForm = new MainForm() ) {
                    Application.Run( mainForm );
                }
            }
            catch ( Exception exception ) {
                exception.More();
            }
            finally {
                Log.Shutdown();
            }
        }
    }
}
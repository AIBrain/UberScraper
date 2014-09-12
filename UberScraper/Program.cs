using System;
using System.Windows.Forms;

namespace UberScraper {
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
                Application.Run( mainForm );
            }

        }
    }
}

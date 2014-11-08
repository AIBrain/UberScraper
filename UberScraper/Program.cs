namespace UberScraper {

    using System;
    using System.Reflection;
    using System.Windows.Forms;
    using Librainian;
    using Librainian.Extensions;

    internal static class Program {

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        private static void Main() {
            try {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault( false );

                Diagnostical.HasConsoleBeenAllocated = NativeWin32.AllocConsole();
                if ( Diagnostical.HasConsoleBeenAllocated ) {
                    Console.WriteLine( "Logging console activated." );
                    Console.WriteLine( "Assembly version {0}", Assembly.GetEntryAssembly().GetName().Version );
                    Console.WriteLine( "Loading MainForm." );
                }

                using ( var mainForm = new MainForm() ) {
                    Application.Run( mainForm );
                }
            }
            catch ( Exception exception ) {
                exception.Error();
            }
            finally {
                if ( Diagnostical.HasConsoleBeenAllocated ) {
                    Diagnostical.HasConsoleBeenAllocated = !NativeWin32.FreeConsole();
                }
            }



        }
    }
}

namespace UberScraper {
    using System.Windows.Forms;
    using Librainian.Annotations;
    using Librainian.Magic;

    public partial class MainForm : Form {
        public MainForm() {
            InitializeComponent();
        }

        [CanBeNull]
        public IUber Uber {
            get;
            set;
        }

        private void MainForm_Load( object sender, System.EventArgs e ) {
            this.Uber = Ioc.Container.TryGet<Uber>();
        }

        private async void MainForm_Shown( object sender, System.EventArgs e ) {
            var uber = this.Uber;
            if ( null == uber ) {
                return;
            }
            await uber.Start();
        }
    }
}

namespace UberScraper {

    using System;
    using System.Windows.Forms;
    using Librainian.Extensions;

    public sealed partial class AboutBox1 : Form {

        public AboutBox1() {
            InitializeComponent();
            this.Text = String.Format( "About {0}", AssemblyInformation.Title );
            this.labelProductName.Text = AssemblyInformation.Product;
            this.labelVersion.Text = String.Format( "Version {0}", AssemblyInformation.Version );
            this.labelCopyright.Text = AssemblyInformation.Copyright;
            this.labelCompanyName.Text = AssemblyInformation.Company;
            this.textBoxDescription.Text = AssemblyInformation.Description;
        }


    }
}
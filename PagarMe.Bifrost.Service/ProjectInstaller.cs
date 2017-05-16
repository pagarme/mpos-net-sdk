using System.ComponentModel;
using System.Configuration.Install;

namespace PagarMe.Bifrost.Service
{
    [RunInstaller(true)]
    public partial class ProjectInstaller : Installer
    {
        public ProjectInstaller()
        {
            InitializeComponent();
        }
    }
}
using System.ComponentModel;
using System.Configuration.Install;

namespace PagarMe.Mpos.Bridge.Service
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
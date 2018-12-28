using System.ComponentModel;
using System.Configuration.Install;
using System.ServiceProcess;

namespace IPFilter
{
    [RunInstaller(true)]
    public class FilterServiceInstaller : Installer
    {
        readonly ServiceProcessInstaller installer;
        ServiceInstaller serviceInstaller;

        public FilterServiceInstaller()
        {
            installer = new ServiceProcessInstaller
            {
                Account = ServiceAccount.User
            };

            serviceInstaller = new ServiceInstaller
            {
                DelayedAutoStart = true,
                Description = "IPFilter Updater",
                DisplayName = "IPFilter",
                ServiceName = "ipfilter",
                StartType = ServiceStartMode.Automatic
            };

            Installers.Add(serviceInstaller);
            Installers.Add(installer);
        }
    }
}
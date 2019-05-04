using System.ServiceProcess;
using System.ComponentModel;


namespace SmtpTelegramRelay
{
    [RunInstaller(true)]
    class Installer : System.Configuration.Install.Installer
    {
        public Installer()
        {
            using (ServiceProcessInstaller serviceProcessInstaller = new ServiceProcessInstaller())
            {
                using (ServiceInstaller serviceInstaller = new ServiceInstaller())
                {
                    serviceInstaller.ServiceName = Resources.ApplicationName;
                    serviceInstaller.StartType = ServiceStartMode.Automatic;

                    Installers.AddRange(new System.Configuration.Install.Installer[] { serviceProcessInstaller, serviceInstaller});
                }
            }
        }
    }
}

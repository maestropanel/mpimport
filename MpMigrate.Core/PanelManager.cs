namespace MpMigrate.Core
{
    using MpMigrate.Core.Discovery;
    using System;
    using System.IO;

    public class PanelManager
    {
        public PanelTypes PanelType { get; set; }
        public IDiscovery CurrentPanel { get; set; }

        public void DetermineInstalledPanel()
        {
            PanelType = PanelTypes.Unknown;

            PanelType = DetectPlesk();
            if (PanelType != PanelTypes.Unknown)            
                return;

            PanelType = DetectMaestroPanel();

            if (PanelType != PanelTypes.Unknown)
                return;            
        }


        private PanelTypes DetectPlesk()
        {
            PanelTypes plesk_Panel = PanelTypes.Unknown;

            var check_environment = Environment.GetEnvironmentVariable("plesk_dir", EnvironmentVariableTarget.Machine);

            if (Directory.Exists(check_environment))
            {
                var plesk_version = CoreHelper.GetRegistryKeyValue(@"SOFTWARE\PLESK\PSA Config\Config", "PRODUCT_VERSION");

                if (plesk_version.StartsWith("8.6"))
                {
                    plesk_Panel = PanelTypes.Plesk_86;
                    CurrentPanel = new Plesk_86();
                }
                else if (plesk_version.StartsWith("9.5"))
                {
                    plesk_Panel = PanelTypes.Plesk_95;
                }
                else
                    plesk_Panel = PanelTypes.Unknown;
            }
            else
            {
                plesk_Panel = PanelTypes.Unknown;
            }

            return plesk_Panel;
        }

        private PanelTypes DetectMaestroPanel()
        {
            var check_environment = Environment.GetEnvironmentVariable("MaestroPanelPath", EnvironmentVariableTarget.Machine);

            if (Directory.Exists(check_environment))
                return PanelTypes.MaestroPanel;
            else
                return PanelTypes.Unknown;
        }

    }

    public enum PanelTypes
    {
        Unknown,
        Plesk_86,
        Plesk_95,
        MaestroPanel        
    }
}

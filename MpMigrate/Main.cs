namespace MpMigrate
{
    using MpMigrate.Core;
    using System.Windows.Forms;
    using System;
    using System.Linq;

    public partial class Main : Form
    {
        private PanelManager panel = new PanelManager();        

        public Main()
        {
            InitializeComponent();
            
        }

        private void buttonAutoDiscover_Click(object sender, System.EventArgs e)
        {
            panel.DetermineInstalledPanel();

            labelPanelVersion.Text = panel.CurrentPanel.Version();            

            var database = panel.CurrentPanel.GetDatabase();

            SelectSourceDatabase(database.Provider);
            SelectSourcePanel(panel.PanelType);

            textboxDefaultVhost.Text = panel.CurrentPanel.VhostPath();
            textboxSourceEmailPath.Text = panel.CurrentPanel.GetEmailPath();
        }

        private void SelectSourceDatabase(DatabaseProviders provider)
        {
            switch (provider)
            {
                case DatabaseProviders.Unknown:
                    break;
                case DatabaseProviders.MSSQL:
                    comboSourceDatabase.SelectedIndex = comboSourceDatabase.FindStringExact("MS SQL");
                    break;
                case DatabaseProviders.MYSQL:
                    comboSourceDatabase.SelectedIndex = comboSourceDatabase.FindStringExact("MySQL");
                    break;
                case DatabaseProviders.SQLITE:
                    comboSourceDatabase.SelectedIndex = comboSourceDatabase.FindStringExact("SQLite");
                    break;
                case DatabaseProviders.SQLCE:
                    comboSourceDatabase.SelectedIndex = comboSourceDatabase.FindStringExact("SQLCE");
                    break;
                case DatabaseProviders.ACCESS:
                    comboSourceDatabase.SelectedIndex = comboSourceDatabase.FindStringExact("MS Access");
                    break;
                default:
                    break;
            }
        }

        private void SelectSourcePanel(PanelTypes paneltype)
        {
            switch (paneltype)
            {
                case PanelTypes.Unknown:
                    break;
                case PanelTypes.Plesk_86:
                    comboSourcePanel.SelectedIndex = comboSourcePanel.FindStringExact("Plesk 8.6");
                    break;
                case PanelTypes.Plesk_95:
                    comboSourcePanel.SelectedIndex = comboSourcePanel.FindStringExact("Plesk 9.5");
                    break;
                case PanelTypes.MaestroPanel:
                    comboSourcePanel.SelectedIndex = comboSourcePanel.FindStringExact("MaestroPanel");
                    break;
                default:
                    break;
            }
        }
    }
}

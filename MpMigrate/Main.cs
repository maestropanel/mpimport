namespace MpMigrate
{
    using MpMigrate.Core;
    using System.Windows.Forms;
    using System;
    using System.Linq;
    using MpMigrate.MaestroPanel.Api;
    using MpMigrate.MaestroPanel.Api.Entity;
    using MpMigrate.Core.Entity;
    using MpMigrate.Data.Entity;
    using System.Threading.Tasks;
    using System.IO;
    using System.Threading;

    public partial class Main : Form
    {        
        private MigrateManager _migrate = new MigrateManager();
        private PanelStats panelStats;
        private Task executeTask;
        private CancellationTokenSource ctSource;
        private CancellationToken cancelToken;

        private Whoami currentPanelUser;

        private bool DatabaseConnectionTest;
        private bool ApiConnectionTest;

        private bool DestinationWebTest;
        private bool DestinationMailTest;

        private string defaultApiFormat = "JSON";
        private bool apiSuppressStatusCode = true;
        private int apiDefaultApiPort = 9715;
        private int progressbarMaximum = 0;

        public Main()
        {
            DatabaseConnectionTest = false;
            ApiConnectionTest = false;
            ctSource = new CancellationTokenSource();
            cancelToken = ctSource.Token;

            InitializeComponent();

            _migrate.Action += _migrate_Action;
            executeTask = new Task(delegate { _migrate.Execute(); }, cancelToken);
        }

        private void buttonAutoDiscover_Click(object sender, System.EventArgs e)
        {
            _migrate.DetermineInstalledPanel();
            
            if (_migrate.PanelType == PanelTypes.Unknown)
            {
                MessageBox.Show("Panel cannot be determine", "MaestroPanel", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);

                return;
            }
            
            if (_migrate.CurrentPanel == null)
            {
                MessageBox.Show(String.Format("Panel is {0} but discovery time error. (Provider not found)", _migrate.PanelType), "MaestroPanel", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);

                return;
            }
            
            labelPanelVersion.Text = _migrate.CurrentPanel.Version();

            SelectSourcePanel(_migrate.PanelType);
            SelectSourceDatabase(_migrate.CurrentPanel.GetDatabaseProvider());
            
            textboxDefaultVhost.Text = _migrate.CurrentPanel.VhostPath();
            textboxSourceEmailPath.Text = _migrate.CurrentPanel.GetEmailPath();

            panelStats = _migrate.PanelData.GetPanelStats();
            SetPanelStats();

            DatabaseConnectionTest = true;
        }

        private void SetPanelStats()
        {
            if (panelStats == null)
                return;

            labelDomainCount.Text = panelStats.TotalDomainCount.ToString();
            labelResellerCount.Text = panelStats.TotalResellerCount.ToString();
            labelEmailCount.Text = panelStats.TotalEmailCount.ToString();
            labelDatabaseCount.Text = panelStats.TotalDatabaseCount.ToString();
            labelAliasCount.Text = panelStats.TotalDomainAliasCount.ToString();
            labelSubdomainCount.Text = panelStats.TotalSubdomainCount.ToString();

            labelDomainSpace.Text = String.Format("({0:N2} GB)", panelStats.DomainDiscSpaceToGB());
            labelEmaiSpace.Text = String.Format("({0:N2} GB)", panelStats.EmailDiskSpaceToGB());
            labelDatabaseSpace.Text = String.Format("({0:N2} GB)", panelStats.DatabaseDiskSpaceToGB());
            labelSubdomainDiskSpace.Text = String.Format("({0:N2} GB)", panelStats.SubdomainDiskSpaceToGB());
        }

        private PanelTypes SetSourcePanelType()
        {
            var ptype = PanelTypes.Unknown;

            switch (comboSourcePanel.SelectedItem.ToString())
            {
                case "Plesk 11.x":
                    ptype = PanelTypes.Plesk_11;
                    break;
                case "Plesk 10.x":
                    ptype = PanelTypes.Plesk_10;
                    break;
                case "Plesk 9.x":
                    ptype = PanelTypes.Plesk_95;
                    break;
                case "Plesk 8.x":
                    ptype = PanelTypes.Plesk_86;
                    break;
                case "MaestroPanel":
                    ptype = PanelTypes.MaestroPanel;
                    break;
            }

            return ptype;
        }

        private DatabaseProviders SetSourceDatabaseType()
        {
            var dbtype = DatabaseProviders.Unknown;

            switch (comboSourceDatabase.SelectedItem.ToString())
            {
                case "MySQL":
                    dbtype = DatabaseProviders.MYSQL;
                    break;
                case "MS SQL":
                    dbtype = DatabaseProviders.MSSQL;
                    break;
                case "SQLite":
                    dbtype = DatabaseProviders.SQLITE;
                    break;
                case "MS Access":
                    dbtype = DatabaseProviders.ACCESS;
                    break;
                case "SQLCe":
                    dbtype = DatabaseProviders.SQLCE;
                    break;
                default:
                    dbtype = DatabaseProviders.Unknown;
                    break;
            }

            return dbtype;
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
            //Plesk 11.x
            //Plesk 10.x
            //Plesk 9.x
            //Plesk 8.x

            switch (paneltype)
            {
                case PanelTypes.Unknown:
                    break;
                case PanelTypes.Plesk_86:
                    comboSourcePanel.SelectedIndex = comboSourcePanel.FindStringExact("Plesk 8.x");
                    break;
                case PanelTypes.Plesk_95:
                    comboSourcePanel.SelectedIndex = comboSourcePanel.FindStringExact("Plesk 9.x");
                    break;
                case PanelTypes.MaestroPanel:
                    comboSourcePanel.SelectedIndex = comboSourcePanel.FindStringExact("MaestroPanel");
                    break;
                case PanelTypes.Plesk_11:
                    comboSourcePanel.SelectedIndex = comboSourcePanel.FindStringExact("Plesk 11.x");
                    break;
                case PanelTypes.Plesk_10:
                    comboSourcePanel.SelectedIndex = comboSourcePanel.FindStringExact("Plesk 10.x");
                    break;
                default:
                    break;
            }
        }

        private void connectionButton_Click(object sender, EventArgs e)
        {
            var selectedProvider = comboSourceDatabase.SelectedItem != null ? comboSourceDatabase.SelectedItem.ToString() : String.Empty;

            if (String.IsNullOrEmpty(selectedProvider))
            {
                MessageBox.Show("Please select database provider", "", MessageBoxButtons.OK, MessageBoxIcon.Error);
                comboSourceDatabase.Focus();

                return;
            }

            if (_migrate.PanelType == PanelTypes.Unknown)
            {
                MessageBox.Show("Please select panel", "", MessageBoxButtons.OK, MessageBoxIcon.Error);
                comboSourcePanel.Focus();                

                return;
            }

            using (var dbform = new DatabaseForm(selectedProvider))
            {
                if (dbform.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    DatabaseConnectionTest = true;

                    _migrate.SourceDatabase.Username = dbform.Username;
                    _migrate.SourceDatabase.Password = dbform.Password;
                    _migrate.SourceDatabase.Port = dbform.Port;
                    _migrate.SourceDatabase.Provider = dbform.Provider;
                    _migrate.SourceDatabase.Host = dbform.Host;
                    _migrate.SourceDatabase.DataseFile = dbform.DatabaseFile;
                    _migrate.SourceDatabase.Database = dbform.DatabaseName;

                    _migrate.LoadInstalledPanel(_migrate.PanelType, _migrate.SourceDatabase);

                    panelStats = _migrate.PanelData.GetPanelStats();
                    SetPanelStats();
                }

                dbform.Dispose();
            }
        }

        private void buttonApiTest_Click(object sender, EventArgs e)
        {
            int apiPort = apiDefaultApiPort;

            if (int.TryParse(textPort.Text, out apiPort))
                _migrate.Plan.Destination.ApiPort = int.Parse(textPort.Text);
            else
                _migrate.Plan.Destination.ApiPort = apiDefaultApiPort;

            _migrate.Plan.Destination.ApiHost = textHost.Text;
            _migrate.Plan.Destination.ApiKey = textboxApiKey.Text;            
            _migrate.Plan.Destination.UseHttps = checkBoxHttps.Checked;

            if (String.IsNullOrEmpty(_migrate.Plan.Destination.ApiKey))
            {
                MessageBox.Show("API Key cannot be null", "", MessageBoxButtons.OK, MessageBoxIcon.Error);
                textboxApiKey.Focus();

                return;
            }

            if (String.IsNullOrEmpty(_migrate.Plan.Destination.ApiHost))
            {
                MessageBox.Show("API Host cannot be null", "", MessageBoxButtons.OK, MessageBoxIcon.Error);
                textHost.Focus();

                return;
            }

            _migrate.Api = new ApiClient(_migrate.Plan.Destination.ApiKey, _migrate.Plan.Destination.ApiHost,
                                            _migrate.Plan.Destination.ApiPort, _migrate.Plan.Destination.UseHttps,
                                            defaultApiFormat, apiSuppressStatusCode);
            try
            {                
                var WhoApiResult = _migrate.Api.Whoami();

                if (WhoApiResult.ErrorCode != 0)
                {
                    MessageBox.Show(String.Format("{0}\nResponse Code: {1}\nError Code: {2}", WhoApiResult.Message, WhoApiResult.StatusCode, WhoApiResult.ErrorCode),
                        "MaestroPanel", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    ApiConnectionTest = false;
                }
                else
                {
                    currentPanelUser = WhoApiResult.Details;

                    MessageBox.Show(String.Format("Connected Successful. \n\r Username: {0}", currentPanelUser.Username), "API Test",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);

                    ApiConnectionTest = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(String.Format(ex.Message), "API Test",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);

                ApiConnectionTest = false;
            }
        }

        private void buttonDestination_Click(object sender, EventArgs e)
        {

            using (var dbform = new AuthenticationForm(_migrate.Plan.Destination.DestinationWeb))
            {
                if (dbform.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    _migrate.Plan.Destination.DestinationWeb = dbform.DestHost;
                    DestinationWebTest = true;

                    textBoxDestinationPath.Text = textBoxDestinationPath.Text.Replace("{DESTINATION}", dbform.DestHost.Host);
                }

                dbform.Dispose();
            }
        }

        private void _migrate_Action(MigrateManager m, ApiAction e)
        {
            this.UIThread(delegate 
            {
                progressBarFinish.Value = e.Count;
                labelFinisDomain.Text = e.DomainName;
                labelFinishMessage.Text = e.Message;
                labelFinishCounter.Text = String.Format("{0}/{1}", e.Count, progressbarMaximum);
                labelFinishErrorCount.Text = e.ErrorCount.ToString();

                if (executeTask.IsCompleted)
                {
                    MessageBox.Show("Migration Complete", "", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    buttonShowLogs.Enabled = true;
                }

                if (executeTask.IsCanceled)
                {
                    MessageBox.Show("Operation cancelled by user", "", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    buttonShowLogs.Enabled = true;
                }

                if (executeTask.IsFaulted)
                {
                    MessageBox.Show("Error: "+ executeTask.Exception.Message, "", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    buttonShowLogs.Enabled = true;
                }
            });            
        }        


        private void comboSourcePanel_SelectedIndexChanged(object sender, EventArgs e)
        {            
            _migrate.PanelType =  SetSourcePanelType();
        }

        private void comboSourceDatabase_SelectedIndexChanged(object sender, EventArgs e)
        {
            var selectedDatabase = new PanelDatabase();
            selectedDatabase.Provider = SetSourceDatabaseType();

            _migrate.SourceDatabase = selectedDatabase;
        }

        private void stepSourcePage_Commit(object sender, AeroWizard.WizardPageConfirmEventArgs e)
        {
            if (_migrate.PanelType == PanelTypes.Unknown)
            {
                e.Cancel = true;

                MessageBox.Show("Please select panel type", "", MessageBoxButtons.OK, MessageBoxIcon.Error);
                comboSourcePanel.Focus();

                return;
            }

            if (_migrate.SourceDatabase.Provider == DatabaseProviders.Unknown)
            {
                e.Cancel = true;

                MessageBox.Show("Please select database provider", "", MessageBoxButtons.OK, MessageBoxIcon.Error);
                comboSourceDatabase.Focus();

                return;
            }

            if (!DatabaseConnectionTest)
            {
                e.Cancel = true;

                MessageBox.Show("The database connection could not be tested.", "", MessageBoxButtons.OK, MessageBoxIcon.Error);
                connectionButton.Focus();

                return;
            }
        }

        private void SetProgressBar()
        {
            progressBarFinish.Minimum = 0;

            if (_migrate.Plan.Filter)
            {
                if (_migrate.Plan.FilterType == FilterTypes.Domain)
                {
                    progressBarFinish.Maximum = _migrate.Plan.FilterDomains.Count;
                }

                if (_migrate.Plan.FilterType == FilterTypes.Reseller)
                {
                    progressBarFinish.Maximum = _migrate.PanelData.GetDomains()
                                                            .Where(m => _migrate.Plan.FilterResellers.Contains(m.ClientName))
                                                            .Count();
                }
            }
            else
            {                
                    progressBarFinish.Maximum = (int)panelStats.TotalDomainCount;
            }

            progressbarMaximum = progressBarFinish.Maximum;
            
        }

        private void stepFinishPage_Commit(object sender, AeroWizard.WizardPageConfirmEventArgs e)
        {
            SetProgressBar();

            if (!executeTask.IsCompleted && !executeTask.IsCanceled && !executeTask.IsFaulted)
            {                
                executeTask.Start();
                executeTask.ContinueWith((t) => 
                {
                    MessageBox.Show("Operation Completed","", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    if (t.Exception != null)
                    {
                        foreach (var item in t.Exception.InnerExceptions)
                        {
                            MessageBox.Show(item.Message, "", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }

                    buttonShowLogs.Enabled = true;

                }, TaskContinuationOptions.OnlyOnFaulted);
            }
            else
            {
                MessageBox.Show("Migration already start. Please wait.", "", MessageBoxButtons.OK, MessageBoxIcon.Warning);                
            }
        }

        private void stepDestinationPage_Commit(object sender, AeroWizard.WizardPageConfirmEventArgs e)
        {
            if (String.IsNullOrEmpty(textboxApiKey.Text))
            {
                e.Cancel = true;
                MessageBox.Show("API Key cannot be null", "", MessageBoxButtons.OK, MessageBoxIcon.Error);
                textboxApiKey.Focus();

                return;
            }

            if (String.IsNullOrEmpty(textHost.Text))
            {
                e.Cancel = true;
                MessageBox.Show("API Host cannot be null", "", MessageBoxButtons.OK, MessageBoxIcon.Error);
                textHost.Focus();

                return;
            }

            if (String.IsNullOrEmpty(textPlanName.Text))
            {
                e.Cancel = true;
                MessageBox.Show("Default Domain Plan cannot be null", "", MessageBoxButtons.OK, MessageBoxIcon.Error);
                textPlanName.Focus();

                return;
            }

            if (!ApiConnectionTest)
            {
                e.Cancel = true;

                MessageBox.Show("The API connection could not be tested. Please press test button.", "", MessageBoxButtons.OK, MessageBoxIcon.Error);
                buttonApiTest.Focus();

                return;
            }

            _migrate.Plan.Destination.ApiHost = textHost.Text;
            _migrate.Plan.Destination.ApiKey = textboxApiKey.Text;

            int apiPort = apiDefaultApiPort;

            if (int.TryParse(textPort.Text, out apiPort))
                _migrate.Plan.Destination.ApiPort = int.Parse(textPort.Text);
            else
                _migrate.Plan.Destination.ApiPort = apiDefaultApiPort;

            _migrate.Plan.Destination.UseHttps = checkBoxHttps.Checked;
            _migrate.Plan.Destination.DefaultPlan = textPlanName.Text;            
        }

        private void buttonDestinationMail_Click(object sender, EventArgs e)
        {
            using (var dbform = new AuthenticationForm(_migrate.Plan.Destination.DestinationEmail))
            {
                if (dbform.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    _migrate.Plan.Destination.DestinationEmail = dbform.DestHost;
                    DestinationMailTest = true;

                    textBoxDestinationEmail.Text = textBoxDestinationEmail.Text.Replace("{DESTINATION}", dbform.DestHost.Host);
                }

                dbform.Dispose();
            }
        }

        private void copyFileZip_CheckedChanged(object sender, EventArgs e)
        {
            var destHostStringWeb = _migrate.Plan.Destination.DestinationWeb == null ? "{DESTINATION}" : _migrate.Plan.Destination.DestinationWeb.Host;
            var destHostStringMail = _migrate.Plan.Destination.DestinationEmail == null ? "{DESTINATION}" : _migrate.Plan.Destination.DestinationEmail.Host;

            textBoxDestinationPath.Text = String.Format(@"\\{0}\c$\Packages\{{DOMAIN}}_http.zip", destHostStringWeb);
            textBoxDestinationEmail.Text = String.Format(@"\\{0}\c$\Packages\{{DOMAIN}}_{{MAILBOX}}_mail.zip", destHostStringMail);

            _migrate.Plan.CopyMethod = CopyFileMethods.Zip;
        }

        private void copyFileRaw_CheckedChanged(object sender, EventArgs e)
        {
            var destHostStringWeb = _migrate.Plan.Destination.DestinationWeb == null ? "{DESTINATION}" : _migrate.Plan.Destination.DestinationWeb.Host;
            var destHostStringMail = _migrate.Plan.Destination.DestinationEmail == null ? "{DESTINATION}" : _migrate.Plan.Destination.DestinationEmail.Host;

            textBoxDestinationPath.Text = String.Format(@"\\{0}\c$\vhosts\{{DOMAIN}}\http", destHostStringWeb);
            textBoxDestinationEmail.Text = String.Format(@"\\{0}\c$\Program Files (x86)\Mail Enable\Postoffices\{{DOMAIN}}\MAILROOT\{{MAILBOX}}", destHostStringMail);

            _migrate.Plan.CopyMethod = CopyFileMethods.Raw;
        }

        private void filterDomains_CheckedChanged(object sender, EventArgs e)
        {
            if (filterDomains.Checked)
            {
                using (var domainForm = new FilterDomains(_migrate.PanelData))
                {
                    if (domainForm.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        _migrate.Plan.Filter = true;
                        _migrate.Plan.FilterType = FilterTypes.Domain;
                        _migrate.Plan.FilterDomains = domainForm.Domains;
                    }

                    domainForm.Close();
                    domainForm.Dispose();
                }
            }
        }

        private void filterReseller_CheckedChanged(object sender, EventArgs e)
        {
            if (filterReseller.Checked)
            {
                using (var resellerform = new FilterReseller(_migrate.PanelData))
                {
                    if (resellerform.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        _migrate.Plan.Filter = true;
                        _migrate.Plan.FilterType = FilterTypes.Reseller;
                        _migrate.Plan.FilterResellers = resellerform.Resellers;
                    }

                    resellerform.Close();
                    resellerform.Dispose();
                }
            }
        }

        private void filterNone_CheckedChanged(object sender, EventArgs e)
        {
            if (filterNone.Checked)
            {
                _migrate.Plan.Filter = false;
                _migrate.Plan.FilterType = FilterTypes.None;
                _migrate.Plan.FilterDomains.Clear();
                _migrate.Plan.FilterResellers.Clear();
            }
        }

        private void stepSelectedPage_Commit(object sender, AeroWizard.WizardPageConfirmEventArgs e)
        {
            _migrate.Plan.DomainAliases = SelectAliases.Checked;
            _migrate.Plan.Domains = SelectDomains.Checked;
            _migrate.Plan.Emails = SelectEmails.Checked;
            _migrate.Plan.HostLimits = SelectHostLimits.Checked;
            _migrate.Plan.Resellers = SelectResellers.Checked;
            _migrate.Plan.ResellerLimits = SelectResellerLimits.Checked;
            _migrate.Plan.Subdomains = SelectSubdomains.Checked;
            _migrate.Plan.Databases = SelectDatabases.Checked;
            _migrate.Plan.DnsRecords = SelectDnsRecords.Checked;

            _migrate.Plan.CopyDatabase = CopyDatabase.Checked;
            _migrate.Plan.CopyEmailFiles = CopyEmail.Checked;
            _migrate.Plan.CopyHttpFiles = CopyDomain.Checked;

            _migrate.Plan.DeletePackageAfterMoving = deleteAfterMoving.Checked;
            _migrate.Plan.MiscGeneratePassword = MiscGeneratePassword.Checked;

            _migrate.Api = new ApiClient(_migrate.Plan.Destination.ApiKey, _migrate.Plan.Destination.ApiHost,
                                            _migrate.Plan.Destination.ApiPort, _migrate.Plan.Destination.UseHttps,
                                            defaultApiFormat, apiSuppressStatusCode, generatePassword: MiscGeneratePassword.Checked);
        }

        private void SelectResellers_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void SelectDomains_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void buttonShowLogs_Click(object sender, EventArgs e)
        {
            var LogFilePath = Path.Combine(Environment.CurrentDirectory, "mpimport_log.html");

            if (File.Exists(LogFilePath))
            {
                System.Diagnostics.Process.Start(LogFilePath);
            }
            else
            {
                MessageBox.Show("Log file not found: mpimport_log.html", "", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void label19_Click(object sender, EventArgs e)
        {

        }

        private void stepSource_Cancelling(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (MessageBox.Show("Are you sure cancel this operation?", "", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == System.Windows.Forms.DialogResult.Yes)
            {
                if (!executeTask.IsCompleted && !executeTask.IsCanceled)
                {                    
                    buttonShowLogs.Enabled = true;

                    MessageBox.Show("Operation cancelled", "", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    Application.Exit();
                }
            }
        }

        //private void stepSource_Cancelled(object sender, EventArgs e)
        //{

        //    MessageBox.Show("Cancel", "", MessageBoxButtons.OK, MessageBoxIcon.Warning);

        //    //if (MessageBox.Show("Are you sure cancel this operation?", "", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == System.Windows.Forms.DialogResult.Yes)
        //    //{
        //    //    if (!executeTask.IsCompleted && !executeTask.IsCanceled)
        //    //    {
        //    //        cancelToken.ThrowIfCancellationRequested();
        //    //        buttonShowLogs.Enabled = true;

        //    //        MessageBox.Show("Operation cancelled", "", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        //    //    }
        //    //}            
        //}

    }

    static class ControlExtensions
    {
        static public void UIThread(this Control control, Action code)
        {
            if (control.InvokeRequired)
            {
                control.BeginInvoke(code);
                return;
            }
            code.Invoke();
        }

        static public void UIThreadInvoke(this Control control, Action code)
        {
            if (control.InvokeRequired)
            {
                control.Invoke(code);
                return;
            }

            code.Invoke();
        }
    }
}

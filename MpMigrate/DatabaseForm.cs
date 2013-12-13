namespace MpMigrate
{
    using MpMigrate.Core;
    using System;
    using System.Windows.Forms;

    public partial class DatabaseForm : Form
    {
        public DatabaseProviders Provider { get; set; }

        public string Host { get; set; }
        public int Port { get; set; }
        public string DatabaseName { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string DatabaseFile { get; set; }



        public DatabaseForm()
        {
            Port = 0;
            InitializeComponent();
        }

        public DatabaseForm(string provider)
        {
            Port = 0;
            InitializeComponent();

            if(!String.IsNullOrEmpty(provider))
                comboProvider.SelectedIndex = comboProvider.FindStringExact(provider);
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {            
            DisableForProvider(comboProvider.SelectedItem.ToString());
        }

        private void DisableForProvider(string selectedText)
        {
            switch (selectedText)
            {
                case "MySQL":
                case "Ms SQL":
                    ChangeEnabled(true);
                    break;
                case "SQLite":
                case "SQLCe":
                case "MS Access":
                    ChangeEnabled(false);
                    break;
                default:
                    ChangeEnabled(true);
                    break;
            }            

        }

        private void ChangeEnabled(bool enable)
        {
            buttonDatabaseFileBrowse.Enabled = !enable;
            textboxDatabaseFile.Enabled = !enable;

            textBoxHost.Enabled = enable;
            textBoxPort.Enabled = enable;
            textBoxDatabase.Enabled = enable;
            textBoxUsername.Enabled = enable;
            textBoxPassword.Enabled = enable;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            var errorMsg = String.Empty;
            var currentPort = 0;

            if (Provider == DatabaseProviders.MSSQL || Provider == DatabaseProviders.MYSQL)
            {
                if (String.IsNullOrEmpty(textBoxHost.Text))
                {
                    MessageBox.Show("Host cannot be null", "", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    textBoxHost.Focus();

                    return;
                }

                if (!int.TryParse(textBoxPort.Text, out currentPort))
                {
                    MessageBox.Show("Invlid Port Number", "", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    textBoxPort.Focus();

                    return;
                }

                if (String.IsNullOrEmpty(textBoxDatabase.Text))
                {
                    MessageBox.Show("Database Name cannot be null", "", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    textBoxDatabase.Focus();

                    return;
                }

                if (String.IsNullOrEmpty(textBoxDatabase.Text))
                {
                    MessageBox.Show("Database Name cannot be null", "", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    textBoxDatabase.Focus();

                    return;
                }

                if (String.IsNullOrEmpty(textBoxUsername.Text))
                {
                    MessageBox.Show("User Name cannot be null", "", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    textBoxUsername.Focus();

                    return;
                }

                if (String.IsNullOrEmpty(textBoxPassword.Text))
                {
                    MessageBox.Show("Password cannot be null", "", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    textBoxPassword.Focus();

                    return;
                }
            }

            SetSourceDatabaseType();
            SetConnectionParams();

            var panelDb = new PanelDatabase();
            panelDb.Provider = Provider;
            panelDb.Port = Port;
            panelDb.Password = Password;
            panelDb.Host = Host;
            panelDb.Username = Username;
            panelDb.Database = DatabaseName;
            panelDb.DataseFile = DatabaseFile;

            if (!panelDb.TestConnection(out errorMsg))
            {
                MessageBox.Show(errorMsg, "", MessageBoxButtons.OK, MessageBoxIcon.Error);
                button2.Focus();                
            }
            else
            {
                MessageBox.Show("Connection Success", "", MessageBoxButtons.OK, MessageBoxIcon.Information);
                buttonOK.Enabled = true;
                buttonOK.Focus();
            }
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {

        }

        private void SetSourceDatabaseType()
        {
            switch (comboProvider.SelectedItem.ToString())
            {
                case "MySQL":
                    Provider = DatabaseProviders.MYSQL;
                    break;
                case "MS SQL":
                    Provider = DatabaseProviders.MSSQL;
                    break;
                case "SQLite":
                    Provider = DatabaseProviders.SQLITE;
                    break;
                case "MS Access":
                    Provider = DatabaseProviders.ACCESS;
                    break;
                case "SQLCe":
                    Provider = DatabaseProviders.SQLCE;
                    break;
                default:
                    Provider = DatabaseProviders.Unknown;
                    break;
            }
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            var currentPort = 0;

            if (Provider == DatabaseProviders.MSSQL || Provider == DatabaseProviders.MYSQL)
            {
                if (String.IsNullOrEmpty(textBoxHost.Text))
                {
                    MessageBox.Show("Host cannot be null", "", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    textBoxHost.Focus();

                    return;
                }

                if (!int.TryParse(textBoxPort.Text, out currentPort))
                {
                    MessageBox.Show("Invalid Port Number", "", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    textBoxPort.Focus();

                    return;
                }

                if (String.IsNullOrEmpty(textBoxDatabase.Text))
                {
                    MessageBox.Show("Database Name cannot be null", "", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    textBoxDatabase.Focus();

                    return;
                }

                if (String.IsNullOrEmpty(textBoxDatabase.Text))
                {
                    MessageBox.Show("Database Name cannot be null", "", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    textBoxDatabase.Focus();

                    return;
                }

                if (String.IsNullOrEmpty(textBoxUsername.Text))
                {
                    MessageBox.Show("User Name cannot be null", "", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    textBoxUsername.Focus();

                    return;
                }

                if (String.IsNullOrEmpty(textBoxPassword.Text))
                {
                    MessageBox.Show("Password cannot be null", "", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    textBoxPassword.Focus();

                    return;
                }

            }

            SetConnectionParams();            
            this.DialogResult = System.Windows.Forms.DialogResult.OK;

            this.Close();
        }

        private void buttonDatabaseFileBrowse_Click(object sender, EventArgs e)
        {

            if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                textboxDatabaseFile.Text = openFileDialog1.FileName;
                openFileDialog1.Dispose();
            }
        }

        private void SetConnectionParams()
        {
            int portNumber = 0;

            Host = textBoxHost.Text;
            Username = textBoxUsername.Text;
            Password = textBoxPassword.Text;
            DatabaseFile = textboxDatabaseFile.Text;
            DatabaseName = textBoxDatabase.Text;

            if(int.TryParse(textBoxPort.Text, out portNumber))
                Port = portNumber;
        }
    }
}

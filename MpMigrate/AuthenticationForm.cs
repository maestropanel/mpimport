namespace MpMigrate
{
    using MpMigrate.Core;
    using MpMigrate.Core.Entity;
    using System;
    using System.Windows.Forms;

    public partial class AuthenticationForm : Form
    {
        public UncLocation DestHost { get; set; }

        public AuthenticationForm()
        {
            InitializeComponent();
            DestHost = new UncLocation();            
        }

        public AuthenticationForm(UncLocation host)
        {
            InitializeComponent();
            DestHost = host ?? new UncLocation();
            LoadUncLocation();
        }

        private void LoadUncLocation()
        {
            textBoxHost.Text = DestHost.Host;

            if(!String.IsNullOrEmpty(DestHost.Username))
                textBoxUsername.Text = DestHost.Username;

            textBoxPassword.Text = DestHost.Password;
        }

        private void buttonOK_Click(object sender, System.EventArgs e)
        {

            if (String.IsNullOrEmpty(textBoxHost.Text))
            {
                MessageBox.Show("Host cannot be null", "", MessageBoxButtons.OK, MessageBoxIcon.Error);
                textBoxHost.Focus();
                return;
            }

            if (String.IsNullOrEmpty(textBoxUsername.Text))
            {
                MessageBox.Show("Username cannot be null", "", MessageBoxButtons.OK, MessageBoxIcon.Error);
                textBoxUsername.Focus();
                return;
            }

            if (String.IsNullOrEmpty(textBoxPassword.Text))
            {
                MessageBox.Show("Password cannot be null", "", MessageBoxButtons.OK, MessageBoxIcon.Error);
                textBoxPassword.Focus();
                return;
            }

            DestHost.Host = textBoxHost.Text;
            DestHost.Username = textBoxUsername.Text;
            DestHost.Password = textBoxPassword.Text;

            this.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.Close();
        }

        private void buttonTest_Click(object sender, EventArgs e)
        {
            var errorMsg = String.Empty;

            if (String.IsNullOrEmpty(textBoxHost.Text))
            {
                MessageBox.Show("Host cannot be null", "", MessageBoxButtons.OK, MessageBoxIcon.Error);
                textBoxHost.Focus();
                return;
            }

            if (String.IsNullOrEmpty(textBoxUsername.Text))
            {
                MessageBox.Show("Username cannot be null", "", MessageBoxButtons.OK, MessageBoxIcon.Error);
                textBoxUsername.Focus();
                return;
            }

            if (String.IsNullOrEmpty(textBoxPassword.Text))
            {
                MessageBox.Show("Password cannot be null", "", MessageBoxButtons.OK, MessageBoxIcon.Error);
                textBoxPassword.Focus();
                return;
            }

            DestHost.Host = textBoxHost.Text;
            DestHost.Username = textBoxUsername.Text;
            DestHost.Password = textBoxPassword.Text;

            if (CoreHelper.TestUncPathConnection(DestHost.Host, DestHost.Username, DestHost.Password, out errorMsg))
            {
                MessageBox.Show("Connection Successfully.", "", MessageBoxButtons.OK, MessageBoxIcon.Information);
                buttonOK.Enabled = true;
                buttonOK.Focus();
            }
            else
            {
                MessageBox.Show(errorMsg, "", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {

        }


    }
}

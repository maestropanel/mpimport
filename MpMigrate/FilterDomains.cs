namespace MpMigrate
{
    using MpMigrate.Data.Entity;
    using System;
    using System.Collections.Generic;
    using System.Windows.Forms;

    public partial class FilterDomains : Form
    {
        private DboFactory _paneldb;

        public List<string> Domains { get; set; }


        public FilterDomains(DboFactory paneldb)
        {
            _paneldb = paneldb;
            InitializeComponent();

            Domains = new List<string>();

            LoadDomains();

        }

        public FilterDomains()
        {
            InitializeComponent();
            Domains = new List<string>();
        }

        private void buttonOk_Click(object sender, EventArgs e)
        {

        }

        private void LoadDomains()
        {
            var domainList = _paneldb.GetDomains();
            checkedDomainList.Items.Clear();

            foreach (var item in domainList)            
                checkedDomainList.Items.Add(item.Name);            
        }

        private void checkedDomainList_ItemCheck(object sender, ItemCheckEventArgs e)
        {            
            var selectedItem = checkedDomainList.Items[e.Index].ToString();

            if (e.NewValue == CheckState.Checked)
                if (!Domains.Contains(selectedItem))
                    Domains.Add(selectedItem);

            if (e.NewValue == CheckState.Unchecked)
                if (Domains.Contains(selectedItem))
                    Domains.Remove(selectedItem);            
        }
    }
}

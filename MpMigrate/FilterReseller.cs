namespace MpMigrate
{
    using MpMigrate.Data.Entity;
    using System.Collections.Generic;
    using System.Windows.Forms;

    public partial class FilterReseller : Form
    {
        private DboFactory _paneldb;


        public List<string> Resellers { get; set; }

        public FilterReseller()
        {
            InitializeComponent();
            Resellers = new List<string>();
        }

        public FilterReseller(DboFactory paneldb)
        {
            _paneldb = paneldb;
            InitializeComponent();
            Resellers = new List<string>();

            LoadDomains();
        }

        private void LoadDomains()
        {
            var resellerList = _paneldb.GetResellers();
            checkedResellerlist.Items.Clear();

            foreach (var item in resellerList)
                checkedResellerlist.Items.Add(item.Username);
        }

        private void checkedResellerlist_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            var selectedItem = checkedResellerlist.Items[e.Index].ToString();

            if (e.NewValue == CheckState.Checked)
                if (!Resellers.Contains(selectedItem))
                    Resellers.Add(selectedItem);

            if (e.NewValue == CheckState.Unchecked)
                if (Resellers.Contains(selectedItem))
                    Resellers.Remove(selectedItem);  
        }
    }
}

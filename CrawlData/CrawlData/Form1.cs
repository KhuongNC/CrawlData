using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CrawlData.Model;

namespace CrawlData
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

            // Add items for Website combobox
            CbbWebsite.Items.Add("https://www.cgv.vn/");
            CbbWebsite.Items.Add("https://www.24h.com.vn/");

            // Add items for Formatter combobox
            CbbFormatter.Items.Add(".txt");
            CbbFormatter.Items.Add(".csv");
            CbbFormatter.Items.Add(".xlsx");
            CbbFormatter.Items.Add(".pdf");
        }

        private void BtnExport_Click(object sender, EventArgs e)
        {

        }

        private void CrawlData()
        {
            try
            {
                
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }
    }
}

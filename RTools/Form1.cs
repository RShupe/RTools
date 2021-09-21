using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RTools
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

            const string currentVersion = "1.2.1.0";

            var webRequest = WebRequest.Create(@"https://github.com/RShupe/RTools/raw/main/currentreleaseversion.txt");
            string strContent= "";
            using (var response = webRequest.GetResponse())
            using (var content = response.GetResponseStream())
            using (var reader = new StreamReader(content))
            {
                 strContent = reader.ReadToEnd();
            }
            if (strContent != currentVersion)
            {
                DialogResult dialogResult = MessageBox.Show("A new update is available. Would you like to download it?", "RTools Updater", MessageBoxButtons.YesNo);
                if (dialogResult == DialogResult.Yes)
                {
                    System.Diagnostics.Process.Start("https://github.com/RShupe/RTools/releases");
                }
                else if (dialogResult == DialogResult.No)
                {
                    ///do nothing
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();   
        }

        private Form activeForm = null;
        private void openChildForm(Form childForm)
        {
            if(activeForm!=null)
            {
                activeForm.Close();
            }
            activeForm = childForm;
            childForm.TopLevel = false;
            childForm.FormBorderStyle = FormBorderStyle.None;
            childForm.Dock = DockStyle.Fill;
            childFormPanel.Controls.Add(childForm);
            childFormPanel.Tag = childForm;
            childForm.BringToFront();
            childForm.Show();
        }

        private void btnTextFileJoiner_Click(object sender, EventArgs e)
        {
            openChildForm(new TextFileJoiner());
        }

        private void button1_Click(object sender, EventArgs e)
        {
            openChildForm(new SearchAppend());
        }

        private void button3_Click(object sender, EventArgs e)
        {
            Form helpForm = new HelpForm();
            helpForm.Show();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            openChildForm(new CSVExtractor());
        }
    }
}

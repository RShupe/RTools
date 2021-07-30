using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
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
    }
}

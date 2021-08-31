using Microsoft.Office.Interop.Excel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RTools
{
    public partial class CSVExtractor : Form
    {

        string path1;
        string path2;
        public CSVExtractor()
        {
            InitializeComponent();
            checkParams();
        }

        private System.Data.DataTable CreateTable()
        {
            System.Data.DataTable table = new System.Data.DataTable();

            //columns
            table.Columns.Add("CARD", typeof(string));
            table.Columns.Add("DATE TIME", typeof(string));
            table.Columns.Add("DESCR TYP CODE", typeof(string));
            table.Columns.Add("AMOUNT", typeof(string));
            table.Columns.Add("MERCH ID", typeof(string));
            table.Columns.Add("MCC", typeof(string));
            table.Columns.Add("LOCATION", typeof(string));
            table.Columns.Add("COMPANY", typeof(string));
            table.Columns.Add("CARD STATUS", typeof(string));
            table.Columns.Add("CardHit", typeof(int));
            table.Columns.Add("MerchHit", typeof(int));
            table.Columns.Add("Count", typeof(int));

            //rows
            table.Rows.Add("=\"4855820001106896\"", "=\"8/16 03:02 0000\"", "=\"PUR-CHK PA 057\"", "=\"998.10\"", "=\"000445487599\"", "=\"5970\"", "=\"KY LOUISVILLE\"", "=\"143VINUL\"", "=\"ODE=0100 CARD NOT PRESENT SNP MOBL\"", 1, 1, 1);

            
            return table;
        }

        private void checkParams()
        {
            if (path1Box.Text != "Input file directory" && path2Box.Text != "Output file directory")
            {
                startButton.Enabled = true;
            }
            else
            {
                startButton.Enabled = false;
            }
        }

        private void startButton_Click(object sender, EventArgs e)
        {
            System.Data.DataTable table = CreateTable();
            table.ToCSV(path2);
            MessageBox.Show("Done!");
        }

        private void path1Button_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();

            openFileDialog.InitialDirectory = "c:\\";
            openFileDialog.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
            openFileDialog.FilterIndex = 2;
            openFileDialog.RestoreDirectory = true;

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                //Get the path of specified file
                path1 = openFileDialog.FileName;
                path1Box.Text = openFileDialog.FileName;
            }
            checkParams();
        }

        private void path2Button_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();

            openFileDialog.InitialDirectory = "c:\\";
            openFileDialog.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
            openFileDialog.FilterIndex = 2;
            openFileDialog.RestoreDirectory = true;

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                //Get the path of specified file
                path2 = openFileDialog.FileName;
                path2Box.Text = openFileDialog.FileName;
            }
            checkParams();
        }
    }
}

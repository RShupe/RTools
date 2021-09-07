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
            table.Columns.Add("CARD NUMBER", typeof(string));
            table.Columns.Add("ACCOUNT", typeof(string));
            table.Columns.Add("DATE TIME", typeof(string));
            table.Columns.Add("DESCR TYP CODE", typeof(string));
            table.Columns.Add("AMOUNT", typeof(string));
            table.Columns.Add("MERCH ID", typeof(string));
            table.Columns.Add("MCC", typeof(string));
            table.Columns.Add("LOCATION", typeof(string));
            table.Columns.Add("COMPANY", typeof(string));
            table.Columns.Add("CARD STATUS", typeof(string));



            //populate table by reading in file
            using (StreamReader sr = new StreamReader(path1))
            {
                string currentEntry = "";
                bool bai = false;

                //skip the 12 header lines
                for (int i = 0; i < 11; i++)
                {
                    string tmp = sr.ReadLine();
                }

                bool skip = false;

                while (sr.Peek() >= 0)
                { 
                    //get whole entry
                    for(int i = 0; i < 4; i++)
                    {
                        currentEntry += sr.ReadLine();
                        if (i==2 && currentEntry.Contains("WDL-CHK") || 
                            i == 2 && currentEntry.Contains("INQ-SAV"))
                        {
                            skip = true;
                            i++;
                        }
                        if ((i == 2 && currentEntry.Contains("INQ-CHK")))
                        {
                            skip = true;

                            currentEntry += sr.ReadLine();
                            i++;
                        }
                        if ((i == 2 && currentEntry.Contains("MTC-CHK")) ||
                            (i == 2 && currentEntry.Contains("MTF-CHK")))
                        {
                            skip = true;
                            //bai = true;
                            currentEntry += sr.ReadLine();
                            currentEntry += sr.ReadLine();
                            i++;
                        }
                    }
                    if (currentEntry.StartsWith(" M"))
                    {
                        currentEntry = currentEntry.Remove(0, 17);

                        currentEntry += sr.ReadLine();
                    }
                    if (currentEntry.StartsWith(" AIRLINE"))
                    {
                        currentEntry = currentEntry.Remove(0, 29);

                        currentEntry += sr.ReadLine();
                    }

                    if (!skip)
                    {
                        if (currentEntry.StartsWith("4"))
                        {
                            //break it into variables
                            string cardNum = currentEntry.Substring(0, 16);
                            if (cardNum.Substring(0, 4) == "4855")
                            {
                                string account = "";
                                string dateTime = "";
                                string descTypCode = "";
                                string amount = "";
                                string merchID = "";
                                string mcc = "";
                                string location = "";
                                string company = "";
                                string cardstatus = "";
                                try
                                {
                                    
                                        account = currentEntry.Substring(143, 10).Trim();
                                        dateTime = currentEntry.Substring(23, 12).Trim();
                                        descTypCode = currentEntry.Substring(74, 10).Trim();
                                        amount = currentEntry.Substring(91, 14).Trim();
                                        merchID = currentEntry.Substring(386, 16).Trim();
                                        mcc = currentEntry.Substring(408, 5).Trim();
                                        location = currentEntry.Substring(205, 18).Trim();
                                        company = currentEntry.Substring(224, 21).Trim();

                                        if (company.Contains(","))
                                        {
                                            company = company.Replace(",", "");

                                        }

                                        if (amount.Contains(","))
                                        {
                                            amount = amount.Replace(",", "");

                                        }
                                        cardstatus = currentEntry.Substring(314, 42).Trim();
                                        currentEntry = "";
                                    

                                    if (cardstatus.EndsWith("="))
                                    {
                                        cardstatus = cardstatus.Remove(cardstatus.Length - 1);
                                        currentEntry = "";
                                    }
                                }
                                catch (Exception)
                                {
                                    Console.WriteLine("Something went wrong with assigning variables");
                                    currentEntry = "";
                                }

                                table.Rows.Add("=\"" + cardNum + "\"", "=\"" + account + "\"", "=\"" + dateTime + "\"", "=\"" + descTypCode + "\"",
                                    "=\"" + amount + "\"", "=\"" + merchID + "\"", "=\"" + mcc + "\"", "=\"" + location + "\"", "=\"" + company + "\"",
                                    "=\"" + cardstatus + "\"");
                                currentEntry = "";
                                bai = false;
                            }
                            else
                            {
                                currentEntry = "";
                            }
                        }
                        else
                        {
                            currentEntry = "";
                            //skip the 12 header lines
                            for (int i = 0; i < 8; i++)
                            {
                                string tmp = sr.ReadLine();
                            }
                            currentEntry = "";
                        }
                    }
                    else
                    {
                        currentEntry = "";
                        skip = false;
                    }
                }
            }

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

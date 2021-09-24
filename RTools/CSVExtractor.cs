using Microsoft.Office.Interop.Excel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
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
                int entries = 1;
                

                //skip the 12 header lines
                for (int i = 0; i < 11; i++)
                {
                    string tmp = sr.ReadLine();
                }
                
                int nextEntry = 0;
                while (sr.Peek() >= 0)
                {

                    char c;
                    if (nextEntry > 0)
                    {
                        currentEntry += nextEntry;
                        nextEntry = 0;
                    }
                    else
                    { 
                        c = (char)sr.Read();
                        currentEntry += c.ToString();
                    }


                    if (currentEntry.StartsWith("485582"))
                    {
                        bool end = false;
                        while (!end)
                        {
                            if (sr.Peek() == -1)
                            {
                                break;
                            }
                            c = (char)sr.Read();
                            currentEntry += c.ToString();
                            if (currentEntry.EndsWith("485582") || currentEntry.EndsWith("424447") || currentEntry.EndsWith("568513"))
                            {

                                if (currentEntry.EndsWith("485582"))
                                {
                                    nextEntry = 485582;
                                }
                                else if (currentEntry.EndsWith("42447"))
                                {
                                    nextEntry = 424447;
                                }
                                else
                                {
                                    nextEntry = 568513;
                                }

                                end = true;

                                if (currentEntry.Contains("WDL-CHK") ||
                                    currentEntry.Contains("INQ-SAV") ||
                                    currentEntry.Contains("INQ-CHK") ||
                                    currentEntry.Contains("MTC-CHK") ||
                                    currentEntry.Contains("WDL-CCR") ||
                                    currentEntry.Contains("WDL-SAV") ||
                                    currentEntry.Contains("INQ-CCR") ||
                                    currentEntry.Contains("TFR-CHK") ||
                                    currentEntry.Contains("MTF-CHK"))
                                {
                                    currentEntry = "";
                                }
                                else
                                {
                                    string cardNum = currentEntry.Substring(0, 16);
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

                                        account = currentEntry.Substring(145, 10).Trim();
                                        dateTime = currentEntry.Substring(23, 12).Trim();
                                        descTypCode = currentEntry.Substring(74, 10).Trim();
                                        amount = currentEntry.Substring(91, 14).Trim();
                                        merchID = currentEntry.Substring(392, 16).Trim();
                                        mcc = currentEntry.Substring(415, 5).Trim();

                                        if (mcc.EndsWith("P"))
                                        {
                                            mcc = currentEntry.Substring(413, 5).Trim();
                                        }

                                        location = currentEntry.Substring(207, 18).Trim();
                                        company = currentEntry.Substring(224, 21).Trim();

                                        if (company.Contains(","))
                                        {
                                            company = company.Replace(",", "");

                                        }
                                        if (location.Contains(","))
                                        {
                                            location = location.Replace(",", "");

                                        }

                                        if (amount.Contains(","))
                                        {
                                            amount = amount.Replace(",", "");

                                        }
                                        cardstatus = currentEntry.Substring(314, 41).Trim();
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
                                        if (sr.Peek() == -1)
                                        {
                                            break;
                                        }
                                    }

                                    table.Rows.Add("=\"" + cardNum + "\"", "=\"" + account + "\"", "=\"" + dateTime + "\"", "=\"" + descTypCode + "\"",
                                        "=\"" + amount + "\"", "=\"" + merchID + "\"", "=\"" + mcc + "\"", "=\"" + location + "\"", "=\"" + company + "\"",
                                        "=\"" + cardstatus + "\"");
                                    currentEntry = "";
                                    if (sr.Peek() == -1)
                                    {
                                        break;
                                    }

                                }

                            }
                        }
                    }
                    else if (currentEntry.StartsWith("424447"))
                    {
                        bool end = false;
                        while (!end)
                        {
                            if (sr.Peek() == -1)
                            {
                                break;
                            }
                            c = (char)sr.Read();
                            currentEntry += c.ToString();
                            if (currentEntry.EndsWith("485582") || currentEntry.EndsWith("424447") || currentEntry.EndsWith("568513"))
                            {
                                if (currentEntry.EndsWith("485582"))
                                {
                                    nextEntry = 485582;
                                }
                                else if (currentEntry.EndsWith("42447"))
                                {
                                    nextEntry = 424447;
                                }
                                else
                                {
                                    nextEntry = 568513;
                                }

                                end = true;
                                currentEntry = "";
                            }
                        }
                    }
                    else if (currentEntry.StartsWith("568513"))
                    {
                        bool end = false;

                        while (!end)
                        {
                            if(sr.Peek() == -1)
                            {
                                break;
                            }
                            c = (char)sr.Read();
                            currentEntry += c.ToString();
                            if (currentEntry.EndsWith("485582") || currentEntry.EndsWith("424447") || currentEntry.EndsWith("568513"))
                            {
                                if (currentEntry.EndsWith("485582"))
                                {
                                    nextEntry = 485582;
                                }
                                else if (currentEntry.EndsWith("42447"))
                                {
                                    nextEntry = 424447;
                                }
                                else
                                {
                                    nextEntry = 568513;
                                }

                                end = true;
                                currentEntry = "";
                                if (sr.Peek() == -1)
                                {
                                    break;
                                }
                            }
                        }
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

        private async void startButton_Click(object sender, EventArgs e)
        {
            await ImportAsync();
        }

        private async Task ImportAsync()
        {
            loadingCircle.Visible = true;
            startButton.Enabled = false;
            await Task.Run(() =>
            {
                System.Data.DataTable table = CreateTable();
                table.ToCSV(path2);
            });

            startButton.Enabled = true;
            loadingCircle.Visible = false;
            MessageBox.Show("Done!\n\nFile saved as: output.csv");
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
            FolderBrowserDialog folderDlg = new FolderBrowserDialog();
            folderDlg.ShowNewFolderButton = true;
            // Show the FolderBrowserDialog.  
            DialogResult result = folderDlg.ShowDialog();
            if (result == DialogResult.OK)
            {
                path2 = folderDlg.SelectedPath;
                path2Box.Text = folderDlg.SelectedPath;

                Environment.SpecialFolder root = folderDlg.RootFolder;


                path2 += "\\output.csv";
            }
            checkParams();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            MessageBox.Show("INQ-SAV\nINQ-CHK\nINQ-CCR\n\nWDL-CCR\nWDL-SAV\nWDL-CHK\n\nTFR-CHK\nMTC-CHK\nMTF-CHK");
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }
    }
}

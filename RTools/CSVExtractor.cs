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
                bool bai = false;

                //skip the 12 header lines
                for (int i = 0; i < 11; i++)
                {
                    string tmp = sr.ReadLine();
                }
                int entry = 0;
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
                            c = (char)sr.Read();
                            currentEntry += c.ToString();
                            if (currentEntry.EndsWith("485582") || currentEntry.EndsWith("424447") || currentEntry.EndsWith("56851"))
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
                                    nextEntry = 56851;
                                }

                                end = true;

                                if (currentEntry.Contains("WDL-CHK") ||
                                    currentEntry.Contains("INQ-SAV") ||
                                    currentEntry.Contains("INQ-CHK") ||
                                    currentEntry.Contains("MTC-CHK") ||
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
                                }

                            }
                        }
                    }
                    else if (currentEntry.StartsWith("424447"))
                    {
                        bool end = false;
                        while (!end)
                        {
                            c = (char)sr.Read();
                            currentEntry += c.ToString();
                            if (currentEntry.EndsWith("485582") || currentEntry.EndsWith("424447") || currentEntry.EndsWith("56851"))
                            {
                                if (currentEntry.EndsWith("485582"))
                                {
                                    nextEntry = 485582;
                                }
                                else if(currentEntry.EndsWith("42447"))
                                {
                                    nextEntry = 424447;
                                }
                                else
                                {
                                    nextEntry = 56851;
                                }

                                end = true;
                                currentEntry = "";
                            }
                        }
                    }
                    else if (currentEntry.StartsWith("56851")) 
                    {  
                        bool end = false;

                    while (!end)
                    {
                        c = (char)sr.Read();
                        currentEntry += c.ToString();
                        if (currentEntry.EndsWith("485582") || currentEntry.EndsWith("424447") || currentEntry.EndsWith("56851"))
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
                                nextEntry = 56851;
                            }

                            end = true;
                            currentEntry = "";
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

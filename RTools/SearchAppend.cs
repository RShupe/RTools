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
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace RTools
{
    public partial class SearchAppend : Form
    {
        public SearchAppend()
        {
            InitializeComponent();
            checkParams();
        }

        string path1 = "";
        string path2 = "";

        List<string> kwList = new List<string>();

        public void DoWork(IProgress<int> progress)
        {
            List<string> allLines = new List<string>(File.ReadAllLines(path1));
            List<string> webLines = new List<string>();
            List<int> linesToRemove = new List<int>();

            for (int i = 0; i < listBox1.Items.Count; i++)
            {
                kwList.Add(listBox1.Items[i].ToString());
            }
            progress.Report(25);

            for (int i = 0; i < allLines.Count; i++)
            {
                for (int y = 0; y < kwList.Count; y++)
                {
                    if (allLines[i].Contains(kwList[y]))
                    {
                        webLines.Add(allLines[i]);
                        linesToRemove.Add(i);
                        break;
                    }
                }
            }
            progress.Report(50);
            try
            {
                linesToRemove.Reverse();
                for (int i = 0; i < linesToRemove.Count; i++)
                {
                    allLines.RemoveAt(linesToRemove[i]);
                }
            }
            catch (Exception)
            {
                Console.WriteLine("Failed trying to remove lines!");
            }
            progress.Report(60);
            File.AppendAllLines(path2, webLines.ToArray());
            progress.Report(75);
            File.WriteAllLines(path1, allLines.ToArray());
            progress.Report(100);
            System.Threading.Thread.Sleep(500);
        }

        private void checkParams()
        {
            if (path1Box.Text != "Input file directory" && path2Box.Text != "Output file directory" && listBox1.Items.Count > 0)
            {
                startButton.Enabled = true;
            }
            else
            {
                startButton.Enabled = false;
            }
        }

        private void path1Button_Click_1(object sender, EventArgs e)
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

        private void path2Button_Click_1(object sender, EventArgs e)
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

        private void addButton_Click_1(object sender, EventArgs e)
        {
            AddList addForm = new AddList();

            if (addForm.ShowDialog(this) == DialogResult.OK)
            {
                listBox1.Items.Add(addForm.getItem());
                addForm.Close();
                addForm.Dispose();
            }

            listBox1.Refresh();
            checkParams();
        }

        private void RemoveButton_Click_1(object sender, EventArgs e)
        {
            try
            {
                listBox1.Items.RemoveAt(listBox1.SelectedIndex);
            }
            catch (Exception)
            {
                Console.WriteLine("Nothing selected in the listbox");
            }

            checkParams();
        }

        private async void startButton_Click_1(object sender, EventArgs e)
        {
            var progress = new Progress<int>(v =>
            {
                // This lambda is executed in context of UI thread,
                // so it can safely update form controls
                progressBar1.Value = v;
            });

            // Run operation in another thread
            await Task.Run(() => DoWork(progress));


            if (progressBar1.Value == 100)
            {
                progressBar1.Value = 0;
                MessageBox.Show("Success!");
            }
            else
            {
                MessageBox.Show("Something went wrong!");
            }
        }
    }
}

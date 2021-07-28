using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RTools
{
    public partial class TextFileJoiner : Form
    {

        private FileHandler handler;

        public TextFileJoiner()
        {
            InitializeComponent();
            handler = new FileHandler();
        }


        private void btnOpen_Click(object sender, EventArgs e)
        {
            numberLabel.Text = "Number of files to be joined: " +
                                handler.OpenFileHandler().ToString();

            if (numberLabel.Text != "Number of files to be joined: 0")
            {
                readyLabel.Text = "Ready to join files!";
            }
        }

        private void btnConvert_Click(object sender, EventArgs e)
        {
            if (handler.getNumFiles() == 0)
            {
                DialogResult dialogResult = MessageBox.Show("They're no files selected to be joined. Are you sure you want to write a blank file?", "Write a blank file?", MessageBoxButtons.YesNo);
                if (dialogResult == DialogResult.Yes)
                {
                    handler.SaveFileHandler();
                    handler.Convert();

                    numberLabel.Text = "Number of files to be joined: 0";
                    readyLabel.Text = "Waiting for files...";
                    MessageBox.Show("Success!");
                    this.Refresh();
                }
                else if (dialogResult == DialogResult.No)
                {
                    return;
                }
                else
                {
                    handler.SaveFileHandler();
                    handler.Convert();

                    numberLabel.Text = "Number of files to be joined: 0";
                    readyLabel.Text = "Waiting for files...";
                    MessageBox.Show("Success!");
                    this.Refresh();
                }
            }
            else
            {
                handler.SaveFileHandler();
                handler.Convert();

                numberLabel.Text = "Number of files to be joined: 0";
                readyLabel.Text = "Waiting for files...";
                MessageBox.Show("Success!");
                this.Refresh();
            }
        }

        private void deleteFilesBox_CheckedChanged(object sender, EventArgs e)
        {
            if (deleteFilesBox.Checked)
            {
                handler.SetDeleteFiles(true);
            }
            else
            {
                handler.SetDeleteFiles(false);
            }
        }

        private void numSpaces_ValueChanged(object sender, EventArgs e)
        {
            handler.SetNumOfSpaces((int)numSpaces.Value);
        }
    }
}

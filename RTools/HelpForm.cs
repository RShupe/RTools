﻿using System;
using System.Windows.Forms;

namespace RTools
{
    public partial class HelpForm : Form
    {
        public HelpForm()
        {
            InitializeComponent();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnTextFileJoiner_Click(object sender, EventArgs e)
        {
            richTextBox1.Text = "This program takes in multiple text files and combines them into one while keeping the formatting." +
                "\nThis program is open source and available on GitHub at: https://github.com/RShupe/RTools" +
                "\n\n\n\n" +
                "Running Text File Joiner in the console example: rtools.exe tfj outputpath.txt linestoseperate (Y/N)Deleteoriginalfiles inputpath1.txt inputpath2.txt ...\n\n" +
                "ex. rtools.exe tfj C:\\\\output.txt 5 N C:\\\\input1.txt C:\\input2.txt";
        }


        private void button1_Click(object sender, EventArgs e)
        {
            richTextBox1.Text = "This program takes in a file and looks for a line with a keyword. \nThe program then takes the line out of the initial file and appends it to a new file." +
                "\nThis program is open source and available on GitHub at: https://github.com/RShupe/RTools" +
                "\n\n\n\n" +
                "Running Search Append in the console example: rtools.exe sa inputpath.txt outputpath.txt keyword1 keyword2... " +
                "\n\n ex. rtools.exe sa C:\\\\input.txt C:\\\\output.txt keyword1 keyword2 ..." +
                "\n\n[Console Only] If you have quotes in your keywords, use a forward slash before them!";
        }
        private void button3_Click(object sender, EventArgs e)
        {
            richTextBox1.Text = "This program is open source and available on GitHub at: https://github.com/RShupe/RTools" +
                "\n\n Version 1.2.2";
        }

        private void button4_Click(object sender, EventArgs e)
        {
            richTextBox1.Text = "This tool takes in a specific text file called a \"Cardholder Transaction Detail Report\"" +
                "\n\nThis early version of the program detects debit card transactions and exports them from the text file into a CSV Format." +
                "\n\nFuture versions of this program will have more customizability on what it looks for and how it is formatted." +
                "\n\nThis program is open source and available on GitHub at: https://github.com/RShupe/RTools" +
                "\n\n\n\n";
        }
    }
}

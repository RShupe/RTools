using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Net.Mime.MediaTypeNames;

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
                "\n\n Version 1.0.0.2";
        }
    }
}

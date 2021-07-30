using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RTools
{
    internal class FileHandler
    {
        public List<string> fileNames;
        public string saveDestination = "";
        public int numberOfSpaces;
        private string spaces = "";
        private bool deleteOriginalFiles;

        /// <summary>
        /// Basic no argument constructor.
        /// </summary>
        public FileHandler()
        {
            fileNames = new List<string>();
            SetNumOfSpaces(1);
        }

        /// <summary>
        /// This method sets the number of spaces in between each text file in the output
        /// </summary>
        /// <param name="inspaces"></param>
        public void SetNumOfSpaces(int inspaces)
        {
            numberOfSpaces = inspaces;
            this.spaces = "";
            for (int i = 0; i <= numberOfSpaces; i++)
            {
                this.spaces += "\n";
            }
        }

        /// <summary>
        /// This sets if the handler will delete the original files or not.
        /// </summary>
        /// <param name="inBool"></param>
        public void SetDeleteFiles(bool inBool)
        {
            deleteOriginalFiles = inBool;
        }

        /// <summary>
        /// this method returns the number of files ready to be joined.
        /// </summary>
        /// <returns>int number of files to be joined.</returns>
        public int getNumFiles()
        {
            return fileNames.Count;
        }

        /// <summary>
        /// This method copys the text from the selected files and adds them to the new file created.
        /// </summary>
        public void Convert()
        {
            for (int i = 0; i < fileNames.Count; i++)
            {
                string content = "";
                content += File.ReadAllText(fileNames[i]);

                try
                {
                    if(i == fileNames.Count - 1)
                    {
                        File.AppendAllText(saveDestination, content);
                        content = "";
                    }
                    else
                    {
                        File.AppendAllText(saveDestination, content);
                        content = "";

                        File.AppendAllText(saveDestination, spaces);
                    }
                    
                }
                catch (Exception)
                {
                    Console.WriteLine("Failed. Save Dialog was probably closed without selecting a file.");
                }
            }

            if (deleteOriginalFiles)
            {
                for (int i = 0; i < fileNames.Count; i++)
                {
                    File.Delete(fileNames[i]);
                }

                fileNames.Clear();
            }

            fileNames.Clear();
        }

        /// <summary>
        /// Opens the open file dialog and stores the file path of the selected files.
        /// </summary>
        /// <returns>the number of files to be translated for the main window to use.</returns>
        public int OpenFileHandler()
        {
            int numberOfFiles = 0;
            fileNames.Clear();

            OpenFileDialog OpenDlg = new OpenFileDialog();
            OpenDlg.Filter = "text files|*.txt;*.text";
            OpenDlg.Multiselect = true;
            OpenDlg.InitialDirectory = Application.StartupPath;
            OpenDlg.Title = "Select text files you wish to combine.";

            if (DialogResult.Cancel != OpenDlg.ShowDialog())
            {
                int i = 0;
                foreach (String file in OpenDlg.FileNames)
                {
                    numberOfFiles++;
                    fileNames.Add(OpenDlg.FileNames[i]);
                    i++;
                }
            }
            return numberOfFiles;
        }

        /// <summary>
        /// Opens the save file dialog and gets the filepath of the file to be written.
        /// </summary>
        public void SaveFileHandler()
        {
            SaveFileDialog dlg = new SaveFileDialog();

            dlg.InitialDirectory = Application.StartupPath;
            dlg.Title = "Save the text file";
            dlg.Filter = "text files|*.txt|all files|*.*";

            if (dlg.ShowDialog() == DialogResult.Cancel)
            {
                return;
            }

            saveDestination = dlg.FileName;

            StreamWriter writer = null;

            try
            {
                writer = new StreamWriter(new FileStream(dlg.FileName, FileMode.Create, FileAccess.Write));
                saveDestination = dlg.FileName;
            }
            finally
            {
                if (writer != null)
                {
                    writer.Close();
                }
            }
        }
    }
}

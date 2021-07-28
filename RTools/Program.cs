using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RTools
{
    static class Program
    {
        // defines for commandline output
        [DllImport("kernel32.dll")]
        static extern bool AttachConsole(int dwProcessId);
        private const int ATTACH_PARENT_PROCESS = -1;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            // redirect console output to parent process;
            // must be before any calls to Console.WriteLine()
            AttachConsole(ATTACH_PARENT_PROCESS);

            if (args.Length >= 4)
            {
                //search and append
                if(args[0] == "sa")
                {
                    string path1 = args[1].ToString();
                    string path2 = args[2].ToString();

                    List<string> kwList = new List<string>();
                    for (int i = 3; i < args.Length; i++)
                    {
                        kwList.Add(args[i].ToString());
                    }

                    List<string> allLines = new List<string>(File.ReadAllLines(path1));
                    List<string> webLines = new List<string>();
                    List<int> linesToRemove = new List<int>();

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
                    File.AppendAllLines(path2, webLines.ToArray());
                    File.WriteAllLines(path1, allLines.ToArray());
                    Application.Exit();
                }
                //TextFileJoiner
                else if(args[0] == "tfj")
                {

                }
                else
                {
                    Console.WriteLine("[ERROR] Tool not found!");
                }
                
            }
            else if (args.Length < 3 && args.Length != 0)
            {
                Console.WriteLine("[ERROR] NOT ENOUGH ARGUMENTS, ATLEAST 4 REQUIRED, {tool to use} {path to search} {path to output} {keyword1}");
            }
            else
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new Form1());
            }
        }
    }
}

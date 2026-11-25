using System;

namespace RTools
{
    internal static class ConsoleHelp
    {
        public const string JoinSyntax = "RTools.exe tfj <output> <blank lines> <Y/N delete> <input1> [input2 ...]";
        public const string SearchSyntax = "RTools.exe sa <input> <output> <keyword1> [keyword2 ...]";
        public const string JoinExample = @"start """" /wait RTools.exe tfj ""C:\output.txt"" 1 N ""C:\input1.txt"" ""C:\input2.txt""";
        public const string SearchExample = @"start """" /wait RTools.exe sa ""C:\input.txt"" ""C:\output.txt"" ""ERROR"" ""WARNING""";
        public const string JoinDescription = "Join text files in argument order. Blank lines: 0–1000. N keeps originals; Y deletes them only after successful output. An existing output is replaced without asking.";
        public const string SearchDescription = "Move entire lines containing any literal keyword (case-sensitive) from the input to the output. Existing output is appended to. The source is modified and a backup is created when lines are moved. Use the GUI for copy-only or ignore-case mode.";
        public static string Usage(string version)
        {
            return "RTools " + version + Environment.NewLine +
                "  " + JoinSyntax + Environment.NewLine +
                "    " + JoinDescription + Environment.NewLine +
                "  " + SearchSyntax + Environment.NewLine +
                "    " + SearchDescription + Environment.NewLine +
                "  RTools.exe --help (also: help, -h)" + Environment.NewLine +
                "  RTools.exe (opens the toolbox)" + Environment.NewLine +
                "Quote paths and keywords containing spaces. Only tfj and sa have console modes." + Environment.NewLine +
                "In an interactive Command Prompt, use start \"\" /wait to wait for the WinExe to finish." + Environment.NewLine +
                "Examples:" + Environment.NewLine + "  " + JoinExample + Environment.NewLine + "  " + SearchExample;
        }
    }
}

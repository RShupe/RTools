using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RTools
{
    public partial class MainForm : Form
    {
        private MainFormRuntime? runtime;

        public MainForm()
        {
            InitializeComponent();
        }

        // Only Program.Main calls this. Visual Studio instantiates the plain form
        // without loading settings, creating pages, styling controls or doing I/O.
        internal void StartRuntime(bool checkUpdatesOnStartup)
        {
            if (runtime != null)
                return;
            runtime = new MainFormRuntime(this, checkUpdatesOnStartup);
            Load += runtime.OnLoad;
        }

        internal bool Busy { get { return runtime != null && runtime.Busy; } }
        internal CancellationToken Lifetime { get { return runtime == null ? CancellationToken.None : runtime.Lifetime; } }
        internal string ActiveTool { get { return runtime == null ? "Home" : runtime.ActiveTool; } }
        internal static Version CurrentVersion { get { return Assembly.GetExecutingAssembly().GetName().Version ?? new Version(0, 0, 0); } }
        internal static string DisplayVersion { get { return CurrentVersion.ToString(3); } }

        internal Task Run<T>(Func<CancellationToken, IProgress<string>, T> work, Action<T> completed)
        {
            if (runtime == null) throw new InvalidOperationException("The application runtime has not started.");
            return runtime.Run(work, completed);
        }
    }
}

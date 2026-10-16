using System;
using System.Threading;
using System.Windows.Forms;

namespace GhostHunterLauncher
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            bool firstInstance;
            using (new Mutex(true, "GhostHunterLauncher.SingleInstance", out firstInstance))
            {
                if (!firstInstance) return;
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new LauncherForm());
            }
        }
    }
}

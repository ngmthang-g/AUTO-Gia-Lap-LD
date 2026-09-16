using System;
using System.Windows.Forms;
using AutoGiaLapLD.UI;

namespace AutoGiaLapLD
{
    internal static class Program
    {
        [STAThread]
        private static int Main(string[] args)
        {
            if (args != null && Array.Exists(args, a => string.Equals(a, "--self-test", StringComparison.OrdinalIgnoreCase)))
            {
                return SelfTests.RunAll() ? 0 : 1;
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
            return 0;
        }
    }
}

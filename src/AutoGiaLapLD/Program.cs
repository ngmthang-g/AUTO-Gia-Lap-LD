using System;
using System.Windows.Forms;

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

            MessageBox.Show("V0.1 production UI is not implemented in the RED test commit.", "AUTO Gia Lap LD");
            return 0;
        }
    }
}

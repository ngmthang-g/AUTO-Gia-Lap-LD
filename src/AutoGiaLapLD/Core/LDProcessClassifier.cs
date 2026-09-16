using System;

namespace AutoGiaLapLD.Core
{
    internal static class LDProcessClassifier
    {
        public static bool IsLDPlayerWindow(string processName, string windowTitle, string className)
        {
            string process = processName ?? string.Empty;
            string title = windowTitle ?? string.Empty;
            string cls = className ?? string.Empty;

            if (process.Equals("dnplayer", StringComparison.OrdinalIgnoreCase)) return true;
            if (process.IndexOf("ldplayer", StringComparison.OrdinalIgnoreCase) >= 0) return true;
            if (process.IndexOf("leidian", StringComparison.OrdinalIgnoreCase) >= 0) return true;
            if (title.IndexOf("LDPlayer", StringComparison.OrdinalIgnoreCase) >= 0) return true;
            if (cls.IndexOf("LDPlayer", StringComparison.OrdinalIgnoreCase) >= 0) return true;

            return false;
        }
    }
}

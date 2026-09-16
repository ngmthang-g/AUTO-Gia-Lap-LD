using System;

namespace AutoGiaLapLD.Models
{
    internal sealed class LDInstance
    {
        public int ProcessId { get; set; }
        public string ProcessName { get; set; }
        public string Title { get; set; }
        public IntPtr MainHandle { get; set; }
        public string MainClass { get; set; }
        public IntPtr TargetHandle { get; set; }
        public string TargetClass { get; set; }
        public int TargetWidth { get; set; }
        public int TargetHeight { get; set; }

        public string MainHandleHex { get { return FormatHandle(MainHandle); } }
        public string TargetHandleHex { get { return FormatHandle(TargetHandle); } }
        public string TargetSize { get { return TargetWidth + "x" + TargetHeight; } }

        public override string ToString()
        {
            return string.Format("{0} | PID {1} | {2}", Title, ProcessId, TargetHandleHex);
        }

        private static string FormatHandle(IntPtr handle)
        {
            unchecked
            {
                return "0x" + handle.ToInt64().ToString("X");
            }
        }
    }
}

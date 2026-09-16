using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using AutoGiaLapLD.Core;
using AutoGiaLapLD.Interop;
using AutoGiaLapLD.Models;

namespace AutoGiaLapLD.Services
{
    internal sealed class LDWindowScanner
    {
        private sealed class WindowCandidate
        {
            public IntPtr Handle;
            public string ClassName;
            public string Text;
            public int Width;
            public int Height;
            public long Area { get { return (long)Width * Height; } }
        }

        public List<LDInstance> Scan()
        {
            var result = new List<LDInstance>();

            NativeMethods.EnumWindows((hWnd, lParam) =>
            {
                try
                {
                    if (!NativeMethods.IsWindowVisible(hWnd)) return true;

                    uint rawPid;
                    NativeMethods.GetWindowThreadProcessId(hWnd, out rawPid);
                    if (rawPid == 0 || rawPid > int.MaxValue) return true;

                    Process process;
                    try
                    {
                        process = Process.GetProcessById((int)rawPid);
                    }
                    catch
                    {
                        return true;
                    }

                    string processName;
                    try { processName = process.ProcessName; }
                    catch { processName = string.Empty; }

                    string title = GetWindowText(hWnd);
                    string className = GetClassName(hWnd);

                    if (!LDProcessClassifier.IsLDPlayerWindow(processName, title, className))
                        return true;

                    WindowCandidate target = FindBestTarget(hWnd);
                    if (target == null)
                    {
                        target = BuildCandidate(hWnd);
                    }

                    if (target == null || target.Handle == IntPtr.Zero)
                        return true;

                    if (string.IsNullOrWhiteSpace(title))
                        title = processName + " PID " + rawPid;

                    result.Add(new LDInstance
                    {
                        ProcessId = (int)rawPid,
                        ProcessName = processName,
                        Title = title,
                        MainHandle = hWnd,
                        MainClass = className,
                        TargetHandle = target.Handle,
                        TargetClass = target.ClassName,
                        TargetWidth = target.Width,
                        TargetHeight = target.Height
                    });
                }
                catch
                {
                    // A window can disappear while EnumWindows is running. Skip it and continue.
                }

                return true;
            }, IntPtr.Zero);

            return result
                .GroupBy(x => x.MainHandle)
                .Select(g => g.First())
                .OrderBy(x => x.Title, StringComparer.OrdinalIgnoreCase)
                .ThenBy(x => x.ProcessId)
                .ToList();
        }

        private static WindowCandidate FindBestTarget(IntPtr mainHandle)
        {
            WindowCandidate main = BuildCandidate(mainHandle);
            if (main == null) return null;

            var children = new List<WindowCandidate>();
            NativeMethods.EnumChildWindows(mainHandle, (child, lParam) =>
            {
                WindowCandidate candidate = BuildCandidate(child);
                if (candidate != null && candidate.Width > 0 && candidate.Height > 0)
                    children.Add(candidate);
                return true;
            }, IntPtr.Zero);

            if (children.Count == 0) return main;

            WindowCandidate markedRender = children
                .Where(IsLikelyRenderWindow)
                .OrderByDescending(x => x.Area)
                .FirstOrDefault();

            if (markedRender != null)
                return markedRender;

            long minimumUsefulArea = Math.Max(1L, (long)(main.Area * 0.35));
            WindowCandidate largestUsefulChild = children
                .Where(x => x.Area >= minimumUsefulArea)
                .OrderByDescending(x => x.Area)
                .FirstOrDefault();

            return largestUsefulChild ?? main;
        }

        private static bool IsLikelyRenderWindow(WindowCandidate candidate)
        {
            string marker = (candidate.ClassName + " " + candidate.Text).ToLowerInvariant();
            return marker.Contains("render") ||
                   marker.Contains("surface") ||
                   marker.Contains("opengl") ||
                   marker.Contains("subwin") ||
                   marker.Contains("player");
        }

        private static WindowCandidate BuildCandidate(IntPtr hWnd)
        {
            if (hWnd == IntPtr.Zero || !NativeMethods.IsWindow(hWnd)) return null;

            NativeMethods.RECT rect;
            if (!NativeMethods.GetClientRect(hWnd, out rect)) return null;

            int width = Math.Max(0, rect.Width);
            int height = Math.Max(0, rect.Height);

            return new WindowCandidate
            {
                Handle = hWnd,
                ClassName = GetClassName(hWnd),
                Text = GetWindowText(hWnd),
                Width = width,
                Height = height
            };
        }

        private static string GetWindowText(IntPtr hWnd)
        {
            int length = NativeMethods.GetWindowTextLength(hWnd);
            var buffer = new StringBuilder(Math.Max(256, length + 2));
            NativeMethods.GetWindowText(hWnd, buffer, buffer.Capacity);
            return buffer.ToString();
        }

        private static string GetClassName(IntPtr hWnd)
        {
            var buffer = new StringBuilder(512);
            NativeMethods.GetClassName(hWnd, buffer, buffer.Capacity);
            return buffer.ToString();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using AutoGiaLapLD.Core;
using AutoGiaLapLD.Services;

namespace AutoGiaLapLD
{
    internal static class SelfTests
    {
        public static bool RunAll()
        {
            var results = new List<string>();
            bool ok = true;

            ok &= Run(results, "recognizes dnplayer process", () =>
                AssertTrue(LDProcessClassifier.IsLDPlayerWindow("dnplayer", "Game 1", "Qt5QWindowIcon")));

            ok &= Run(results, "recognizes LDPlayer title", () =>
                AssertTrue(LDProcessClassifier.IsLDPlayerWindow("unknown", "LDPlayer 9", "AnyClass")));

            ok &= Run(results, "rejects unrelated window", () =>
                AssertFalse(LDProcessClassifier.IsLDPlayerWindow("chrome", "ChatGPT", "Chrome_WidgetWin_1")));

            ok &= Run(results, "maps zoomed preview center", () =>
            {
                Point mapped;
                AssertTrue(PreviewCoordinateMapper.TryMap(
                    new Size(1000, 800), new Size(800, 600), new Point(500, 400), out mapped));
                AssertEqual(new Point(400, 300), mapped);
            });

            ok &= Run(results, "rejects preview letterbox click", () =>
            {
                Point mapped;
                AssertFalse(PreviewCoordinateMapper.TryMap(
                    new Size(1000, 800), new Size(800, 600), new Point(500, 10), out mapped));
            });

            ok &= Run(results, "packs XY into LPARAM", () =>
            {
                IntPtr packed = KAutoCompat.MakeLParamFromXY(321, 654);
                long value = packed.ToInt64() & 0xFFFFFFFFL;
                int x = (int)(value & 0xFFFF);
                int y = (int)((value >> 16) & 0xFFFF);
                AssertEqual(321, x);
                AssertEqual(654, y);
            });

            File.WriteAllLines("selftest-results.txt", results.ToArray());
            return ok;
        }

        private static bool Run(List<string> results, string name, Action action)
        {
            try
            {
                action();
                results.Add("PASS: " + name);
                return true;
            }
            catch (Exception ex)
            {
                results.Add("FAIL: " + name + " -> " + ex.Message);
                return false;
            }
        }

        private static void AssertTrue(bool value)
        {
            if (!value) throw new InvalidOperationException("expected true");
        }

        private static void AssertFalse(bool value)
        {
            if (value) throw new InvalidOperationException("expected false");
        }

        private static void AssertEqual(Point expected, Point actual)
        {
            if (expected != actual)
                throw new InvalidOperationException("expected " + expected + " but got " + actual);
        }

        private static void AssertEqual(int expected, int actual)
        {
            if (expected != actual)
                throw new InvalidOperationException("expected " + expected + " but got " + actual);
        }
    }
}

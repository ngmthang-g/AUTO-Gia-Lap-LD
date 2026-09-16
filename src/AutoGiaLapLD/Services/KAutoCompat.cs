using System;
using System.Drawing;
using AutoGiaLapLD.Interop;

namespace AutoGiaLapLD.Services
{
    internal static class KAutoCompat
    {
        public static bool SendClickOnPosition(IntPtr controlHandle, int x, int y, int clickTimes = 1)
        {
            if (controlHandle == IntPtr.Zero || !NativeMethods.IsWindow(controlHandle)) return false;
            if (x < 0 || y < 0 || x > 65535 || y > 65535) return false;
            if (clickTimes < 1) clickTimes = 1;

            IntPtr point = MakeLParamFromXY(x, y);
            bool allPosted = true;

            for (int i = 0; i < clickTimes; i++)
            {
                bool activate = NativeMethods.PostMessage(
                    controlHandle,
                    NativeMethods.WM_ACTIVATE,
                    new IntPtr(NativeMethods.WA_ACTIVE),
                    point);

                bool down = NativeMethods.PostMessage(
                    controlHandle,
                    NativeMethods.WM_LBUTTONDOWN,
                    new IntPtr(NativeMethods.MK_LBUTTON),
                    point);

                bool up = NativeMethods.PostMessage(
                    controlHandle,
                    NativeMethods.WM_LBUTTONUP,
                    IntPtr.Zero,
                    point);

                allPosted &= activate && down && up;
            }

            return allPosted;
        }

        public static Bitmap CaptureWindow(IntPtr handle)
        {
            if (handle == IntPtr.Zero || !NativeMethods.IsWindow(handle)) return null;

            NativeMethods.RECT windowRect;
            if (!NativeMethods.GetWindowRect(handle, out windowRect)) return null;

            int width = windowRect.Width;
            int height = windowRect.Height;
            if (width <= 0 || height <= 0) return null;

            IntPtr hdcSrc = IntPtr.Zero;
            IntPtr hdcDest = IntPtr.Zero;
            IntPtr hBitmap = IntPtr.Zero;
            IntPtr hOld = IntPtr.Zero;

            try
            {
                hdcSrc = NativeMethods.GetWindowDC(handle);
                if (hdcSrc == IntPtr.Zero) return null;

                hdcDest = NativeMethods.CreateCompatibleDC(hdcSrc);
                if (hdcDest == IntPtr.Zero) return null;

                hBitmap = NativeMethods.CreateCompatibleBitmap(hdcSrc, width, height);
                if (hBitmap == IntPtr.Zero) return null;

                hOld = NativeMethods.SelectObject(hdcDest, hBitmap);
                bool copied = NativeMethods.BitBlt(
                    hdcDest, 0, 0, width, height,
                    hdcSrc, 0, 0, NativeMethods.SRCCOPY);

                if (!copied) return null;

                using (Image temp = Image.FromHbitmap(hBitmap))
                {
                    return new Bitmap(temp);
                }
            }
            finally
            {
                if (hOld != IntPtr.Zero && hdcDest != IntPtr.Zero)
                    NativeMethods.SelectObject(hdcDest, hOld);

                if (hBitmap != IntPtr.Zero)
                    NativeMethods.DeleteObject(hBitmap);

                if (hdcDest != IntPtr.Zero)
                    NativeMethods.DeleteDC(hdcDest);

                if (hdcSrc != IntPtr.Zero)
                    NativeMethods.ReleaseDC(handle, hdcSrc);
            }
        }

        internal static IntPtr MakeLParamFromXY(int x, int y)
        {
            unchecked
            {
                int packed = (y << 16) | (x & 0xFFFF);
                return new IntPtr(packed);
            }
        }
    }
}

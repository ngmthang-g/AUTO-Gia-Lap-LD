using System;
using System.Drawing;

namespace AutoGiaLapLD.Core
{
    internal static class PreviewCoordinateMapper
    {
        public static bool TryMap(Size boxSize, Size imageSize, Point boxPoint, out Point imagePoint)
        {
            imagePoint = Point.Empty;

            if (boxSize.Width <= 0 || boxSize.Height <= 0 || imageSize.Width <= 0 || imageSize.Height <= 0)
                return false;

            double scale = Math.Min(
                boxSize.Width / (double)imageSize.Width,
                boxSize.Height / (double)imageSize.Height);

            double displayedWidth = imageSize.Width * scale;
            double displayedHeight = imageSize.Height * scale;
            double offsetX = (boxSize.Width - displayedWidth) / 2.0;
            double offsetY = (boxSize.Height - displayedHeight) / 2.0;

            if (boxPoint.X < offsetX || boxPoint.Y < offsetY ||
                boxPoint.X >= offsetX + displayedWidth || boxPoint.Y >= offsetY + displayedHeight)
                return false;

            int x = (int)((boxPoint.X - offsetX) / scale);
            int y = (int)((boxPoint.Y - offsetY) / scale);

            if (x < 0) x = 0;
            if (y < 0) y = 0;
            if (x >= imageSize.Width) x = imageSize.Width - 1;
            if (y >= imageSize.Height) y = imageSize.Height - 1;

            imagePoint = new Point(x, y);
            return true;
        }
    }
}

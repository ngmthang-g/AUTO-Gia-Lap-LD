using System.Drawing;

namespace AutoGiaLapLD.Core
{
    internal static class PreviewCoordinateMapper
    {
        public static bool TryMap(Size boxSize, Size imageSize, Point boxPoint, out Point imagePoint)
        {
            imagePoint = Point.Empty;
            return false;
        }
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Linq;
using EarthquakeLibrary;

namespace EarthquakeMap.Map
{
    public static class MapCommon
    {
        public const int ImageWidth = 8192;
        public const int ImageHeight = 6805;
        public const int SaveWidth = 773;
        public const int SaveHeight = 435;
        public const double LatMin = 23.45;
        public const double LatMax = 46.56;
        public const double LonMin = 121.93;
        public const double LonMax = 149.75;
        public const int InitialCutWidth = 1440;
        public const int InitialCutHeight = 810;


        public static Bitmap DrawMap(
            IEnumerable<(float lat, float lon, Intensity)> filteredPoints,
            IEnumerable<(float lat, float lon, Intensity intensity)> drawPoints,
            float defaultLat, float defaultLon,
            float? epicenterLat = null, float? epicenterLon = null,
            bool isArea = false
        )
        {
            var stopwatch = Stopwatch.StartNew();
            
            float xMin, xMax, yMin, yMax;
            xMin = xMax = defaultLat;
            yMin = yMax = defaultLon;
            var array = filteredPoints.ToArray();
            if (array.Any())
            {
                (xMin, yMin) = ToPixelCoordinate(array.Max(c => c.lat), array.Min(c => c.lon));
                (xMax, yMax) = ToPixelCoordinate(array.Min(c => c.lat), array.Max(c => c.lon));
            }

            float centerX = (xMin + xMax) / 2, centerY = (yMin + yMax) / 2;
            float iconSize = isArea ? 48 : 32;

            float diffX = xMax - xMin, diffY = yMax - yMin;
            int cutWidth = InitialCutWidth, cutHeight = InitialCutHeight;
            float zoomRate = 1;

            if (diffY + iconSize * 2 > cutHeight)
            {
                zoomRate = diffY / cutHeight;
                cutHeight = (int) Math.Ceiling(diffY + iconSize * zoomRate * 2);
                cutWidth = (int) Math.Ceiling(cutHeight * 16f / 9);
            }

            if (diffX + iconSize * 2 > cutWidth)
            {
                zoomRate = diffX / cutWidth;
                cutWidth = (int) Math.Ceiling(diffX + iconSize * zoomRate * 2);
                cutHeight = (int) Math.Ceiling(cutWidth * 9f / 16);
            }

            float epicenterSize = 80 * zoomRate;
            iconSize = iconSize * zoomRate;

            float offsetX = centerX - cutWidth / 2f;
            if (offsetX >= 0 || offsetX + cutWidth <= ImageWidth)
            {
                if (offsetX < 0) offsetX = 0;
                else if (offsetX + cutWidth > ImageWidth) offsetX -= offsetX + cutWidth - ImageWidth;
            }
            float offsetY = centerY - cutHeight / 2f;
            if (offsetY >= 0 || offsetY + cutHeight <= ImageHeight)
            {
                if (offsetY < 0) offsetY = 0;
                else if (offsetY + cutHeight > ImageHeight) offsetY -= offsetY + cutHeight - ImageHeight;
            }

            // Console.WriteLine($"Calculation {stopwatch.ElapsedMilliseconds}");

            var font = new Font(MainForm.RobotoFont, iconSize * 0.8f, FontStyle.Regular, GraphicsUnit.Pixel);
            var stringFormat = new StringFormat {Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center};

            using var bitmap = new Bitmap(cutWidth, cutHeight);
            using var graphics = Graphics.FromImage(bitmap);
            graphics.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
            graphics.SmoothingMode = SmoothingMode.AntiAlias;
            // Console.WriteLine($"Create Bitmap {stopwatch.ElapsedMilliseconds}");
            graphics.FillRectangle(
                new SolidBrush(Color.FromArgb(32, 32, 32)),
                new Rectangle(0, 0, cutWidth, cutHeight)
            );
            graphics.DrawImage(MainForm.BaseImage, -offsetX, -offsetY, ImageWidth, ImageHeight);
            // Console.WriteLine($"Draw Bitmap {stopwatch.ElapsedMilliseconds}");

            if ((epicenterLat, epicenterLon) is ({ } epiLat, { } epiLon))
            {
                var (epiX, epiY) = ToPixelCoordinate(epiLat, epiLon);
                graphics.DrawImage(
                    Image.FromFile(MainForm.ImagePath + "Epicenter.png"),
                    epiX - offsetX - epicenterSize / 2f,
                    epiY - offsetY - epicenterSize / 2f,
                    epicenterSize,
                    epicenterSize
                );
            }

            foreach (var (lat, lon, intensity) in drawPoints.Where(c => c.intensity >= Intensity.Int1))
            {
                var textColor =
                    intensity.Equals(Intensity.Unknown) || Intensity.Int3 <= intensity && intensity <= Intensity.Int5Minus
                        ? Color.Black
                        : Color.White;

                var (x, y) = ToPixelCoordinate(lat, lon);
                x -= offsetX;
                y -= offsetY;

                if (isArea)
                {
                    graphics.FillRectangle(
                        new SolidBrush(MainForm.Colors[intensity.EnumOrder]),
                        x - iconSize / 2f, y - iconSize / 2f, iconSize, iconSize
                    );
                }
                else
                {
                    graphics.FillEllipse(
                        new SolidBrush(MainForm.Colors[intensity.EnumOrder]),
                        x - iconSize / 2f, y - iconSize / 2f, iconSize, iconSize
                    );
                }
                graphics.DrawString(intensity.ShortString.Replace('-', '‒'), font, new SolidBrush(textColor),
                    new RectangleF(x - iconSize, y - iconSize / 2f + iconSize / 20f, iconSize * 2, iconSize), stringFormat);
            }
            // Console.WriteLine($"Draw icons {stopwatch.ElapsedMilliseconds}");
            var result = new Bitmap(bitmap, SaveWidth, SaveHeight);
            Console.WriteLine($"Complete: {stopwatch.ElapsedMilliseconds} ms");
            return result;
        }


        public static (float, float) ToPixelCoordinate(float lat, float lon)
        {
            var x = (lon - LonMin) * ImageWidth / (LonMax - LonMin);
            var y = ImageHeight - (lat - LatMin) * ImageHeight / (LatMax - LatMin);

            return ((float) x, (float) y);
        }
    }
}
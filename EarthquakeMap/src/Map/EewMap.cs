/*
QuakeMapCS
Copyright (c) 2018 Oruponu
Released under the MIT License.
*/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Linq;
using System.Threading.Tasks;
using EarthquakeLibrary;
using static System.Math;

namespace EarthquakeMap.Map
{
    internal static class EewMap
    {
        private const int ImageWidth = 8192;
        private const int ImageHeight = 6805;
        private const int SaveWidth = 773;
        private const int SaveHeight = 435;
        private const double LatMin = 23.45;
        private const double LatMax = 46.56;
        private const double LonMin = 121.93;
        private const double LonMax = 149.75;
        private const string ImagePath = @"materials\Jishin\";
        public static async Task<Bitmap> Draw(bool filter = true)
        {
            return await Task.Run(() => {
                var sw = Stopwatch.StartNew();
                var eew = InformationsChecker.LatestEew;
                var latlon = ToPixelCoordinate((
                    eew.Coordinate.Latitude,
                    eew.Coordinate.Longitude
                ));
                var (lat, lon) = (latlon[0], latlon[1]);
                var estshindo = eew.EstShindo;
                var pointSorted = estshindo.Where(x => x != null && x.AnalysisResult > 0.4)
                    .OrderBy(x => x.AnalysisResult)
                    .ToDictionary(x => x.Location, x => Intensity.FromValue(x.AnalysisResult ?? 0f));

                var pointPixel = pointSorted.ToDictionary(x => ToPixelCoordinate((x.Key.Latitude, x.Key.Longitude)),
                    x => x.Value);

                int cutWidth = 1440, cutHeight = 810;

                var filtered = filter ? FilterDrawIntensity(pointPixel) : pointPixel;
                var xMin = filtered.Any() ? filtered.Min(x => x.Key[0]) : lat;
                var xMax = filtered.Any() ? filtered.Max(x => x.Key[0]) : lat;
                var yMin = filtered.Any() ? filtered.Min(x => x.Key[1]) : lon;
                var yMax = filtered.Any() ? filtered.Max(x => x.Key[1]) : lon;

                var centerX = (xMin + xMax) / 2;
                var centerY = (yMin + yMax) / 2;

                // 地図を縮小
                const float areaIntSize = 40f;
                var cityIntSize = 32f;
                var zoomRate = 1f;
                var diffWidth = xMax - xMin;
                var diffHeight = yMax - yMin;
                if (diffHeight + areaIntSize * 2 > cutHeight) {
                    zoomRate = diffHeight / cutHeight;
                    cutHeight = (int)Ceiling(diffHeight + areaIntSize * zoomRate * 2);
                    cutWidth = cutHeight * 16 / 9;
                }
                if (diffWidth + areaIntSize * 2 > cutWidth) {
                    zoomRate = diffWidth / cutWidth;
                    cutWidth = (int)Ceiling(diffWidth + areaIntSize * zoomRate * 2);
                    cutHeight = cutWidth * 9 / 16;
                }
                var epiSize = 80f / 2;
                epiSize = (int)Ceiling(epiSize * zoomRate);
                cityIntSize = (int)Ceiling(cityIntSize * zoomRate);

                var orgX = (int)Ceiling(centerX) - cutWidth / 2;
                var adjustX = 0;
                if (orgX >= 0 || orgX + cutWidth <= ImageWidth) {
                    if (orgX < 0) {
                        adjustX = 0 - orgX;
                        orgX = 0;
                    } else if (orgX + cutWidth > ImageWidth) {
                        adjustX = ImageWidth - (orgX + cutWidth);
                        orgX -= orgX + cutWidth - ImageWidth;
                    }
                }
                var orgY = (int)Ceiling(centerY) - cutHeight / 2;
                var adjustY = 0;
                if (orgY >= 0 || orgY + cutHeight <= ImageHeight) {
                    if (orgY < 0) {
                        adjustY = 0 - orgY;
                        orgY = 0;
                    } else if (orgY + cutHeight > ImageHeight) {
                        adjustY = ImageHeight - (orgY + cutHeight);
                        orgY -= orgY + cutHeight - ImageHeight;
                    }
                }
                var font = new Font(MainForm.RobotoFont, cityIntSize * 0.8f, FontStyle.Regular, GraphicsUnit.Pixel);
                var sf = new StringFormat
                    { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };

                Console.WriteLine($"Calclation completed ({sw.ElapsedMilliseconds} ms)");
                var orgCityBitmap = new Bitmap(ImageWidth, ImageHeight);
                var orgCityGraphics = Graphics.FromImage(orgCityBitmap);
                orgCityGraphics.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
                orgCityGraphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (var image = Image.FromFile(ImagePath + "Base.png"))
                    orgCityGraphics.DrawImage(image, adjustX, adjustY, ImageWidth, ImageHeight);
                Console.WriteLine($"Drawed base image ({sw.ElapsedMilliseconds} ms)");

                orgCityGraphics.DrawImage(Image.FromFile(ImagePath + "Epicenter.png"),
                    lat - epiSize / 2 + adjustX, lon - epiSize / 2 + adjustY, epiSize, epiSize);
                foreach (var pixel in pointPixel.Where(kv => kv.Value >= Intensity.Int1))
                {
                    var intensity = pixel.Value;
                    var textColor = intensity.Equals(Intensity.Int3) ||
                                    intensity.Equals(Intensity.Int4) ||
                                    intensity.Equals(Intensity.Int5Minus) ||
                                    intensity.Equals(Intensity.Unknown)
                        ? Color.Black
                        : Color.White;

                    orgCityGraphics.FillEllipse(
                        new SolidBrush(MainForm.Colors[intensity.EnumOrder]),
                        pixel.Key[0] - cityIntSize / 2f + adjustX, 
                        pixel.Key[1] - cityIntSize / 2f + adjustY, 
                        cityIntSize,
                        cityIntSize
                    );
                    orgCityGraphics.DrawString(
                        intensity.ShortString.Replace('-', '‒'),
                        font,
                        new SolidBrush(textColor),
                        new RectangleF(pixel.Key[0] - cityIntSize + adjustX,
                            pixel.Key[1] - cityIntSize / 2f + adjustY + cityIntSize / 20f, cityIntSize * 2, cityIntSize),
                        sf
                    );
                }
                orgCityGraphics.Dispose();
                Console.WriteLine($"Drawed intensity icons ({sw.ElapsedMilliseconds} ms)");

                // 切り取り
                var cutCityBitmap = new Bitmap(cutWidth, cutHeight);
                var cutCityGraphics = Graphics.FromImage(cutCityBitmap);
                cutCityGraphics.FillRectangle(new SolidBrush(Color.FromArgb(32, 32, 32)), new Rectangle(0, 0, cutWidth, cutHeight));
                cutCityGraphics.DrawImage(orgCityBitmap, new Rectangle(0, 0, cutWidth, cutHeight), new Rectangle(orgX, orgY, cutWidth, cutHeight), GraphicsUnit.Pixel);
                orgCityBitmap.Dispose();
                cutCityGraphics.Dispose();
                Console.WriteLine($"Cut ({sw.ElapsedMilliseconds} ms)");

                // 保存
                var saveCityBitmap = new Bitmap(cutCityBitmap, SaveWidth, SaveHeight);

                Console.WriteLine($"EEW Drawing Complete: {sw.ElapsedMilliseconds} ms");
                return saveCityBitmap;
            });
        }


        private static Dictionary<float[], Intensity> FilterDrawIntensity(Dictionary<float[], Intensity> intList)
        {
            var dictionary = new Dictionary<float[], Intensity>();
            switch (InformationsChecker.LatestEew.MaxIntensity.ShortString)
            {
                case "1":
                case "2":
                case "3":
                case "4":
                    dictionary = intList;
                    break;
                case "5-":
                case "5+":
                    dictionary = intList.Where(x => x.Value >= Intensity.Int2).ToDictionary(x => x.Key, x => x.Value);
                    break;
                case "6-":
                case "6+":
                case "7":
                    dictionary = intList.Where(x => x.Value >= Intensity.Int3).ToDictionary(x => x.Key, x => x.Value);
                    break;
            }
            return dictionary;
        }

        private static Dictionary<float[], string> FilterDrawIntensity(Dictionary<float[], string> intList)
        {
            var dictionary = new Dictionary<float[], string>();
            switch (InformationsChecker.LatestEew.MaxIntensity.ShortString) {
                case "1":
                case "2":
                case "3":
                case "4":
                    dictionary = intList;
                    break;
                case "5-":
                case "5+":
                    dictionary = intList.Where(x => x.Value == "2" || x.Value == "3" || x.Value == "4" || x.Value == "5弱" || x.Value == "5強" || x.Value == "6弱" || x.Value == "6強").ToDictionary(x => x.Key, x => x.Value);
                    break;
                case "6-":
                case "6+":
                case "7":
                    dictionary = intList.Where(x => x.Value == "3" || x.Value == "4" || x.Value == "5弱" || x.Value == "5強" || x.Value == "6弱" || x.Value == "6強").ToDictionary(x => x.Key, x => x.Value);
                    //dictionary = intList.Where(x => x.Value == "7").ToDictionary(x => x.Key, x => x.Value);
                    break;
            }
            return dictionary;
        }

        private static float[] GetPointCoordinate(string pointList, string name)
        {
            var split = pointList.Split('\n');
            return split.Select(x => x.Split(','))
                .Where(x => x[1] == name)
                .Select(x => new[] { float.Parse(x[2]), float.Parse(x[3]) })
                .FirstOrDefault();
        }

        private static float[] ToPixelCoordinate((float, float) latLon)
        {
            var (lat, lon) = latLon;
            var x = (lon - LonMin) * ImageWidth / (LonMax - LonMin);
            var y = ImageHeight - (lat - LatMin) * ImageHeight / (LatMax - LatMin);

            return new[] { (float)x, (float)y };
        }
    }
}

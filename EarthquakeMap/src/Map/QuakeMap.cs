/*
QuakeMapCS
Copyright (c) 2018 Oruponu
Released under the MIT License.
*/
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EarthquakeLibrary;
using EarthquakeMap.Properties;
using EarthquakeLibrary.Core;
using EarthquakeLibrary.Information;
using static System.Math;

namespace EarthquakeMap.Map
{
    static class QuakeMap
    {
        private const int ImageWidth = 8192;
        private const int ImageHeight = 6805;
        private const int SaveWidth = 773;
        private const int SaveHeight = 435;
        private const double LatMin = 23.45;
        private const double LatMax = 46.56;
        private const double LonMin = 121.93;
        private const double LonMax = 149.75;
        private const string ImagePath = @"Images\Jishin\";

        private static readonly string[] IntList = { "7", "6強", "6弱", "5強", "5弱", "震度５弱以上未入電", "4", "3", "2", "1" };

        public static async Task<Bitmap> Draw(bool filter = true, bool cityToArea = false)
        {
            return await Task.Run(() => {
                var info = InformationsChecker.LatestInformation;
                var epicenter = new[] { 0.0f, 0.0f };
                if (info.Location != null)
                    epicenter = ToPixelCoordinate(new[] {
                        info.Location.Latitude,
                        info.Location.Longitude
                    });
                if (!cityToArea &&
                (info.InformationType == InformationType.EarthquakeInfo ||
                info.MaxIntensity == Intensity.Int1 ||
                    info.MaxIntensity == Intensity.Int2)) {
                    var cityInt = info.Shindo
                        .SelectMany(x => x.Place.SelectMany(y => y.Place.Select(z => new {
                            Place = z,
                            Intensity = x.Intensity.LongString.Replace("震度", "")
                        }))).ToDictionary(city => city.Place, city => city.Intensity);
                    var citySorted = new Dictionary<float[], string>();
                    cityInt = cityInt.OrderBy(x => x.Value, new IntensityComparer())
                    .ToDictionary(x => x.Key, x => x.Value);
                    foreach (var keyValue in cityInt) {
                        var point = GetPointCoordinate(Resources.CityPoint, keyValue.Key);
                        if (point != null) {
                            citySorted.Add(point, keyValue.Value);
                        }
                    }

                    // ピクセル座標に変換
                    var cityPixel = citySorted.ToDictionary(x => ToPixelCoordinate(x.Key), x => x.Value);

                    var cutWidth = 1440;
                    var cutHeight = 810;

                    // 中心を設定
                    var filtered = filter ? FilterDrawIntensity(cityPixel) : cityPixel;
                    if (epicenter[0] != 0.0f)   // 震度速報でない
                    {
                        filtered.Add(epicenter, "E");
                    }
                    if (!filtered.Any()) return null;
                    var xMin = filtered.Min(x => x.Key[0]);
                    var xMax = filtered.Max(x => x.Key[0]);
                    var yMin = filtered.Min(x => x.Key[1]);
                    var yMax = filtered.Max(x => x.Key[1]);
                    var centerX = (xMin + xMax) / 2;
                    var centerY = (yMin + yMax) / 2;

                    // 地図を縮小
                    var areaIntSize = 36f;
                    var cityIntSize = 24f;
                    var zoomRate = 1f;
                    var diffWidth = filtered.Max(x => x.Key[0]) - filtered.Min(x => x.Key[0]);
                    var diffHeight = filtered.Max(x => x.Key[1]) - filtered.Min(x => x.Key[1]);
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
                    var epiSize = 80f;
                    epiSize = (int)Ceiling(epiSize * zoomRate);
                    cityIntSize = (int)Ceiling(cityIntSize * zoomRate);

                    // 画像読み込み
                    var imageCityList = IntList.Reverse().ToDictionary(intensity => intensity, intensity => new Bitmap(Image.FromFile(ImagePath + "Station\\" + intensity + ".png")));

                    // 地図の範囲外であった場合、拡張する
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

                    // 描画
                    var orgCityBitmap = new Bitmap(ImageWidth, ImageHeight);
                    var orgCityGraphics = Graphics.FromImage(orgCityBitmap);
                    using (var image = Image.FromFile(ImagePath + "Base.png"))
                        orgCityGraphics.DrawImage(image,
                            0 + adjustX, 0 + adjustY, ImageWidth, ImageHeight);
                    //震源描画
                    orgCityGraphics.DrawImage(Image.FromFile(ImagePath + "Epicenter.png"),
                        epicenter[0] - epiSize / 2 + adjustX, epicenter[1] - epiSize / 2 + adjustY, epiSize, epiSize);
                    //震度描画
                    foreach (var pixel in cityPixel) {
                        if (imageCityList.ContainsKey(pixel.Value)) {
                            orgCityGraphics.DrawImage(imageCityList[pixel.Value],
                                pixel.Key[0] - cityIntSize / 2 + adjustX, pixel.Key[1] - cityIntSize / 2 + adjustY, cityIntSize, cityIntSize);
                        }
                    }
                    orgCityGraphics.Dispose();

                    // 切り取り
                    var cutCityBitmap = new Bitmap(cutWidth, cutHeight);
                    var cutCityGraphics = Graphics.FromImage(cutCityBitmap);
                    cutCityGraphics.FillRectangle(new SolidBrush(Color.FromArgb(32, 32, 32)), new Rectangle(0, 0, cutWidth, cutHeight));
                    cutCityGraphics.DrawImage(orgCityBitmap, new Rectangle(0, 0, cutWidth, cutHeight), new Rectangle(orgX, orgY, cutWidth, cutHeight), GraphicsUnit.Pixel);
                    orgCityBitmap.Dispose();
                    cutCityGraphics.Dispose();

                    // 保存
                    var saveCityBitmap = new Bitmap(cutCityBitmap, SaveWidth, SaveHeight);
                    var saveCityGraphics = Graphics.FromImage(saveCityBitmap);
                    saveCityGraphics.Dispose();
                    return saveCityBitmap;
                } else {
                    var areaInt_ = info.Shindo
                        .SelectMany(x => x.Place.SelectMany(y => y.Place.Select(z => new {
                            Place = info.InformationType == InformationType.EarthquakeInfo ? Form1.CityToArea[z] : z,
                            Intensity = x.Intensity.LongString.Replace("震度", "")
                        })));
                    var areaInt = new Dictionary<string, string>();
                    foreach (var ai in areaInt_.Where(x => !areaInt.ContainsKey(x.Place)))
                        areaInt.Add(ai.Place, ai.Intensity);
                    
                    var areaStored = new Dictionary<float[], string>();
                    areaInt = areaInt.OrderBy(x => x.Value, new IntensityComparer())
                    .ToDictionary(x => x.Key, x => x.Value);
                    foreach (var keyValue in areaInt) {
                        var point = GetPointCoordinate(Resources.AreaPoint, keyValue.Key);
                        if (point != null) {
                            areaStored.Add(point, keyValue.Value);
                        }
                    }

                    // ピクセル座標に変換
                    var areaPixel = areaStored.ToDictionary(x => ToPixelCoordinate(x.Key), x => x.Value);

                    var cutWidth = 1440;
                    var cutHeight = 810;

                    // 中心を設定
                    var filtered = filter ? FilterDrawIntensity(areaPixel) : areaPixel;
                    if (epicenter[0] != 0.0f)   // 震度速報でない
                    {
                        filtered.Add(epicenter, "E");
                    }
                    if (!filtered.Any()) return null;
                    var xMin = filtered.Min(x => x.Key[0]);
                    var xMax = filtered.Max(x => x.Key[0]);
                    var yMin = filtered.Min(x => x.Key[1]);
                    var yMax = filtered.Max(x => x.Key[1]);
                    var centerX = (xMin + xMax) / 2;
                    var centerY = (yMin + yMax) / 2;
                    //var maxint = info.MaxIntensity.LongString.Replace("震度", "");
                    //var centerX = filtered.Where(x => x.Value == maxint).Select(x => x.Key[0]).Average();
                    //var centerY = filtered.Where(x => x.Value == maxint).Select(x => x.Key[1]).Average();


                    // 地図を縮小
                    var areaIntSize = 48f;
                    //var cityIntSize = 24f;
                    var zoomRate = 1f;
                    var diffWidth = filtered.Max(x => x.Key[0]) - filtered.Min(x => x.Key[0]);
                    var diffHeight = filtered.Max(x => x.Key[1]) - filtered.Min(x => x.Key[1]);
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
                    var epiSize = 80f;
                    epiSize = (int)Ceiling(epiSize * zoomRate);
                    areaIntSize = (int)Ceiling(areaIntSize * zoomRate);

                    // 画像読み込み
                    var imageAreaList = IntList.Reverse().ToDictionary(intensity => intensity, intensity => new Bitmap(Image.FromFile(ImagePath + "Area\\" + intensity + ".png")));

                    // 地図の範囲外であった場合、拡張する
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

                    // 描画
                    var orgAreaBitmap = new Bitmap(ImageWidth, ImageHeight);
                    var orgAreaGraphics = Graphics.FromImage(orgAreaBitmap);
                    using (var image = Image.FromFile(ImagePath + "Base.png"))
                        orgAreaGraphics.DrawImage(image,
                                0 + adjustX, 0 + adjustY, ImageWidth, ImageHeight);

                    //震源描画
                    if (info.InformationType != InformationType.SesimicInfo)
                        orgAreaGraphics.DrawImage(Image.FromFile(ImagePath + "Epicenter.png"),
                            epicenter[0] - epiSize / 2 + adjustX, epicenter[1] - epiSize / 2 + adjustY, epiSize, epiSize);
                    //震度描画
                    foreach (var pixel in areaPixel) {
                        if (imageAreaList.ContainsKey(pixel.Value)) {
                            orgAreaGraphics.DrawImage(imageAreaList[pixel.Value],
                                pixel.Key[0] - areaIntSize / 2 + adjustX, pixel.Key[1] - areaIntSize / 2 + adjustY, areaIntSize, areaIntSize);
                        }
                    }

                    orgAreaGraphics.Dispose();

                    // 切り取り
                    var cutAreaBitmap = new Bitmap(cutWidth, cutHeight);
                    var cutAreaGraphics = Graphics.FromImage(cutAreaBitmap);
                    cutAreaGraphics.FillRectangle(new SolidBrush(Color.FromArgb(32, 32, 32)), new Rectangle(0, 0, cutWidth, cutHeight));
                    cutAreaGraphics.DrawImage(orgAreaBitmap, new Rectangle(0, 0, cutWidth, cutHeight), new Rectangle(orgX, orgY, cutWidth, cutHeight), GraphicsUnit.Pixel);
                    orgAreaBitmap.Dispose();
                    cutAreaGraphics.Dispose();

                    // 保存
                    var saveAreaBitmap = new Bitmap(cutAreaBitmap, SaveWidth, SaveHeight);
                    return saveAreaBitmap;
                }
            });
        }

        private static Dictionary<float[], string> FilterDrawIntensity(Dictionary<float[], string> intList)
        {
            var dictionary = new Dictionary<float[], string>();
            switch (InformationsChecker.LatestInformation.MaxIntensity.ShortString) {
                case "1":
                case "2":
                case "3":
                case "4":
                    dictionary = intList;
                    break;
                case "5-":
                case "5+":
                    dictionary = intList.Where(x => x.Value == "2" || x.Value == "3" || x.Value == "4" || x.Value == "5弱" || x.Value == "5強").ToDictionary(x => x.Key, x => x.Value);
                    break;
                case "6-":
                case "6+":
                    dictionary = intList.Where(x => x.Value == "3" || x.Value == "4" || x.Value == "5弱" || x.Value == "5強" || x.Value == "6弱" || x.Value == "6強").ToDictionary(x => x.Key, x => x.Value);
                    break;
                case "7":
                    dictionary = intList.Where(x => x.Value == "4" || x.Value == "5弱" || x.Value == "5強" || x.Value == "6弱" || x.Value == "6強" || x.Value == "7").ToDictionary(x => x.Key, x => x.Value);
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

        private static float[] ToPixelCoordinate(float[] latLon)
        {
            var x = (latLon[1] - LonMin) * ImageWidth / (LonMax - LonMin);
            var y = ImageHeight - (latLon[0] - LatMin) * ImageHeight / (LatMax - LatMin);

            return new[] { (float)x, (float)y };
        }
    }
}

using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using AllInformationViewer2.Properties;
using EarthquakeLibrary.Core;
using EarthquakeLibrary.Information;
using static System.Math;

namespace AllInformationViewer2.Map
{
    class QuakeMap
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

        public static async Task<Bitmap> Draw()
        {
            return await Task.Run(() => {
                var info = InformationsChecker.LatestInformation;
                var epicenter = new[] { 0.0f, 0.0f };
                if (info.Location != null)
                    epicenter = ToPixelCoordinate(new[] {
                        info.Location.Latitude,
                        info.Location.Longitude
                    });
                //var areaInt = new Dictionary<int, string>();
                if (info.InformationType == InformationType.EarthquakeInfo) {
                    var cityInt = info.Shindo
                        .SelectMany(x => x.Place.SelectMany(y => y.Place.Select(z => new {
                            Place = z,
                            Intensity = x.Intensity.ToLongString().Replace("震度", "")
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
                    //var areaPixel = new Dictionary<float[], string>();
                    var cityPixel = citySorted.ToDictionary(x => ToPixelCoordinate(x.Key), x => x.Value);

                    var cutWidth = 1440;
                    var cutHeight = 810;

                    // 中心を設定
                    var filtered = FilterDrawIntensity(cityPixel);
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
                    //areaIntSize = (int)Ceiling(areaIntSize * zoomRate);
                    cityIntSize = (int)Ceiling(cityIntSize * zoomRate);

                    // 画像読み込み
                    //var imageAreaList = new Dictionary<string, Bitmap>();
                    var imageCityList = new Dictionary<string, Bitmap>();
                    foreach (var intensity in IntList.Reverse()) {
                        //imageAreaList.Add(intensity, new Bitmap(Image.FromFile(ImagePath + "Area\\" + intensity + ".png")));
                        imageCityList.Add(intensity, new Bitmap(Image.FromFile(ImagePath + "Station\\" + intensity + ".png")));
                    }

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
                    //var orgAreaBitmap = new Bitmap(ImageWidth, ImageHeight);
                    var orgCityBitmap = new Bitmap(ImageWidth, ImageHeight);
                    //var orgAreaGraphics = Graphics.FromImage(orgAreaBitmap);
                    var orgCityGraphics = Graphics.FromImage(orgCityBitmap);
                    //orgAreaGraphics.DrawImage(Image.FromFile(ImagePath + "Base.png"),
                    //    0 + adjustX, 0 + adjustY, ImageWidth, ImageHeight);
                    using (var image = Image.FromFile(ImagePath + "Base.png"))
                        orgCityGraphics.DrawImage(image,
                            0 + adjustX, 0 + adjustY, ImageWidth, ImageHeight);
                    //orgAreaGraphics.DrawImage(Image.FromFile(ImagePath + "Epicenter.png"),
                    //    epicenter[0] - epiSize / 2 + adjustX, epicenter[1] - epiSize / 2 + adjustY, epiSize, epiSize);
                    orgCityGraphics.DrawImage(Image.FromFile(ImagePath + "Epicenter.png"),
                        epicenter[0] - epiSize / 2 + adjustX, epicenter[1] - epiSize / 2 + adjustY, epiSize, epiSize);
                    foreach (var pixel in cityPixel) {
                        if (imageCityList.ContainsKey(pixel.Value)) {
                            orgCityGraphics.DrawImage(imageCityList[pixel.Value],
                                pixel.Key[0] - cityIntSize / 2 + adjustX, pixel.Key[1] - cityIntSize / 2 + adjustY, cityIntSize, cityIntSize);
                        }
                    }
                    //orgAreaGraphics.Dispose();
                    orgCityGraphics.Dispose();

                    // 切り取り
                    //var cutAreaBitmap = new Bitmap(cutWidth, cutHeight);
                    var cutCityBitmap = new Bitmap(cutWidth, cutHeight);
                    //var cutAreaGraphics = Graphics.FromImage(cutAreaBitmap);
                    var cutCityGraphics = Graphics.FromImage(cutCityBitmap);
                    //cutAreaGraphics.FillRectangle(new SolidBrush(Color.FromArgb(32, 32, 32)), new Rectangle(0, 0, cutWidth, cutHeight));
                    cutCityGraphics.FillRectangle(new SolidBrush(Color.FromArgb(32, 32, 32)), new Rectangle(0, 0, cutWidth, cutHeight));
                    //cutAreaGraphics.DrawImage(orgAreaBitmap, new Rectangle(0, 0, cutWidth, cutHeight), new Rectangle(orgX, orgY, cutWidth, cutHeight), GraphicsUnit.Pixel);
                    cutCityGraphics.DrawImage(orgCityBitmap, new Rectangle(0, 0, cutWidth, cutHeight), new Rectangle(orgX, orgY, cutWidth, cutHeight), GraphicsUnit.Pixel);
                    //orgAreaBitmap.Dispose();
                    orgCityBitmap.Dispose();
                    //cutAreaGraphics.Dispose();
                    cutCityGraphics.Dispose();

                    // 保存
                    //var saveAreaBitmap = new Bitmap(cutAreaBitmap, SaveWidth, SaveHeight);
                    var saveCityBitmap = new Bitmap(cutCityBitmap, SaveWidth, SaveHeight);
                    //var saveAreaGraphics = Graphics.FromImage(saveAreaBitmap);
                    var saveCityGraphics = Graphics.FromImage(saveCityBitmap);
                    //saveAreaGraphics.Dispose();
                    saveCityGraphics.Dispose();
                    return saveCityBitmap;
                } 
                else {
                    var areaInt = info.Shindo
                        .SelectMany(x => x.Place.SelectMany(y => y.Place.Select(z => new {
                            Place = z,
                            Intensity = x.Intensity.ToLongString().Replace("震度", "")
                        }))).ToDictionary(city => city.Place, city => city.Intensity);
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
                    //var cityPixel = citySorted.ToDictionary(x => ToPixelCoordinate(x.Key), x => x.Value);

                    var cutWidth = 1440;
                    var cutHeight = 810;

                    // 中心を設定
                    var filtered = areaStored;
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
                    areaIntSize = (int)Ceiling(areaIntSize * zoomRate);
                    //cityIntSize = (int)Ceiling(cityIntSize * zoomRate);

                    // 画像読み込み
                    var imageAreaList = new Dictionary<string, Bitmap>();
                    //var imageCityList = new Dictionary<string, Bitmap>();
                    foreach (var intensity in IntList.Reverse()) {
                        imageAreaList.Add(intensity, new Bitmap(Image.FromFile(ImagePath + "Station\\" + intensity + ".png")));
                        //imageCityList.Add(intensity, new Bitmap(Image.FromFile(ImagePath + "Station\\" + intensity + ".png")));
                    }

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
                    //var orgCityBitmap = new Bitmap(ImageWidth, ImageHeight);
                    var orgAreaGraphics = Graphics.FromImage(orgAreaBitmap);
                    //var orgCityGraphics = Graphics.FromImage(orgCityBitmap);
                    orgAreaGraphics.DrawImage(Image.FromFile(ImagePath + "Base.png"),
                        0 + adjustX, 0 + adjustY, ImageWidth, ImageHeight);
                    using (var image = Image.FromFile(ImagePath + "Base.png"))
                        //orgCityGraphics.DrawImage(image,
                        orgAreaGraphics.DrawImage(image,
                            0 + adjustX, 0 + adjustY, ImageWidth, ImageHeight);
                    if (info.InformationType == InformationType.EpicenterInfo)
                        orgAreaGraphics.DrawImage(Image.FromFile(ImagePath + "Epicenter.png"),
                            epicenter[0] - epiSize / 2 + adjustX, epicenter[1] - epiSize / 2 + adjustY, epiSize, epiSize);
                    //orgCityGraphics.DrawImage(Image.FromFile(ImagePath + "Epicenter.png"),
                    //    epicenter[0] - epiSize / 2 + adjustX, epicenter[1] - epiSize / 2 + adjustY, epiSize, epiSize);
                    //foreach (var pixel in cityPixel) {
                    foreach (var pixel in areaPixel) { 
                        if (imageAreaList.ContainsKey(pixel.Value)) {
                            orgAreaGraphics.DrawImage(imageAreaList[pixel.Value],
                                pixel.Key[0] - cityIntSize / 2 + adjustX, pixel.Key[1] - cityIntSize / 2 + adjustY, cityIntSize, cityIntSize);
                        }
                    }
                    orgAreaGraphics.Dispose();
                    //orgCityGraphics.Dispose();

                    // 切り取り
                    var cutAreaBitmap = new Bitmap(cutWidth, cutHeight);
                    //var cutCityBitmap = new Bitmap(cutWidth, cutHeight);
                    var cutAreaGraphics = Graphics.FromImage(cutAreaBitmap);
                    //var cutCityGraphics = Graphics.FromImage(cutCityBitmap);
                    cutAreaGraphics.FillRectangle(new SolidBrush(Color.FromArgb(32, 32, 32)), new Rectangle(0, 0, cutWidth, cutHeight));
                    //cutCityGraphics.FillRectangle(new SolidBrush(Color.FromArgb(32, 32, 32)), new Rectangle(0, 0, cutWidth, cutHeight));
                    cutAreaGraphics.DrawImage(orgAreaBitmap, new Rectangle(0, 0, cutWidth, cutHeight), new Rectangle(orgX, orgY, cutWidth, cutHeight), GraphicsUnit.Pixel);
                    //cutCityGraphics.DrawImage(orgCityBitmap, new Rectangle(0, 0, cutWidth, cutHeight), new Rectangle(orgX, orgY, cutWidth, cutHeight), GraphicsUnit.Pixel);
                    orgAreaBitmap.Dispose();
                    //orgCityBitmap.Dispose();
                    cutAreaGraphics.Dispose();
                    //cutCityGraphics.Dispose();

                    // 保存
                    var saveAreaBitmap = new Bitmap(cutAreaBitmap, SaveWidth, SaveHeight);
                    //var saveCityBitmap = new Bitmap(cutCityBitmap, SaveWidth, SaveHeight);
                    //var saveAreaGraphics = Graphics.FromImage(saveAreaBitmap);
                    //var saveCityGraphics = Graphics.FromImage(saveCityBitmap);
                    //saveAreaGraphics.Dispose();
                    //saveCityGraphics.Dispose();
                    return saveAreaBitmap;
                }
            });
        }

        private static Dictionary<float[], string> FilterDrawIntensity(Dictionary<float[], string> intList)
        {
            var dictionary = new Dictionary<float[], string>();
            switch (InformationsChecker.LatestInformation.MaxIntensity.ToShortString()) {
                case "1":
                    dictionary = intList.Where(x => x.Value == "1").ToDictionary(x => x.Key, x => x.Value);
                    break;
                case "2":
                    dictionary = intList.Where(x => x.Value == "1" || x.Value == "2").ToDictionary(x => x.Key, x => x.Value);
                    break;
                case "3":
                    dictionary = intList.Where(x => x.Value == "1" || x.Value == "2" || x.Value == "3").ToDictionary(x => x.Key, x => x.Value);
                    break;
                case "4":
                    dictionary = intList.Where(x => x.Value == "1" || x.Value == "2" || x.Value == "3" || x.Value == "4").ToDictionary(x => x.Key, x => x.Value);
                    break;
                case "5-":
                    dictionary = intList.Where(x => x.Value == "3" || x.Value == "4" || x.Value == "5弱").ToDictionary(x => x.Key, x => x.Value);
                    break;
                case "5+":
                    dictionary = intList.Where(x => x.Value == "3" || x.Value == "4" || x.Value == "5弱" || x.Value == "5強").ToDictionary(x => x.Key, x => x.Value);
                    break;
                case "6-":
                    dictionary = intList.Where(x => x.Value == "4" || x.Value == "5弱" || x.Value == "5強" || x.Value == "6弱").ToDictionary(x => x.Key, x => x.Value);
                    break;
                case "6+":
                    dictionary = intList.Where(x => x.Value == "4" || x.Value == "5弱" || x.Value == "5強" || x.Value == "6弱" || x.Value == "6強").ToDictionary(x => x.Key, x => x.Value);
                    break;
                case "7":
                    dictionary = intList.Where(x => x.Value == "5弱" || x.Value == "5強" || x.Value == "6弱" || x.Value == "6強" || x.Value == "7").ToDictionary(x => x.Key, x => x.Value);
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

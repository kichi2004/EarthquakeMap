using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Globalization;
using System.Threading.Tasks;
using System.Windows.Forms;
using EarthquakeMap.Objects;
using Codeplex.Data;
using EarthquakeLibrary;
using EarthquakeLibrary.Information;
using KyoshinMonitorLib;
using KyoshinMonitorLib.Images;
using static EarthquakeMap.Utilities;
using ColorConverter = KyoshinMonitorLib.Images.ColorConverter;

namespace EarthquakeMap
{
    internal static class InformationsChecker
    {
        internal static NewEarthquakeInformation LatestInformation { get; private set; }
        internal static Eew LatestEew { get; private set; }

        private static int _lastnum;
        private static bool _isLastUnknown;
        private static int _lastId;

        private static unsafe ImageAnalysisResult[] ParseScale(Bitmap bitmap) {
            var points = MainForm.ObservationPoints.Select(s => new ImageAnalysisResult(s)).ToArray();
            var data = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height),ImageLockMode.ReadOnly,PixelFormat.Format8bppIndexed);
            var pixelData = new Span<byte>(data.Scan0.ToPointer(), bitmap.Height * bitmap.Width);
            foreach (var point in points)
            {
                if (point.ObservationPoint.Point == null || point.ObservationPoint.IsSuspended)
                {
                    point.AnalysisResult = null;
                    continue;
                }

                try
                {
                    var color = bitmap.Palette.Entries[pixelData[bitmap.Width * point.ObservationPoint.Point.Value.Y + point.ObservationPoint.Point.Value.X]];
                    point.Color = color;
                    if (color.A != 255)
                    {
                        point.AnalysisResult = null;
                        continue;
                    }

                    point.AnalysisResult = ColorConverter.ConvertToScaleAtPolynomialInterpolation(color);
                }
                catch
                {
                    point.AnalysisResult = null;
                }
            }

            bitmap.UnlockBits(data);
            return points.ToArray();
        }
        
        //private static bool a = true;
        internal static async Task<(bool eew, bool info)> Get(DateTime time, bool forceInfo = false, string url = null) {
            //新強震取得
            var eewJson = await DownloadStringAsync(UrlGenerator.GenerateEewJson(time));
            var eewobj = DynamicJson.Parse(eewJson);
            var infoflag = false;
            //地震情報取得
            NewEarthquakeInformation info = !forceInfo && time.Second % 20 != 0
                ? null
                : await Information.GetNewEarthquakeInformationFromYahooAsync(
                    url ?? Information.YahooUrl
                );

            //a = false;
            //変化あるか確認
            if (info != null) {
                if (info.InformationType == InformationType.Other ||
                    info.InformationType == InformationType.UnknownSesimic)
                {
                    if (!_isLastUnknown && LatestInformation == null)
                    {
                        var flag = info.Oldinfo?.Any() != true;
                        if (flag)
                        {
                            info = await Information.GetNewEarthquakeInformationFromYahooAsync();
                        }

                        MessageBox.Show(flag
                                ? @"指定された地震情報URLは震度不明地震（海外地震等）のため、最新の地震情報を表示します。"
                                : @"最新の地震情報が震度不明地震（海外地震等）のため、1つ前の地震情報を表示します。",
                            @"EqMap", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);

                        if (!flag ||
                            info.InformationType == InformationType.Other ||
                            info.InformationType == InformationType.UnknownSesimic)
                        {
                            info = await Information.GetNewEarthquakeInformationFromYahooAsync(
                                info.Oldinfo.Skip(1).First(x => x.MaxIntensity != Intensity.Unknown)
                                    .Info_url);
                        }
                        infoflag = true;
                        LatestInformation = info;
                        _isLastUnknown = true;
                    }
                }

                else if (
                    LatestInformation == null ||
                    info.OriginTime != LatestInformation.OriginTime ||
                    //info.Announced_time != LatestInformation.Announced_time ||
                    info.Epicenter != LatestInformation.Epicenter ||
                    info.Magnitude != LatestInformation.Magnitude ||
                    info.Depth != LatestInformation.Depth ||
                    info.InformationType != LatestInformation.InformationType ||
                    !info.Shindo.SequenceEqual(LatestInformation.Shindo)) {

                    infoflag = true;
                    LatestInformation = info;
                }
            }

            if (infoflag) return (false, true);
            var num = 0;
            int id = -1;
            if (eewobj.result.message == "データがありません" ||
                (num = int.Parse(eewobj.report_num)) <= _lastnum && (id = int.Parse(eewobj.report_id)) == _lastId) {
                _lastnum = num;
                _lastId = id;
                return (false, false);
            }
            _lastnum = num;
            _lastId = id;

            var eewImageUrl = UrlGenerator.GenerateEewImage(time);
            var eewImage = await DownloadImageAsync(eewImageUrl);
            var eew = new Eew
            {
                IsWarn = eewobj.alertflg == "警報",
                MaxIntensity = Intensity.Parse(eewobj.calcintensity),
                Depth = int.Parse(eewobj.depth.Replace("km", "")),
                IsLast = eewobj.is_final,
                Coordinate = new Coordinate(float.Parse(eewobj.latitude),
                    float.Parse(eewobj.longitude)),
                Magnitude = float.Parse(eewobj.magunitude),
                OccurrenceTime = DateTime.ParseExact(eewobj.origin_time,
                    "yyyyMMddHHmmss", CultureInfo.CurrentCulture, DateTimeStyles.None),
                Epicenter = eewobj.region_name,
                AnnouncedTime = DateTime.Parse(eewobj.report_time),
                Number = num,
                EstShindo = ParseScale(eewImage)
            };
            LatestEew = eew;
            return (true, false);
        }
    }
}

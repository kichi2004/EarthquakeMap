using System;
using System.Linq;
using System.Globalization;
using System.Threading.Tasks;
using System.Windows.Forms;
using EarthquakeMap.Objects;
using Codeplex.Data;
using EarthquakeLibrary;
using EarthquakeLibrary.Information;
using KyoshinMonitorLib;
using static EarthquakeMap.Utilities;

namespace EarthquakeMap
{
    internal static class InformationsChecker
    {
        internal static NewEarthquakeInformation LatestInformation { get; private set; }
        internal static Eew LatestEew { get; private set; }

        private static int _lastnum;
        private static bool _isLastUnknown;
        private static string _lastId;
        //private static bool a = true;
        internal static async Task<(bool eew, bool info)> Get(DateTime time, bool forceInfo = false, string url = null) {
            //新強震取得
            var eewJson = await DownloadStringAsync(UrlGenerator.GenerateEewJson(time));
            var eewobj = DynamicJson.Parse(eewJson);
            var infoflag = false;
            //地震情報取得
            var info = !forceInfo && time.Second % 20 != 0
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
            string id = null;
            if (eewobj.result.message == "データがありません" ||
                (num = int.Parse(eewobj.report_num)) == _lastnum && (id = eewobj.report_id) == _lastId) {
                _lastnum = num;
                _lastId = id;
                return (false, false);
            }
            _lastnum = num;
            _lastId = id;

            var task = DownloadImageAsync(UrlGenerator.GenerateEewImage(time));
            var eew = new Eew
            {
                IsWarn = eewobj.alertflg == "警報",
                MaxIntensity = Enums.Intensity.Parse(eewobj.calcintensity),
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
                EstShindo = Form1.ObservationPoints.ParseIntensityFromImage(await task)
            };
            LatestEew = eew;
            return (true, false);
        }
    }
}

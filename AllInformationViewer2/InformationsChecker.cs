using System;
using System.Globalization;
using System.Threading.Tasks;
using AllInformationViewer2.Enums;
using AllInformationViewer2.Objects;
using Codeplex.Data;
using EarthquakeLibrary.Information;

using static AllInformationViewer2.Utilities;

namespace AllInformationViewer2
{
    static class InformationsChecker
    {
        internal static NewEarthquakeInformation LatestInformation { get; private set; }
        internal static Eew LatestEew { get; private set; }

        private static int _lastnum;
        private static string _lastId;
        private static NewEarthquakeInformation _lastinfo;

        internal static async Task<(bool eew, bool info)> Get(DateTime time, bool forceInfo = false)
        {
            //新強震取得
            var eew_json = await DownloadStringAsync(
                $"http://www.kmoni.bosai.go.jp/new/webservice/hypo/eew/" +
                $"{time:yyyyMMddHHmmss}.json");
            var eewobj = DynamicJson.Parse(eew_json);
            bool eewflag = false, infoflag = false;
            //地震情報取得
            var info = !forceInfo && time.Second % 20 != 0 ? null :
                await Information.GetNewEarthquakeInformationFromYahooAsync(
                //"https://typhoon.yahoo.co.jp/weather/jp/earthquake/20160416012510.html"
                );
            //変化あるか確認
            if (info != null) {
                if((_lastinfo == null ||
                info.Epicenter != _lastinfo.Epicenter ||
                info.Depth != _lastinfo.Depth ||
                info.Magnitude != _lastinfo.Magnitude ||
                info.Announced_time != _lastinfo.Announced_time ||
                info.Origin_time != _lastinfo.Origin_time)) {
                    infoflag = true;
                    LatestInformation = info;
                }
                _lastinfo = info;
            }

            if (!infoflag && eewobj.result.message != "データがありません") {
                int num = int.Parse(eewobj.report_num);
                string id = eewobj.report_id;
                if (num != _lastnum || _lastId != id) {
                    _lastnum = num;
                    _lastId = id;
                    var eew = new Eew() {
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
                        Number = num
                    };
                    LatestEew = eew;
                    eewflag = true;
                }
            }
            //差分なし
            return (eewflag, infoflag);
        }
    }
}
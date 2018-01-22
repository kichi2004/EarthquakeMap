using System;
using System.Linq;
using System.Globalization;
using System.Threading.Tasks;
using AllInformationViewer2.Enums;
using AllInformationViewer2.Objects;
using Codeplex.Data;
using EarthquakeLibrary.Information;
using KyoshinMonitorLib;
using static AllInformationViewer2.Utilities;

namespace AllInformationViewer2
{
    static class InformationsChecker
    {
        internal static NewEarthquakeInformation LatestInformation { get; private set; }
        internal static Eew LatestEew { get; private set; }

        private static int _lastnum;
        private static string _lastId;
        //private static bool a = true;
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
                //a ? "https://typhoon.yahoo.co.jp/weather/jp/earthquake/20180122012716.html" 
                //: "https://typhoon.yahoo.co.jp/weather/jp/earthquake/20180122012434.html"
                );
            //a = false;
            //変化あるか確認
            if (info != null) {
                if((LatestInformation == null ||
                info.Origin_time != LatestInformation.Origin_time ||
                info.Announced_time != LatestInformation.Announced_time ||
                info.Epicenter != LatestInformation.Epicenter ||
                info.Magnitude != LatestInformation.Magnitude ||
                info.Depth != LatestInformation.Depth ||
                info.InformationType != LatestInformation.InformationType || 
                !info.Shindo.SequenceEqual(LatestInformation.Shindo))) {
                    infoflag = true;
                    LatestInformation = info;
                }
            }

            if (!infoflag && eewobj.result.message != "データがありません") {
                int num = int.Parse(eewobj.report_num);
                string id = eewobj.report_id;
                
                if (num != _lastnum || _lastId != id) {
                    _lastnum = num;
                    _lastId = id;
                    var task = DownloadImageAsync($"http://www.kmoni.bosai.go.jp/new/data/" +
                    $"map_img/EstShindoImg/eew/{time:yyyyMMdd}/{time:yyyyMMddHHmmss}.eew.gif");
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
                        Number = num,
                        EstShindo = Form1.observationPoints.ParseIntensityFromImage(await task)
                    };
                    eewflag = true;
                    LatestEew = eew;
                }
            }
            //差分なし
            return (eewflag, infoflag);
        }
    }
}
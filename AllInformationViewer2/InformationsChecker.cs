using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AllInformationViewer2.Enums;
using AllInformationViewer2.Objects;
using Codeplex.Data;
using EarthquakeLibrary.Information;

using static AllInformationViewer2.Utilities;

namespace AllInformationViewer2
{
    class InformationsChecker
    {
        internal EventHandler EewRecieved = delegate { };
        internal EventHandler InfoReceived = delegate { };

        internal NewEarthquakeInformation LatestInformation { get; private set; }
        internal Eew LatestEew { get; private set; }

        private int _lastnum;
        private string _lastId;
        private NewEarthquakeInformation _lastinfo;

        internal async Task<(bool eew, bool info)> Get(DateTime time)
        {
            //新強震取得
            var eew_json = await DownloadStringAsync(
                $"http://www.kmoni.bosai.go.jp/new/webservice/hypo/eew/" +
                $"{time:yyyyMMddHHmmss}.json");
            var eewobj = DynamicJson.Parse(eew_json);
            bool eewflag = false, infoflag = false;
            int num = 0;
            string id = null;
            if (eewobj.result.message != "データがありません" &&
                (_lastnum != (num = int.Parse(eewobj.report_num)) ||
                _lastId != (id = eewobj.report_id))) {
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
                _lastnum = num;
                _lastId = id;
                LatestEew = eew;
                eewflag = true;
            }
            //差分なし
            //地震情報取得
            var info = await Information.GetNewEarthquakeInformationFromYahooAsync();
            //変化あるか確認
            if (info != null &&
                _lastinfo == null || 
                info.Epicenter != _lastinfo.Epicenter ||
                info.Depth != _lastinfo.Depth ||
                info.Magnitude != _lastinfo.Magnitude ||
                info.Announced_time != _lastinfo.Announced_time ||
                info.Origin_time != _lastinfo.Origin_time ||
                info.Shindo != _lastinfo.Shindo) {
                infoflag = true;
                LatestInformation = info;
            }
            _lastinfo = info;
            return (eewflag, infoflag);
        }
    }
}
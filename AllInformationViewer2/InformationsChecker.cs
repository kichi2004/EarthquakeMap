using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        private int _lastId;
        private NewEarthquakeInformation _lastinfo;

        internal async void Get(DateTime time)
        {
            //新強震取得
            var eew_json = await DownloadStringAsync(
                $"http://www.kmoni.bosai.go.jp/new/webservice/hypo/eew/" +
                $"{time:yyyyMMddHHmmss}.json");
            var parsed = DynamicJson.Parse(eew_json);
            bool eewpass_flag = true;
            if (parsed.message != "データがありません") {

            }
            //差分なし
            //地震情報取得
        }
    }
}

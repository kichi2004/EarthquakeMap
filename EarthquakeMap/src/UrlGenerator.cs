using System;

namespace EarthquakeMap
{
    public static class UrlGenerator
    {
        private const string EewJsonUrl = "http://www.kmoni.bosai.go.jp/webservice/hypo/eew/{0}.json";

        public static string GenerateEewJson(DateTime time) =>
            string.Format(EewJsonUrl, time.ToString("yyyyMMddHHmmss"));

        private const string EewImageUrl =
            "http://www.kmoni.bosai.go.jp/data/map_img/EstShindoImg/eew/{0}/{1}.eew.gif";

        public static string GenerateEewImage(DateTime time) =>
            String.Format(EewImageUrl, time.ToString("yyyyMMdd"), time.ToString("yyyyMMddHHmmss"));

    }
}
namespace EarthquakeMap.Objects
{
    class Coordinate
    {
        /// <summary>
        /// 緯度・経度をもとにクラスを初期化します。
        /// </summary>
        /// <param name="lat">緯度</param>
        /// <param name="lon">経度</param>
        internal Coordinate(float lat, float lon)
        {
            Latitude = lat;
            Longitude = lon;
        }
        /// <summary>
        /// 緯度
        /// </summary>
        internal float Latitude { get; set; }

        /// <summary>
        /// 経度
        /// </summary>
        internal float Longitude { get; set; }
    }
}

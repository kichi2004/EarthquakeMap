/*
QuakeMapCS
Copyright (c) 2018 Oruponu
Released under the MIT License.
*/
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using EarthquakeLibrary;
using EarthquakeMap.Properties;
using EarthquakeLibrary.Information;
using static System.Math;
using ShindoInformation=EarthquakeLibrary.Information.EarthquakeInformation.ShindoInformation;

namespace EarthquakeMap.Map
{
    internal static class QuakeMap
    {
        private const int ImageWidth = 8192;
        private const int ImageHeight = 6805;
        private const int SaveWidth = 773;
        private const int SaveHeight = 435;
        private const double LatMin = 23.45;
        private const double LatMax = 46.56;
        private const double LonMin = 121.93;
        private const double LonMax = 149.75;

        private static readonly string[] IntList = {"7", "6強", "6弱", "5強", "5弱", "震度５弱以上未入電", "4", "3", "2", "1"};

        static Dictionary<string, (float, float)> _cityDict, _areaDict;

        static Dictionary<string, (float, float)> CsvToDictionary(string csvSource)
        {
            using var stream = new StringReader(csvSource);
            var res = new Dictionary<string, (float, float)>();
            while (stream.Peek() != -1)
            {
                var line = stream.ReadLine().Split(',');
                res[line[1]] = (float.Parse(line[2]), float.Parse(line[3]));
            }
            return res;
        }

        static Dictionary<string, (float, float)> CityDict => _cityDict ??= CsvToDictionary(Resources.CityPoint);
        static Dictionary<string, (float, float)> AreaDict => _areaDict ??= CsvToDictionary(Resources.AreaPoint);

        private static IEnumerable<(string Place, Intensity Intensity)> CitySelectManyFunc(ShindoInformation information)
        {
            return information.Place.SelectMany(place => place.Place.Select(z => (Place: z, information.Intensity)));
        }

        private static Func<ShindoInformation, IEnumerable<(string Place, Intensity Intensity)>> GetAreaSelectManyFunc(
            bool isEarhquakeInformation)
        {
            return x => x.Place.SelectMany(
                y => y.Place.Select(z => (
                        Place: isEarhquakeInformation
                            ? MainForm.CityToArea.ContainsKey(z) ? MainForm.CityToArea[z] : null
                            : z,
                        x.Intensity
                    )
                ).Where(z => z.Place != null)
            );
        }


        public static async Task<Bitmap> Draw(bool filter = true, bool cityToArea = false)
        {
            return await Task.Run(() =>
            {
                var info = InformationsChecker.LatestInformation;

                var isCityMap = (
                    info.InformationType == InformationType.EarthquakeInfo ||
                    info.MaxIntensity.EnumOrder is 1 or 2
                ) && !cityToArea;

                var func = isCityMap ? CitySelectManyFunc : GetAreaSelectManyFunc(info.InformationType == InformationType.EarthquakeInfo);
                var intList = info.Shindo.SelectMany(func)
                    .Select(a => (c: GetPointCoordinate(a.Place, isCityMap), a.Intensity))
                    .Where(x => x.c != null)
                    .Select(a => (a.c.Value.lat, a.c.Value.lon, a.Intensity))
                    .OrderBy(x => x.Intensity.EnumOrder)
                    .ToArray();

                var filtered = (filter ? FilterDrawIntensity(info.MaxIntensity, intList) : intList).ToList();
                // 震度速報でない
                if (info.Location is {Latitude: var lat, Longitude: var lon})
                    filtered.Add((lat, lon, Intensity.Unknown));

                if (!filtered.Any()) return null;
                return MapCommon.DrawMap(filtered, intList, -1, -1, info.Location?.Latitude, info.Location?.Longitude, !isCityMap);
            });
        }


        private static (float lat, float lon)? GetPointCoordinate(string name, bool isCity)
        {
            var dict = isCity ? CityDict : AreaDict;
            return dict.TryGetValue(name, out var a) ? a : null;
        }

        private static IEnumerable<(float, float, Intensity)> FilterDrawIntensity(
            Intensity intensity,
            IEnumerable<(float x, float y, Intensity intensity)> intList
        ) =>
            intensity.EnumOrder switch
            {
                5 or 6 => intList.Where(x => x.intensity >= Intensity.Int2),
                7 or 8 => intList.Where(x => x.intensity >= Intensity.Int3),
                9 => intList.Where(x => x.intensity >= Intensity.Int4),
                _ => intList
            };
    }
}

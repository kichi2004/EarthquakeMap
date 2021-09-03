/*
QuakeMapCS
Copyright (c) 2018 Oruponu
Released under the MIT License.
*/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Linq;
using System.Threading.Tasks;
using EarthquakeLibrary;
using static System.Math;

namespace EarthquakeMap.Map
{
    internal static class EewMap
    {
        public static async Task<Bitmap> Draw(bool filter = true)
        {
            return await Task.Run(() => {
                var sw = Stopwatch.StartNew();
                var eew = InformationsChecker.LatestEew;
                var (lat, lon) = (eew.Coordinate.Latitude, eew.Coordinate.Longitude);
                
                var points =  eew.EstShindo
                    .OrderBy(x => x.AnalysisResult)
                    .Select(x => new
                        {
                            x.ObservationPoint.Location,
                            Intensity = Intensity.FromValue((float) (x.GetResultToIntensity() ?? 0))
                        }
                    )
                    .Where(x => x.Intensity >= Intensity.Int1)
                    .Select(x => (x.Location.Latitude, x.Location.Longitude, x.Intensity))
                    .ToArray();

                var filtered = (filter ? FilterDrawIntensity(eew.MaxIntensity, points) : points).ToList();
                filtered.Add((lat, lon, Intensity.Unknown));
                return MapCommon.DrawMap(filtered, points, lat, lon, lat, lon);
            });
        }


        private static IEnumerable<(float, float, Intensity)> FilterDrawIntensity(
            Intensity intensity,
            IEnumerable<(float x, float y, Intensity intensity)> intList
        ) =>
            intensity.EnumOrder switch
            {
                5 or 6 => intList.Where(x => x.intensity >= Intensity.Int2),
                7 or 8 or 9 => intList.Where(x => x.intensity >= Intensity.Int3),
                _ => intList
            };
    }
}

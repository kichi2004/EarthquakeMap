using System;
using System.Drawing;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;

namespace EarthquakeMap
{
    internal static class Utilities
    {
        /// <summary>
        /// 指定されたURLから文字列をダウンロードします。
        /// </summary>
        /// <param name="url">URL</param>
        /// <returns>ダウンロードされた文字列</returns>
        internal static async Task<string> DownloadStringAsync(string url)
        {
            if (url == null)
                throw new ArgumentNullException(nameof(url));
            using var http = new HttpClient();
            http.DefaultRequestHeaders.Add("User-Agent", $"EarthquakeMap/{Version}");
            return await http.GetStringAsync(url).ConfigureAwait(false);
        }

        /// <summary>
        /// 指定されたURLから画像をダウンロードします。
        /// </summary>
        /// <param name="url">URL</param>
        /// <returns>ダウンロードされた画像</returns>
        internal static async Task<Bitmap> DownloadImageAsync(string url)
        {
            if (url == null)
                throw new ArgumentNullException(nameof(url));
            using var http = new HttpClient();
            var res = await http.GetAsync(url).ConfigureAwait(false);
            var bytes = await res.Content.ReadAsByteArrayAsync();
            using var stream = new MemoryStream(bytes);
            return new Bitmap(stream);
        }

        internal static string Version
        {
            get
            {
                var ver = Assembly.GetExecutingAssembly().GetName().Version;
                return $@"{ver.Major}.{ver.Minor}.{ver.Build}" + (ver.Revision > 0 ? $"-dev{ver.Revision}" : "");
            }
        }
    }
}

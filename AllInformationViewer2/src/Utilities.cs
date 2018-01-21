using System;
using System.Drawing;
using System.Net.Http;
using System.Threading.Tasks;

namespace AllInformationViewer2
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
            using (var http = new HttpClient())
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
            using (var http = new HttpClient()) {
                var stream = await http.GetStreamAsync(url).ConfigureAwait(false);
                return new Bitmap(stream);
            }
        }
    }
}

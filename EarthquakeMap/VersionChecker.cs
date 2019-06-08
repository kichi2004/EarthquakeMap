using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Codeplex.Data;

namespace EarthquakeMap
{
    class VersionChecker
    {
        public VersionChecker()
        {
            _webClient = new WebClient {Encoding = Encoding.UTF8};
        }
        private readonly WebClient _webClient;
        private const string Url = "http://api.kichi2004.jp/eqmap/version.json";

        public void Check()
        {
            var asm = Assembly.GetExecutingAssembly();
            var ver = asm.GetName().Version;

            var raw = _webClient.DownloadString(Url);
            var json = DynamicJson.Parse(raw);
            string[] arr = json.oldversions;

            var s = $@"{ver.Major}.{ver.Minor}.{ver.Build}";
            if (!arr.Contains(s)) return;
            var res = MessageBox.Show(
                $"新しいバージョン {json.version} がリリースされています!\r\n" +
                $"主なアップデート内容: {json.text.Replace("\\n", "\r\n")}\r\n" +
                $"配布ページを開きますか？",
                @"新しいバージョンがあります!",
                MessageBoxButtons.YesNo, MessageBoxIcon.Information,
                MessageBoxDefaultButton.Button1);
            if (res == DialogResult.No) return;
            Process.Start(json.url);
        }

    }
}

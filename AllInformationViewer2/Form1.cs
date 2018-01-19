using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Timers;

using Timer = System.Timers.Timer;

namespace AllInformationViewer2
{
    public partial class Form1 : Form
    {
        private DateTime _now;


        public Form1()
        {
            InitializeComponent();
            _ = Handle;
            Initialize();   
        }

        private async void Initialize()
        {
            var timer = new Timer(100);
            timer.Elapsed += this.TimerElapsed;
            //TODO: 例外処理
            await SetTime();
            timer.Start();
        }

        private async void TimerElapsed(object sender, ElapsedEventArgs e)
        {
            DateTime time;
            if (_now.Minute % 10 == 0 &&
                _now.Second == 0 && _now.Millisecond <= 100) {
                await SetTime();
            } else {
                _now = _now.AddMilliseconds(100);
            }
            time = _now;
            if (time.Millisecond > 100) return;

            this.BeginInvoke( new Action(() =>
            nowtime.Text = _now.ToString("HH:mm:ss")));
            time = time.AddSeconds(-1);
            //強震モニタ画像取得
            string kmoniUrl = 
                $"http://www.kmoni.bosai.go.jp/new/data/map_img/RealTimeImg/" +
                $"jma_s/{time:yyyyMMdd}/{time:yyyyMMddHHmmss}.jma_s.gif";
            var kmoniImage = await DownloadImageAsync(kmoniUrl);
            //できれば予測震度とか載せたいけどとりあえず放置
            kyoshinMonitor.Image = kmoniImage;


        }

        /// <summary>
        /// 時刻を合わせます。
        /// </summary>
        private async Task SetTime()
        {
            var str = 
                (await DownloadStringAsync("http://ntp-a1.nict.go.jp/cgi-bin/jst"))
                .Split('\n')[3];
            var time = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                .AddSeconds(double.Parse(str)).ToLocalTime();
            _now = time;
        }
        
        


        /// <summary>
        /// 指定されたURLから文字列をダウンロードします。
        /// </summary>
        /// <param name="url">URL</param>
        /// <returns>ダウンロードされた文字列</returns>
        private async Task<string> DownloadStringAsync(string url)
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
        private async Task<Bitmap> DownloadImageAsync(string url)
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

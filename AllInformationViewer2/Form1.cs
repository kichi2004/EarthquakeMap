using System;
using System.Drawing;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Forms;
using AllInformationViewer2.Enums;

using static AllInformationViewer2.Utilities;

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
            var timer = new Timer() {
                Interval = 100
            };
            timer.Tick += this.TimerElapsed;
            //TODO: 例外処理
            await SetTime();
            timer.Start();
        }

        private async void TimerElapsed(object sender, EventArgs e)
        {
            DateTime time;
            if (_now.Minute % 10 == 0 &&
                _now.Second == 0 && _now.Millisecond <= 100) {
                await SetTime();
            } else
                _now = _now.AddSeconds(0.1);
            
            //時刻補正
            time = _now.AddSeconds(0.1);
            Console.WriteLine(time.ToString("HH:mm:ss.ff"));
            if (time.Millisecond > 100) return;

            //できれば予測震度とか載せたいけどとりあえず放置
            var kmoniImage = await GetKyoshinMonitorImageAsync(time);

            //EEW・地震情報取得




            //フォーム関連は最後にまとめて
            this.BeginInvoke( new Action(() => {
                nowtime.Text = _now.ToString("HH:mm:ss");
                kyoshinMonitor.Image = kmoniImage;
            }));
        }

        /// <summary>
        /// 強震モニタの画像を取得します。
        /// </summary>
        /// <param name="time">取得する時刻</param>
        /// <returns></returns>
        private async Task<Bitmap> GetKyoshinMonitorImageAsync(DateTime time)
        {
            time = time.AddSeconds(-1);
            //強震モニタ画像取得
            string kmoniUrl =
                $"http://www.kmoni.bosai.go.jp/new/data/map_img/RealTimeImg/" +
                $"jma_s/{time:yyyyMMdd}/{time:yyyyMMddHHmmss}.jma_s.gif";
            Console.WriteLine("monitor url: " + kmoniUrl);
            return await DownloadImageAsync(kmoniUrl);
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
    }
}

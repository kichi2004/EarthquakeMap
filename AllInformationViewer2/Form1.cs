using System;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Linq;
using KyoshinMonitorLib;
using static AllInformationViewer2.Utilities;
using System.Drawing.Text;
using System.Drawing.Drawing2D;
using EarthquakeLibrary.Information;
using AllInformationViewer2.Enums;
using EarthquakeLibrary.Core;

namespace AllInformationViewer2
{
    public partial class Form1 : Form
    {
        private DateTime _now;
        private InformationsChecker checker;
        private ObservationPoint[] observationPoints;
        private FontFamily _koruriFont;

        public Form1()
        {
            InitializeComponent();
            _ = Handle;
            Initialize();
        }

        private async void Initialize()
        {
            var pfc = new PrivateFontCollection();
            pfc.AddFontFile("Koruri-Regular.ttf");
            _koruriFont = pfc.Families[0];
            observationPoints = ObservationPoint.LoadFromPbf(
                Directory.GetCurrentDirectory() + @"\lib\kyoshin_points");

            checker = new InformationsChecker();
            var timer = new Timer() {
                Interval = 100
            };
            timer.Tick += this.TimerElapsed;
            //TODO: 例外処理
            await SetTime();
            timer.Start();
        }
        int i = 0;
        private async void TimerElapsed(object sender, EventArgs e)
        {
            DateTime time;
            if (_now.Minute % 10 == 0 &&
                _now.Second == 0 && _now.Millisecond <= 100) {
                await SetTime();
            } else
                _now = _now.AddSeconds(0.1);

            //時刻補正
            time = _now.AddSeconds(0);
            if (time.Millisecond > 100) return;

            //できれば予測震度とか載せたいけどとりあえず放置
            var kmoniImage = await GetKyoshinMonitorImageAsync(time);

            //EEW・地震情報取得
            //time = new DateTime(2016, 4, 16, 1, 25, 10).AddSeconds(i++);
            //label1.Text = time.ToString("HH:mm:ss");
            Bitmap mainBitmap = null;
            (bool eewflag, bool infoflag) = await checker.Get(time);
            if (!infoflag && !eewflag) goto last;
            mainBitmap = new Bitmap(523, 435);
            var font1 = new Font(_koruriFont, 22f, FontStyle.Bold);
            var font2 = new Font(_koruriFont, 12f);
            var g = Graphics.FromImage(mainBitmap);
            g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            var format = new StringFormat {
                Alignment = StringAlignment.Center
            };
            var brush = Brushes.Black;
            if (infoflag) {
                var info = checker.LatestInformation;
                infoType.ForeColor = Color.Black;
                switch (info.InformationType) {
                    case InformationType.SesimicInfo:
                        infoType.Text = "震度速報";
                        break;
                    case InformationType.EpicenterInfo:
                        infoType.Text = "震源情報";
                        break;
                    case InformationType.EarthquakeInfo:
                        infoType.Text = "各地の震度";
                        break;
                    default:
                        goto last;
                }
                //地図描画

                //文字描画
                g.DrawString($"■{info.Origin_time:H時mm分}ごろ", font1, brush, 12, 12);
                g.DrawString($"　震源地　", font2, brush, new Point(15, 50));
                g.DrawString(info.Epicenter, font2, brush, new Point(102, 50));
                g.DrawString($"震源の深さ", font2, brush, new Point(15, 70));
                g.DrawString($"{(info.Depth != null ? $"{info.Depth}km" : "ごく浅い")}", font2, brush, new Point(102, 70));
                g.DrawString($"地震の規模", font2, brush, new Point(15, 90));
                g.DrawString($"M{info.Magnitude:0.0}", font2, brush, new Point(102, 90));
                g.DrawString($"最大震度", font2, brush, new Point(25, 110));
                g.DrawString(info.MaxIntensity.ToLongString().Replace("震度", ""), font2, brush, new Point(102, 110));

            } else if (eewflag) {
                var eew = checker.LatestEew;
                infoType.Text = "緊急地震速報";
                infoType.ForeColor = eew.IsLast ? Color.Red : Color.Black;
                Console.WriteLine($"緊急地震速報(第{eew.Number}報) {eew.Epicenter} " +
                    $"{eew.Depth}km M{eew.Magnitude} {eew.MaxIntensity.LongString}");
                string max = eew.MaxIntensity.LongString.Replace("震度", "").Replace("1", "１").Replace("2", "２")
                    .Replace("3", "３").Replace("4", "４").Replace("5", "５").Replace("6", "６").Replace("7", "７");
                //EEW予測震度画像を取得・解析
                var estImage = await DownloadImageAsync($"http://www.kmoni.bosai.go.jp/new/data/" +
                    $"map_img/EstShindoImg/eew/{time:yyyyMMdd}/{time:yyyyMMddHHmmss}.eew.gif");
                var result = observationPoints.ParseIntensityFromImage(estImage);
                string mypResult = "";
                var myPoint = result.FirstOrDefault(x => x.Region == "福岡県" && x.Name == "福岡");
                if (myPoint != null) {
                    var val = myPoint.AnalysisResult;
                    mypResult = $"{myPoint.Region} {myPoint.Name}\r\n推定震度: {Intensity.FromValue(val ?? 0).LongString.Replace("震度", "")} ({val:0.0})";
                }
                result = result.Where(x => x.AnalysisResult > 0.4);
                var maxint = result.Max(y => y.AnalysisResult) ?? 0;
                var max_points = result.Where(x => x.AnalysisResult == maxint);
                var maxpt_str = string.Join(
                    ", ", max_points.Select(x => $"{x.Region} {x.Name}").Distinct().ToArray());
                var detail = $"{mypResult}\r\n" +
                    $"最大震度: {Intensity.FromValue(maxint).LongString.Replace("震度", "")} ({maxint:0.0})\r\n{maxpt_str}";
                detailTextBox.Text = detail;
                //描画
                mainBitmap = new Bitmap(523, 435);
                //地図描画

                //文字描画
                g.FillRectangle(Brushes.Yellow, 8, 10, 239, 120);
                g.DrawString($"最大震度 {max}", font1, brush, new Rectangle(12, 12, 230, 41), format);
                g.DrawString($"　震源地　", font2, brush, new Point(15, 50));
                g.DrawString(eew.Epicenter, font2, brush, new Point(102, 50));
                g.DrawString($"震源の深さ", font2, brush, new Point(15, 70));
                g.DrawString($"{eew.Depth}km", font2, brush, new Point(102, 70));
                g.DrawString($"地震の規模", font2, brush, new Point(15, 90));
                g.DrawString($"M{eew.Magnitude:0.0}", font2, brush, new Point(102, 90));
                g.DrawString($"発生時刻", font2, brush, new Point(25, 110));
                g.DrawString($"{eew.OccurrenceTime:HH:mm:ss}", font2, brush, new Point(102, 110));
            }
            font1.Dispose();
            font2.Dispose();
            g.Dispose();
            last:
            //フォーム関連は最後にまとめて
            this.BeginInvoke(new Action(() => {
                nowtime.Text = _now.ToString("HH:mm:ss");
                kyoshinMonitor.Image = kmoniImage;
                if (eewflag || infoflag) mainPicbox.Image = mainBitmap;
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

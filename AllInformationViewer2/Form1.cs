using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using AllInformationViewer2.Enums;
using EarthquakeLibrary.Core;
using EarthquakeLibrary.Information;
using KyoshinMonitorLib;
using static AllInformationViewer2.Utilities;

namespace AllInformationViewer2
{
    public partial class Form1 : Form
    {
        private DateTime _now;
        private ObservationPoint[] observationPoints;
        private FontFamily _koruriFont;
        private Bitmap _mainBitmap = null;
        private bool _isFirst = true;

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
            time = _now.AddSeconds(0);
            if (time.Millisecond > 100) return;
            Console.WriteLine(time);
            //できれば予測震度とか載せたいけどとりあえず放置
            this.BeginInvoke(new Action(() => 
                nowtime.Text = _now.ToString("HH:mm:ss")));

                var kmoniImage = await GetKyoshinMonitorImageAsync(time);

            //EEW・地震情報取得
            (bool eewflag, bool infoflag) = await InformationsChecker.Get(time, _isFirst);
            if (!infoflag && !eewflag) goto last;
            var font1 = new Font(_koruriFont, 20f, FontStyle.Bold);
            var font2 = new Font(_koruriFont, 12f);
            var font3 = new Font(_koruriFont, 19f, FontStyle.Bold);
            _mainBitmap = new Bitmap(773, 435);
            var g = Graphics.FromImage(_mainBitmap);
            g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            var format = new StringFormat {
                Alignment = StringAlignment.Center
            };
            var brush = Brushes.White;
            if (infoflag) {
                var info = InformationsChecker.LatestInformation;
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
                using (var bmp = await Task.Run(() => Map.QuakeMap.Draw()))
                    g.DrawImage(bmp, 0, 0, bmp.Width, bmp.Height);
                //文字描画
                g.FillRectangle(new SolidBrush(Color.FromArgb(130, Color.Black)), 8, 10, 239, 120);
                g.DrawString($"{info.Origin_time:H時mm分}ごろ", font3, brush, 12, 12);
                g.DrawString($"　震源地　", font2, brush, new Point(15, 44));
                g.DrawString(info.Epicenter, font2, brush, new Point(102, 44));
                g.DrawString($"震源の深さ", font2, brush, new Point(15, 64));
                g.DrawString($"{(info.Depth != null ? $"{info.Depth}km" : "ごく浅い")}", font2, brush, new Point(102, 64));
                g.DrawString($"地震の規模", font2, brush, new Point(15, 84));
                g.DrawString($"M{info.Magnitude:0.0}", font2, brush, new Point(102, 84));
                g.DrawString($"最大震度", font2, brush, new Point(25, 104));
                g.DrawString(info.MaxIntensity.ToLongString().Replace("震度", ""), font2, brush, new Point(102, 104));

                var sindDetail = new StringBuilder();
                foreach(var sind1 in info.Shindo) {
                    sindDetail.Append($"［{sind1.Intensity.ToLongString()}］");
                    var places = sind1.Place.SelectMany(x => x.Place);
                    sindDetail.AppendLine(string.Join(" ", places));
                }
                detailTextBox.Text = sindDetail.ToString();
                
            } else if (eewflag) {
                var eew = InformationsChecker.LatestEew;
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
                    mypResult = $"{myPoint.Region} {myPoint.Name}\r\n" +
                        $"推定震度: {Intensity.FromValue(val ?? 0).LongString.Replace("震度", "")} " +
                        $"({val:0.0})";
                }
                result = result.Where(x => x.AnalysisResult > 0.4);
                var maxint = result.Max(y => y.AnalysisResult) ?? 0;
                var max_points = result.Where(x => x.AnalysisResult == maxint);
                var maxpt_str = string.Join(
                    ", ", max_points.Select(x => $"{x.Region} {x.Name}").Distinct().ToArray());
                var detail = $"{mypResult}\r\n" +
                    $"最大震度: {Intensity.FromValue(maxint).LongString.Replace("震度", "")} ({maxint:0.0})\r\n{maxpt_str}";
                detailTextBox.Text = detail;
                //地図描画
                //文字描画
                g.FillRectangle(Brushes.Yellow, 8, 10, 239, 120);
                g.DrawString($"最大震度 {max}", font1, brush, new Rectangle(10, 12, 230, 39), format);
                g.DrawString($"　震源地　", font2, brush, new Point(15, 44));
                g.DrawString(eew.Epicenter, font2, brush, new Point(102, 44));
                g.DrawString($"震源の深さ", font2, brush, new Point(15, 64));
                g.DrawString($"{eew.Depth}km", font2, brush, new Point(102, 64));
                g.DrawString($"地震の規模", font2, brush, new Point(15, 84));
                g.DrawString($"M{eew.Magnitude:0.0}", font2, brush, new Point(102, 84));
                g.DrawString($"発生時刻", font2, brush, new Point(25, 104));
                g.DrawString($"{eew.OccurrenceTime:HH:mm:ss}", font2, brush, new Point(102, 104));
            }
            font1.Dispose();
            font2.Dispose();
            last:
            _isFirst = false;
            //フォーム関連は最後にまとめて
            this.BeginInvoke(new Action(() => {
                if(kmoniImage != null) kyoshinMonitor.Image = kmoniImage;
                if (eewflag || infoflag) SwapImage(_mainBitmap);
            }));
        }

        private void SwapImage(Bitmap image)
        {
            var old = mainPicbox.Image;
            mainPicbox.Image = image;
            if(old != null) {
                old.Dispose();
                old = null;
            }
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
            Bitmap res = null;
            try {
                res = await DownloadImageAsync(kmoniUrl);
            } catch {
                res = null;
            }
            return res;
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
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
        internal static ObservationPoint[] observationPoints;
        private DateTime _now;
        private FontFamily _koruriFont;
        private Bitmap _mainBitmap = null;
        private bool _isFirst = true;
        private float _latitude;
        private float _longitude;
        private int _depth;
        private float _magnitude;
        private bool _isWarn;
        private DateTime _lastTime;
        private Intensity _lastIntensity;


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

            var timer = new FixedTimer() {
                Interval = TimeSpan.FromMilliseconds(100)
            };
            timer.Elapsed += this.TimerElapsed;
            //TODO: 例外処理
            try {
                await SetTime();
                timer.Start();
            } catch {
                MessageBox.Show("時刻合わせに失敗しました。");
                var timer2 = new System.Timers.Timer(60000);
                timer2.Elapsed += async (s, e) => {
                    try {
                        await SetTime();
                        timer2.Stop();
                        timer.Start();
                    } catch {
                    }
                };
            }
        }
        //private DateTime _time = new DateTime(2016, 11, 22, 6, 0, 0);
        private async void TimerElapsed()
        {
            DateTime time;
            if (_now.Minute % 10 == 0 &&
                _now.Second == 0 && _now.Millisecond <= 100) {
                await SetTime();
            } else
                _now = _now.AddSeconds(0.1);

            //時刻補正
            time = _now;
            Console.WriteLine(time.ToString("HH:mm:ss.fff"));
            if (time.Millisecond > 100) return;
            //できれば予測震度とか載せたいけどとりあえず放置
            this.BeginInvoke(new Action(() =>
                nowtime.Text = _now.ToString("HH:mm:ss")));

            var kmoniImage = await GetKyoshinMonitorImageAsync(time.AddSeconds(-1));

            //_time = _time.AddSeconds(1);

            //EEW・地震情報取得
            string infotype = null, detailText = null;
            (bool eewflag, bool infoflag) = (false, false);
            try {
                //↓_timeでテスト用
                (eewflag, infoflag) = await InformationsChecker.Get(time, _isFirst);
                _isFirst = false;
            } catch {
                _isFirst = false;
                goto last;
            }
            if (!infoflag && !eewflag) goto last;
            try {
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
                    switch (info.InformationType) {
                        case InformationType.SesimicInfo:
                            infotype = "震度速報";
                            break;
                        case InformationType.EpicenterInfo:
                            infotype = "震源情報";
                            break;
                        case InformationType.EarthquakeInfo:
                            infotype = "各地の震度";
                            break;
                        default:
                            goto last;
                    }
                    //地図描画
                    using (var bmp = await Task.Run(() => Map.QuakeMap.Draw()))
                        if (bmp != null) g.DrawImage(bmp, 0, 0, bmp.Width, bmp.Height);
                    bool isDetail = info.InformationType != InformationType.SesimicInfo;
                    //文字描画
                    g.FillRectangle(new SolidBrush(Color.FromArgb(130, Color.Black)), 8, 10,
                        isDetail ? 260 : 190, isDetail ? 120 : 40);
                    g.DrawString($"{info.Origin_time:H時mm分}ごろ", font3, brush, 12, 12);
                    if (isDetail) {
                        g.DrawString($"　震源地　", font2, brush, new Point(15, 44));
                        g.DrawString(info.Epicenter, font2, brush, new Point(102, 44));
                        g.DrawString($"震源の深さ", font2, brush, new Point(15, 64));
                        g.DrawString($"{(info.Depth != null ? $"{info.Depth}km" : "ごく浅い")}", font2, brush, new Point(102, 64));
                        g.DrawString($"地震の規模", font2, brush, new Point(15, 84));
                        g.DrawString($"M{info.Magnitude:0.0}", font2, brush, new Point(102, 84));
                        g.DrawString($"最大震度", font2, brush, new Point(25, 104));
                        g.DrawString(info.MaxIntensity.ToLongString().Replace("震度", ""), font2, brush, new Point(102, 104));
                    }
                    var sindDetail = new StringBuilder();
                    foreach (var sind1 in info.Shindo) {
                        sindDetail.Append($"［{sind1.Intensity.ToLongString()}］");
                        var places = sind1.Place.SelectMany(x => x.Place);
                        sindDetail.AppendLine(string.Join(" ", places));
                    }
                    detailText = sindDetail.ToString();
                } else if (eewflag) {
                    var eew = InformationsChecker.LatestEew;

                    infotype = eew.IsWarn ? "警報" : "予報";
                    Console.WriteLine($"緊急地震速報(第{eew.Number}報) {eew.Epicenter} " +
                        $"{eew.Depth}km M{eew.Magnitude} {eew.MaxIntensity.LongString}");
                    string max = eew.MaxIntensity.LongString.Replace("震度", "").Replace("1", "１").Replace("2", "２")
                        .Replace("3", "３").Replace("4", "４").Replace("5", "５").Replace("6", "６").Replace("7", "７");
                    //EEW予測震度画像を取得・解析
                    var estShindo = eew.EstShindo;
                    string mypResult = "";
                    var myPoint = estShindo.FirstOrDefault(x => x.Region == "福岡県" && x.Name == "福岡");
                    if (myPoint != null) {
                        var val = myPoint.AnalysisResult;
                        mypResult = $"{myPoint.Region} {myPoint.Name}\r\n" +
                            $"推定震度: {Intensity.FromValue(val ?? 0).LongString.Replace("震度", "")} " +
                            $"({val:0.0})";
                    }
                    estShindo = estShindo.Where(x => x.AnalysisResult > 0.4);
                    var maxint = estShindo.Max(y => y.AnalysisResult) ?? 0;
                    var max_points = estShindo.Where(x => x.AnalysisResult == maxint);
                    var maxpt_str = string.Join(
                        ", ", max_points.Select(x => $"{x.Region} {x.Name}").Distinct().ToArray());
                    var detail = $"{mypResult}\r\n" +
                        $"最大震度: {Intensity.FromValue(maxint).LongString.Replace("震度", "")} ({maxint:0.0})\r\n{maxpt_str}";
                    detailText = detail;
                    //地図描画
                    if (eew.Coordinate.Latitude == _latitude &&
                        eew.Coordinate.Longitude == _longitude &&
                        eew.Depth == _depth &&
                        eew.Magnitude == _magnitude &&
                        eew.MaxIntensity == _lastIntensity &&
                        eew.IsWarn == _isWarn &&
                        eew.OccurrenceTime == _lastTime)
                        goto last;
                    else
                        using (var bmp = await Map.EewMap.Draw()) {
                            g.DrawImage(bmp, 0, 0, bmp.Width, bmp.Height);
                        }
                    _latitude = eew.Coordinate.Latitude;
                    _longitude = eew.Coordinate.Longitude;
                    _depth = eew.Depth;
                    _magnitude = eew.Magnitude;
                    _isWarn = eew.IsWarn;
                    _lastIntensity = eew.MaxIntensity;
                    _lastTime = eew.OccurrenceTime;
                    //文字描画
                    g.FillRectangle(new SolidBrush(Color.FromArgb(130, Color.Black)), 8, 10, 260, 120);
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
            } catch (Exception e) {
                MessageBox.Show("地図描画に失敗しました。", "失敗", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Console.WriteLine(e);
            }
            goto last;
            last:
            //フォーム関連は最後にまとめて
            this.Invoke(new Action(() => {
                if (kmoniImage != null) kyoshinMonitor.Image = kmoniImage;
                if (_mainBitmap != null) SwapImage(_mainBitmap);
                if (infotype != null) {
                    if (infotype == "警報") {
                        infoType.ForeColor = Color.Red;
                        infoType.Text = "緊急地震速報";
                        detailTextBox.Font = new Font(detailTextBox.Font.FontFamily, 12f, FontStyle.Regular);
                    } else if (infotype == "予報") {
                        infoType.ForeColor = Color.Black;
                        infoType.Text = "緊急地震速報";
                        detailTextBox.Font = new Font(detailTextBox.Font.FontFamily, 12f, FontStyle.Regular);
                    } else {
                        infoType.Text = infotype;
                        detailTextBox.Font = new Font(detailTextBox.Font.FontFamily, 10f, FontStyle.Regular);
                    }
                }
                if (detailText != null) detailTextBox.Text = detailText;
            }));
        }

        private void SwapImage(Bitmap image)
        {
            var old = mainPicbox.Image;
            mainPicbox.Image = image;
            //old?.Dispose();
            old = null;
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
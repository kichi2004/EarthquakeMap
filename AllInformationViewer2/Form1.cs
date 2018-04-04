using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using EarthquakeMap.Enums;
using EarthquakeMap.Properties;
using EarthquakeLibrary.Core;
using EarthquakeLibrary.Information;
using KyoshinMonitorLib;
using Microsoft.JScript;
using static EarthquakeMap.Utilities;

namespace EarthquakeMap
{
    public partial class Form1 : Form
    {
        internal static ObservationPoint[] observationPoints;
        internal static Dictionary<string, string> _cityToArea;
        private DateTime _now;
        private FontFamily _koruriFont;
        private Bitmap _mainBitmap;
        private bool _isFirst = true;
        private float _latitude;
        private float _longitude;
        private int _depth;
        private float _magnitude;
        private bool _isWarn;
        private DateTime _lastTime;
        private Intensity _lastIntensity;
        //private bool _cityToAreaFlag;
        private int _myPointIndex;
        private bool _forceInfo;
        private Timer _timer = new Timer {Interval = 5 * 60 * 1000};
        private Dictionary<string, string> _prefToAreaDictionary;

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
            observationPoints = ObservationPoint.LoadFromMpk(
                Directory.GetCurrentDirectory() + @"\lib\kyoshin_points", true);

            myPointComboBox.SelectedIndexChanged += (s, e) =>
                _myPointIndex = myPointComboBox.SelectedIndex;

            myPointComboBox.Items.AddRange(
                observationPoints.Select(x => $"{x.Region} {x.Name}").ToArray());
            _myPointIndex = myPointComboBox.SelectedIndex = Settings.Default.myPointIndex;
            myPointComboBox.SelectedIndex = Settings.Default.myPointIndex;
            cityToArea.Checked = Settings.Default.cityToArea;
            checkBox1.Checked = Settings.Default.cutOnInfo;
            checkBox2.Checked = Settings.Default.cutOnEew;
            this.checkBox3.Checked = Settings.Default.eewArea;

            redrawButton.Click += (s, e) =>
                _forceInfo = true;

            //設定保存
            this.FormClosing += (s, e) => {
                Settings.Default.myPointIndex = _myPointIndex;
                Settings.Default.cityToArea = cityToArea.Checked;
                Settings.Default.cutOnInfo = checkBox1.Checked;
                Settings.Default.cutOnEew = checkBox2.Checked;
                Settings.Default.eewArea = this.checkBox3.Checked;
                Settings.Default.Save();
            };
            this._timer.Tick += (s, e) => this._forceInfo = true;

            var timer = new FixedTimer() {
                Interval = TimeSpan.FromMilliseconds(100)
            };
            timer.Elapsed += this.TimerElapsed;

            _cityToArea = Resources.CityToArea.Replace("\r", "").Split('\n')
                .Select(x => x.Split(',')).Where(x => x.Length == 2)
                .ToDictionary(x => x[0], x => x[1]);
            this._prefToAreaDictionary =
                Resources.kyoshin_area.Replace("\r", "").Split('\n')
                    .Select(x => x.Split(','))
                    .ToDictionary(x => x[0], x => x[1]);
            
            try {
                await SetTime();
                timer.Start();
            } catch {
                MessageBox.Show(@"時刻合わせに失敗しました。");
                var timer2 = new System.Timers.Timer(60000);
                timer2.Elapsed += async (s, e) => {
                    try {
                        await SetTime();
                        timer2.Stop();
                        timer.Start();
                    } catch {
                        //Do nothing
                    }
                };
            }
        }

        //private DateTime _time = new DateTime(2016, 11, 22, 6, 0, 0);
        private async void TimerElapsed()
        {
            if (_now.Minute % 10 == 0 &&
                _now.Second == 0 && _now.Millisecond <= 100) {
                await SetTime();
            } else
                _now = _now.AddSeconds(0.1);

            //時刻補正
            var time = this._now;
            Console.WriteLine(time.ToString("HH:mm:ss.fff"));
            if (time.Millisecond > 100) return;
            //できれば予測震度とか載せたいけどとりあえず放置
            this.BeginInvoke(new Action(() =>
                nowtime.Text = _now.ToString("HH:mm:ss")));

            //var kmoniImage = await GetKyoshinMonitorImageAsync(time.AddSeconds(-1));

            //_time = _time.AddSeconds(1);

            //EEW・地震情報取得
            string infotype = null, detailText = null;
            (bool eewflag, bool infoflag) = (false, false);
            try {
                //↓_timeでテスト用
                //var _time = new DateTime(2016, 8, 1, 17, 11, 0);
                (eewflag, infoflag) = await InformationsChecker.Get(time, _forceInfo || _isFirst);

                if (_forceInfo) {
                    infoflag = true;
                    _forceInfo = false;
                }
                _isFirst = false;
            } catch(Exception ex) {
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
                    if (_timer.Enabled) this._timer.Stop();
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
                    using (var bmp = await Task.Run(() => Map.QuakeMap.Draw(checkBox1.Checked, cityToArea.Checked)))
                        if (bmp != null) g.DrawImage(bmp, 0, 0, bmp.Width, bmp.Height);
                    var isDetail = info.InformationType != InformationType.SesimicInfo;
                    //文字描画
                    g.FillRectangle(new SolidBrush(Color.FromArgb(130, Color.Black)), 8, 5,
                        190, 40);
                    g.DrawString($"{info.Origin_time:H時mm分}ごろ", font3, brush, 12, 7);
                    if (isDetail) {
                        g.DrawImage(Image.FromFile(@"Images\Jishin\summary.png"), new Point(495, 5));
                        g.DrawString(info.Epicenter, font2, brush, new Point(590, 27));
                        g.DrawString(info.Depth != 0 ? $"約{info.Depth}km" : "ごく浅い", font1, brush, new Point(590, 61));
                        g.DrawString($"M{info.Magnitude:0.0}", font1, brush, new Point(590, 102));
                        //g.DrawString($"　震源地　", font2, brush, new Point(15, 44));
                        //g.DrawString($"震源の深さ", font2, brush, new Point(15, 64));
                        //g.DrawString($"地震の規模", font2, brush, new Point(15, 84));
                        //g.DrawString($"最大震度", font2, brush, new Point(25, 104));
                        //g.DrawString(info.MaxIntensity.ToLongString().Replace("震度", ""), font2, brush, new Point(102, 104));
                    }
                    var sindDetail = new StringBuilder();
                    foreach (var sind1 in info.Shindo) {
                        sindDetail.Append($"［{sind1.Intensity.ToLongString()}］");
                        var places = sind1.Place.SelectMany(x => x.Place);
                        sindDetail.AppendLine(string.Join(" ", places));
                    }
                    detailText = sindDetail.ToString().TrimEnd();
                } else {
                    if (_timer.Enabled) _timer.Stop();
                    _timer.Stop();
                    var eew = InformationsChecker.LatestEew;

                    infotype = eew.IsWarn ? "警報" : "予報";
                    Console.WriteLine($@"緊急地震速報(第{eew.Number}報) {eew.Epicenter} " +
                                      $@"{eew.Depth}km M{eew.Magnitude} {eew.MaxIntensity.LongString}");
                    var max = eew.MaxIntensity.LongString.Replace("震度", "").Replace("1", "１").Replace("2", "２")
                        .Replace("3", "３").Replace("4", "４").Replace("5", "５").Replace("6", "６").Replace("7", "７");
                    //EEW予測震度画像を取得・解析
                    var estShindo = eew.EstShindo.ToArray();
                    var mypResult = "";
                    var myPoint = estShindo[this._myPointIndex];
                    if (myPoint != null) {
                        var val = myPoint.AnalysisResult;
                        mypResult = "予測震度: " + 
                                    Intensity.FromValue(val ?? 0).
                                        LongString.Replace("震度", "") + 
                                    $" ({val ?? 0:0.0})";
                    }

                    var res = estShindo.Where(x => x.AnalysisResult >= 0.5);
                    var res2 = this.checkBox3.Checked
                        ? res
                            .Select(x =>(
                                    Region: this._prefToAreaDictionary[x.Region],
                                    Intensity: Intensity.FromValue(x.AnalysisResult ?? 0.0f)
                                ))
                            .Where(x => x.Region != "null")
                        : res
                            .Select(x => (
                                Region: x.Region,
                                Intensity: Intensity.FromValue(x.AnalysisResult ?? 0.0f)
                            ));
                    detailText =
                        $"第{eew.Number}報{(eew.IsLast ? "(最終)" : "")} {eew.AnnouncedTime:HH:mm:ss}発報\r\n{mypResult}\r\n\r\n" +
                        string.Join("\r\n",
                            res2
                                .OrderByDescending(x => x.Intensity.EnumOrder)
                                .Distinct(new IntensityEqualComparer()).GroupBy(x => x.Item2)
                                .Select(x => $"［{x.Key.LongString}］{string.Join(" ", x.Select(y => y.Item1))}"));
                    //地図描画
                    if (eew.Coordinate.Latitude == this._latitude &&
                        eew.Coordinate.Longitude == this._longitude &&
                        eew.Depth == this._depth &&
                        eew.Magnitude == this._magnitude &&
                        eew.MaxIntensity == this._lastIntensity &&
                        eew.IsWarn == this._isWarn &&
                        eew.OccurrenceTime == this._lastTime)
                        goto last;
                    using (var bmp = await Map.EewMap.Draw(this.checkBox2.Checked)) {
                        g.DrawImage(bmp, 0, 0, bmp.Width, bmp.Height);
                    }

                    this._latitude = eew.Coordinate.Latitude;
                    this._longitude = eew.Coordinate.Longitude;
                    this._depth = eew.Depth;
                    this._magnitude = eew.Magnitude;
                    this._isWarn = eew.IsWarn;
                    this._lastIntensity = eew.MaxIntensity;
                    this._lastTime = eew.OccurrenceTime;
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

            last:
            //フォーム関連は最後にまとめて
            try {
                this.Invoke(new Action(() => {
                    if (IsDisposed) return;
                    if (_mainBitmap != null) SwapImage(_mainBitmap);
                    if (infotype != null) {
                        if (infotype == "警報") {
                            infoType.ForeColor = Color.Red;
                            infoType.Text = "緊急地震速報";
                        } else if (infotype == "予報") {
                            infoType.ForeColor = Color.Black;
                            infoType.Text = "緊急地震速報";
                        } else {
                            infoType.ForeColor = Color.Black;
                            infoType.Text = infotype;
                        }
                    }
                    if (detailText != null)
                        detailTextBox.Text = detailText;
                }));
            } catch {

            }
        }

        private void SwapImage(Bitmap image)
        {
            var old = mainPicbox.Image;
            mainPicbox.Image = image;
            //old?.Dispose();
            old = null;
        }

        ///// <summary>
        ///// 強震モニタの画像を取得します。
        ///// </summary>
        ///// <param name="time">取得する時刻</param>
        ///// <returns></returns>
        //private async Task<Bitmap> GetKyoshinMonitorImageAsync(DateTime time)
        //{
        //    time = time.AddSeconds(-1);
        //    //強震モニタ画像取得
        //    string kmoniUrl =
        //        $"http://www.kmoni.bosai.go.jp/new/data/map_img/RealTimeImg/" +
        //        $"jma_s/{time:yyyyMMdd}/{time:yyyyMMddHHmmss}.jma_s.gif";
        //    Bitmap res = null;
        //    try {
        //        res = await DownloadImageAsync(kmoniUrl);
        //    } catch {
        //        res = null;
        //    }
        //    return res;
        //}

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
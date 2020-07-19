using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using EarthquakeLibrary;
using EarthquakeLibrary.Information;
using EarthquakeMap.Map;
using EarthquakeMap.Properties;
using KyoshinMonitorLib;
using static EarthquakeMap.Utilities;
using Timer = System.Timers.Timer;

// ReSharper disable CompareOfFloatsByEqualityOperator

namespace EarthquakeMap
{
    public partial class Form1 : Form
    {
        internal static ObservationPoint[] ObservationPoints;
        internal static Dictionary<string, string> CityToArea;
        internal static string Url;
        internal static Dictionary<int, Color> Colors = new Dictionary<int, Color>
        {
            {1, Color.FromArgb(70, 100, 110)},
            {2, Color.FromArgb(30, 110, 230)},
            {3, Color.FromArgb(0, 200, 200)},
            {4, Color.FromArgb(250, 250, 100)},
            {5, Color.FromArgb(255, 180, 0)},
            {6, Color.FromArgb(255, 120, 0)},
            {7, Color.FromArgb(230, 0, 0)},
            {8, Color.FromArgb(160, 0, 0)},
            {9, Color.FromArgb(150, 0, 150)},
        };

        private DateTime _now, _time;
        private FontFamily _koruriFont;
        private Bitmap _mainBitmap, _lastBitmap;
        private bool _isFirst = true;
        private bool _isTest;
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
        private readonly Timer _timer = new Timer { Interval = 5 * 60 * 1000 };
        private Dictionary<string, string> _prefToAreaDictionary;
        private VersionChecker _checker;

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
            ObservationPoints = ObservationPoint.LoadFromMpk(
                Directory.GetCurrentDirectory() + @"\lib\kyoshin_points", true);

            myPointComboBox.SelectedIndexChanged += (s, e) =>
                _myPointIndex = myPointComboBox.SelectedIndex;

            myPointComboBox.Items.AddRange(
                ObservationPoints.Select(x => $"{x.Region} {x.Name}" as object).ToArray());
            _myPointIndex = myPointComboBox.SelectedIndex = Settings.Default.myPointIndex;
            myPointComboBox.SelectedIndex = Settings.Default.myPointIndex;
            cityToArea.Checked = Settings.Default.cityToArea;
            checkBox1.Checked = Settings.Default.cutOnInfo;
            checkBox2.Checked = Settings.Default.cutOnEew;
            checkBox3.Checked = Settings.Default.eewArea;
            keepSetting.SelectedIndex = 0;
            redrawButton.Click += (s, e) =>
                _forceInfo = true;
            mainPicbox.Paint += (s, e) => {
                if (_mainBitmap == null) return;
                var img = _mainBitmap;
                e.Graphics.DrawImage(img, 0, 0);
            };
            saveImageButton.Click += (s, e) => SaveImage();
            detailTextBox.KeyDown += (s, e) =>
            {
                if(e.Control && e.KeyCode == Keys.A) detailTextBox.SelectAll();
            };


            //設定保存
            FormClosing += SaveSettings;
            _timer.Elapsed += (s, e) => _forceInfo = true;

            var timer = new FixedTimer
            {
                Interval = TimeSpan.FromMilliseconds(100)
            };
            timer.Elapsed += TimerElapsed;

            CityToArea = Resources.CityToArea.Replace("\r", "").Split('\n')
                .Select(x => x.Split(',')).Where(x => x.Length == 2)
                .ToDictionary(x => x[0], x => x[1]);
            _prefToAreaDictionary =
                Resources.kyoshin_area.Replace("\r", "").Split('\n')
                    .Select(x => x.Split(','))
                    .ToDictionary(x => x[0], x => x[1]);
        //テスト設定読み込み
        reset:
            try
            {
                var passes = new[] { @"config\url.txt", @"config\eew.txt", @"config\position.txt" };
                if (!Directory.Exists("config"))
                    Directory.CreateDirectory("config");
                if (!File.Exists(passes[0]))
                    using (var st = File.CreateText(passes[0]))
                        st.Write(Information.YahooUrl);

                Url = File.ReadAllText(passes[0]).Trim();

                if (!File.Exists(passes[1]))
                    using (var st = File.CreateText(passes[1]))
                        st.Write(@"#通常は編集しないでください。
#有効/無効：Enableで有効、Disableで無効。
test=Disable
#日時：yyyyMMddHHmmss形式
time=20180101000000");
                var eewtxt = File.ReadAllLines(passes[1]);
                _isTest = Convert.ToBoolean(eewtxt[2].Split('=').ElementAtOrDefault(1) == "Enable");
                _time = DateTime.ParseExact(eewtxt[4].Split('=')[1], "yyyyMMddHHmmss",
                    CultureInfo.CurrentCulture, DateTimeStyles.None);
                if(!File.Exists(passes[2]))
                    using (var st = File.CreateText(passes[2]))
                        st.Write(@"0,0");

                var positionStr = File.ReadLines(passes[2]).First()
                    .Split(',').Select(a => a.Trim()).ToArray();
                if (int.TryParse(positionStr[0], out var x) && int.TryParse(positionStr[1], out var y))
                {
                    Left = x;
                    Top = y;
                }
            }
            catch
            {
                foreach (var filename in Directory.EnumerateFiles("config"))
                    File.Delete(filename);
                goto reset;
            }

            try
            {
                await SetTime();
                timer.Start();
            }
            catch
            {
                MessageBox.Show(@"時刻合わせに失敗しました。");
                var timer2 = new Timer(60000);
                timer2.Elapsed += async (s, e) => {
                    try
                    {
                        await SetTime();
                        timer2.Stop();
                        timer.Start();
                    }
                    catch
                    {
                        //Do nothing
                    }
                };
            }

            _checker = new VersionChecker();
            _checker.Check();
        }

        private void SaveSettings(object s, FormClosingEventArgs e)
        {
            Settings.Default.myPointIndex = _myPointIndex;
            Settings.Default.cityToArea = cityToArea.Checked;
            Settings.Default.cutOnInfo = checkBox1.Checked;
            Settings.Default.cutOnEew = checkBox2.Checked;
            Settings.Default.eewArea = checkBox3.Checked;
            Settings.Default.Save();

            if (Left + Width <= 0 || Top + Height <= 0) return;
            if (!Directory.Exists("config"))
                Directory.CreateDirectory("config");
            File.WriteAllText("config/position.txt", $@"{Left},{Top}");
        }

        private async void TimerElapsed()
        {
            if (_now.Minute % 10 == 0 &&
                _now.Second == 0 && _now.Millisecond <= 100)
            {
                if((_now.Hour == 6 || _now.Hour == 18) && _now.Minute == 0)
                    _checker.Check();
                await SetTime();
            }
            else
                _now = _now.AddSeconds(0.1);

            Bitmap pic = null;
            //時刻補正
            var time = _now;
            //Console.WriteLine(time.ToString("HH:mm:ss.fff"));
            if (time.Millisecond > 100) return;
            //できれば予測震度とか載せたいけどとりあえず放置
            BeginInvoke(new Action(() =>
                nowtime.Text = _now.ToString("HH:mm:ss")));

            //var kmoniImage = await GetKyoshinMonitorImageAsync(time.AddSeconds(-1));


            //EEW・地震情報取得
            string infotype = null, detailText = null;
            bool eewflag, infoflag;
            try
            {
                //↓_timeでテスト用
                var f = _isFirst;
                _isFirst = false;
                (eewflag, infoflag) = await InformationsChecker.Get(_isTest ? _time : time, _forceInfo || f, Url);
                if (_isTest)
                    _time = _time.AddSeconds(1);

                if (_forceInfo)
                {
                    infoflag = true;
                    _forceInfo = false;
                }

                _isFirst = false;
            }
            catch (Exception)
            {
                _isFirst = false;
                goto last;
            }

            if (!infoflag && !eewflag) goto last;
            var selectedIndex = -1;
            Invoke((Action)(() => { selectedIndex = keepSetting.SelectedIndex; }));
            if (selectedIndex != 0)
            {
                var intensity = infoflag
                    ? InformationsChecker.LatestInformation.MaxIntensity
                    : InformationsChecker.LatestEew.MaxIntensity;
                if (intensity.EnumOrder < selectedIndex + 2)
                    goto last;
            }
            try
            {
                var font9 = new Font(_koruriFont, 9);
                var font10 = new Font(_koruriFont, 10);
                var font11 = new Font(_koruriFont, 11);
                var font12 = new Font(_koruriFont, 12);
                var font13 = new Font(_koruriFont, 13);
                var font14 = new Font(_koruriFont, 14);
                var font16 = new Font(_koruriFont, 16);
                var font40 = new Font(_koruriFont, 40);
                var font20 = new Font(_koruriFont, 20);
                var font23 = new Font(_koruriFont, 23);
                var font19b = new Font(_koruriFont, 19, FontStyle.Bold);
                var font20b = new Font(_koruriFont, 20, FontStyle.Bold);
                var roboto = new Font("roboto", 40);
                var robotoI = new Font("roboto", 40, FontStyle.Italic);
                pic = new Bitmap(773, 435);
                var g = Graphics.FromImage(pic);
                g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                var sfCC = new StringFormat
                    { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                var sfCF = new StringFormat
                    { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Far };
                const int left = 8, top = 50;
                var secondary = Color.FromArgb(191, 191, 191);
                if (infoflag)
                {
                    var info = InformationsChecker.LatestInformation;
                    if (_timer.Enabled) _timer.Stop();

                    switch (info.InformationType)
                    {
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
                    using (var bmp = await Task.Run(() => QuakeMap.Draw(checkBox1.Checked, cityToArea.Checked)))
                        if (bmp != null)
                            g.DrawImage(bmp, 0, 0, bmp.Width, bmp.Height);

                    if (info.InformationType == InformationType.SesimicInfo)
                    {
                        g.FillRectangle(new SolidBrush(Color.FromArgb(130, Color.Black)), left, 10, 140, 65);
                        TextRenderer.DrawText(g, "震度速報",font23, new Rectangle(left, 10, 140, 40), Color.White);
                        TextRenderer.DrawText(g, $"{info.OriginTime:HH時mm分頃発生}", font12, new Rectangle(left, 50, 140, 20), Color.White);
                    }
                    else
                    {
                        g.FillRectangle(new SolidBrush(Color.FromArgb(130, Color.Black)), left, 10, 360, 102);
                        TextRenderer.DrawText(g, $"{info.OriginTime:d日H時mm分}頃発生", font9, new Point(left, 65), Color.White);
                        TextRenderer.DrawText(g, "震源地", font10, new Point(left + 150, 10), secondary);
                        TextRenderer.DrawText(g, info.Epicenter,
                            info.Epicenter.Length < 8 ? font16 : font14,
                            new Rectangle(left + 160, 25, 200, 27), Color.White, TextFormatFlags.VerticalCenter);
                        TextRenderer.DrawText(g, "深さ", font10, new Point(left + 150, 61), secondary);
                        var depth = info.Depth == null ? "----" : info.Depth != 0 ? $"約{info.Depth}km" : "ごく浅い";
                        TextRenderer.DrawText(g, depth, font14, new Point(left + 180, 55), Color.White);
                        TextRenderer.DrawText(g, "規模", font10, new Point(left + 270, 61), secondary);
                        TextRenderer.DrawText(g, "M", font11, new Point(left + 301, 60), Color.White);
                        TextRenderer.DrawText(g, info.Magnitude == null ? "---" : $"{info.Magnitude:0.0}", font16,
                            new Point(left + 317, 52), Color.White);
                        var tsunamiMessage = "";
                        var tsunamiColor = Color.Yellow;
                        var tsunamiFont = font16;

                        if ((info.Message & MessageType.NoTsunami) != 0)
                        {
                            tsunamiColor = Color.White;
                            tsunamiMessage = "津波の心配なし";
                        }
                        else if ((info.Message & MessageType.SeaLevelChange) != 0)
                        {
                            tsunamiMessage = "若干の海面変動 被害の心配なし";
                        }
                        else if ((info.Message & MessageType.TsunamiInformation) != 0)
                        {
                            tsunamiFont = font13;
                            tsunamiMessage = "津波に関する情報（津波警報等）を発表中";
                        }

                        TextRenderer.DrawText(g, tsunamiMessage, tsunamiFont, new Rectangle(left, 78, 360, 32),
                            tsunamiColor, TextFormatFlags.VerticalCenter | TextFormatFlags.Right);

                        if (info.InformationType == InformationType.EarthquakeInfo)
                        {
                            var color = Colors[info.MaxIntensity.EnumOrder];
                            var textColor = info.MaxIntensity.Equals(Intensity.Int3) ||
                                            info.MaxIntensity.Equals(Intensity.Int4) ||
                                            info.MaxIntensity.Equals(Intensity.Int5Minus) ||
                                            info.MaxIntensity.Equals(Intensity.Unknown)
                                ? Color.Black
                                : Color.White;
                            g.FillRectangle(new SolidBrush(color), left, 10, 147, 55);
                            var s = info.MaxIntensity.ShortString[0].ToString();
                            g.DrawString("最大震度", font14, new SolidBrush(textColor), left, 39);
                            if (info.MaxIntensity.Equals(Intensity.Unknown))
                            {
                                g.DrawString("不明", font20, Brushes.Black, new Point(left + 80, top + 27));
                            }
                            else if (info.MaxIntensity.ShortString.Length == 2)
                            {
                                g.DrawString(s, robotoI, new SolidBrush(textColor),
                                    new Rectangle(left + 79, 16, 35, 53), sfCC);
                                var pmChar = info.MaxIntensity.LongString.Last().ToString();
                                g.DrawString(pmChar, font20b, new SolidBrush(textColor),
                                    new Rectangle(left + 104, 16, 45, 53), sfCF);
                            }
                            else
                            {
                                g.DrawString(s, robotoI, new SolidBrush(textColor),
                                    new Rectangle(left + 74, 16, 80, 53), sfCC);
                            }
                        }
                        else
                        {
                            TextRenderer.DrawText(g, "震源情報", font20, new Rectangle(left + 203, 10, 147, 55),
                                Color.White, TextFormatFlags.VerticalCenter | TextFormatFlags.HorizontalCenter);
                        }
                    }
                    var sindDetail = new StringBuilder();
                    foreach (var sind1 in info.Shindo)
                    {
                        sindDetail.Append($"［{sind1.Intensity.LongString}］");
                        var places = sind1.Place.SelectMany(x => x.Place);
                        sindDetail.AppendLine(string.Join(" ", places));
                    }

                    detailText = sindDetail.ToString().TrimEnd();
                }
                else
                {
                    _timer.Stop();
                    _timer.Start();
                    var eew = InformationsChecker.LatestEew;

                    infotype = eew.IsWarn ? "警報" : "予報";
                    Console.WriteLine($@"緊急地震速報(第{eew.Number}報) {eew.Epicenter} " +
                                      $@"{eew.Depth}km M{eew.Magnitude} {eew.MaxIntensity.LongString}");
                    //EEW予測震度画像を取得・解析
                    var estShindo = eew.EstShindo.ToArray();
                    var mypResult = "";
                    var myPoint = estShindo[_myPointIndex];
                    if (myPoint != null)
                    {
                        var val = myPoint.AnalysisResult;
                        mypResult = "地点予測震度: " +
                                    Intensity.FromValue(val ?? 0).LongString.Replace("震度", "") +
                                    $" ({val ?? 0:0.0})";
                    }

                    var res = estShindo.Where(x => x.AnalysisResult >= 0.5);
                    var res2 = checkBox3.Checked
                        ? res
                            .Select(x => (
                                Region: _prefToAreaDictionary[x.Region],
                                Intensity: Intensity.FromValue(x.AnalysisResult ?? 0.0f),
                                Value: x.AnalysisResult ?? 0.0f
                            ))
                            .Where(x => x.Region != "null")
                        : res
                            .Select(x => (x.Region,
                                    Intensity: Intensity.FromValue(x.AnalysisResult ?? 0.0f),
                                    Value: x.AnalysisResult ?? 0.0f
                                ));
                    detailText =
                        $"第{eew.Number}報{(eew.IsLast ? "(最終)" : "")} {eew.AnnouncedTime:HH:mm:ss}発報\r\n{mypResult}\r\n\r\n" +
                        string.Join("\r\n",
                            res2
                                .OrderByDescending(x => x.Value)
                                .Distinct(new IntensityEqualComparer()).GroupBy(x => x.Item2)
                                .Select(x => $"［{x.Key.LongString}］{string.Join(" ", x.Select(y => y.Item1))}"));

                    Bitmap bmp;
                    //地図描画
                    if (eew.Coordinate.Latitude == _latitude &&
                        eew.Coordinate.Longitude == _longitude &&
                        eew.Depth == _depth &&
                        eew.Magnitude == _magnitude &&
                        eew.MaxIntensity.Equals(_lastIntensity) &&
                        eew.IsWarn == _isWarn &&
                        eew.OccurrenceTime == _lastTime)
                    {
                        bmp = _lastBitmap;
                    }
                    else
                    {
                        bmp = await Task.Run(() => EewMap.Draw(checkBox2.Checked));
                        _lastBitmap = bmp;
                    }

                    g.DrawImage(bmp, 0, 0, bmp.Width, bmp.Height);

                    _latitude = eew.Coordinate.Latitude;
                    _longitude = eew.Coordinate.Longitude;
                    _depth = eew.Depth;
                    _magnitude = eew.Magnitude;
                    _isWarn = eew.IsWarn;
                    _lastIntensity = eew.MaxIntensity;
                    _lastTime = eew.OccurrenceTime;
                    if (!Colors.TryGetValue(eew.MaxIntensity.EnumOrder, out var color)) color = Color.White;
                    var textColor = eew.MaxIntensity.Equals(Intensity.Int3) ||
                                    eew.MaxIntensity.Equals(Intensity.Int4) ||
                                    eew.MaxIntensity.Equals(Intensity.Int5Minus) ||
                                    eew.MaxIntensity.Equals(Intensity.Unknown)
                        ? Color.Black
                        : Color.White;
                    var pointTopLeft = new Point(left, top);
                    g.FillRectangle(new SolidBrush(Color.FromArgb(130, Color.Black)), left, 10, 260, 160);
                    g.FillRectangle(new SolidBrush(color), left, top + 16, 147, 55);
                    var last = eew.IsLast ? "(最終)" : "";
                    TextRenderer.DrawText(g, "緊急地震速報", font23, new Rectangle(left - 4, 10, 260, 40), Color.White, TextFormatFlags.VerticalCenter);
                    TextRenderer.DrawText(g, eew.IsWarn ? "（警報）" : "（予報）", font14, new Point(left + 180, 35), Color.White, TextFormatFlags.VerticalCenter);
                    TextRenderer.DrawText(g, $"第{eew.Number}報{last}", font9, pointTopLeft, Color.White);
                    var s = eew.MaxIntensity.ShortString.Replace("-", "").Replace("+", "");
                    g.DrawString("最大震度", font14, new SolidBrush(textColor), left, top + 45);
                    if (eew.MaxIntensity.Equals(Intensity.Unknown))
                    {
                        g.DrawString("不明", font20, Brushes.Black, new Point(left + 80, top + 27));
                    }
                    else if (eew.MaxIntensity.ShortString.Length == 2)
                    {
                        g.DrawString(s, robotoI, new SolidBrush(textColor),
                            new Rectangle(left + 79, top + 20, 35, 53), sfCC);
                        var pmChar = eew.MaxIntensity.LongString.Last().ToString();
                        g.DrawString(pmChar, font20b, new SolidBrush(textColor),
                            new Rectangle(left + 104, top + 22, 45, 53), sfCF);
                    }
                    else
                    {
                        g.DrawString(s, robotoI, new SolidBrush(textColor),
                            new Rectangle(left + 74, top + 20, 80, 53), sfCC);
                    }

                    TextRenderer.DrawText(g, "M", font20, new Point(left + 145, top + 36), Color.White);
                    TextRenderer.DrawText(g, $"{eew.Magnitude:0.0}", font40, new Point(left + 166, top + 7), Color.White);
                    TextRenderer.DrawText(g, $"{eew.OccurrenceTime:HH:mm:ss}発生", font9, new Rectangle(left, top, 260, 15),
                        Color.White, TextFormatFlags.Right);
                    TextRenderer.DrawText(g, "震源地", font10, new Point(left + 1, top + 75), secondary);
                    TextRenderer.DrawText(g, eew.Epicenter,
                        eew.Epicenter.Length < 8 ? font16 : font12,
                        new Rectangle(left + 1, top + 90, 170, 27), Color.White, TextFormatFlags.VerticalCenter);
                    TextRenderer.DrawText(g, "深さ", font10, new Point(left + 174, top + 75), secondary);
                    TextRenderer.DrawText(g, $"{eew.Depth}km", font16, new Point(left + 172, top + 89), Color.White);
                }

                new List<Font> {font9, font10, font14, font16, font20, font23, font40, font19b, font20b, roboto}
                    .ForEach(x => x.Dispose());
            }
            catch (Exception e)
            {
                //MessageBox.Show("地図描画に失敗しました。", "失敗", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Console.WriteLine(e);
            }

            last:
            //フォーム関連は最後にまとめて
            try
            {
                BeginInvoke(new Action(() =>
                {
                    if (IsDisposed) return;
                    if (infotype != null)
                    {
                        switch (infotype)
                        {
                            case "警報":
                                infoType.ForeColor = Color.Red;
                                infoType.Text = @"緊急地震速報";
                                break;
                            case "予報":
                                infoType.ForeColor = Color.Black;
                                infoType.Text = @"EEW予測震度";
                                break;
                            default:
                                infoType.ForeColor = Color.Black;
                                infoType.Text = infotype;
                                break;
                        }
                    }

                    if (detailText != null)
                        detailTextBox.Text = detailText;
                    if (pic == null) return;
                    var old = _mainBitmap;
                    _mainBitmap = pic;
                    old?.Dispose();
                    mainPicbox.Refresh();
                }));
            }
            catch
            {
                //失敗してもとりあえず何もしない
            }
        }
        /*
        private void SwapImage(Image newImage)
        {
            if (this.mainPicbox == null)
                throw new ArgumentNullException(nameof(this.mainPicbox));
            var oldImg = this.mainPicbox.Image;
            this.mainPicbox.Image = newImage;
            oldImg?.Dispose();
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
                $"http://www.kmoni.bosai.go.jp/data/map_img/RealTimeImg/" +
                $"jma_s/{time:yyyyMMdd}/{time:yyyyMMddHHmmss}.jma_s.gif";
            Bitmap res = null;
            try {
                res = await DownloadImageAsync(kmoniUrl);
            } catch {
                res = null;
            }
            return res;
        }
        */

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

        private void SaveImage()
        {
            if (_mainBitmap == null) return;
            if (!Directory.Exists("images"))
                Directory.CreateDirectory("images");
            var fileNameBase = $"images/{DateTime.Now:yyyyMMddHHmmss}";
            var fileName = fileNameBase + ".png";
            if (File.Exists(fileName))
            {
                var flag = true;
                for (var i = 1; i <= 99; i++)
                {
                    fileName = $"{fileNameBase}_{i}.png";
                    if (File.Exists(fileName)) continue;
                    flag = false;
                    break;
                }

                if (flag) return;
            }

            _mainBitmap?.Save(fileName, ImageFormat.Png);
        }
    }
}
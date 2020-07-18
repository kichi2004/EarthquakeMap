﻿using System;
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
                var font9 = new Font(_koruriFont, 9f);
                var font10 = new Font(_koruriFont, 10f);
                var font12 = new Font(_koruriFont, 12f);
                var font14 = new Font(_koruriFont, 14f);
                var font16 = new Font(_koruriFont, 16f);
                var font40 = new Font(_koruriFont, 40f);
                var font20 = new Font(_koruriFont, 20);
                var font19b = new Font(_koruriFont, 19f, FontStyle.Bold);
                var font20b = new Font(_koruriFont, 20f, FontStyle.Bold);
                var roboto = new Font("roboto", 40f);
                var robotoI = new Font("roboto", 40f, FontStyle.Italic);
                pic = new Bitmap(773, 435);
                var g = Graphics.FromImage(pic);
                g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                var brush = Brushes.White;
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
                    var isDetail = info.InformationType != InformationType.SesimicInfo;
                    //文字描画
                    g.FillRectangle(new SolidBrush(Color.FromArgb(130, Color.Black)), 8, 5,
                        190, 40);
                    g.DrawString($"{info.OriginTime:H時mm分}ごろ", font19b, brush, 12, 7);

                    if (isDetail)
                    {
                        var dictepi = new Dictionary<string, string>
                        {
                            {"留萌地方中北部", "留萌地方\r\n中北部"},
                            {"胆振地方中東部", "胆振地方\r\n中東部"},
                            {"釧路地方中南部", "釧路地方\r\n中南部"},
                            {"根室半島南東沖", "根室半島\r\n南東沖"},
                            {"青森県津軽北部", "青森県\r\n津軽北部"},
                            {"青森県津軽南部", "青森県\r\n津軽南部"},
                            {"青森県三八上北地方", "青森県\r\n三八上北地方"},
                            {"青森県下北地方", "青森県\r\n下北地方"},
                            {"岩手県沿岸北部", "岩手県\r\n沿岸北部"},
                            {"岩手県沿岸南部", "岩手県\r\n沿岸南部"},
                            {"岩手県内陸北部", "岩手県\r\n内陸北部"},
                            {"岩手県内陸南部", "岩手県\r\n内陸南部"},
                            {"秋田県沿岸北部", "秋田県\r\n沿岸北部"},
                            {"秋田県沿岸南部", "秋田県\r\n沿岸南部"},
                            {"秋田県内陸北部", "秋田県\r\n内陸北部"},
                            {"秋田県内陸南部", "秋田県\r\n内陸南部"},
                            {"山形県庄内地方", "山形県\r\n庄内地方"},
                            {"山形県最上地方", "山形県\r\n最上地方"},
                            {"山形県村山地方", "山形県\r\n村山地方"},
                            {"山形県置賜地方", "山形県\r\n置賜地方"},
                            {"埼玉県秩父地方", "埼玉県\r\n秩父地方"},
                            {"房総半島南方沖", "房総半島\r\n南方沖"},
                            {"東京都多摩東部", "東京都\r\n多摩東部"},
                            {"東京都多摩西部", "東京都\r\n多摩西部"},
                            {"新潟県上越地方", "新潟県\r\n上越地方"},
                            {"新潟県中越地方", "新潟県\r\n中越地方"},
                            {"新潟県下越地方", "新潟県\r\n下越地方"},
                            {"新潟県上中越沖", "新潟県\r\n上中越沖"},
                            {"石川県能登地方", "石川県\r\n能登地方"},
                            {"石川県加賀地方", "石川県\r\n加賀地方"},
                            {"山梨県中・西部", "山梨県\r\n中・西部"},
                            {"山梨県東部・富士五湖", "山梨県東部・\r\n富士五湖"},
                            {"岐阜県飛騨地方", "岐阜県\r\n飛騨地方"},
                            {"岐阜県美濃東部", "岐阜県\r\n美濃東部"},
                            {"岐阜県美濃中西部", "岐阜県\r\n美濃中西部"},
                            {"静岡県伊豆地方", "静岡県\r\n伊豆地方"},
                            {"伊豆半島東方沖", "伊豆半島\r\n東方沖"},
                            {"新島・神津島近海", "新島・神津島\r\n近海"},
                            {"和歌山県南方沖", "和歌山県\r\n南方沖"},
                            {"福岡県福岡地方", "福岡県\r\n福岡地方"},
                            {"福岡県北九州地方", "福岡県\r\n北九州地方"},
                            {"福岡県筑豊地方", "福岡県\r\n筑豊地方"},
                            {"福岡県筑後地方", "福岡県\r\n筑後地方"},
                            {"長崎県島原半島", "長崎県\r\n島原半島"},
                            {"熊本県阿蘇地方", "熊本県\r\n阿蘇地方"},
                            {"熊本県熊本地方", "熊本県\r\n熊本地方"},
                            {"熊本県球磨地方", "熊本県\r\n球磨地方"},
                            {"熊本県天草・芦北地方", "熊本県天草・\r\n芦北地方"},
                            {"宮崎県北部平野部", "宮崎県\r\n北部平野部"},
                            {"宮崎県北部山沿い", "宮崎県\r\n北部山沿い"},
                            {"宮崎県南部平野部", "宮崎県\r\n南部平野部"},
                            {"宮崎県南部山沿い", "宮崎県\r\n南部山沿い"},
                            {"鹿児島県薩摩地方", "鹿児島県\r\n薩摩地方"},
                            {"鹿児島県大隅地方", "鹿児島県\r\n大隅地方"},
                            {"壱岐・対馬近海", "壱岐・対馬\r\n近海"},
                            {"薩摩半島西方沖", "薩摩半島\r\n西方沖"},
                            {"トカラ列島近海", "トカラ列島\r\n近海"},
                            {"奄美大島北西沖", "奄美大島\r\n北西沖"},
                            {"大隅半島東方沖", "大隅半島\r\n東方沖"},
                            {"九州地方南東沖", "九州地方\r\n南東沖"},
                            {"奄美大島北東沖", "奄美大島\r\n北東沖"},
                            {"沖縄本島南方沖", "沖縄本島\r\n南方沖"},
                            {"沖縄本島北西沖", "沖縄本島\r\n北西沖"},
                            {"オホーツク海南部", "オホーツク海\r\n南部"},
                            {"サハリン西方沖", "サハリン\r\n西方沖"},
                            {"千島列島南東沖", "千島列島\r\n南東沖"},
                            {"東北地方東方沖", "東北地方\r\n東方沖"},
                            {"小笠原諸島西方沖", "小笠原諸島\r\n西方沖"},
                            {"小笠原諸島東方沖", "小笠原諸島\r\n東方沖"},
                            {"薩南諸島東方沖", "薩南諸島\r\n東方沖"},
                            {"サハリン南部付近", "サハリン南部\r\n付近"}
                        };
                        string epi = null;
                        if (info.Epicenter.Length <= 6 || dictepi.TryGetValue(info.Epicenter, out epi))
                        {
                            var b = epi == null;
                            if (b) epi = info.Epicenter;
                            g.DrawImage(Image.FromFile(@"materials\Jishin\" + (b ? "Summary1.png" : "Summary2.png")),
                                new Point(495, 5));
                            g.DrawString(epi, new Font(_koruriFont, 20f), brush, new Point(587, 12));
                            g.DrawString(info.Depth != 0 ? $"約{info.Depth}km" : "ごく浅い", font20b, brush,
                                b ? new Point(587, 52) : new Point(587, 85));
                            g.DrawString($"M{info.Magnitude:0.0}", font20b, brush,
                                b ? new Point(587, 94) : new Point(587, 126));
                        }
                        else
                        {
                            Point epicenterPoint;
                            Font epicenterFont;
                            if (info.Epicenter.Length == 10)
                            {
                                epicenterFont = new Font(_koruriFont, 18f);
                                epicenterPoint = new Point(506, 49);
                            }
                            else
                            {
                                epicenterFont = new Font(_koruriFont, 20f);
                                epicenterPoint = new Point(506, 47);
                            }

                            g.DrawImage(Image.FromFile(@"materials\Jishin\Summary2.png"), new Point(495, 5));
                            g.DrawString(info.Epicenter, epicenterFont, brush, epicenterPoint);
                            g.DrawString(info.Depth != 0 ? $"約{info.Depth}km" : "ごく浅い", font20b, brush,
                                new Point(587, 85));
                            g.DrawString($"M{info.Magnitude:0.0}", font20b, brush, new Point(587, 126));
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
                    var max = eew.MaxIntensity.LongString.Replace("震度", "").Replace("1", "１").Replace("2", "２")
                        .Replace("3", "３").Replace("4", "４").Replace("5", "５").Replace("6", "６").Replace("7", "７");
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
                    //文字描画
                    // var roboto14 = new Font("Roboto", 14, FontStyle.Regular);
                    var colors = new Dictionary<int, Color>
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
                    if (!colors.TryGetValue(eew.MaxIntensity.EnumOrder, out var color)) color = Color.White;
                    const TextFormatFlags flag = TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter;
                    var textColor = eew.MaxIntensity.Equals(Intensity.Int3) ||
                                    eew.MaxIntensity.Equals(Intensity.Int4) ||
                                    eew.MaxIntensity.Equals(Intensity.Int5Minus) ||
                                    eew.MaxIntensity.Equals(Intensity.Unknown)
                        ? Color.Black
                        : Color.White;

                    g.FillRectangle(new SolidBrush(Color.FromArgb(130, Color.Black)), 8, 10, 260, 120);
                    g.FillRectangle(new SolidBrush(color), 8, 26, 147, 55);
                    var sfCC = new StringFormat
                        { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                    var sfCF = new StringFormat
                        {Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Far};
                    var last = eew.IsLast ? "（最終）" : "";
                    var warn = eew.IsWarn ? " 警報" : "";
                    TextRenderer.DrawText(g, $"第{eew.Number}報{last}{warn}", font9, new Point(8, 10), Color.White);
                    var s = eew.MaxIntensity.ShortString.Replace("-", "").Replace("+", "");
                    g.DrawString("最大震度", font14, new SolidBrush(textColor), 8, 55);
                    if (eew.MaxIntensity.Equals(Intensity.Unknown))
                    {
                        g.DrawString("不明", font20, Brushes.Black, new Point(88, 37));
                    }
                    else if (eew.MaxIntensity.ShortString.Length == 2)
                    {
                        //g.FillRectangle(new SolidBrush(Color.FromArgb(130, Color.White)),82, 30, 35, 53);
                        g.DrawString(s, robotoI, new SolidBrush(textColor),
                            new Rectangle(87, 30, 35, 53),
                            sfCC);
                        var pmChar = eew.MaxIntensity.LongString.Last().ToString();
                        g.DrawString(pmChar, font20b, new SolidBrush(textColor),
                            new Rectangle(112, 32, 45, 53), sfCF);
                    }
                    else
                    {
                        g.DrawString(s, robotoI, new SolidBrush(textColor),
                            new Rectangle(82, 30, 80, 53),
                            sfCC);
                    }

                    TextRenderer.DrawText(g, "M", font20, new Point(153, 46), Color.White);
                    TextRenderer.DrawText(g, $"{eew.Magnitude:0.0}", font40, new Point(174, 17), Color.White);
                    TextRenderer.DrawText(g, $"{eew.OccurrenceTime:HH:mm:ss}発生", font9, new Rectangle(8, 10, 260, 15),
                        Color.White, TextFormatFlags.Right);
                    TextRenderer.DrawText(g, "震源地", font10, new Point(9, 85), Color.White);
                    TextRenderer.DrawText(g, eew.Epicenter,
                        eew.Epicenter.Length < 8 ? font16 : font12,
                        new Rectangle(9, 100, 170, 27), Color.White, TextFormatFlags.VerticalCenter);
                    TextRenderer.DrawText(g, "深さ", font10, new Point(182, 85), Color.White);
                    TextRenderer.DrawText(g, $"{eew.Depth}km", font16, new Point(180, 99), Color.White);
                }

                new List<Font> {font9, font10, font14, font16, font20, font40, font19b, font20b, roboto}.ForEach(x =>
                    x.Dispose());
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
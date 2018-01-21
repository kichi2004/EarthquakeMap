using System;

namespace AllInformationViewer2.Enums
{
    class Intensity
    {
        /// <summary>
        /// 震度不明
        /// </summary>
        internal static Intensity Unknown => new Intensity("不明", "震度不明", -1);
        /// <summary>
        /// 震度1未満
        /// </summary>
        internal static Intensity Int0 => new Intensity("0", "震度0", 0);
        /// <summary>
        /// 震度1
        /// </summary>
        internal static Intensity Int1 => new Intensity("1", "震度1", 1);
        /// <summary>
        /// 震度2
        /// </summary>
        internal static Intensity Int2 => new Intensity("2", "震度2", 2);
        /// <summary>
        /// 震度3
        /// </summary>
        internal static Intensity Int3 => new Intensity("3", "震度3", 3);
        /// <summary>
        /// 震度4
        /// </summary>
        internal static Intensity Int4 => new Intensity("4", "震度4", 4);
        /// <summary>
        /// 震度5弱
        /// </summary>
        internal static Intensity Int5Minus => new Intensity("5-", "震度5弱", 5);
        /// <summary>
        /// 震度5強
        /// </summary>
        internal static Intensity Int5Plus => new Intensity("5+", "震度5強", 6);
        /// <summary>
        /// 震度6弱
        /// </summary>
        internal static Intensity Int6Minus => new Intensity("6-", "震度6弱", 7);
        /// <summary>
        /// 震度6強
        /// </summary>
        internal static Intensity Int6Plus => new Intensity("6+", "震度6強", 8);
        /// <summary>
        /// 震度7
        /// </summary>
        internal static Intensity Int7 => new Intensity("7", "震度7", 9);

        private Intensity(string shorts, string longs, int ord)
        {
            EnumOrder = ord;
            ShortString = shorts;
            LongString = longs;
        }

        /// <summary>
        /// 震度の順番(震度0:0、震度7:9)
        /// </summary>
        internal int EnumOrder { get; }
        /// <summary>
        /// 短い文字列
        /// </summary>
        internal string ShortString { get; }
        /// <summary>
        /// 長い文字列
        /// </summary>
        internal string LongString { get; }

        /// <summary>
        /// 震度実測値を <seealso cref="Intensity"/> に変換します。
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        internal static Intensity FromValue(float value)
        {
            if (value < 0.5) return Int0;
            else if (value < 1.5) return Int1;
            else if (value < 2.5) return Int2;
            else if (value < 3.5) return Int3;
            else if (value < 4.5) return Int4;
            else if (value < 5.0) return Int5Minus;
            else if (value < 5.5) return Int5Plus;
            else if (value < 6.0) return Int6Minus;
            else if (value < 6.5) return Int6Plus;
            else return Int7;
        }

        /// <summary>
        /// 文字列の震度への変換を試みます。
        /// </summary>
        /// <param name="s">文字列</param>
        /// <param name="intensity">震度</param>
        /// <returns>成功したかどうか</returns>
        internal static bool TryParse(string s, out Intensity intensity)
        {
            switch (s) {
                case "1":
                case "１":
                    intensity = Int1;
                    return true;
                case "2":
                case "２":
                    intensity = Int2;
                    return true;
                case "3":
                case "３":
                    intensity = Int3;
                    return true;
                case "4":
                case "４":
                    intensity = Int4;
                    return true;
                case "5-":
                case "5弱":
                case "５弱":
                    intensity = Int5Minus;
                    return true;
                case "5+":
                case "5強":
                case "５強":
                    intensity = Int5Plus;
                    return true;
                case "6-":
                case "6弱":
                case "６弱":
                    intensity = Int6Minus;
                    return true;
                case "6+":
                case "6強":
                case "６強":
                    intensity = Int6Plus;
                    return true;
                case "7":
                case "７":
                    intensity = Int7;
                    return true;
                case "不明":
                    intensity = Unknown;
                    return true;
                default:
                    intensity = null;
                    return false;
            }
        }

        /// <summary>
        /// 文字列を震度に変換します。
        /// </summary>
        /// <param name="s">文字列</param>
        /// <exception cref="FormatException"/>
        /// <returns>変換された震度</returns>
        internal static Intensity Parse(string s)
        {
            switch (s) {
                case "1":
                case "１":
                    return Int1;
                case "2":
                case "２":
                    return Int2;
                case "3":
                case "３":
                    return Int3;
                case "4":
                case "４":
                    return Int4;
                case "5-":
                case "5弱":
                case "５弱":
                    return Int5Minus;
                case "5+":
                case "5強":
                case "５強":
                    return Int5Plus;
                case "6-":
                case "6弱":
                case "６弱":
                    return Int6Minus;
                case "6+":
                case "6強":
                case "６強":
                    return Int6Plus;
                case "7":
                case "７":
                    return Int7;
                case "不明":
                    return Unknown;
                default:
                    throw new FormatException();
            }
        }

        public static explicit operator Intensity(Int32 val)
        {
            switch (val) {
                case 0:
                    return Int0;
                case 1:
                    return Int1;
                case 2:
                    return Int2;
                case 3:
                    return Int3;
                case 4:
                    return Int4;
                case 5:
                    return Int5Minus;
                case 6:
                    return Int5Plus;
                case 7:
                    return Int6Plus;
                case 8:
                    return Int6Plus;
                case 9:
                    return Int7;
                default:
                    throw new ArgumentException(nameof(val));
            }
        }

        public static explicit operator Int32(Intensity intensity)
        {
            if (intensity == null) throw new ArgumentNullException("intensity");
            return intensity.EnumOrder;
        }
    }
}

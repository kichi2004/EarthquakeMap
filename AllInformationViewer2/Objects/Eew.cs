using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AllInformationViewer2.Enums;

namespace AllInformationViewer2.Objects
{
    class Eew
    { 
        /// <summary>
        /// 情報発表時刻
        /// </summary>
        internal DateTime AnnouncedTime { get; set; }
        /// <summary>
        /// 発生時刻
        /// </summary>
        internal DateTime OccurrenceTime { get; set; }
        /// <summary>
        /// 震源地
        /// </summary>
        internal string Epicenter { get; set; }
        /// <summary>
        /// 震央座標
        /// </summary>
        internal Coordinate Coordinate { get; set; }
        /// <summary>
        /// 震源の深さ
        /// </summary>
        internal int Depth { get; set; }
        /// <summary>
        /// 地震の規模
        /// </summary>
        internal float Magnitude { get; set; }
        /// <summary>
        /// 情報番号
        /// </summary>
        internal int Number { get; set; }
        /// <summary>
        /// 最終報か
        /// </summary>
        internal bool IsLast { get; set; }
        /// <summary>
        /// 警報を含む内容であるか
        /// </summary>
        internal bool IsWarn { get; set; }
        /// <summary>
        /// 最大推定震度
        /// </summary>
        internal Intensity MaxIntensity { get; set; }
    }
}

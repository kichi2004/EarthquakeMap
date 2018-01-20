using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AllInformationViewer2
{
    class IntensityComparer : IComparer<string>
    {
        private static readonly string[] IntList = { "1", "2", "3", "4", "震度５弱以上未入電", "5弱", "5強", "6弱", "6強", "7" };
        public int Compare(string x, string y) => Array.IndexOf(IntList, y) - Array.IndexOf(IntList, x);
    }
}

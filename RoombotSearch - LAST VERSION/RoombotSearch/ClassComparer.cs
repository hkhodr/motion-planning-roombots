using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RoombotSearch
{

    class ClassComparer : IComparer<int>
    {
        public int Compare(int x, int y)
        {
            if (x < y)
                return -1;
            else return 1;
        }
    }
}

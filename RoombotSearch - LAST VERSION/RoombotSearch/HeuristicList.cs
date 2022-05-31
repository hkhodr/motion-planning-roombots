using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RoombotSearch
{
    class HeuristicList
    {
        public SortedList<int, string> heur_list = new SortedList<int, string>();

        public HeuristicList(SortedList<int, string> list)
        {
            heur_list = new SortedList<int, string>(list);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RoombotSearch
{
    class ConnectionObject
    {
        public int connected_to {get; set;}
        public int voxel { get; set; }
        public int side { get; set; }

        public ConnectionObject(int c,int v,int s)
        {
            connected_to = c;
            voxel = v;
            side = s;

        }
    }
}

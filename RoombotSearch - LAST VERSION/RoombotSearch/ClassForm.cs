using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RoombotSearch
{
    class ClassForm
    {
        public int nx, ny, nz, RBnb;
        public bool[,,] inputOccupancyGrid, outputOccupancyGrid;
        public RoombotStatus[] RoombotStatusesInit, RoombotStatusesGoal;
        public bool[,] floor, ceiling, wall1, wall2, wall3, wall4;

        public ClassForm(int x,int y,int z,int rb,bool [,,] i,bool [,,]o,RoombotStatus[] Ri,RoombotStatus[] Ro,bool[,]f, bool[,] c, bool[,] w1, bool[,] w2, bool[,] w3, bool[,] w4)
        {
            nx = x;
            ny = y;
            nz = z;
            RBnb = rb;
            inputOccupancyGrid = i;
            outputOccupancyGrid = o;
            RoombotStatusesInit = Ri;
            RoombotStatusesGoal = Ro;
            floor=f;
            ceiling=c;
            wall1=w1;
            wall2=w2;
            wall3=w3;
            wall4=w4;

        }
    }
}

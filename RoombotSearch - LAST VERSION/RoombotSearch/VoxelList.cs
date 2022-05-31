using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RoombotSearch
{
    class VoxelList
    {
        public SortedList<string, int[]> voxel_list = new SortedList<string, int[]>();
        /*{ 0,0,0 },
{ -1,0,1},
{ 0,-1,1},
{ 1,0,1 },
{ 0,1,1 },
{ 0,0,2 } };*/
        public VoxelList()
        {
            voxel_list = new SortedList<string, int[]>();
            voxel_list.Add("000", new int[3] { 0, 0, -1 });
            voxel_list.Add("-101", new int[3] { -1, 0, 0 });
            voxel_list.Add("0-11", new int[3] { 0,-1,0});
            voxel_list.Add("101", new int[3] { 1, 0, 0 });
            voxel_list.Add("011", new int[3] { 0, 1, 0});
            voxel_list.Add("002", new int[3] { 0, 0, 1 });
        }
    }
}

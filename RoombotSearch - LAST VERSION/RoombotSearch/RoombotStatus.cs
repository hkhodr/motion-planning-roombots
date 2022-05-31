using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RoombotSearch
{
    class RoombotStatus
    {
        public int[][,] H_ACM;                // Homogeneous matrix for ACM
        public int[] MotorAngles;            // Motor angles saved here 
        public bool[] ToggleACM;                 // Open/Close ACM. 0 is 0 is closed and 1 is open, 1 is 1 is closed and 0 is open 
        public int[][] ACM_Voxel;                //the voxel occupied by acm0
        //public  int[] ACM1_Voxel;                // the voxel occupied by acm1
        public int Base;
        public int EndEffector;
        public bool can_move;
        public bool obstacle ;
        public bool MM;
        public bool[] connected_to_gnd;
        public List<ConnectionObject> connection_list= new List<ConnectionObject>();

        public RoombotStatus(int[][,] H, int[] motors, bool[] acm, int[][] vx, int b, int ee, bool m, bool mm, bool[] gnd)
        {
            H_ACM = new int[2][,];
            for (int i = 0; i < 2; i++)
                H_ACM[i] = (int[,])H[i].Clone();

            MotorAngles = (int[]) motors.Clone();
            ToggleACM=(bool[])acm.Clone();

            ACM_Voxel = new int[2][];
            for (int i=0;i<2;i++)
                ACM_Voxel[i] = (int[])vx[i].Clone();
            Base = b;
            EndEffector = ee;
            can_move = m;
            MM = mm;
            connected_to_gnd = (bool [])gnd.Clone();
            obstacle = false;

            connection_list.Add(new ConnectionObject(-1,Base,0));

        }
        public RoombotStatus(int[][,] H, int[] motors, bool[] acm, int[][] vx, int b, int ee, bool m, bool mm, bool[] gnd,List<ConnectionObject> c)
        {
            H_ACM = new int[2][,];
            for (int i = 0; i < 2; i++)
                H_ACM[i] = (int[,])H[i].Clone();

            MotorAngles = (int[])motors.Clone();
            ToggleACM = (bool[])acm.Clone();

            ACM_Voxel = new int[2][];
            for (int i = 0; i < 2; i++)
                ACM_Voxel[i] = (int[])vx[i].Clone();
            Base = b;
            EndEffector = ee;
            can_move = m;
            MM = mm;
            connected_to_gnd = (bool[])gnd.Clone();
            connection_list = new List<ConnectionObject>(c);
            obstacle = false;

        }
        public RoombotStatus(RoombotStatus RBToBecopied)
        {
            H_ACM = new int[2][,];
            for (int i=0;i<2;i++)
                H_ACM[i] = (int[,])RBToBecopied.H_ACM[i].Clone();

            MotorAngles = (int[])RBToBecopied.MotorAngles.Clone();
            ToggleACM = (bool [])RBToBecopied.ToggleACM.Clone();
            ACM_Voxel = new int[2][];
            for (int i =0; i< 2;i++)
                ACM_Voxel[i] = (int[])RBToBecopied.ACM_Voxel[i].Clone();

            Base = RBToBecopied.Base;
            EndEffector = RBToBecopied.EndEffector;
            can_move = RBToBecopied.can_move;
            MM = RBToBecopied.MM;
            connected_to_gnd = (bool[]) RBToBecopied.connected_to_gnd.Clone();
            connection_list = new List<ConnectionObject>(RBToBecopied.connection_list);
            obstacle = RBToBecopied.obstacle;
        }
    }
}

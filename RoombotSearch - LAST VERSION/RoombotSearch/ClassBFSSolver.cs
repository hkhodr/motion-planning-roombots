using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoombotSearch
{
    class ClassBFSSolver
    {
        public ClassPuzzle P;
        ClassNode N;
        List<ClassNode> Ns;

        ConcurrentQueue<ClassNode> Q = new ConcurrentQueue<ClassNode>();

        SortedList<string, ClassNode> Hist = new SortedList<string, ClassNode>();

        List<ClassNode> Sol = new List<ClassNode>();
        int MaxQ = 0;           // Nodes Stored
        int ExpCnt = 0;
        int SolTime = 0;
        DateTime ClkSt = new DateTime();
        DateTime ClkEnd = new DateTime();
        INCLUDE_OR with_m1 = INCLUDE_OR.WITHOUT_M1;
        public bool abort = false;
        public ClassBFSSolver(INCLUDE_OR m, ClassPuzzle PuzzleToBeSolved)
        {
            P = PuzzleToBeSolved;
            with_m1 = m;
        }

        public ClassPuzzle SolveIt()
        {
            ClkSt = DateTime.Now;
            Q.Enqueue(P.InitNode);
            MaxQ = 1;
            Hist.Add(_KeyBuild(with_m1,P.InitNode), P.InitNode);

            while (!P.b_abort && Q.Count > 0 && ! abort)
            {
                bool dequeue_success = Q.TryDequeue(out N);
                int count_dequeue = 0;
                while (!dequeue_success)
                {
                    count_dequeue++;
                    dequeue_success = Q.TryDequeue(out N);
                    Console.WriteLine("Dequeue notsuccessful ");
                    if(count_dequeue>10)
                        break;
                }
                ExpCnt++;
                if (_AreNodesSame(N, P.GoalNode))
                {
                    P.AStarManSolved = true;
                    // N is Solution... Search in Hist for it parents till the very beginnig
                    break;
                }
                Ns = N.GetSuccessors();
                object sync = new Object();
                Parallel.ForEach(Ns, SN =>//{//ClassNode SN in Ns)
                //foreach (ClassNode SN in Ns)
                {
                    string key = _KeyBuild(with_m1, SN);
                    bool KeyInHist = true;      // For Exeption handling when stopping the puzzle solving
                    lock (sync)
                    {
                        try
                        { KeyInHist = Hist.ContainsKey(key); }
                        catch
                        { }

                        if (KeyInHist)  // If state already found in hist list, do not do anything 
                        { }
                        else         // if SN not in Hist List add only successors that are not opened yet
                        {
                        try
                        {
                            Hist.Add(key, SN);
                            Q.Enqueue(SN);
                            if (Q.Count > MaxQ)
                                MaxQ = Q.Count;
                        }
                        catch { Console.WriteLine("already in hist "); }// Adding to the history"); }

                        }
                    }

                });
                ClkEnd = DateTime.Now;
                SolTime = (ClkEnd.Millisecond - ClkSt.Millisecond) + (ClkEnd.Second - ClkSt.Second) * 1000 + (ClkEnd.Minute - ClkSt.Minute) * 60 * 1000 + (ClkEnd.Hour - ClkSt.Hour) * 60 * 60 * 1000 + (ClkEnd.DayOfYear - ClkSt.DayOfYear) * 24 * 60 * 60 * 1000; // DO NOT USE THIS PROGRAM AT NEW YEAR PARTY!!!
                if (SolTime > 3600000 / 2)
                {
                    P.b_abort = true;
                    abort = true;
                }
            }

            ClassNode DN = N;
            while (DN != null)
            {
                Sol.Add(DN);
                DN = DN.GetParent();
            }

            Sol.Reverse();

            ClkEnd = DateTime.Now;
            SolTime = (ClkEnd.Millisecond - ClkSt.Millisecond) + (ClkEnd.Second - ClkSt.Second) * 1000 + (ClkEnd.Minute - ClkSt.Minute) * 60 * 1000 + (ClkEnd.Hour - ClkSt.Hour) * 60 * 60 * 1000 + (ClkEnd.DayOfYear - ClkSt.DayOfYear) * 24 * 60 * 60 * 1000; // DO NOT USE THIS PROGRAM AT NEW YEAR PARTY!!!
            P.AStarManSolTime = SolTime;     // in milli sec
            P.AStarManNumStoNodes = MaxQ;
            P.AStarManNumExpNodes = ExpCnt;
            P.AStarManSolStep = Sol.Count - 1;          // It is derived from History List
            P.AStarManSolSteps = Sol;



            return P;
        }

        private bool _AreNodesSame(ClassNode N1, ClassNode N2)      // Checks whether sent 2 nodes have the same state or not
        {
            /*for (int i = 0; i < N1.ModulesNb; i++)
            {
                if (N1.RoombotModules[i].ACM_Voxel[0][2] != 3 && N1.RoombotModules[i].ACM_Voxel[1][2] != 3)
                    return false;
                else if (N1.RoombotModules[i].ACM_Voxel[0][2] == 3 && N1.RoombotModules[i].ACM_Voxel[1][2] == 3)
                    return false;


            }
            return true;
            */
            for (int i = 0; i < N1.NodeSizeZ; i++)
                for (int j = 0; j < N1.NodeSizeY; j++)
                    for (int k = 0; k < N1.NodeSizeX; k++)
                        if (N1.OccupancyGrid[i, j, k] != N2.OccupancyGrid[i, j, k])
                            return false;
            return true;
        }


        private string _KeyBuild(INCLUDE_OR is_with_m1, ClassNode Nk)
        {
            string s = "";
            SortedList<string, int> sortedRBs = new SortedList<string, int>();
            for (int i = 0; i < Nk.ModulesNb; i++)
            {
                string key = Nk.RoombotModules[i].ACM_Voxel[0][0].ToString() + Nk.RoombotModules[i].ACM_Voxel[0][1].ToString() + Nk.RoombotModules[i].ACM_Voxel[0][2].ToString();
                sortedRBs.Add(key, i);
                //Console.WriteLine(key + i.ToString());
            }

            foreach (int i in sortedRBs.Values)
            {

                //Console.WriteLine(i.ToString());

                s = s + Nk.RoombotModules[i].H_ACM[0][0, 3].ToString() + ",";
                s = s + Nk.RoombotModules[i].H_ACM[0][1, 3].ToString() + ",";
                s = s + Nk.RoombotModules[i].H_ACM[0][2, 3].ToString() + ",";

                s = s + Nk.RoombotModules[i].H_ACM[1][0, 3].ToString() + ",";
                s = s + Nk.RoombotModules[i].H_ACM[1][1, 3].ToString() + ",";
                s = s + Nk.RoombotModules[i].H_ACM[1][2, 3].ToString() + ",";



                s = s + Nk.RoombotModules[i].ToggleACM[0].ToString() + ",";
                s = s + Nk.RoombotModules[i].ToggleACM[1].ToString() + ",";

                //Include orientation if required..
                if (is_with_m1 == INCLUDE_OR.WITH_M1)
                {
                    s = s + Nk.RoombotModules[i].MotorAngles[1].ToString() + ",";
                    for (int ii = 0; ii < 3; ii++)
                        for (int j = 0; j < 3; j++)
                            s = s + Nk.RoombotModules[i].H_ACM[Nk.RoombotModules[i].Base][ii, j].ToString() + ",";
                }

            }
            //Console.WriteLine(s);
            return s;
        }



    }
}

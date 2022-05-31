using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RoombotSearch
{
    class ClassIDFSSolver
    {
        public ClassPuzzle P;
        ClassNode N;
        List<ClassNode> Ns;
        Stack<ClassNode> S = new Stack<ClassNode>();
        SortedList<string, ClassNode> Hist = new SortedList<string, ClassNode>();
        List<ClassNode> Sol = new List<ClassNode>();
        int MaxHist = 0;           // Nodes Stored
        int ExpCnt = 0;
        int SolTime = 0;
        DateTime ClkSt = new DateTime();
        DateTime ClkEnd = new DateTime();
        INCLUDE_OR with_m1;




        public ClassIDFSSolver(INCLUDE_OR wm,ClassPuzzle PuzzleToBeSolved)
        {
            P = PuzzleToBeSolved;
            with_m1 = wm;
        }

        public ClassPuzzle SolveIt()
        {
            ClkSt = DateTime.Now;
            ExpCnt = 0;

            N = P.InitNode;
            ClassNode DNd;

            for (int i = 1; i < 250000; i++)
            {
                Hist.Clear();
                Hist.Add(_KeyBuild(with_m1,P.InitNode), P.InitNode);
                //Console.WriteLine("Iteration: {0}", i.ToString());
                DNd = DepthLimitedDFS(P.InitNode, i);
                if (DNd != null)
                {
                    N = DNd;
                    break;
                }
            }

            ClassNode DN = N;
            while (DN != null)
            {
                Sol.Add(DN);
                DN = DN.GetParent();
            }

            Sol.Reverse();

            // THEY NEED TO BE CHECKED...
            ClkEnd = DateTime.Now;
            SolTime = (ClkEnd.Millisecond - ClkSt.Millisecond) + (ClkEnd.Second - ClkSt.Second) * 1000 + (ClkEnd.Minute - ClkSt.Minute) * 60 * 1000 + (ClkEnd.Hour - ClkSt.Hour) * 60 * 60 * 1000 + (ClkEnd.DayOfYear - ClkSt.DayOfYear) * 24 * 60 * 60 * 1000; // DO NOT USE THIS PROGRAM AT NEW YEAR PARTY!!!
            P.AStarManSolTime = SolTime;     // in milli sec
            P.AStarManNumStoNodes = MaxHist;        // Although the History is not used....
            P.AStarManNumExpNodes = ExpCnt;
            P.AStarManSolStep = Sol.Count - 1;          // It is derived from History List
            P.AStarManSolSteps = Sol;

            return P;
        }



        ClassNode DepthLimitedDFS(ClassNode Nd, int limit)      // Hist olmadan bakınca hızlı bakıyor ama stateler inanılmaz fazla oluyor...
        {
            ExpCnt++;
            int lim = limit;
            if (_AreNodesSame(Nd, P.GoalNode))
            {
                P.AStarManSolved = true;
                return Nd;
            }
            else if (lim == 0)
            {
                return null;
            }
            lim--;


            List<ClassNode> Nsuc = Nd.GetSuccessors();
            foreach (ClassNode CNd in Nsuc)
            {
                ClassNode DumN = DepthLimitedDFS(CNd, lim);

                if (DumN != null)
                {
                    return DumN;
                }

            }
            return null;        // Should not reach here ???
        }

        private bool _AreNodesSame(ClassNode N1, ClassNode N2)      // Checks whether sent 2 nodes have the same state or not
        {
            /*for (int i = 0; i < N1.ModulesNb; i++)
                if (N1.RoombotModules[i].ACM_Voxel[0][0] > 3 && N1.RoombotModules[i].ACM_Voxel[1][0] > 3)
                    return false;
            return true;
            */
            for (int i = 0; i < N1.ModulesNb; i++)
            {
                if (N1.RoombotModules[i].ACM_Voxel[0][2] != 3 && N1.RoombotModules[i].ACM_Voxel[1][2] != 3)
                    return false;
                else if (N1.RoombotModules[i].ACM_Voxel[0][2] == 3 && N1.RoombotModules[i].ACM_Voxel[1][2] == 3)
                    return false;


            }
            return true;

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
            for (int i = 0; i < Nk.ModulesNb; i++)
            {
                for (int ii = 0; ii < 3; ii++)
                    for (int j = 0; j < 3; j++)
                        s = s + Nk.RoombotModules[i].H_ACM[0][ii, j].ToString() + ",";
                s = s + Nk.RoombotModules[i].H_ACM[0][0, 3].ToString() + ",";
                s = s + Nk.RoombotModules[i].H_ACM[0][1, 3].ToString() + ",";
                s = s + Nk.RoombotModules[i].H_ACM[0][2, 3].ToString() + ",";

                s = s + Nk.RoombotModules[i].H_ACM[1][0, 3].ToString() + ",";
                s = s + Nk.RoombotModules[i].H_ACM[1][1, 3].ToString() + ",";
                s = s + Nk.RoombotModules[i].H_ACM[1][2, 3].ToString() + ",";



                s = s + Nk.RoombotModules[i].ToggleACM[0].ToString() + ",";
                s = s + Nk.RoombotModules[i].ToggleACM[1].ToString() + ",";

                if (is_with_m1 == INCLUDE_OR.WITH_M1)
                    s = s + Nk.RoombotModules[i].MotorAngles[1].ToString() + ","; // only motor 1 can have no effect on changing the position of ACM


            }
            //Console.WriteLine(s);
            return s;
        }


    }
    }

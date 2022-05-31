using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoombotSearch
{

    class ClassAStarNonAdmissibleSolver
    {

        public ClassPuzzle P;
        ClassNode N;
        List<ClassNode> Ns;
        SortedList<string, ClassNode> Hist = new SortedList<string, ClassNode>();
        List<ClassNode> Sol = new List<ClassNode>();
        int MaxL = 0;               // Nodes Stored
        int ExpCnt = 0;             // Nodes Processed (expanded)
        int SolTime = 0;
        DateTime ClkSt = new DateTime();
        DateTime ClkEnd = new DateTime();

        INCLUDE_OR with_m1 = INCLUDE_OR.WITHOUT_M1;
        bool CostInHistLarger = false;  // For Exeption handling when stopping the puzzle solving
        bool LgotLarger = false;
        bool KeyInHist = true;
        public bool abort = false;
        private static readonly int[,,] heuristic = new Heuristic_Table().heuristic;

        public ClassAStarNonAdmissibleSolver(INCLUDE_OR m1, ClassPuzzle PuzzleToBeSolved)
        {
            P = PuzzleToBeSolved;
            with_m1 = m1;
        }

        public ClassPuzzle SolveIt()
        {
            ClassComparer Comp = new ClassComparer();
            SortedList<int, ClassNode> L = new SortedList<int, ClassNode>(Comp);

            ClkSt = DateTime.Now;
            ExpCnt = 0;

            N = P.InitNode;
            N.Cost = 0;
            N.Heur = _CalcHeurMan(N);
            L.Add(N.Heur + N.Cost, N);
            Hist.Add(_KeyBuild(with_m1, N), N);
            int loopcount = 0;
            while (!P.b_abort && L.Count > 0 && !abort)
            {
                loopcount++;
                N = L.First().Value;
                L.RemoveAt(0);
                ExpCnt++;           // Node is about to be processed
                if (_AreNodesSame(N, P.GoalNode))
                {
                    P.AStarManSolved = true;
                    break;
                }
                //Console.WriteLine("Node now, 0,"+loopcount.ToString()+" ,"+N._StateInString(N));

                Ns = N.GetSuccessors();
                object sync = new Object();
                int counter = 0;
                //foreach(ClassNode SN in Ns)
                Parallel.ForEach(Ns, SN =>//{//ClassNode SN in Ns)
                {

                    counter++;
                    SN.Heur = _CalcHeurMan(SN);
                    //Console.WriteLine("Node now, " +loopcount.ToString()+","+ counter.ToString()+","+ SN._StateInString(SN));
                    //Console.WriteLine("Successor " + counter.ToString()+" , " + SN.Heur.ToString()+" : " +SN._StateInString(SN));

                    string key = _KeyBuild(with_m1, SN);
                    lock (sync)
                    {
                        try { KeyInHist = Hist.ContainsKey(key); }      // For Exeption handling when stopping the puzzle solving
                        catch { }

                        //Console.WriteLine("Successor hist key " + KeyInHist.ToString());
                        if (KeyInHist)
                        {
                            try { CostInHistLarger = (Hist[key].Cost > SN.Cost); }     // For Exeption handling when stopping the puzzle solving
                            catch { }
                            if (CostInHistLarger)
                            {

                                lock (sync)
                                {
                                    Hist[key].Cost = SN.Cost;
                                    L.Add(SN.Heur + SN.Cost, SN);
                                }


                                try { LgotLarger = (L.Count > MaxL); }      // For Exeption handling when stopping the puzzle solving
                                catch { }
                                if (LgotLarger)
                                    MaxL = L.Count;
                            }
                        }
                        else         // Then add only successors that are not opened yet
                        {
                            //Console.WriteLine("Successor not opened " + counter.ToString());
                            lock (sync)
                            {
                                L.Add(SN.Heur + SN.Cost, SN);
                                Hist.Add(key, SN);
                            }

                            try { LgotLarger = (L.Count > MaxL); }      // For Exeption handling when stopping the puzzle solving
                            catch { }
                            if (LgotLarger)
                                MaxL = L.Count;
                        }
                    }
                });
                ClkEnd = DateTime.Now;
                SolTime = (ClkEnd.Millisecond - ClkSt.Millisecond) + (ClkEnd.Second - ClkSt.Second) * 1000 + (ClkEnd.Minute - ClkSt.Minute) * 60 * 1000 + (ClkEnd.Hour - ClkSt.Hour) * 60 * 60 * 1000 + (ClkEnd.DayOfYear - ClkSt.DayOfYear) * 24 * 60 * 60 * 1000; // DO NOT USE THIS PROGRAM AT NEW YEAR PARTY!!!
                if (SolTime > 360000)
                    P.b_abort = true;

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
            P.AStarManNumStoNodes = MaxL;
            P.AStarManNumExpNodes = ExpCnt;
            P.AStarManSolStep = Sol.Count - 1;          // It is derived from History List
            P.AStarManSolSteps = Sol;

            return P;
        }


        private bool _AreNodesSame(ClassNode N1, ClassNode N2)      // Checks whether sent 2 nodes have the same state or not
        {
             for (int i = 0; i < N1.NodeSizeZ; i++)
                 for (int j = 0; j < N1.NodeSizeY; j++)
                     for (int k = 0; k < N1.NodeSizeX; k++)
                         if (N1.OccupancyGrid[i, j, k] != N2.OccupancyGrid[i, j, k])
                             return false;
             return true;
           /* for (int i = 0; i < N1.ModulesNb; i++)
            {
                if (N1.RoombotModules[i].ACM_Voxel[0][0] < 4)
                    return false;
                else if (N1.RoombotModules[i].ACM_Voxel[0][1] < 4)
                    return false;
                else if (N1.RoombotModules[i].ACM_Voxel[0][2] < 4)
                    return false;  
                //Console.WriteLine(key + i.ToString());
            }
            return true;*/
        }

        private int _CalcHeurMan(ClassNode Nh)
        {
            int He = 0;
            SortedList<string, int> nodes_visited = new SortedList<string, int>();
            HeuristicList[] heuristic_list = new HeuristicList[Nh.ModulesNb * 2];
            ClassComparer Comp = new ClassComparer();
            SortedList<int, string> heurist_list = new SortedList<int, string>(Comp);

            int x = 0, y = 0, z = 0, l = -1;
            for (int i = 0; i < Nh.NodeSizeZ; i++)
                for (int j = 0; j < Nh.NodeSizeY; j++)
                {
                    for (int k = 0; k < Nh.NodeSizeX; k++)
                    {
                        if (Nh.OccupancyGrid[i, j, k])
                        {

                            l++;
                            nodes_visited.Add(_KeyBuildforHeur(k, j, i), l);
                            for (int ii = 0; ii < Nh.NodeSizeZ; ii++)
                                for (int jj = 0; jj < Nh.NodeSizeY; jj++)
                                {
                                    for (int kk = 0; kk < Nh.NodeSizeX; kk++)
                                    {
                                        if (P.GoalNode.OccupancyGrid[ii, jj, kk])// && (!heur_list.ContainsKey(_KeyBuildforHeur(kk, jj, ii))))
                                        {
                                            heurist_list.Add(Math.Max(Math.Max(Math.Abs(ii - i), Math.Abs(jj - j)), Math.Abs(kk - k)), _KeyBuildforHeur(kk, jj, ii));
                                        }
                                    }

                                }
                            heuristic_list[l] = new HeuristicList(heurist_list);
                            heurist_list.Clear();

                        }
                    }
                }
            string corresp_vx = "";
            int heur_now;
            Tuple<int, string, int> heur_old;
            SortedList<string, Tuple<int, string, int>> sorted_vx_goal = new SortedList<string, Tuple<int, string, int>>();
            bool next;
            while (nodes_visited.Count != 0)
            {
                string vx_now = nodes_visited.First().Key;
                l = nodes_visited.First().Value;
                nodes_visited.RemoveAt(0);
                next = false;
                while (!next)
                {
                    corresp_vx = heuristic_list[l].heur_list.First().Value;
                    heur_now = heuristic_list[l].heur_list.First().Key;
                    heuristic_list[l].heur_list.RemoveAt(0);

                    if (!sorted_vx_goal.ContainsKey(corresp_vx))
                    {
                        sorted_vx_goal.Add(corresp_vx, Tuple.Create(heur_now, vx_now, l));
                        next = true;
                    }
                    else
                    {
                        sorted_vx_goal.TryGetValue(corresp_vx, out heur_old);
                        if (heur_old.Item1 > heur_now)
                        {
                            sorted_vx_goal[corresp_vx] = Tuple.Create(heur_now, vx_now, l);
                            nodes_visited.Add(heur_old.Item2, heur_old.Item3);
                            next = true;
                        }
                    }
                }

            }
            int max_heu = 0;

            for (int i = 0; i < sorted_vx_goal.Count; i++)
            {
                int new_h = (sorted_vx_goal.ElementAt(i).Value.Item1);
                He = He + new_h;
                if (max_heu < new_h)
                    max_heu = new_h;
            }

            if (max_heu != 0)
                He = He + (max_heu - 1); //acm open and close. 

            return He;
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

                s = s + Nk.RoombotModules[i].H_ACM[0][0, 3].ToString() + ",";
                s = s + Nk.RoombotModules[i].H_ACM[0][1, 3].ToString() + ",";
                s = s + Nk.RoombotModules[i].H_ACM[0][2, 3].ToString() + ",";

                s = s + Nk.RoombotModules[i].H_ACM[1][0, 3].ToString() + ",";
                s = s + Nk.RoombotModules[i].H_ACM[1][1, 3].ToString() + ",";
                s = s + Nk.RoombotModules[i].H_ACM[1][2, 3].ToString() + ",";

                s = s + Nk.RoombotModules[i].ToggleACM[0].ToString() + ",";
                s = s + Nk.RoombotModules[i].ToggleACM[1].ToString() + ",";

                if (is_with_m1 == INCLUDE_OR.WITH_M1)
                {
                    s = s + Nk.RoombotModules[i].MotorAngles[1].ToString() + ","; // only motor 1 can have no effect on changing the position of ACM
                    for (int ii = 0; ii < 3; ii++)
                        for (int j = 0; j < 3; j++)
                            s = s + Nk.RoombotModules[i].H_ACM[Nk.RoombotModules[i].Base][ii, j].ToString() + ",";
                }

            }
            //Console.WriteLine(s);
            return s;
        }

        private string _KeyBuildforHeur(int i, int j, int k)
        {
            string s = "";
            s = s + i.ToString();
            s = s + j.ToString();
            s = s + k.ToString();
            return s;

        }


    }
}

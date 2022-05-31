using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RoombotSearch
{
    class ClassBidirectionalSearchSolver_2
    {

        public ClassPuzzle P_fwd;
        public ClassPuzzle P_bwd;
        private ClassNode N_fwd;
        private ClassNode N_bwd;
        //List<ClassNode> Ns;
        private SortedList<string, ClassNode> Hist_fwd = new SortedList<string, ClassNode>();
        private SortedList<string, ClassNode> Hist_bwd = new SortedList<string, ClassNode>();

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
        public bool solved = false;
        Direction direction;
        private static readonly int[,,] heuristic = new Heuristic_Table().heuristic;

        public ClassBidirectionalSearchSolver_2(INCLUDE_OR m1, ClassPuzzle PuzzleToBeSolved)
        {
            P_fwd = PuzzleToBeSolved;
            P_bwd = new ClassPuzzle(true, PuzzleToBeSolved);
            with_m1 = m1;

        }

        public ClassPuzzle SolveIt()
        {
            ClkSt = DateTime.Now;
            ExpCnt = 0;

            Thread t_fwd = new Thread(() => DoSolve(true, P_fwd));
            Thread t_bwd = new Thread(() => DoSolve(false, P_bwd));
            t_fwd.Start(); t_bwd.Start();
            Random rnd = new Random();
            for (int i=0;i<P_bwd.InitNode.ModulesNb;i++)
            {
                    int or = rnd.Next(1, 3);
                    ClassPuzzle NewPuz = new ClassPuzzle(false, P_bwd);
                    NewPuz.InitNode.RoombotModules[i].H_ACM = (int[][,])to_H(or, NewPuz.InitNode.RoombotModules[i].H_ACM).Clone();
                    Thread myThread = new Thread(() => DoSolve(false, NewPuz));
                    myThread.Start();
            }

            t_fwd.Join(); t_bwd.Join();

            Console.WriteLine("both threads out");

            ClassNode DN1;
            ClassNode DN2;
            if (direction == Direction.FWD)
            {
                DN1 = N_fwd;
                DN2 = N_bwd;
            }
            else
            {
                DN1 = N_bwd;
                DN2 = N_fwd;
            }

            while (DN1 != null)
            {
                Sol.Add(DN1);
                DN1 = DN1.GetParent();
                Console.WriteLine("DN1");
            }

            Sol.Reverse();
            List<ClassNode> Sol2 = new List<ClassNode>();
            while (DN2 != null)
            {
                Sol2.Add(DN2);
                DN2 = DN2.GetParent();
                Console.WriteLine("DN2");
            }
            //Sol2.Reverse();
            Sol.AddRange(Sol2);
            if (P_fwd.AStarManSolved || P_bwd.AStarManSolved)
                P_fwd.AStarManSolved = true;
            P_fwd.AStarManSolved = solved;
            // THEY NEED TO BE CHECKED...
            ClkEnd = DateTime.Now;
            SolTime = (ClkEnd.Millisecond - ClkSt.Millisecond) + (ClkEnd.Second - ClkSt.Second) * 1000 + (ClkEnd.Minute - ClkSt.Minute) * 60 * 1000 + (ClkEnd.Hour - ClkSt.Hour) * 60 * 60 * 1000 + (ClkEnd.DayOfYear - ClkSt.DayOfYear) * 24 * 60 * 60 * 1000; // DO NOT USE THIS PROGRAM AT NEW YEAR PARTY!!!
            P_fwd.AStarManSolTime = SolTime;     // in milli sec
            P_fwd.AStarManNumStoNodes = MaxL;
            P_fwd.AStarManNumExpNodes = ExpCnt;
            P_fwd.AStarManSolStep = Sol.Count - 1;          // It is derived from History List
            P_fwd.AStarManSolSteps = Sol;

            return P_fwd;
        }

        private static int[][,] to_H(int or,int[][,] input)
        {
            int[][,] H = new int[2][,];
            switch (or)
            {
                case 0:
                    H[0] = new int[4, 4]{
                            {1,0,0,0},
                            {0,1,0,0},
                            {0,0,1,0},
                            {0,0,0,1}};

                    H[1] = new int[4, 4]{
                            {0,1,0,0},
                            {1,0,0,0},
                            {0,0,-1,0},
                            {0,0,0,1}};
                    break;
                case 1:
                    H[0] = new int[4, 4]{
                            {0,1,0,0},
                            {-1,0,0,0},
                            {0,0,1,0},
                            {0,0,0,1}};

                    H[1] = new int[4, 4]{
                            {1,0,0,0},
                            {0,-1,0,0},
                            {0,0,-1,0},
                            {0,0,0,1}};
                    break;
                case 2:
                    H[0] = new int[4, 4]{
                            {-1,0,0,0},
                            {0,-1,0,0},
                            {0,0,1,0},
                            {0,0,0,1}};

                    H[1] = new int[4, 4]{
                            {0,-1,0,0},
                            {-1,0,0,0},
                            {0,0,-1,0},
                            {0,0,0,1}};
                    break;
                case 3:
                    H[0] = new int[4, 4]{
                            {0,-1,0,0},
                            {1,0,0,0},
                            {0,0,1,0},
                            {0,0,0,1}};

                    H[1] = new int[4, 4]{
                            {-1,0,0,0},
                            {0,1,0,0},
                            {0,0,-1,0},
                            {0,0,0,1}};
                    break;
            }
            H[0][0, 3] = input[0][0, 3];
            H[0][1, 3] = input[0][1, 3];
            H[0][2, 3] = input[0][2, 3];

            H[1][0, 3] = input[1][0, 3];
            H[1][1, 3] = input[1][1, 3];
            H[1][2, 3] = input[1][2, 3];

            return H;
        }
        
        private void DoSolve(bool fwd, ClassPuzzle P)
        {

            ClassNode N;
            ClassComparer Comp = new ClassComparer();
            SortedList<int, ClassNode> L = new SortedList<int, ClassNode>(Comp);
            SortedList<string, ClassNode> Histf = new SortedList<string, ClassNode>();
            SortedList<string, ClassNode> Histb = new SortedList<string, ClassNode>();
            Direction dir;
            List<ClassNode> Ns;

            object _sync = new object();
            if (fwd)
            {

                Histf = Hist_fwd;
                Histb = Hist_bwd;
                dir = Direction.FWD;
            }

            else
            {
                Histf = Hist_bwd;
                Histb = Hist_fwd;
                dir = Direction.BWD;
            }

            Console.WriteLine("Thread for " + dir.ToString());
            N = P.InitNode;
            N.Cost = 0;
            N.Heur = _CalcHeurMan(dir, N, P);
            L.Add(N.Heur + N.Cost, N);

            lock (_sync)
            {
                try
                {
                    Histf.Add(_KeyBuild(with_m1, N), N);
                }
                catch { }

                
            }

            int loopcount = 0;
            while (!P.b_abort && L.Count > 0 && !abort)
            {
                try
                {
                    loopcount++;
                    N = L.First().Value;
                    L.RemoveAt(0);
                    ExpCnt++;           // Node is about to be processed

                    if (_AreNodesSame(N, P.GoalNode))
                    {
                        N_fwd = N;
                        P.AStarManSolved = true;
                        abort = true;
                        solved = true;
                        break;
                    }



                    //Console.WriteLine("Node now, 0,"+loopcount.ToString()+" ,"+N._StateInString(N));
                    Ns = N.GetSuccessors();
                    int counter = 0;
                    Parallel.ForEach(Ns, SN =>//foreach (ClassNode SN in Ns)
                    {
                        counter++;
                        SN.Heur = _CalcHeurMan(dir, SN, P);
                        //Console.WriteLine("Node now, " +loopcount.ToString()+","+ counter.ToString()+","+ SN._StateInString(SN));
                        //Console.WriteLine("Successor " + counter.ToString()+" , " + SN.Heur.ToString()+" : " +SN._StateInString(SN));

                        string key = _KeyBuild(with_m1, SN);
                        lock (_sync)
                        {
                            try
                            {
                                if (Histb.ContainsKey(key))
                                {

                                    Histb.TryGetValue(key, out N_bwd);
                                    Console.WriteLine("key: " + key + "  " + N_bwd._StateInString(N_bwd));
                                    Histb.TryGetValue(key, out N_bwd);
                                    Console.WriteLine("key: " + key + "  " + N_bwd._StateInString(N_bwd));

                                    N_fwd = N;
                                    direction = dir;
                                    P.AStarManSolved = true;
                                    Console.WriteLine("key: " + key + "  " + N._StateInString(N));
                                    Console.WriteLine("key: " + key + "  " + N_bwd._StateInString(N_bwd));
                                    Console.WriteLine("solve first by" + direction);
                                    abort = true;
                                    solved = true;
                                    //break;
                                }
                            }
                            catch { }
                        }

                        try { KeyInHist = Histf.ContainsKey(key); }      // For Exeption handling when stopping the puzzle solving
                        catch { }
                        //Console.WriteLine("Successor hist key " + KeyInHist.ToString());
                        if (KeyInHist)
                        {
                            try { CostInHistLarger = (Histf[key].Cost > SN.Cost); }     // For Exeption handling when stopping the puzzle solving
                            catch { }
                            if (CostInHistLarger)
                            {
                                Histf[key].Cost = SN.Cost;
                                L.Add(SN.Heur + SN.Cost, SN);

                                try { LgotLarger = (L.Count > MaxL); }      // For Exeption handling when stopping the puzzle solving
                                catch { }
                                if (LgotLarger)
                                    MaxL = L.Count;
                            }
                        }
                        else         // Then add only successors that are not opened yet
                        {
                            //Console.WriteLine("Successor not opened " + counter.ToString());

                            lock (_sync)
                            {
                                try
                                {
                                    Histf.Add(key, SN);
                                    L.Add(SN.Heur + SN.Cost, SN);
                                }
                                catch
                                {
                                    Console.WriteLine("Excpetion in adding to Histf (already existing key)");
                                }

                            }

                            try { LgotLarger = (L.Count > MaxL); }      // For Exeption handling when stopping the puzzle solving
                            catch { }
                            if (LgotLarger)
                                MaxL = L.Count;
                        }
                    });
                }
                catch { }

                ClkEnd = DateTime.Now;
                SolTime = (ClkEnd.Millisecond - ClkSt.Millisecond) + (ClkEnd.Second - ClkSt.Second) * 1000 + (ClkEnd.Minute - ClkSt.Minute) * 60 * 1000 + (ClkEnd.Hour - ClkSt.Hour) * 60 * 60 * 1000 + (ClkEnd.DayOfYear - ClkSt.DayOfYear) * 24 * 60 * 60 * 1000; // DO NOT USE THIS PROGRAM AT NEW YEAR PARTY!!!
                if (SolTime > 3600000 / 2)
                {
                    P.b_abort = true;
                    abort = true;
                }
                if (L.Count == 0 && fwd)
                    abort = true;
            }

            /*if (abort && !solved)
            {
                if (dir == Direction.BWD)
                    N_bwd = N;
                else
                    N_fwd = N;
            }*/

        }

        private bool _AreNodesSame(ClassNode N1, ClassNode N2)      // Checks whether sent 2 nodes have the same state or not
        {
            for (int i = 0; i < N1.NodeSizeZ; i++)
                for (int j = 0; j < N1.NodeSizeY; j++)
                    for (int k = 0; k < N1.NodeSizeX; k++)
                        if (N1.OccupancyGrid[i, j, k] != N2.OccupancyGrid[i, j, k])
                            return false;
            return true;
        }

        private int _CalcHeurMan(Direction dir, ClassNode Nh, ClassPuzzle P)
        {
            //return 1;
            int He = 0;
            SortedList<string, int> nodes_visited = new SortedList<string, int>();
            HeuristicList[] heuristic_list = new HeuristicList[Nh.ModulesNb * 2];
            ClassComparer Comp = new ClassComparer();
            SortedList<int, string> heurist_list = new SortedList<int, string>(Comp);

            int l = -1;
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
                                            heurist_list.Add(heuristic[Math.Abs(ii - i), Math.Abs(jj - j), Math.Abs(kk - k)], _KeyBuildforHeur(kk, jj, ii));
                                        }
                                    }

                                }
                            try
                            {
                                heuristic_list[l] = new HeuristicList(heurist_list);
                                heurist_list.Clear();
                            }
                            catch
                            {
                                Console.WriteLine("exception " + l + " " + dir.ToString());
                                abort = true;
                                Console.WriteLine("NH " + Nh._StateInString(Nh));

                                Console.WriteLine();

                                Console.WriteLine("Puzzle  " + P.GoalNode._StateInString(P.GoalNode));
                                return 10;
                            }


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
            if (He > 2)
            {
                He = (int)(He / 3);
                int rem = He % 3;
                if (rem != 0)
                    He = He + 1;
            }
            else
                if (He > 0)
                He = 1;

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
                string key = Nk.RoombotModules[i].H_ACM[0][0, 3].ToString() + Nk.RoombotModules[i].H_ACM[0][1, 3].ToString() + Nk.RoombotModules[i].H_ACM[0][2, 3].ToString();
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
                                                                                  //Console.WriteLine(i.ToString());
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

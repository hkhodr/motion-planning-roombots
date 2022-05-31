using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RoombotSearch
{
    class ClassBidirectionalBFSSolver
    {

        public ClassPuzzle P_fwd;
        public ClassPuzzle P_bwd;

        private ClassNode N_fwd;
        private ClassNode N_bwd;

        private SortedList<string, ClassNode> Hist_fwd = new SortedList<string, ClassNode>();
        private SortedList<string, ClassNode> Hist_bwd = new SortedList<string, ClassNode>();

        List<ClassNode> Sol = new List<ClassNode>();
        int MaxQ=0;               // Nodes Stored
        int ExpCnt = 0;             // Nodes Processed (expanded)
        int SolTime = 0;
        private DateTime ClkSt = new DateTime();
        private DateTime ClkEnd = new DateTime();

        INCLUDE_OR with_m1 = INCLUDE_OR.WITHOUT_M1;

        bool CostInHistLarger = false;  // For Exeption handling when stopping the puzzle solving
        bool LgotLarger = false;
        bool KeyInHist = true;

        public bool abort = false;
        public bool solved = false;
        Direction direction;

        private static readonly int[,,] heuristic = new Heuristic_Table().heuristic;

        public ClassBidirectionalBFSSolver(INCLUDE_OR m1, ClassPuzzle PuzzleToBeSolved)
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
            P_fwd.AStarManNumStoNodes = MaxQ;
            P_fwd.AStarManNumExpNodes = ExpCnt;
            P_fwd.AStarManSolStep = Sol.Count - 1;          // It is derived from History List
            P_fwd.AStarManSolSteps = Sol;

            return P_fwd;
        }

        private void DoSolve(bool fwd, ClassPuzzle P)
        {

            ClassNode N=P.InitNode;

            ConcurrentQueue<ClassNode> Q = new ConcurrentQueue<ClassNode>();

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
            Q.Enqueue(P.InitNode);
            MaxQ = 1;
            lock (_sync)
            {
                Histf.Add(_KeyBuild(with_m1, P.InitNode), P.InitNode);
            }

            int loopcount = 0;
            while (!P.b_abort && Q.Count > 0 && !abort)
            {
                try
                {
                    bool dequeue_success = Q.TryDequeue(out N);
                    int count_dequeue = 0;
                    while (!dequeue_success)
                    {
                        count_dequeue++;
                        dequeue_success = Q.TryDequeue(out N);
                        Console.WriteLine("Dequeue notsuccessful ");
                        if (count_dequeue > 10)
                            break;
                    }
                    ExpCnt++;

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
                        { }
                        else         // Then add only successors that are not opened yet
                        {
                            //Console.WriteLine("Successor not opened " + counter.ToString());

                            lock (_sync)
                            {
                                try
                                {
                                    Histf.Add(key, SN);
                                    Q.Enqueue(SN);
                                    if (Q.Count > MaxQ)
                                        MaxQ = Q.Count;
                                }
                                catch
                                {
                                    Console.WriteLine("Excpetion in adding to Histf (already existing key)");
                                }

                            }
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
            }

            if (abort && !solved)
            {
                if (dir == Direction.BWD)
                    N_bwd = N;
                else
                    N_fwd = N;
            }

        }

        private bool _AreNodesSame(ClassNode N1, ClassNode N2)      // Checks whether sent 2 nodes have the same state or not
        {
            /*for (int i = 0; i < N1.ModulesNb; i++)
                if (N1.RoombotModules[i].ACM_Voxel[0][0] < 4 && N1.RoombotModules[i].ACM_Voxel[1][0] < 4)
                    return false;
            return true;*/
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
                string key = Nk.RoombotModules[i].H_ACM[0][0, 3].ToString() + Nk.RoombotModules[i].H_ACM[0][1, 3].ToString() + Nk.RoombotModules[i].H_ACM[0][2, 3].ToString();
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

                if (is_with_m1 == INCLUDE_OR.WITH_M1)
                {
                    for (int ii = 0; ii < 3; ii++)
                        for (int j = 0; j < 3; j++)
                            s = s + Nk.RoombotModules[i].H_ACM[Nk.RoombotModules[i].Base][ii, j].ToString() + ",";

                    s = s + Nk.RoombotModules[i].MotorAngles[1].ToString() + ","; // only motor 1 can have no effect on changing the position of ACM
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

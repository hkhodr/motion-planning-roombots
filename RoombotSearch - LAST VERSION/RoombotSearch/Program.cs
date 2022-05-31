using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;

namespace RoombotSearch
{
    public enum INCLUDE_OR
    {
        WITHOUT_M1,
        WITH_M1
    }
    class Program
    {

        // instance for each solver ... 
        static public ClassAStarSolver S_AMan;
        static public ClassAStarNonAdmissibleSolver S_ANA;
        static public ClassBFSSolver S_BFS;
        static public ClassDFSSolver S_DFS;
        static public ClassBidirectionalSearchSolver S_BD;
        static public ClassBidirectionalSearchSolver_2 S_BD2;
        static public ClassBidirectionalBFSSolver S_BBFS;


        static public ClassPuzzle PuzzleToSolve;
        

        public static readonly CollisionLookUpTable collisionTableM0 = new CollisionLookUpTable(ActionType.M0);
        public static readonly CollisionLookUpTable collisionTableM1 = new CollisionLookUpTable(ActionType.M1);
        public static readonly CollisionLookUpTable collisionTableM2 = new CollisionLookUpTable(ActionType.M2);

        public static readonly MMCollisionLookUpTable MMcollisionTableM0_0 = new MMCollisionLookUpTable(ActionType.M0, 0);
        public static readonly MMCollisionLookUpTable1 MMcollisionTableM1_0 = new MMCollisionLookUpTable1(ActionType.M1, 0);
        public static readonly MMCollisionLookUpTable2 MMcollisionTableM2_0 = new MMCollisionLookUpTable2(ActionType.M2, 0);

        public static readonly MMCollisionLookUpTable MMcollisionTableM0_1 = new MMCollisionLookUpTable(ActionType.M0, 1);
        public static readonly MMCollisionLookUpTable1 MMcollisionTableM1_1 = new MMCollisionLookUpTable1(ActionType.M1, 1);
        public static readonly MMCollisionLookUpTable2 MMcollisionTableM2_1 = new MMCollisionLookUpTable2(ActionType.M2, 1);

        public static readonly MMSelfCollisionLookUpTable SMMcollisionTableM0 = new MMSelfCollisionLookUpTable(ActionType.M0);
        public static readonly MMSelfCollisionLookUpTable SMMcollisionTableM1 = new MMSelfCollisionLookUpTable(ActionType.M1);
        public static readonly MMSelfCollisionLookUpTable SMMcollisionTableM2 = new MMSelfCollisionLookUpTable(ActionType.M2);

        public static bool[,] floor;
        public static bool[,] ceiling;
        public static bool[,] wall1;
        public static bool[,] wall2;
        public static bool[,] wall3;
        public static bool[,] wall4;

        public static int which_algo;
        static void Main(string[] args)
        {
            Console.WriteLine(" PROGRAM INIT DONE");
            Console.CancelKeyPress += new ConsoleCancelEventHandler(myHandler); //craetes a handler when ctrl c is pressed: when abort, save data before exiting..  

            int nx, ny, nz, RBnb;
            string test_case="";
            INCLUDE_OR include_m1;

            //User input 
            string inputString = "";

            Console.Write(" Enter seed for random number generator: ");
            inputString = Console.ReadLine();

            int seed;
            while (!Int32.TryParse(inputString, out seed))
            {
                Console.WriteLine("Parsing error. Enter seed for random number generator: ");
                inputString = Console.ReadLine();
            }

            Console.Write("\n Orientation Choice: \n 1 for with orientation, \n 0 for without orientation: \n ");
            inputString = Console.ReadLine();
            while (inputString != "0" && inputString != "1")
            {
                Console.Write("WRONG input. (" + inputString + ") Please enter 1 for with orientation, 0 for without orientation:  ");
                inputString = Console.ReadLine();
            }

            include_m1 = (INCLUDE_OR)Enum.Parse(typeof(INCLUDE_OR), inputString);


            Console.Write("\n Choose the case you want to run: \n 0) User Defined. \n 1) 2 RBs on floor -> 2 RBs on ceiling. \n 2) Going over a gap \n 3) 3 going over a gap \n Case:  ");

            inputString = Console.ReadLine();

            int which_case = 3;
            while (!Int32.TryParse(inputString, out which_case))
            {
                Console.WriteLine("Parsing error. Enter Case :");
                inputString = Console.ReadLine();
            }

            which_algo = 0;
            Console.Write("\n Choose which algorithm you want to run: \n 0) A* admissible \n 1) Bidirectional. \n 2) BFS. \n 3) DFS. \n 4) Bidirectional multiple start. \n 5) A star non admissible. \n 6) Bidirectional BFS \n Algorithm: ");
            inputString = Console.ReadLine();
            while (!Int32.TryParse(inputString, out which_algo))
            {
                Console.WriteLine("Parsing error. Enter Case :");
                inputString = Console.ReadLine();
            }

            int looping = 1;
            Console.Write("\n How many iterations? ");
            inputString = Console.ReadLine();
            while (!Int32.TryParse(inputString, out looping))
            {
                Console.WriteLine("Parsing error. Enter iterations : ");
                inputString = Console.ReadLine();
            }

            RBnb = 2;
            int a= 0;
            nx = 5; ny = 5; nz = 4;
            int[][] input, mot,fix_list,output;
            bool[][] acm_input,mm,acm_output;
            int[] b,bo;
            int[][,] gvx;
            FixedSupport[] fix;
            ClassNode initial_config;
 
            // variable used to initialize a puzzle. 
            input = new int[RBnb][];        //[orientation x y z] note: x y z are multiple of 2. 
            mot = new int[RBnb][];          //[M0 M1 M2]
            acm_input = new bool[RBnb][];   //[ACM0 ACM1] true is closed, false is open.
            b = new int[RBnb];              //input: which base it is? two options 0 or 1
            mm = new bool[RBnb][];          //[can_move metamodule?]
            gvx = new int[RBnb][,];         //goal voxels 
            output = new int[RBnb][];       //[orientation x y z] note: x y z are multiple of 2. 
            acm_output = new bool[RBnb][];  //[ACM0 ACM1] true is closed, false is open.
            bo = new int[RBnb];             //output: which base it is? two options 0 or 1
            fix = new FixedSupport[6];      //state of 6 fixed support: ON, OFF, REMOVE_LINE, REMOVE_COL, CUSTOM
            fix_list = new int[6][];        //if Fixed support state is remove line or colum which ones?

            switch (which_case)
            {
                case 0:
                default:

                    Application.EnableVisualStyles();
                    Application.SetCompatibleTextRenderingDefault(false);
                    Form1 myForm = new Form1();
                    Application.Run(myForm);

                    ClassForm arg = myForm.LePuzzle;
                    floor = arg.floor;
                    ceiling = arg.ceiling;
                    wall1 = arg.wall1;
                    wall2 = arg.wall2;
                    wall3 = arg.wall3;
                    wall4 = arg.wall4;

                    initial_config = new ClassNode(arg.nx, arg.ny, arg.nz, arg.RBnb, arg.inputOccupancyGrid, arg.RoombotStatusesInit);
                    Console.WriteLine(initial_config._StateInString(initial_config));
                    ClassNode goal_config = new ClassNode(arg.nx, arg.ny, arg.nz, arg.RBnb, arg.outputOccupancyGrid, arg.RoombotStatusesGoal);
                    Console.WriteLine(goal_config._StateInString(goal_config));
                    PuzzleToSolve = new ClassPuzzle(initial_config, goal_config, floor, ceiling, wall1, wall2, wall3, wall4);//Creates Puzzle

                    break;
                case 1:
                    input = new int[RBnb][];
                    input[0] = new int[] { 0, 2, 2, 0, 0 }; //[orientation x y z] note: x y z are multiple of 2. 
                    input[1] = new int[] { 0, 2, 6, 0 ,0};

                    mot = new int[RBnb][];
                    mot[0] = new int[] { 0, 0, 0 };
                    mot[1] = new int[] { 0, 0, 0 };

                    acm_input = new bool[RBnb][]; 
                    acm_input[0] = new bool[] { true, false }; //[ACM0 ACM1] true is closed, false is open.
                    acm_input[1] = new bool[] { true, false }; 

                    b = new int[RBnb]; //which is base? two options 0 or 1
                    b[0] = 0;
                    b[1] = 0;

                    mm = new bool[RBnb][]; //[can_move metamodule? ]
                    mm[0] = new bool[] { true, false };
                    mm[1] = new bool[] { true, false };

                    gvx = new int[RBnb][,];
                    gvx[0] = new int[2, 3] { { 1, 3, 3 }, { 1, 3, 2 } };
                    gvx[1] = new int[2, 3] { { 3, 3, 3 }, { 3, 3, 2 } };

                    output = new int[RBnb][];
                    output[0] = new int[] { 0, 2, 6, 4};  //[orientation x y z] note: x y z are multiple of 2. 
                    output[1] = new int[] { 0, 6, 6, 4 };

                    acm_output = new bool[RBnb][];//[ACM0 ACM1] true is closed, false is open.
                    acm_output[0] = new bool[] { false, true };
                    acm_output[1] = new bool[] { false, true };

                    bo = new int[RBnb];//which is base? two options 0 or 1
                    bo[0] = 1;
                    bo[1] = 1;

                    fix = new FixedSupport[6] { FixedSupport.ON, FixedSupport.ON, FixedSupport.OFF, FixedSupport.OFF, FixedSupport.OFF, FixedSupport.OFF };
                    fix_list = new int[6][];
                    fix_list[0] = new int[]{ };
                    fix_list[1] = new int[] { };
                    fix_list[2] = new int[] { };
                    fix_list[3] = new int[] { };
                    fix_list[4] = new int[] { };
                    fix_list[5] = new int[] { };

                    PuzzleToSolve = create_puzzle(RBnb, nx, ny, nz, input, mot, acm_input, b, mm, gvx, output, acm_output, bo, fix, fix_list);
                    break;

                case 2:
                    RBnb = 2;
                    nx = 8;ny = 6;nz = 4;
                    input = new int[RBnb][];
                    input[0] = new int[] { 0, 10, 2, 0 ,0}; //[0] is orientation (4 options) 
                    input[1] = new int[] { 0, 10, 6, 0 ,0};

                    mot = new int[RBnb][];
                    mot[0] = new int[] { 0, 0, 0 };
                    mot[1] = new int[] { 0, 0, 0 };

                    acm_input = new bool[RBnb][];
                    acm_input[0] = new bool[] { true, false };
                    acm_input[1] = new bool[] { true, false };

                    b = new int[RBnb];
                    b[0] = 0;
                    b[1] = 0;

                    mm = new bool[RBnb][];
                    mm[0] = new bool[] { true, false };
                    mm[1] = new bool[] { true, false };

                    gvx = new int[RBnb][,];
                    gvx[0] = new int[2, 3] { { 2, 1, 0 }, { 2, 1 , 1 }};
                    gvx[1] = new int[2, 3] { { 2, 3, 0 }, { 2, 3 , 1 }};

                    output = new int[RBnb][];
                    output[0] = new int[] { 0, 4, 2, 0 }; //[0] is orientation (4 options) 
                    output[1] = new int[] { 0, 4, 6, 0 };

                    acm_output = new bool[RBnb][];
                    acm_output[0] = new bool[] { true, false };
                    acm_output[1] = new bool[] { true, false };

                    bo = new int[RBnb];
                    bo[0] = 0;
                    bo[1] = 0;

                    fix = new FixedSupport[6] { FixedSupport.REMOVE_LINE, FixedSupport.OFF, FixedSupport.OFF, FixedSupport.OFF, FixedSupport.OFF, FixedSupport.OFF };
                    fix_list = new int[6][];
                    a = 4;
                    fix_list[0] = new int[2] { a - 1, a };
                    fix_list[1] = new int[] { };
                    fix_list[2] = new int[] { };
                    fix_list[3] = new int[] { };
                    fix_list[4] = new int[] { };
                    fix_list[5] = new int[] { };

                    PuzzleToSolve = create_puzzle(RBnb, nx, ny, nz, input, mot, acm_input, b, mm, gvx, output, acm_output, bo, fix, fix_list);
                    break;

                case 3:
                    RBnb = 3;
                    nx = 8; ny = 6; nz = 4;
                    input = new int[RBnb][];
                    input[0] = new int[] { 0, 10, 2, 0, 0 }; //[0] is orientation (4 options) 
                    input[1] = new int[] { 0, 10, 6, 0, 0 };
                    input[2] = new int[] { 0, 10, 10, 0, 0 };

                    mot = new int[RBnb][];
                    mot[0] = new int[] { 0, 0, 0 };
                    mot[1] = new int[] { 0, 0, 0 };
                    mot[2] = new int[] { 0, 0, 0 };

                    acm_input = new bool[RBnb][];
                    acm_input[0] = new bool[] { true, false };
                    acm_input[1] = new bool[] { true, false };
                    acm_input[2] = new bool[] { true, false };

                    b = new int[RBnb];
                    b[0] = 0;
                    b[1] = 0;
                    b[2] = 0;

                    mm = new bool[RBnb][];
                    mm[0] = new bool[] { true, false };
                    mm[1] = new bool[] { true, false };
                    mm[2] = new bool[] { true, false };

                    gvx = new int[RBnb][,];
                    gvx[0] = new int[2, 3] { { 2, 1, 0 }, { 2, 1, 1 } };
                    gvx[1] = new int[2, 3] { { 2, 2, 0 }, { 2, 2, 1 } };
                    gvx[2] = new int[2, 3] { { 2, 3, 0 }, { 2, 3, 1 } };

                    output = new int[RBnb][];
                    output[0] = new int[] { 0, 4, 2, 0 }; //[0] is orientation (4 options) 
                    output[1] = new int[] { 0, 4, 4, 0 };
                    output[2] = new int[] { 0, 4, 6, 0 };

                    acm_output = new bool[RBnb][];
                    acm_output[0] = new bool[] { true, false };
                    acm_output[1] = new bool[] { true, false };
                    acm_output[2] = new bool[] { true, false };

                    bo = new int[RBnb];
                    bo[0] = 0;
                    bo[1] = 0;
                    bo[2] = 0;

                    fix = new FixedSupport[6] { FixedSupport.REMOVE_LINE, FixedSupport.OFF, FixedSupport.OFF, FixedSupport.OFF, FixedSupport.OFF, FixedSupport.OFF };
                    fix_list = new int[6][];
                    a = 4;
                    fix_list[0] = new int[2] { a - 1, a };
                    fix_list[1] = new int[] { };
                    fix_list[2] = new int[] { };
                    fix_list[3] = new int[] { };
                    fix_list[4] = new int[] { };
                    fix_list[5] = new int[] { };

                    PuzzleToSolve = create_puzzle(RBnb, nx, ny, nz, input, mot, acm_input, b, mm, gvx, output, acm_output, bo, fix, fix_list);
                    break;

            }
           
            Random rnd = new Random(seed);
            RBnb = PuzzleToSolve.InitNode.ModulesNb;
            nx = PuzzleToSolve.PuzzleSizeX;
            ny = PuzzleToSolve.PuzzleSizeY;
            nz = PuzzleToSolve.PuzzleSizeZ;

            for (int or1 = 0; or1 < looping; or1++)
            {
                if (or1 > 0)
                {
                    input = new int[RBnb][];
                    b = new int[RBnb];
                    acm_input = new bool[RBnb][];

                    for (int ii = 0; ii < RBnb; ii++)
                    {
                        input[ii] = new int[4];
                        input[ii][3] = 0;
                        input[ii][0] = rnd.Next(0, 1);

                        b[ii] = rnd.Next(0, 1);
                        if (b[ii] == 0)
                            acm_input[ii] = new bool[] { true, false };
                        else
                            acm_input[ii] = new bool[] { false, true };

                        input[ii][1] = 2 * rnd.Next(a + 1, nx - 1);
                        input[ii][2] = 2 * rnd.Next(0, ny - 1);
                    }

                    while (!check_init_collision(input, RBnb))
                    {
                        for (int ii = 0; ii < RBnb; ii++)
                        {
                            input[ii][1] = 2 * rnd.Next(a, nx - 1);
                            input[ii][2] = 2 * rnd.Next(0, ny - 1);
                        }


                        Console.WriteLine("Coordinate are the same, repeat");
                    }

                    if (which_case==0)
                    {
                        for (int ii = 0; ii < nz; ii++)
                            for (int j = 0; j < ny; j++)
                                for (int k = 0; k < nx; k++)
                                    PuzzleToSolve.InitNode.OccupancyGrid[ii, j, k] = false;

                        PuzzleToSolve = new ClassPuzzle(false, PuzzleToSolve);
                        for (int i = 0; i < PuzzleToSolve.InitNode.ModulesNb; i++)
                        {
                            int[][,] H = new int[2][,];
                            H = to_H(input[i]);
                            int[][] vx = new int[2][];
                            vx = to_vx(input[i]);

                            int[] motors = new int[] { 0,0,0};

                            bool[] acm = new bool[] { true,false};
                            int basee = 0;
                            int ee = 1 - basee;
                            bool M = true;
                            bool MM = false;


                            PuzzleToSolve.InitNode.RoombotModules[i] = new RoombotStatus((int[][,])H.Clone(), (int[])motors.Clone(), (bool[])acm.Clone(), (int[][])vx.Clone(), basee, ee, M, MM, (bool[])acm.Clone());
                            PuzzleToSolve.InitNode.OccupancyGrid[vx[0][2], vx[0][1], vx[0][0]] = true;
                            PuzzleToSolve.InitNode.OccupancyGrid[vx[1][2], vx[1][1], vx[1][0]] = true;
                        }
                    }
                    else
                    {
                        PuzzleToSolve = create_puzzle(RBnb, nx, ny, nz, input, mot, acm_input, b, mm, gvx, output, acm_output, bo, fix, fix_list);
                    }
                }

                test_case = "";
                test_case = or1.ToString();
                test_case = test_case + "_" + which_algo.ToString();
                test_case = test_case + "_" +seed.ToString();
                test_case = test_case + "_" + include_m1.ToString();
                Console.WriteLine("DONE INIT");


                Console.WriteLine("START SOLVE");
                FileStream ostrm;
                StreamWriter writer;
                TextWriter oldOut = Console.Out;
                try
                {
                    ostrm = new FileStream(string.Format("Redirect_console_{0}.txt", test_case), FileMode.Create, FileAccess.Write);
                    writer = new StreamWriter(ostrm);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Cannot open Redirect.txt for writing");
                    Console.WriteLine(e.Message);
                    return;
                }
                Console.SetOut(writer);
                
                Console.WriteLine("START SAVE");

                Console.WriteLine("START SOLVE");


                switch (which_algo)
                {
                    case 0:
                        S_AMan = new ClassAStarSolver(include_m1, PuzzleToSolve);
                        PuzzleToSolve = S_AMan.SolveIt();
                        break;
                    case 1:
                        S_BD = new ClassBidirectionalSearchSolver(include_m1, PuzzleToSolve);
                        PuzzleToSolve = S_BD.SolveIt();
                        break;

                    case 2:
                        S_BFS = new ClassBFSSolver(include_m1, PuzzleToSolve);
                        PuzzleToSolve = S_BFS.SolveIt();
                        break;
                    case 3:
                        S_DFS = new ClassDFSSolver(include_m1, PuzzleToSolve);
                        PuzzleToSolve = S_DFS.SolveIt();
                        break;
                    case 4:
                        S_BD2 = new ClassBidirectionalSearchSolver_2(include_m1, PuzzleToSolve);
                        PuzzleToSolve = S_BD2.SolveIt();
                        break;
                    case 5:
                        S_ANA = new ClassAStarNonAdmissibleSolver(include_m1, PuzzleToSolve);
                        PuzzleToSolve = S_ANA.SolveIt();
                        break;
                    case 6:
                        S_BBFS = new ClassBidirectionalBFSSolver(include_m1, PuzzleToSolve);
                        PuzzleToSolve = S_BBFS.SolveIt();
                        break;

                }

                Console.WriteLine("SAVE DONE ");
                Console.SetOut(oldOut);
                writer.Close();
                ostrm.Close();

                _SaveSolvedPuzzle(PuzzleToSolve, test_case);

                Console.WriteLine("Done and Saved ");
            }
            

    }

        //Function to check if initial positions result in a collision
        private static bool check_init_collision(int [][] inp, int l)
        {
            bool valid = true;
            for (int i=0;i<l;i++)
            {
                for (int j = i+1; j < l ; j++)
                {
                    Console.WriteLine(i.ToString() + (j).ToString());
                    if (inp[i][1] == inp[j][1] && inp[i][2] == inp[j][2])
                    {
                        return false;
                    }
                }
            }

            return valid;
        }
        private static ClassPuzzle create_puzzle(int Rbnb, int nx, int ny, int nz, int [][] input, int [][] mot,bool[][] acm_input, int[] b, bool[][] mm,
            int [][,]gvx, int[][]output,bool[][]acm_output,int[] bo,
            FixedSupport [] fix, int [][] fix_list)//,int[][] obstacles)
        {
            Console.WriteLine(Rbnb);
            RoombotStatus[] RoombotStatusesInit = new RoombotStatus[Rbnb];
            RoombotStatus[] RoombotStatusesGoal = new RoombotStatus[Rbnb];

            bool[,,] inputOccupancyGrid = new bool[nz, ny, nx];
            bool[,,] outputOccupancyGrid = new bool[nz, ny, nx];
            for (int ii = 0; ii < nz; ii++)
                for (int j = 0; j < ny; j++)
                    for (int k = 0; k < nx; k++)
                        inputOccupancyGrid[ii, j, k] = false;

            //for (int i = 0; i < obstacles.GetLength(1); i++)
            //   inputOccupancyGrid[obstacles[i][2], obstacles[i][1], obstacles[i][0]] = true;
            for (int i = 0; i < Rbnb; i++)
            {
                int [][,] H = new int[2][,];
                H = to_H(input[i]);
                int[][] vx= new int[2][];
                vx = to_vx(input[i]);

                int[] motors = (int []) mot[i].Clone();
                bool[] acm = (bool[]) acm_input[i].Clone();
                int basee = b[i];
                int ee = 1 - basee;
                bool M = mm[i][0];
                bool MM = mm[i][1];


                RoombotStatusesInit[i] = new RoombotStatus((int [][,]) H.Clone(), (int []) motors.Clone(),(bool[])acm.Clone(),(int [][])vx.Clone(), basee,ee,M,MM,(bool[])acm.Clone());
                inputOccupancyGrid[vx[0][2], vx[0][1], vx[0][0]] = true;
                inputOccupancyGrid[vx[1][2], vx[1][1], vx[1][0]] = true;
                if (input[i][4] == 1)
                {
                    //RoombotStatusesInit[i].connection_list.Add(new ConnectionObject(-1,0,0));
                    RoombotStatusesInit[i].obstacle = true;
                }

                H = new int[2][,];
                H = to_H(output[i]);
                vx = to_vx(output[i]);
                RoombotStatusesGoal[i] = new RoombotStatus((int[][,])H.Clone(), (int[])motors.Clone(), (bool[])((bool[])acm_output[i].Clone()).Clone(), (int[][])vx.Clone(), bo[i], 1-bo[i], M, MM, (bool[])acm_output[i].Clone());

            }

            for (int jj=0; jj<Rbnb; jj++)
            {
                outputOccupancyGrid[gvx[jj][0,2], gvx[jj][0,1], gvx[jj][0,0]] = true;
                outputOccupancyGrid[gvx[jj][1, 2], gvx[jj][1, 1], gvx[jj][1, 0]] = true;

            }

            bool[][,] fixedsupport=new bool[6][,];
            fixedsupport[0] = new bool[nx, ny];
            fixedsupport[1] = new bool[nx, ny];
            fixedsupport[2] = new bool[nz, ny];
            fixedsupport[3] = new bool[nz, nx];
            fixedsupport[4] = new bool[nz, ny];
            fixedsupport[5] = new bool[nz, nx];

            
            for(int fix_ind=0; fix_ind < 6; fix_ind++)
            {
                int nxx = fixedsupport[fix_ind].GetLength(0);
                int nyy = fixedsupport[fix_ind].GetLength(1);
                switch (fix[fix_ind])
                {
                    case FixedSupport.OFF: //default new array bool is false 
                        break;
                    case FixedSupport.ON:
                        for (int i = 0; i < nxx; i++)
                            for (int j = 0; j < nyy; j++)
                                fixedsupport[fix_ind][i, j] = true;
                        break;

                    case FixedSupport.REMOVE_LINE:
                        int inc = 0;
                        for (int i = 0; i < nxx; i++)
                        {
                            inc = 0;
                            for (int j = 0; j < nyy; j++)
                                if(inc < fix_list[fix_ind].Length)
                                if (j == fix_list[fix_ind][inc])
                                    inc++;
                                else
                                    fixedsupport[fix_ind][i, j] = true;
                        }



                        break;
                    case FixedSupport.REMOVE_COL:
                        inc = 0;
                        for (int i = 0; i < nyy; i++)
                        {
                            inc = 0;
                            for (int j = 0; j < nxx; j++)
                                if(inc<fix_list[fix_ind].Length)
                                if (j == fix_list[fix_ind][inc])
                                    inc++;
                                else
                                    fixedsupport[fix_ind][j, i] = true;
                            else
                                    fixedsupport[fix_ind][j, i] = true;

                        }
                        break;

                    case FixedSupport.CUSTOM:
                        inc = 0;
                        for (int i = 0; i < nxx; i++)
                            for (int j = 0; j < nyy; j++)
                                if (inc < fix_list[fix_ind].Length)
                                {
                                    if (i == fix_list[fix_ind][inc] && j == fix_list[0][inc + 1])
                                        inc += 2;
                                    else
                                        fixedsupport[fix_ind][j, i] = true;
                                }
                        break;
                }

                floor = fixedsupport[0];
                ceiling = fixedsupport[1];
                wall1 = fixedsupport[2];
                wall2 = fixedsupport[3];
                wall3 = fixedsupport[4];
                wall4 = fixedsupport[5];


            }
            ClassNode initial_config = new ClassNode(nx, ny, nz, Rbnb, inputOccupancyGrid, RoombotStatusesInit);
            Console.WriteLine(initial_config._StateInString(initial_config));

            ClassNode goal_config = new ClassNode(nx, ny, nz, Rbnb, outputOccupancyGrid, RoombotStatusesGoal);

            PuzzleToSolve = new ClassPuzzle(initial_config, goal_config, floor, ceiling, wall1, wall2, wall3, wall4);//Creates Puzzle
            return PuzzleToSolve;

        }

        private static int [][] to_vx(int [] input)
        {
            int[][] vx = new int[2][];
            vx[0] = new int[7];
            vx[0][0] = input[1] / 2;
            vx[0][1] = input[2] / 2;
            vx[0][2] = (input[3]) / 2;
            vx[0][3] = -1;
            vx[0][4] = -1;
            vx[0][5] = -1;
            vx[0][6] = -1;


            vx[1] = new int[7];
            vx[1][0] = vx[0][0];
            vx[1][1] = vx[0][1];
            vx[1][2] = vx[0][2] + 1;
            vx[1][3] = -1;
            vx[1][4] = -1;
            vx[1][5] = -1;
            vx[1][6] = -1;


            return vx;

        }
        private static int[][,] to_H(int [] input)
        {
            int[][,] H = new int[2][,];
            switch (input[0])
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
            
            H[0][0, 3] = input[1];
            H[0][1, 3] = input[2];
            H[0][2, 3] = input[3];

            H[1][0, 3] = input[1];
            H[1][1, 3] = input[2];
            H[1][2, 3] = input[3] + 4;
            
            return H;
        }
        protected static void myHandler(object sender, ConsoleCancelEventArgs args)
        {
            Console.WriteLine("\nThe read operation has been interrupted.");
            Console.WriteLine("  Key pressed: {0}", args.SpecialKey);
            args.Cancel = true;
            PuzzleToSolve.b_abort = true;

            switch (which_algo)
            {
                case 0:
                    S_AMan.abort = true;
                    break;
                case 1:
                    S_BD.abort = true;
                    break;

                case 2:
                    S_BFS.abort = true;
                    break;
                case 3:
                    S_DFS.abort = true;
                    break;
                case 4:
                    S_BD2.abort = true;
                    break;
                case 5:
                    S_ANA.abort = true;
                    break;
                case 6:
                    S_BBFS.abort = true;
                    break;

            }

        }

        private static void _SaveSolvedPuzzle(ClassPuzzle PuzzleToSolve,string test_case)
        {
            XDocument inventoryDoc = new XDocument();

            string SizeX = "";
            string SizeY = "";
            string SizeZ = "";
            string initState = "";
            string goalState = "";
            string NumberOfModules = "";
            
            SizeX = PuzzleToSolve.PuzzleSizeX.ToString();
            SizeY = PuzzleToSolve.PuzzleSizeY.ToString();
            SizeZ = PuzzleToSolve.PuzzleSizeZ.ToString();

            initState = PuzzleToSolve.InitNode._StateInString(PuzzleToSolve.InitNode);
            goalState =  PuzzleToSolve.InitNode._StateInString(PuzzleToSolve.GoalNode);

            NumberOfModules = PuzzleToSolve.InitNode.ModulesNb.ToString();
            string AManSolved = "";
            string AManSolTime = "";
            string AManStoNode = "";
            string AManExpNode = "";
            string AManSolStep = "";
            string AManSolNode = "";                // Will be added Later On
            string APrunedNode = "";
            string AllNodes = "";
            if (PuzzleToSolve.AStarManSolved == true)
            {
                AManSolved = "T";
                AManSolTime = PuzzleToSolve.AStarManSolTime.ToString();
                AManStoNode = PuzzleToSolve.AStarManNumStoNodes.ToString();
                AManExpNode = PuzzleToSolve.AStarManNumExpNodes.ToString();
                AManSolStep = PuzzleToSolve.AStarManSolStep.ToString();
                APrunedNode = PuzzleToSolve.APrunedNodes.ToString();
                AllNodes = PuzzleToSolve.AllNodes.ToString();

            }
            else
            {
                AManSolved = "F";
                AManSolTime = PuzzleToSolve.AStarManSolTime.ToString();
                AManStoNode = PuzzleToSolve.AStarManNumStoNodes.ToString();
                AManExpNode = PuzzleToSolve.AStarManNumExpNodes.ToString();
                AManSolStep = PuzzleToSolve.AStarManSolStep.ToString();
                APrunedNode = PuzzleToSolve.APrunedNodes.ToString();
                AllNodes = PuzzleToSolve.AllNodes.ToString();
            }

            string sf = "";
            //0) Fixed walls/floor/ceiling // it is writing line by line 
            for (int j = 0; j < PuzzleToSolve.PuzzleSizeX; j++)
            {
                for (int k = 0; k < PuzzleToSolve.PuzzleSizeY; k++)
                {
                    sf= sf+ Convert.ToInt16(PuzzleToSolve.Ceiling[j, k]).ToString();
                    sf= sf+ ",";
                }
            }

            for (int j = 0; j < PuzzleToSolve.PuzzleSizeX; j++)
            {
                for (int k = 0; k < PuzzleToSolve.PuzzleSizeY; k++)
                {
                    sf= sf+ Convert.ToInt16(floor[j, k]).ToString();
                    sf= sf+ ",";
                }
            }

            for (int j = 0; j < PuzzleToSolve.PuzzleSizeZ; j++)
            {
                for (int k = 0; k < PuzzleToSolve.PuzzleSizeY; k++)
                {
                    sf= sf+ Convert.ToInt16(wall1[j, k]).ToString();
                    sf= sf+ ",";
                }
            }

            for (int j = 0; j < PuzzleToSolve.PuzzleSizeZ; j++)
            {
                for (int k = 0; k < PuzzleToSolve.PuzzleSizeX; k++)
                {
                    sf= sf+ Convert.ToInt16(wall2[j, k]).ToString();
                    sf= sf+ ",";
                }
            }

            for (int j = 0; j < PuzzleToSolve.PuzzleSizeZ; j++)
            {
                for (int k = 0; k < PuzzleToSolve.PuzzleSizeY; k++)
                {
                    sf= sf+ Convert.ToInt16(wall3[j, k]).ToString();
                    sf= sf+ ",";
                }
            }
            for (int j = 0; j < PuzzleToSolve.PuzzleSizeZ; j++)
            {
                for (int k = 0; k < PuzzleToSolve.PuzzleSizeX; k++)
                {
                    sf= sf+ Convert.ToInt16(wall4[j, k]).ToString();
                    sf= sf+ ",";
                }
            }

            //Console.WriteLine("State: {0}", State);
            XElement newElement = new XElement("RoombotPuzzleSaveFile",
                        new XElement("Puzzle",
                            new XElement("SizeX", SizeX),
                            new XElement("SizeY", SizeY),
                            new XElement("SizeZ", SizeZ),
                            new XElement("NumberOfModules",NumberOfModules),
                            new XElement("FixedSupport", sf),
                            new XElement("InitState", initState),
                            new XElement("GoalState", goalState),
                            new XElement("SolvedAlgorithms",
                                new XElement("Algorithm",
                                    new XElement("Solved", AManSolved),
                                    new XElement("SolutionTime", AManSolTime),
                                    //new XElement("NoOfPrunedNodes", APrunedNode),
                                    //new XElement("NoOfallNodes", AllNodes),
                                    new XElement("NoOfStoredNodes", AManStoNode),
                                    new XElement("NoOfNodeExpansion", AManExpNode),
                                    new XElement("SolutionStep", AManSolStep),
                                    new XElement("SolutionNodes",
                                        new XElement("Node0", initState))))));
            // Add to in-memory object
            inventoryDoc.AddFirst(newElement);          // First Burst is ready

            int cnt = 0;
            foreach (ClassNode Cn in PuzzleToSolve.AStarManSolSteps)
            {
                if (Cn != PuzzleToSolve.AStarManSolSteps[0])
                {
                    string st = "Node" + cnt.ToString();
                    inventoryDoc.Root.Element("Puzzle").Element("SolvedAlgorithms").Element("Algorithm").Element("SolutionNodes").Add(new XElement(st, Cn._StateInString(Cn)));

                }
                cnt++;
            }
            test_case = test_case + "_"+AManSolved;
            inventoryDoc.Save(string.Format("results_{0}.xml", test_case));     // Filename was set when chosen while loading file!!!
        }
    }
}

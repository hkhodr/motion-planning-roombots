using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace RoombotSearch
{
    public enum FixedSupport
    {
        ON,
        OFF,
        REMOVE_LINE,
        REMOVE_COL,
        CUSTOM
    }
    class ClassPuzzle
    {


        public int PuzzleSizeX;
        public int PuzzleSizeY;
        public int PuzzleSizeZ;
        public ClassNode InitNode;
        public ClassNode GoalNode;
        public bool AStarManSolved = false;
        public int AStarManSolTime = -1;
        public int AStarManNumStoNodes = -1;
        public int AStarManNumExpNodes = -1;
        public int AStarManSolStep = -1;
        public int APrunedNodes= -1;
        public int AllNodes = -1;
        public List<ClassNode> AStarManSolSteps = new List<ClassNode>();
        public bool b_abort = false;

        public int MCPuzNum = 0;
        public int WhichSolver = 0;

        public bool[,] Floor;
        public bool[,] Ceiling;
        public bool[,] Wall1;
        public bool[,] Wall2;
        public bool[,] Wall3;
        public bool[,] Wall4;

        //public ClassPuzzle(XmlNode PuzzleInXML)
        public ClassPuzzle(ClassNode initial, ClassNode goal, bool[,] fl, bool[,] ceil, bool[,] w1, bool[,] w2, bool[,] w3, bool[,] w4)
        {
            InitNode = initial;//new ClassNode(StatesVals);       
            GoalNode = goal; //new ClassNode(PuzzleSize);      
            PuzzleSizeX = initial.NodeSizeX;
            PuzzleSizeY = initial.NodeSizeY;
            PuzzleSizeZ = initial.NodeSizeZ;
            Floor = fl;
            Ceiling = ceil;
            Wall1 = w1;
            Wall2 = w2;
            Wall3 = w3;
            Wall4 = w4;
        }

        public ClassPuzzle(bool inverted,ClassPuzzle puzzle)
        {
            if (inverted)
            {
                GoalNode = new ClassNode(false, puzzle.InitNode);
                InitNode = new ClassNode(false, puzzle.GoalNode);
            } 
            else
            {
                InitNode = new ClassNode(false, puzzle.InitNode);
                GoalNode = new ClassNode(false, puzzle.GoalNode);
            }
            PuzzleSizeX = puzzle.PuzzleSizeX;
            PuzzleSizeY = puzzle.PuzzleSizeY;
            PuzzleSizeZ = puzzle.PuzzleSizeZ;
            Floor = puzzle.Floor;
            Ceiling = puzzle.Ceiling;
            Wall1 = puzzle.Wall1;
            Wall2 = puzzle.Wall2;
            Wall3 = puzzle.Wall3;
            Wall4 = puzzle.Wall4;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
//@TODO: if i am connected to something i cannot move. 

namespace RoombotSearch
{
    public enum ActionType
    {
        M0,
        M1,
        M2,
        M0_M1,
        ACM0,
        ACM1
    }

    class ClassNode
    {
        public int NodeSizeX;            // Length in x dimension
        public int NodeSizeY;            // Length in y dimension
        public int NodeSizeZ;            // Length in z dimension

        public bool[,,] OccupancyGrid;    // Occupancy Grid in a 3 dimentional array

        public readonly int ModulesNb;             // nb of modules present in the grid 
        public RoombotStatus[] RoombotModules; //array of status of the roombots present in the grid
        public SortedList<string, int[]> PossibleConnectionSides = new SortedList<string, int[]>(); //Sorted list to keep track if a side is valid for connection or not. 
        public ClassNode ParentNode;

        public int Cost = -1;
        public int Heur = -1;
        public int invalid_Actions = 0;

        //constants used
        private const float b = 0.7071f; //sqrt(2) / 2;

        private static readonly int[,] side = new int[6, 3] { //base, right,back,left,front,up
                                        { 0,0,0 },
                                        { -1,0,1},
                                        { 0,-1,1},
                                        { 1,0,1 },
                                        { 0,1,1 },
                                        { 0,0,2 } };

        private static readonly VoxelList sideList = new VoxelList();
        private static readonly int[] M_0_2 = new int[3] { -120, 0, 120 };
        private static readonly int[] M_1 = new int[4] { 0, 90, 180, -90 };//, -180 };
        private static readonly int[] connection_complement = new int[6] { 5, 3, 4, 1, 2, 0 };

        public ClassNode(int nx, int ny, int nz, int RBnb, bool[,,] inputGrid, RoombotStatus[] RoombotStatuses)
        {
            NodeSizeX = nx;
            NodeSizeY = ny;
            NodeSizeZ = nz;
            OccupancyGrid = (bool[,,])inputGrid.Clone();
            ModulesNb = RBnb;
            RoombotModules = (RoombotStatus[])RoombotStatuses.Clone();
            ParentNode = null;
            PossibleConnectionSides.Clear();
            PossibleConnectionSides = fill_List_OfPossibleConnection(this);
        }

        public ClassNode(bool copy, ClassNode NodeToBeCopied) //create a copy of ClassNode 
        {
            NodeSizeX = NodeToBeCopied.NodeSizeX;
            NodeSizeY = NodeToBeCopied.NodeSizeY;
            NodeSizeZ = NodeToBeCopied.NodeSizeZ;
            ModulesNb = NodeToBeCopied.ModulesNb;
            RoombotModules = new RoombotStatus[ModulesNb];

            for (int i = 0; i < ModulesNb; i++)
                RoombotModules[i] = new RoombotStatus(NodeToBeCopied.RoombotModules[i]);

            OccupancyGrid = (bool[,,])NodeToBeCopied.OccupancyGrid.Clone();

            ParentNode = NodeToBeCopied.ParentNode; //determine my parent 
            Cost = NodeToBeCopied.Cost; //copy cost 
            Heur = NodeToBeCopied.Heur;
            PossibleConnectionSides = new SortedList<string, int[]>(NodeToBeCopied.PossibleConnectionSides);

        }

        public ClassNode(ClassNode NodeToBeCopied) //create a child of ClassNode (+ cost is increased by 1) 
        {
            NodeSizeX = NodeToBeCopied.NodeSizeX;
            NodeSizeY = NodeToBeCopied.NodeSizeY;
            NodeSizeZ = NodeToBeCopied.NodeSizeZ;
            ModulesNb = NodeToBeCopied.ModulesNb;
            RoombotModules = new RoombotStatus[ModulesNb];
            for (int i = 0; i < ModulesNb; i++)
                RoombotModules[i] = new RoombotStatus(NodeToBeCopied.RoombotModules[i]);

            OccupancyGrid = (bool[,,])NodeToBeCopied.OccupancyGrid.Clone();
            ParentNode = NodeToBeCopied;   //determine my parent 
            Cost = NodeToBeCopied.Cost + 1; //increase cost
            PossibleConnectionSides = new SortedList<string, int[]>(NodeToBeCopied.PossibleConnectionSides);
        }


        public List<ClassNode>  GetSuccessors()
        {
            List<ClassNode> result = new List<ClassNode>();
            int invalid = 0;
            //0) ACTION Toggle ACM0 for all modules
            for (int i = 0; i < ModulesNb; i++)
            {
                var check = Check_valid_child(this, i, ActionType.ACM0, 0,0);
                if (check.Item1)
                    result.Add(check.Item2);
                else
                    invalid++;
                //Console.WriteLine("Child ACM0 "+i.ToString()+":  "+check.Item1.ToString()+"\r\n " + _StateInString(check.Item2));
            }
            //1) ACTION Toggle ACM1 for all modules
            for (int i = 0; i < ModulesNb; i++)
            {
                var check = Check_valid_child(this, i, ActionType.ACM1, 0,0);
                if (check.Item1)
                    result.Add(check.Item2);
                else
                    invalid++;
                // Console.WriteLine("Child ACM1 "+i.ToString()+":  "+check.Item1.ToString()+"\r\n " + _StateInString(check.Item2));
            }

            //2) ACTION MOVE M0 
            for (int i = 0; i < ModulesNb; i++)
            {
                int[] M0_Next = M0_2_Next(RoombotModules[i].MotorAngles[0]);
                foreach (int angle in M0_Next)
                {
                    var check = Check_valid_child(this, i, ActionType.M0, angle,0);
                    if (check.Item1)
                        result.Add(check.Item2);
                    else
                        invalid++;
                    //  Console.WriteLine("M0: Child " + i.ToString() + " ," + angle.ToString() + ":  " + check.Item1.ToString() + "\r\n " + _StateInString(check.Item2));

                }

            }

            //3) ACTION MOVE M1
            for (int i = 0; i < ModulesNb; i++)
            {
                foreach (int angle in M_1)
                {
                    if (angle != RoombotModules[i].MotorAngles[1])
                    {
                        var check = Check_valid_child(this, i, ActionType.M1, angle,0);
                        if (check.Item1)
                            result.Add(check.Item2);
                        else
                            invalid++;
                        //    Console.WriteLine("M1: Child " + i.ToString() + " ," + angle.ToString() + ":  " + check.Item1.ToString() + "\r\n " + _StateInString(check.Item2));

                    }

                }

            }

            //4) ACTION MOVE M2
            for (int i = 0; i < ModulesNb; i++)
            {
                int[] M0_Next = M0_2_Next(RoombotModules[i].MotorAngles[2]);
                foreach (int angle in M0_Next)
                {
                    var check = Check_valid_child(this, i, ActionType.M2, angle,0);
                    if (check.Item1)
                        result.Add(check.Item2);
                    else
                        invalid++;
                    //Console.WriteLine("M2: Child " + i.ToString()+ " ,"+angle.ToString() + ":  " + check.Item1.ToString() + "\r\n " + _StateInString(check.Item2));

                }
            }

            
            if (false)
            {
                //4) ACTION MOVE M0_m1
                for (int i = 0; i < ModulesNb; i++)
                {
                    int m;
                    if (RoombotModules[i].Base == 0)
                        m = 0;
                    else
                        m = 2;

                    int[] M0_Next = M0_2_Next(RoombotModules[i].MotorAngles[m]);
                    int[] M2_Next = M0_2_Next(RoombotModules[i].MotorAngles[2-m]);
                    //int[] M2_Next = M0_2_Next(RoombotModules[i].MotorAngles[2]);
                    foreach (int angle1 in M0_Next)
                    {
                        foreach (int angle2 in M_1)
                        {
                            if (angle2 != RoombotModules[i].MotorAngles[1] && Math.Abs(angle2-RoombotModules[i].MotorAngles[1])<=90)
                            {
                                var check = Check_valid_child(this, i, ActionType.M0_M1, angle1, angle2);
                                if (check.Item1)
                                    result.Add(check.Item2);
                               // Console.WriteLine("M2: Child " + i.ToString() + " ," + angle1.ToString()+ angle2.ToString() + ":  " + check.Item1.ToString() + "\r\n " + _StateInString(check.Item2));


                            }
                        }
                    }
                }
            }

            return result;
        }

        public ClassNode GetParent()
        {
            if (ParentNode != null)
            {
                return ParentNode;
            }
            else
                return null;
        }

        public Tuple<bool, ClassNode> Check_valid_child(ClassNode Parent, int RBnb, ActionType type, int angleValue, int angleValue2)
        {
            if (Parent.RoombotModules[RBnb].obstacle)
            {
                //Console.WriteLine("obstacle");
                return Tuple.Create(false, Parent);
            }
                
            bool valid = true;
            ClassNode Child = new ClassNode(Parent);

            switch (type)
            {
                case ActionType.ACM0:
                case ActionType.ACM1:

                    int acm_concerned;
                    if (type == ActionType.ACM0)
                        acm_concerned = 0;
                    else
                        acm_concerned = 1;

                    Child.RoombotModules[RBnb].ToggleACM[acm_concerned] = !Child.RoombotModules[RBnb].ToggleACM[acm_concerned];
                    int[] acm_pos = new int[3];
                    acm_pos[0] = Child.RoombotModules[RBnb].H_ACM[acm_concerned][0, 3];
                    acm_pos[1] = Child.RoombotModules[RBnb].H_ACM[acm_concerned][1, 3];
                    acm_pos[2] = Child.RoombotModules[RBnb].H_ACM[acm_concerned][2, 3];

                    bool acm_closing = Child.RoombotModules[RBnb].ToggleACM[acm_concerned]; //if true acm was open. it is closing. check if in valid position. 
                    int rb_attached_to, vx_attached_to;

                    if (acm_closing)
                    {
                        int[] voxel_attached;
                        if (!Child.PossibleConnectionSides.ContainsKey(_KeyBuildforList(acm_pos, 3))) //if acm connection is not possible return false 
                        {
                            //Console.WriteLine("acm closing, connection not possible");
                            //Console.WriteLine(_StateInString(Child));
                            return Tuple.Create(false, Child);
                        }
                        else
                        {
                            if (!Child.PossibleConnectionSides.TryGetValue(_KeyBuildforList(acm_pos, 3), out voxel_attached))
                            {
                                Console.WriteLine("Error : not in possible connection list.. this should never happen.. ");
                                return Tuple.Create(false, Child);
                            }
                            else
                            {

                                if (voxel_attached.Length > 4) //connected to another RB
                                {
                                    Child.RoombotModules[RBnb].connection_list.Add(new ConnectionObject(voxel_attached[3], acm_concerned, connection_complement[voxel_attached[5]]));
                                    Child.RoombotModules[voxel_attached[3]].connection_list.Add(new ConnectionObject(RBnb, voxel_attached[4], voxel_attached[5]));

                                    Child.RoombotModules[voxel_attached[3]].ACM_Voxel[voxel_attached[4]][5] = RBnb;
                                    Child.RoombotModules[voxel_attached[3]].ACM_Voxel[voxel_attached[4]][6] = acm_concerned;

                                    Child.RoombotModules[RBnb].ACM_Voxel[acm_concerned][3] = voxel_attached[3];
                                    Child.RoombotModules[RBnb].ACM_Voxel[acm_concerned][4] = voxel_attached[4];
                                    Child.RoombotModules[RBnb].connected_to_gnd[acm_concerned] = false;

                                }
                                else //connected to gnd. 
                                {
                                    Child.RoombotModules[RBnb].ACM_Voxel[acm_concerned][3] = -1;
                                    Child.RoombotModules[RBnb].connected_to_gnd[acm_concerned] = true;
                                    Child.RoombotModules[RBnb].connection_list.Add(new ConnectionObject(-1, acm_concerned, voxel_attached[3]));

                                }
                            }
                        }

                    }
                    else //acm is opening need to check connectivity 0) if other acm is attached, 1) if another rb is attched to me 
                    {
                        rb_attached_to = Child.RoombotModules[RBnb].ACM_Voxel[acm_concerned][3];
                        vx_attached_to = Child.RoombotModules[RBnb].ACM_Voxel[acm_concerned][4];

                        Child.RoombotModules[RBnb].ACM_Voxel[acm_concerned][3] = -1;

                        int ret;
                        ret = Child.RoombotModules[RBnb].connection_list.RemoveAll(xx => (xx.connected_to == rb_attached_to && xx.voxel == acm_concerned));
                        if (ret != 1)
                            Console.WriteLine("ERROR: OPENING ACM (1) " + ret.ToString() + " rb attached: " + rb_attached_to.ToString() + " acm_concerned" + acm_concerned.ToString());

                        if (rb_attached_to == -1)
                            Child.RoombotModules[RBnb].connected_to_gnd[acm_concerned] = false;
                        else
                        {
                            Child.RoombotModules[rb_attached_to].ACM_Voxel[vx_attached_to][5] = -1;
                            ret = Child.RoombotModules[rb_attached_to].connection_list.RemoveAll(xx => (xx.connected_to == RBnb && xx.voxel == vx_attached_to));
                            if (ret != 1)
                                Console.WriteLine("ERROR: OPENING ACM (2) " + ret.ToString() + " rb attached: " + RBnb.ToString() + " acm_concerned" + vx_attached_to.ToString());
                        }

                        bool is_valid;
                        is_valid = check_if_disattach_is_valid(Child, RBnb, acm_concerned);
                        if (!is_valid)
                        {
                            return Tuple.Create(false, Child);
                        }


                    }

                    Child = update_after_acm(Child);//, list_changed);
                    break;

                case ActionType.M0:
                case ActionType.M1:
                case ActionType.M2:
                case ActionType.M0_M1:

                    if (!Child.RoombotModules[RBnb].can_move)
                        return Tuple.Create(false, Child);

                    int m_before, m0, m2;

                    int[,] H, base_H, end_effector_H;
                    int[,] old_H;
                    int[] end_effector_voxel, base_voxel;

                    m_before = 0;
                    if (type == ActionType.M0_M1)
                    {
                        Child.Cost++;
                        Child.RoombotModules[RBnb].MotorAngles[1] = angleValue2;
                    }
                    else
                    {
                        m_before = Child.RoombotModules[RBnb].MotorAngles[(int)type];
                        Child.RoombotModules[RBnb].MotorAngles[(int)type] = angleValue; //assign new value to corresponding motor
                        
                    }

                    bool bool_base = Child.RoombotModules[RBnb].Base != 0;
                    int rb_base = Child.RoombotModules[RBnb].Base;
                    int rb_ee = Child.RoombotModules[RBnb].EndEffector;
                    bool _MM = Child.RoombotModules[RBnb].MM;

                    if (rb_base == 0)
                    {
                        m0 = 0;
                        m2 = 2;
                    }
                    else
                    {
                        m0 = 2;
                        m2 = 0;
                        if (type == ActionType.M0)
                            type = ActionType.M2;
                        else if (type == ActionType.M2)
                            type = ActionType.M0;
                    }
                    if (type == ActionType.M0_M1)
                    {
                        m_before = Child.RoombotModules[RBnb].MotorAngles[m0];
                        Child.RoombotModules[RBnb].MotorAngles[m0] = angleValue;
                        //Child.RoombotModules[RBnb].MotorAngles[m2] = angleValue2;
                    }

                    H = Homogenous_matrix(rb_base, Child.RoombotModules[RBnb].MotorAngles[m0],
                                                     Child.RoombotModules[RBnb].MotorAngles[1],
                                                     Child.RoombotModules[RBnb].MotorAngles[m2]);

                    old_H = (int[,])Child.RoombotModules[RBnb].H_ACM[rb_ee].Clone();


                    base_voxel = (int[])Child.RoombotModules[RBnb].ACM_Voxel[rb_base].Clone();
                    end_effector_voxel = (int[])Child.RoombotModules[RBnb].ACM_Voxel[rb_ee].Clone();
                    end_effector_H = (int[,])Child.RoombotModules[RBnb].H_ACM[rb_ee].Clone();
                    base_H = (int[,])Child.RoombotModules[RBnb].H_ACM[rb_base].Clone();

                    Child.RoombotModules[RBnb].H_ACM[rb_ee] = matrixMultiply(Child.RoombotModules[RBnb].H_ACM[rb_base], H); //New Homogenous Matrix

                    if (type == ActionType.M2 && _MM) //CHECK if second module is on acm hemisphere or not 
                    {
                        //Console.WriteLine("check m2");
                        if (!Child.RoombotModules[RBnb].ToggleACM[rb_ee]) //IF acm is not closed 
                        {
                            int[] x_y = find_x_y_from_H(end_effector_H);

                            int sign_x = end_effector_H[x_y[0], 0];
                            int sign_y = end_effector_H[x_y[1], 1];

                            int ee_pos_x = sign_x * end_effector_H[x_y[0], 3];
                            int ee_pos_y = sign_y * end_effector_H[x_y[1], 3];

                            int rb_attached = Child.RoombotModules[RBnb].ACM_Voxel[rb_ee][5];
                            int acm_attached = Child.RoombotModules[RBnb].ACM_Voxel[rb_ee][6];

                            //Console.WriteLine(rb_attached);
                            int scnd_rb_x = sign_x * Child.RoombotModules[rb_attached].H_ACM[acm_attached][x_y[0], 3];
                            int scnd_rb_y = sign_y * Child.RoombotModules[rb_attached].H_ACM[acm_attached][x_y[1], 3];

                            int diff_x = scnd_rb_x - ee_pos_x;
                            int diff_y = scnd_rb_y - ee_pos_y;

                           // Console.WriteLine("check m2: "+ee_pos_x.ToString()+","+ee_pos_y.ToString() +","+ scnd_rb_x.ToString() + "," + scnd_rb_y.ToString()+" RB "+rb_attached.ToString()+","+RBnb.ToString()+" ACM "+acm_attached.ToString()+", "+rb_ee.ToString());
                            if (rb_ee == 1)
                            {
                                if (!((diff_x == -1 && diff_y == 0) || (diff_x == 0 && diff_y == -1)))
                                {
                                    _MM = false;
                                    //Console.WriteLine("It is not a MM");
                                }
                            }
                            else if (rb_ee == 0)
                            {
                                if (!((diff_x == 1 && diff_y == 0) || (diff_x == 0 && diff_y == 1)))
                                {
                                    _MM = false;
                                   // Console.WriteLine("It is not a MM");
                                }
                            }

                            else
                                Console.WriteLine("ERROR: end effector is neither 0 nor 1! ");
                        }

                    }

                    end_effector_H = (int[,])Child.RoombotModules[RBnb].H_ACM[rb_ee].Clone();

                    if (!_MM)
                    {
                        CollisionLookUpTable collisionTable;
                        string key = "";
                        switch (type)
                        {
                            case ActionType.M0:
                            case ActionType.M0_M1:
                                collisionTable = Program.collisionTableM0;
                                key = _KeyBuildforCollision(type, bool_base, base_H, 3, 3, m_before, Child.RoombotModules[RBnb].MotorAngles[m0]);
                                /*if (base_voxel[5] != -1)
                                {
                                    //Console.WriteLine(RBnb.ToString() + ": M0. voxel attached to so I cannot move");
                                    return Tuple.Create(false, Child);
                                }*/

                                break;
                            case ActionType.M1:
                                collisionTable = Program.collisionTableM1;
                                key = _KeyBuildforCollision(type, bool_base, base_H, 3, 3, Child.RoombotModules[RBnb].MotorAngles[m0], m_before);
                                break;
                            case ActionType.M2:
                            default:
                                collisionTable = Program.collisionTableM2;
                                key = _KeyBuildforCollision(type, bool_base, base_H, 3, 3, Child.RoombotModules[RBnb].MotorAngles[m0], Child.RoombotModules[RBnb].MotorAngles[1]);
                                break;
                        }

                        int[] collision_voxels;
                        try
                        {

                            if (!collisionTable.Collision_LookUpTable.TryGetValue(key, out collision_voxels))
                            {
                                Console.WriteLine("Single Module - ERROR : key does not exist " + key + type.ToString() + " type: " + type.ToString()); //should never be the case.. 
                                return Tuple.Create(false, Child);
                            }
                            else
                            {
                                int length = collision_voxels.Length / 3;
                                for (int i = 0; i < length; i++)
                                {
                                    try
                                    {
                                        if (Child.OccupancyGrid[end_effector_voxel[2] + collision_voxels[i * 3 + 2], end_effector_voxel[1] + collision_voxels[i * 3 + 1], end_effector_voxel[0] + collision_voxels[i * 3]])
                                            return Tuple.Create(false, Child);
                                    }
                                    catch (IndexOutOfRangeException)
                                    {
                                        //Console.WriteLine("collision out of bound ");
                                        return Tuple.Create(false, Child);
                                    }
                                }
                            }
                        }
                        catch
                        {
                            Console.WriteLine("ERROR excpetion-single module-collision check");
                            collision_voxels = new int[] { 0 };
                        }

                        if (type == ActionType.M0 || type == ActionType.M0_M1)//only if M0 changed the occupancy will change, if M1 or M2 changed the second hemispher will occupy the same voxel
                        {
                            int[] voxel_occupied = (int[])end_effector_voxel.Clone();
                            try
                            {
                                //Console.WriteLine(end_effector_voxel.Length.ToString());
                                //Console.WriteLine(collision_voxels.Length.ToString());

                                voxel_occupied[0] = end_effector_voxel[0] + collision_voxels[15 + 0];
                                voxel_occupied[1] = end_effector_voxel[1] + collision_voxels[15 + 1];
                                voxel_occupied[2] = end_effector_voxel[2] + collision_voxels[15 + 2];

                                Child.OccupancyGrid[voxel_occupied[2], voxel_occupied[1], voxel_occupied[0]] = true; //occupy new voxel 
                                Child.OccupancyGrid[end_effector_voxel[2], end_effector_voxel[1], end_effector_voxel[0]] = false; //free old voxel

                                Child.RoombotModules[RBnb].ACM_Voxel[rb_ee] = (int[])voxel_occupied.Clone();
                            }
                            catch { }
                        }
                        break;

                    }
                    else //metamodule routine (something is attached to me)
                    {
                        if(type == ActionType.M0_M1)
                            return Tuple.Create(false, Child);
                        //Console.WriteLine(Child._StateInString(Child));
                        SortedList<string, int[]> MMcollisionTable;
                        MMSelfCollisionLookUpTable SMMcollisionTable;
                        string key = "";

                        int second_rb_attached = -1;
                        if (Child.RoombotModules[RBnb].ACM_Voxel[rb_ee][3] != -1)
                            second_rb_attached = 3;
                        else if (Child.RoombotModules[RBnb].ACM_Voxel[rb_ee][5] != -1)
                            second_rb_attached = 5;
                        else
                        {
                            Console.WriteLine("Error: second rb not attached, not MM ??..");
                            if (Child.RoombotModules[RBnb].ACM_Voxel[rb_ee][3] == -1)
                                Console.WriteLine("3");
                            return Tuple.Create(false, Child);
                        }
                        //Console.WriteLine("second attached is " + second_rb_attached);
                        switch (type)
                        {
                            case ActionType.M0:
                                // step 0: check if somehting is attached to the base i cannot move 
                                /*if (base_voxel[5] != -1)
                                {
                                    //Console.WriteLine(RBnb.ToString() + ": MM, M0. voxel attached to so I cannot move");
                                    return Tuple.Create(false, Child);
                                }*/

                                // Step 1: check if action will result in self collision 
                                SMMcollisionTable = Program.SMMcollisionTableM0;
                                key = _KeyBuildforSelfMMCollision(type, Child, RBnb, second_rb_attached, 0, 0, 0);
                                try
                                {
                                    if (Program.SMMcollisionTableM0.CollisionMM_LookUpTable.ContainsKey(key))
                                        return Tuple.Create(false, Child); //self collision will occur 
                                }
                                catch { Console.WriteLine("MM M0 Excetpion, null or comparison error"); }

                                // Step 2: check if action will result in collision with others. 
                                if (rb_base == 0)
                                    MMcollisionTable = Program.MMcollisionTableM0_0.CollisionMM_LookUpTable;
                                else
                                    MMcollisionTable = Program.MMcollisionTableM0_1.CollisionMM_LookUpTable;

                                key = _KeyBuildforMMCollision(type, Child, RBnb, second_rb_attached, m_before, angleValue, 0);
                                //Console.WriteLine("Key: "+rb_base+ " " + key);
                                Console.WriteLine("M0 "+key);
                                Console.WriteLine(Child._StateInString(Child));
                                break;
                            case ActionType.M1:
                                // Step 1: check if action will result in self collision 
                                SMMcollisionTable = Program.SMMcollisionTableM1;
                                key = _KeyBuildforSelfMMCollision(type, Child, RBnb, second_rb_attached, 0, 0, 0);

                                try
                                {
                                    if (Program.SMMcollisionTableM1.CollisionMM_LookUpTable.ContainsKey(key))
                                        return Tuple.Create(false, Child); //self collision will occur 
                                }
                                catch { Console.WriteLine("MM M1 Excetpion, null or comparison error"); }

                                // Step 2: check if action will result in collision with others. 
                                if (rb_base == 0)
                                    MMcollisionTable = Program.MMcollisionTableM1_0.CollisionMM_LookUpTable;
                                else
                                    MMcollisionTable = Program.MMcollisionTableM1_1.CollisionMM_LookUpTable;

                                key = _KeyBuildforMMCollision(type, Child, RBnb, second_rb_attached, m_before, angleValue, 0);
                                //Console.WriteLine("Key: " + rb_base + " " + key);
                                break;
                            case ActionType.M2:
                            default:
                                // Step 1: check if action will result in self collision 
                                SMMcollisionTable = Program.SMMcollisionTableM2;
                                key = _KeyBuildforSelfMMCollision(type, Child, RBnb, second_rb_attached, m_before, angleValue, Child.RoombotModules[RBnb].MotorAngles[1]);
                                try
                                {
                                    if (Program.SMMcollisionTableM2.CollisionMM_LookUpTable.ContainsKey(key))
                                    {
                                        Console.WriteLine("SM2 "+key);
                                        Console.WriteLine(Child._StateInString(Child));
                                        return Tuple.Create(false, Child); //self collision will occur 
                                    }
                                }
                                catch { Console.WriteLine("MM M0 Excetpion, null or comparison error"); }

                                // Step 2: check if action will result in collision with others. 
                                if (rb_base == 0)
                                    MMcollisionTable = Program.MMcollisionTableM2_0.CollisionMM_LookUpTable;
                                else
                                    MMcollisionTable = Program.MMcollisionTableM2_1.CollisionMM_LookUpTable;

                                key = _KeyBuildforMMCollision(type, Child, RBnb, second_rb_attached, m_before, angleValue, Child.RoombotModules[RBnb].MotorAngles[1]);
                                Console.WriteLine("M2 "+key);
                                Console.WriteLine(Child._StateInString(Child));
                                break;
                        }

                        int[] collision_voxels;
                        try
                        {
                            if (!MMcollisionTable.TryGetValue(key, out collision_voxels))
                            {
                                Console.WriteLine("MM: ERROR : key " + key + " does not exist, " + " type " + type.ToString() + " " + Child.RoombotModules[RBnb].Base.ToString()); //should never be the case.. 
                                return Tuple.Create(false, Child);
                            }
                            else
                            {
                                int length = collision_voxels.Length / 3;
                                for (int i = 0; i < length; i++)
                                {
                                    try
                                    {

                                        /*if (type == ActionType.M1)
                                        {
                                            Console.WriteLine(length);
                                            Console.WriteLine(base_voxel[2] + collision_voxels[i * 3 + 2]);
                                            Console.WriteLine(base_voxel[1] + collision_voxels[i * 3 + 1]);
                                            Console.WriteLine(base_voxel[0] + collision_voxels[i * 3]);

                                        }*/

                                        if (Child.OccupancyGrid[base_voxel[2] + collision_voxels[i * 3 + 2], base_voxel[1] + collision_voxels[i * 3 + 1], base_voxel[0] + collision_voxels[i * 3]])
                                            return Tuple.Create(false, Child);
                                    }
                                    catch (IndexOutOfRangeException)
                                    {
                                        Console.WriteLine("collision out of bound ");
                                        return Tuple.Create(false, Child);
                                    }
                                }
                            }
                        }
                        catch
                        {
                            Console.WriteLine("ERROR ");
                            collision_voxels = new int[] { 0 };
                        }

                        //New position: 
                        Child.RoombotModules[end_effector_voxel[second_rb_attached]].H_ACM[0] = matrixMultiply(end_effector_H, matrixMultiply(H_Transpose(old_H), Child.RoombotModules[end_effector_voxel[second_rb_attached]].H_ACM[0]));
                        Child.RoombotModules[end_effector_voxel[second_rb_attached]].H_ACM[1] = matrixMultiply(end_effector_H, matrixMultiply(H_Transpose(old_H), Child.RoombotModules[end_effector_voxel[second_rb_attached]].H_ACM[1]));

                        try
                        {
                            //New Voxel Positions: 
                            int[] vx_new = find_voxel(false, Child.RoombotModules[RBnb].H_ACM[rb_ee], base_voxel);
                            /*if (Child.OccupancyGrid[vx_new[2], vx_new[1], vx_new[0]])
                                return Tuple.Create(false, Child);*/
                            Child.OccupancyGrid[Child.RoombotModules[RBnb].ACM_Voxel[rb_ee][2], Child.RoombotModules[RBnb].ACM_Voxel[rb_ee][1], Child.RoombotModules[RBnb].ACM_Voxel[rb_ee][0]] = false;
                            //Console.WriteLine("before: " + type.ToString() + " " + angleValue.ToString() + " vx0 pose: " + Child.RoombotModules[RBnb].ACM_Voxel[rb_ee][0].ToString() + Child.RoombotModules[RBnb].ACM_Voxel[rb_ee][1].ToString() + Child.RoombotModules[RBnb].ACM_Voxel[rb_ee][2].ToString());
                            Child.RoombotModules[RBnb].ACM_Voxel[rb_ee][0] = vx_new[0];
                            Child.RoombotModules[RBnb].ACM_Voxel[rb_ee][1] = vx_new[1];
                            Child.RoombotModules[RBnb].ACM_Voxel[rb_ee][2] = vx_new[2];

                            //Console.WriteLine("0) Action: " + type.ToString() + " "+angleValue.ToString()+" New position vx pose: " + vx_new[0].ToString() + vx_new[1].ToString() + vx_new[2].ToString());

                            int[] vx_new1 = (int[])find_voxel(true, Child.RoombotModules[end_effector_voxel[second_rb_attached]].H_ACM[end_effector_voxel[1 + second_rb_attached]], (int[])vx_new.Clone()).Clone();
                            /*if (Child.OccupancyGrid[vx_new[2], vx_new[1], vx_new[0]])
                                return Tuple.Create(false, Child);*/
                            Child.OccupancyGrid[Child.RoombotModules[end_effector_voxel[second_rb_attached]].ACM_Voxel[end_effector_voxel[1 + second_rb_attached]][2], Child.RoombotModules[end_effector_voxel[second_rb_attached]].ACM_Voxel[end_effector_voxel[1 + second_rb_attached]][1], Child.RoombotModules[end_effector_voxel[second_rb_attached]].ACM_Voxel[end_effector_voxel[1 + second_rb_attached]][0]] = false;
                            //Console.WriteLine("before: "+ Child.RoombotModules[end_effector_voxel[second_rb_attached]].ACM_Voxel[end_effector_voxel[1 + second_rb_attached]][0].ToString()+ Child.RoombotModules[end_effector_voxel[second_rb_attached]].ACM_Voxel[end_effector_voxel[1 + second_rb_attached]][1].ToString()+ Child.RoombotModules[end_effector_voxel[second_rb_attached]].ACM_Voxel[end_effector_voxel[1 + second_rb_attached]][2].ToString());

                            Child.RoombotModules[end_effector_voxel[second_rb_attached]].ACM_Voxel[end_effector_voxel[second_rb_attached + 1]][0] = vx_new1[0];
                            Child.RoombotModules[end_effector_voxel[second_rb_attached]].ACM_Voxel[end_effector_voxel[second_rb_attached + 1]][1] = vx_new1[1];
                            Child.RoombotModules[end_effector_voxel[second_rb_attached]].ACM_Voxel[end_effector_voxel[second_rb_attached + 1]][2] = vx_new1[2];

                            //Console.WriteLine("1) Action: " + type.ToString()+ " " + angleValue.ToString() + " New position vx pose: " + vx_new1[0].ToString() + vx_new1[1].ToString() + vx_new1[2].ToString());

                            int[] vx_new2 = (int[])find_voxel(false, Child.RoombotModules[end_effector_voxel[second_rb_attached]].H_ACM[1 - end_effector_voxel[1 + second_rb_attached]], (int[])vx_new1.Clone()).Clone();
                            /*if (Child.OccupancyGrid[vx_new[2], vx_new[1], vx_new[0]])
                                return Tuple.Create(false, Child);*/
                            //Console.WriteLine("before: " + Child.RoombotModules[end_effector_voxel[second_rb_attached]].ACM_Voxel[1-end_effector_voxel[1 + second_rb_attached]][0].ToString() + Child.RoombotModules[end_effector_voxel[second_rb_attached]].ACM_Voxel[1 - end_effector_voxel[1 + second_rb_attached]][1].ToString() + Child.RoombotModules[end_effector_voxel[second_rb_attached]].ACM_Voxel[1 - end_effector_voxel[1 + second_rb_attached]][2].ToString());

                            Child.OccupancyGrid[Child.RoombotModules[end_effector_voxel[second_rb_attached]].ACM_Voxel[1 - end_effector_voxel[1 + second_rb_attached]][2], Child.RoombotModules[end_effector_voxel[second_rb_attached]].ACM_Voxel[1 - end_effector_voxel[1 + second_rb_attached]][1], Child.RoombotModules[end_effector_voxel[second_rb_attached]].ACM_Voxel[1 - end_effector_voxel[1 + second_rb_attached]][0]] = false;
                            Child.RoombotModules[end_effector_voxel[second_rb_attached]].ACM_Voxel[1 - end_effector_voxel[1 + second_rb_attached]][0] = vx_new2[0];
                            Child.RoombotModules[end_effector_voxel[second_rb_attached]].ACM_Voxel[1 - end_effector_voxel[1 + second_rb_attached]][1] = vx_new2[1];
                            Child.RoombotModules[end_effector_voxel[second_rb_attached]].ACM_Voxel[1 - end_effector_voxel[1 + second_rb_attached]][2] = vx_new2[2];

                            //Console.WriteLine("2) Action: " + type.ToString()+ " " + angleValue.ToString() + " New position vx pose: " + vx_new2[0].ToString() + vx_new2[1].ToString() + vx_new2[2].ToString());
                            //return Tuple.Create(false, Child);
                            Child.OccupancyGrid[vx_new[2], vx_new[1], vx_new[0]] = true;
                            Child.OccupancyGrid[vx_new1[2], vx_new1[1], vx_new1[0]] = true;
                            Child.OccupancyGrid[vx_new2[2], vx_new2[1], vx_new2[0]] = true;

                        }
                        catch
                        {
                            Console.WriteLine("ERROR: voxel outside occupancy grid");
                            return Tuple.Create(false, Child);
                        }

                    }

                    break;
            }

            //TODO: Not optimal 
            Child.PossibleConnectionSides.Clear();
            Child.PossibleConnectionSides = fill_List_OfPossibleConnection(Child);

            return Tuple.Create(valid, Child);
        }

        private bool check_if_disattach_is_valid(ClassNode N, int RB, int acm)
        {
            // Check 0): if this is the only connection I cannot disconnect. 
            for (int i = 0; i < N.ModulesNb; i++)
            {
                if (N.RoombotModules[i].connection_list.Count == 0)
                    return false;
            }


            //Check 1): if this disconnection will leave some RBs have to handle the weight of more than one module: 
            /* Small explanation: 
             * 0) if i am still connected to gnd from the other acm i am fine. 
             * 1) if the module I am connected to is connected to gnd I am fine.
             * 2) if the module connected to the module I am connected to is connected to gn given that this is in the horizontal plane, I am fine
             * 3) Else this will most probably be leaving the weight on very few modules connected to gnd. 
             * */
            foreach (ConnectionObject cRB in N.RoombotModules[RB].connection_list)
            {

                if (cRB.connected_to == -1)
                    return true;
                else
                {

                    int horizontal_support = cRB.side;
                    foreach (ConnectionObject cRB2 in N.RoombotModules[cRB.connected_to].connection_list)
                    {
                        

                        if (cRB2.connected_to == -1)
                            return true;
                        else
                        {
                            if (N.RoombotModules[cRB2.connected_to].obstacle)
                                return true;

                            if (cRB2.side == horizontal_support && (horizontal_support == 1 || horizontal_support == 3))
                                return false;
                            foreach (ConnectionObject cRB3 in N.RoombotModules[cRB.connected_to].connection_list)
                                if (cRB3.connected_to == -1)
                                    return true;
                        }

                    }
                }

            }
            return false;

        }

        /* This function update the functionalities: can_move, MM or not, base and effector. 
         */
        private ClassNode update_after_acm(ClassNode Node)//, List<int> RBs)
        {
            ClassNode Child = new ClassNode(false, Node);
            //foreach (int RBnb in RBs)
            for (int RBnb = 0; RBnb < Node.ModulesNb; RBnb++)
            {
                    
                // If RB module is connected with both acm to gnd I cannot move. 
                if ((Child.RoombotModules[RBnb].connected_to_gnd[0] && Child.RoombotModules[RBnb].connected_to_gnd[1] )|| Child.RoombotModules[RBnb].obstacle)
                {
                    Child.RoombotModules[RBnb].can_move = false;
                    continue;
                }

                // If RB is connected to just one element, it is a single module and base is the vx it is attached to. 
                if (Child.RoombotModules[RBnb].connection_list.Count == 1)
                {
                    Child.RoombotModules[RBnb].Base = Child.RoombotModules[RBnb].connection_list[0].voxel;
                    Child.RoombotModules[RBnb].EndEffector = 1 - Child.RoombotModules[RBnb].Base;
                    Child.RoombotModules[RBnb].can_move = true;
                    Child.RoombotModules[RBnb].MM = false;
                }
                //else
                 //   Child.RoombotModules[RBnb].can_move = false;

                // If RB is connected to 2 things, 
                else if (Child.RoombotModules[RBnb].connection_list.Count == 2)
                {

                    Child.RoombotModules[RBnb].MM = true;
                    Child.RoombotModules[RBnb].can_move = true;

                    int other_rb;
                    int rb_connected_to = -1;
                    bool con_to_gnd = false;
                    for (int i = 0; i < 2; i++)
                    {
                        other_rb = Child.RoombotModules[RBnb].connection_list[i].connected_to;
                        //Console.WriteLine(RBnb.ToString() + " " + other_rb);
                        if (other_rb == -1)
                        {
                            Child.RoombotModules[RBnb].Base = Child.RoombotModules[RBnb].connection_list[i].voxel;
                            Child.RoombotModules[RBnb].EndEffector = 1 - Child.RoombotModules[RBnb].Base;
                            con_to_gnd = true;
                        }
                        else
                        {
                            if (Child.RoombotModules[other_rb].obstacle)
                            {
                                Child.RoombotModules[RBnb].Base = Child.RoombotModules[RBnb].connection_list[i].voxel;
                                Child.RoombotModules[RBnb].EndEffector = 1 - Child.RoombotModules[RBnb].Base;
                                con_to_gnd = true;

                            }
                            else
                            {
                                rb_connected_to = other_rb;
                                if (Child.RoombotModules[other_rb].connection_list.Count > 1)
                                    Child.RoombotModules[RBnb].can_move = false;
                            }
                        }

                    }

                    if (!con_to_gnd)
                        Child.RoombotModules[RBnb].can_move = false;
                    else if (rb_connected_to == -1)
                        Child.RoombotModules[RBnb].can_move = false;
                    else 
                    {
                        //Console.WriteLine(RBnb.ToString()+" "+rb_connected_to);
                        int vx_concerned = Child.RoombotModules[rb_connected_to].connection_list[0].voxel;
                        if (Child.RoombotModules[RBnb].ToggleACM[Child.RoombotModules[RBnb].EndEffector]) //if it is closed. 
                        {
                            Child.RoombotModules[rb_connected_to].ACM_Voxel[vx_concerned][5] = RBnb;
                            Child.RoombotModules[rb_connected_to].ACM_Voxel[vx_concerned][6] = Child.RoombotModules[RBnb].EndEffector;

                        }
                        else
                        {
                            Child.RoombotModules[RBnb].ACM_Voxel[Child.RoombotModules[RBnb].EndEffector][5] = rb_connected_to;
                            Child.RoombotModules[RBnb].ACM_Voxel[Child.RoombotModules[RBnb].EndEffector][6] = vx_concerned;

                        }
                    }
                    if (!Child.RoombotModules[RBnb].connection_list.Exists(xx => (xx.connected_to == rb_connected_to && xx.voxel == Child.RoombotModules[RBnb].EndEffector)))
                    {
                        Child.RoombotModules[RBnb].can_move = false;
                    }
                }
                //If more than three connection I cannot move. 
                else if (Child.RoombotModules[RBnb].connection_list.Count >= 3)
                    Child.RoombotModules[RBnb].can_move = false;
            }
            //    //Now update base and end_effector and can move
            //    if (Child.RoombotModules[RBnb].ToggleACM[0] && !Child.RoombotModules[RBnb].ToggleACM[1]) //only acm0 is closed... 
            //    {
            //        int other_rb = Child.RoombotModules[RBnb].ACM_Voxel[1][5]; //for the acm which is open check if something is attached to this voxel.
            //        int other_rb_vx = Child.RoombotModules[RBnb].ACM_Voxel[1][6];

            //        if (other_rb != -1)
            //        {
            //            if (Child.RoombotModules[other_rb].ToggleACM[1 - other_rb_vx])
            //                Child.RoombotModules[RBnb].can_move = false;
            //            else
            //            {
            //                Child.RoombotModules[RBnb].Base = 0;
            //                Child.RoombotModules[RBnb].EndEffector = 1;
            //                Child.RoombotModules[RBnb].can_move = true;
            //                Child.RoombotModules[RBnb].MM = true;
            //                //Console.WriteLine("I am MM");
            //            }

            //        }
            //        else
            //        {
            //            Child.RoombotModules[RBnb].Base = 0;
            //            Child.RoombotModules[RBnb].EndEffector = 1;
            //            Child.RoombotModules[RBnb].can_move = true;
            //            Child.RoombotModules[RBnb].MM = false;
            //            //Console.WriteLine("I am not MM");
            //        }


            //    }
            //    else if (!Child.RoombotModules[RBnb].ToggleACM[0] && Child.RoombotModules[RBnb].ToggleACM[1])
            //    {
            //        int other_rb = Child.RoombotModules[RBnb].ACM_Voxel[0][5]; //for the acm which is open check if something is attached to this voxel.
            //        int other_rb_vx = Child.RoombotModules[RBnb].ACM_Voxel[0][6];
            //        if (other_rb != -1)
            //        {
            //            if (Child.RoombotModules[other_rb].ToggleACM[1 - other_rb_vx])
            //                Child.RoombotModules[RBnb].can_move = false;
            //            else
            //            {
            //                Child.RoombotModules[RBnb].Base = 1;
            //                Child.RoombotModules[RBnb].EndEffector = 0;
            //                Child.RoombotModules[RBnb].can_move = true;
            //                Child.RoombotModules[RBnb].MM = true;
            //            }

            //        }
            //        else
            //        {
            //            Child.RoombotModules[RBnb].Base = 1;
            //            Child.RoombotModules[RBnb].EndEffector = 0;
            //            Child.RoombotModules[RBnb].can_move = true;
            //            Child.RoombotModules[RBnb].MM = false;
            //        }

            //    }
            //    else if (!Child.RoombotModules[RBnb].ToggleACM[0] && !Child.RoombotModules[RBnb].ToggleACM[1]) //if both acm are open
            //    {
            //        int both = 0;
            //        for (int i = 0; i < 2; i++)
            //        {
            //            if (Child.RoombotModules[RBnb].ACM_Voxel[i][5] != -1)
            //                both = both + i + 1;
            //        }
            //        if (both > 2)
            //            Child.RoombotModules[RBnb].can_move = false; //if attached to from both sides i cannot move. 
            //        else if (both > 0)
            //        {
            //            Child.RoombotModules[RBnb].Base = both - 1;
            //            Child.RoombotModules[RBnb].EndEffector = 2 - both;
            //            Child.RoombotModules[RBnb].can_move = true;
            //            Child.RoombotModules[RBnb].MM = false;
            //        }
            //        else //both=0 : should never happen 
            //        {
            //            Console.WriteLine("ERROR: RB floating...");
            //        }

            //    }
            //    else //both are attached 
            //    {
            //        int both = 0;
            //        for (int i = 0; i < 2; i++)
            //        {
            //            if (Child.RoombotModules[RBnb].ACM_Voxel[i][3] == -1) //-1 is attached to wall, else attached to something else. 
            //                both = both + i + 1;
            //        }
            //        switch (both - 1)
            //        {
            //            case 2: //Case where both acm are attached to fixed wall/floor/ceil
            //                Child.RoombotModules[RBnb].can_move = false; //we can't move. no base no end effector. 
            //                break;
            //            case 0:
            //            case 1: //case where one acm is attached to a fixed wall, the other acm to another rb 

            //                int other_rb = Child.RoombotModules[RBnb].ACM_Voxel[2 - both][3];
            //                if (Child.RoombotModules[other_rb].connected_to_gnd[0] || Child.RoombotModules[other_rb].connected_to_gnd[1])
            //                {
            //                    Child.RoombotModules[RBnb].can_move = false;
            //                    break;
            //                }

            //                if (!Child.RoombotModules[other_rb].ToggleACM[0] && !Child.RoombotModules[other_rb].ToggleACM[1])
            //                {
            //                    Child.RoombotModules[RBnb].Base = both - 1;
            //                    Child.RoombotModules[RBnb].EndEffector = 2 - both;
            //                    Child.RoombotModules[RBnb].can_move = true;
            //                    Child.RoombotModules[RBnb].MM = true;
            //                }
            //                else
            //                    Child.RoombotModules[RBnb].can_move = false;
            //                //Console.WriteLine("move  " + Child.RoombotModules[RBnb].can_move);
            //                break;
            //            case -1: //case where both acms are attached to other rb. 
            //                     //0) check if fixed kinematic chain from each acm side. 
            //                bool closed = false;
            //                for (int i = 0; i < 2; i++)
            //                {
            //                    other_rb = Child.RoombotModules[RBnb].ACM_Voxel[i][3];
            //                    if (Child.RoombotModules[other_rb].ToggleACM[0] || Child.RoombotModules[other_rb].ToggleACM[1])
            //                    {
            //                        if (closed)
            //                            Child.RoombotModules[RBnb].can_move = false;
            //                        else
            //                        {
            //                            Child.RoombotModules[RBnb].Base = i;
            //                            Child.RoombotModules[RBnb].EndEffector = 1 - i;
            //                            closed = true;
            //                            Child.RoombotModules[RBnb].can_move = true;
            //                            Child.RoombotModules[RBnb].MM = true;
            //                        }
            //                    }
            //                    else
            //                    {
            //                        if (!closed)
            //                            Console.WriteLine("Error: Attached to floating rbs");
            //                    }
            //                }
            //                break;
            //        }
            //    }
            //}

            return Child;
        }
        public int[] find_voxel(bool b_ee, int[,] H, int[] vx)
        {
            //Console.WriteLine("acm pos: " + H[0, 3].ToString() + H[1, 3].ToString() + H[2, 3].ToString());
            //Console.WriteLine("vx pos: " + vx[0].ToString() + vx[1].ToString() + vx[2].ToString());

            int[] diff = new int[3];
            int[] res = new int[3] { 0, 0, 0 };
            int[] result = new int[3];
            string s = "";
            for (int i = 0; i < 3; i++)
            {
                diff[i] = H[i, 3] - 2 * vx[i];
                s = s + diff[i].ToString();
            }
            if (b_ee)
            {
                if (sideList.voxel_list.ContainsKey(s))
                {
                    sideList.voxel_list.TryGetValue(s, out res);
                    //Console.WriteLine("in the list: " +s+" result "+ res[0].ToString() + res[1].ToString() + res[2].ToString());
                    for (int i = 0; i < 3; i++)
                        result[i] = res[i] + vx[i];
                    return result;
                }
            }

            diff[2] = diff[2] - 1;
            for (int i = 0; i < 3; i++)
            {
                result[i] = vx[i] + diff[i] / 2;
            }
            //Console.WriteLine("result: " + result[0].ToString() + result[1].ToString() + result[2].ToString());
            //Console.WriteLine();
            return result;

        }
        public int[,] Homogenous_matrix(int base_ACM, int M0, int M1, int M2)
        {
            int[,] H = new int[4, 4];
            float ca, sa;
            if (base_ACM == 0)
            {
                ca = 0.5736f;// cos(deg2rad(55));
                sa = 0.8192f;// sin(deg2rad(55));  
                M0 = -M0;
                M1 = -M1;
                M2 = -M2;
            }
            else
            {
                ca = -0.5736f;// cos(deg2rad(125));
                sa = 0.8192f;// sin(deg2rad(125));
                M1 = -M1;
            }

            float ct0, ct1, ct2, st0, st1, st2;
            ct0 = cos(M0);
            st0 = sin(M0);

            ct1 = cos(M1);
            st1 = sin(M1);

            ct2 = cos(M2);
            st2 = sin(M2);


            H[0, 3] = Convert.ToInt32(2 * ca * sa * b * ct0 - sa * ct2 * (sa * sa * (b * st0 + ca * b - ca * b * ct0)
                + ca * st1 * (b * ct0 + ca * b * st0) - ca * ct1 * (sa * sa * b + ca * ca * b * ct0 - ca * b * st0))
                - 2 * ca * sa * b - 2 * b * sa * st0 - sa * st2 * (st1 * (sa * sa * b + ca * ca * b * ct0 - ca * b * st0)
                + ct1 * (b * ct0 + ca * b * st0)) - ca * (ca * sa * (b * st0 + ca * b - ca * b * ct0) - sa * st1 * (b * ct0 + ca * b * st0)
                + sa * ct1 * (sa * sa * b + ca * ca * b * ct0 - ca * b * st0)));

            H[1, 3] = Convert.ToInt32(sa * ct2 * (sa * sa * (b * st0 - ca * b + ca * b * ct0) + ca * st1 * (b * ct0 - ca * b * st0)
                + ca * ct1 * (b * ct0 * ca * ca + b * st0 * ca + b * sa * sa)) - ca * (sa * st1 * (b * ct0 - ca * b * st0)
                - ca * sa * (b * st0 - ca * b + ca * b * ct0) + sa * ct1 * (b * ct0 * ca * ca + b * st0 * ca + b * sa * sa))
                - 2 * ca * b * sa + sa * st2 * (ct1 * (b * ct0 - ca * b * st0) - st1 * (b * ct0 * ca * ca + b * st0 * ca + b * sa * sa))
                + 2 * sa * b * st0 + 2 * ca * b * sa * ct0);

            H[2, 3] = Convert.ToInt32(2 * ca * ca + 2 * sa * sa * ct0 - ca * (ca * ct0 * (ca * ca - 1) - sa * sa * st0 * st1 - ca * ca * ca + ca * ct1 * (ca * ca - 1)
                + ca * sa * sa * ct0 * ct1) + sa * sa * ct2 * (ca * ca - ca * ca * ct1 + sa * sa * ct0 + ca * ca * ct0 * ct1 - ca * st0 * st1)
                - sa * sa * st2 * (ct1 * st0 - ca * st1 + ca * ct0 * st1) + 1);


            float commun11 = b * (ct2 * (st1 * (sa * sa * b + ca * ca * b * ct0 - ca * b * st0) + ct1 * (b * ct0 + ca * b * st0))
                - st2 * (sa * sa * (b * st0 + ca * b - ca * b * ct0) + ca * st1 * (b * ct0 + ca * b * st0)
                - ca * ct1 * (sa * sa * b + ca * ca * b * ct0 - ca * b * st0)));

            float commun12 = b * (ca * ct2 * (sa * sa * (b * st0 + ca * b - ca * b * ct0) + ca * st1 * (b * ct0 + ca * b * st0)
                - ca * ct1 * (sa * sa * b + ca * ca * b * ct0 - ca * b * st0)) - sa * (ca * sa * (b * st0 + ca * b - ca * b * ct0)
                - sa * st1 * (b * ct0 + ca * b * st0) + sa * ct1 * (sa * sa * b + ca * ca * b * ct0 - ca * b * st0))
                + ca * st2 * (st1 * (sa * sa * b + ca * ca * b * ct0 - ca * b * st0) + ct1 * (b * ct0 + ca * b * st0)));

            H[0, 0] = Convert.ToInt32(-commun11 - commun12);
            H[0, 1] = Convert.ToInt32(commun11 - commun12);

            H[0, 2] = Convert.ToInt32(ca * (ca * sa * (b * st0 + ca * b - ca * b * ct0) - sa * st1 * (b * ct0 + ca * b * st0)
                + sa * ct1 * (sa * sa * b + ca * ca * b * ct0 - ca * b * st0)) + sa * ct2 * (sa * sa * (b * st0 + ca * b - ca * b * ct0)
                + ca * st1 * (b * ct0 + ca * b * st0) - ca * ct1 * (sa * sa * b + ca * ca * b * ct0 - ca * b * st0))
                + sa * st2 * (st1 * (sa * sa * b + ca * ca * b * ct0 - ca * b * st0) + ct1 * (b * ct0 + ca * b * st0)));

            float commun21 = b * (sa * (sa * st1 * (b * ct0 - ca * b * st0) - ca * sa * (b * st0 - ca * b + ca * b * ct0)
                + sa * ct1 * (b * ct0 * ca * ca + b * st0 * ca + b * sa * sa)) + ca * ct2 * (sa * sa * (b * st0 - ca * b + ca * b * ct0)
                + ca * st1 * (b * ct0 - ca * b * st0) + ca * ct1 * (b * ct0 * ca * ca + b * st0 * ca + b * sa * sa))
                + ca * st2 * (ct1 * (b * ct0 - ca * b * st0) - st1 * (b * ct0 * ca * ca + b * st0 * ca + b * sa * sa)));


            float commun22 = b * (ct2 * (ct1 * (b * ct0 - ca * b * st0) - st1 * (b * ct0 * ca * ca + b * st0 * ca + b * sa * sa))
                - st2 * (sa * sa * (b * st0 - ca * b + ca * b * ct0) + ca * st1 * (b * ct0 - ca * b * st0)
                + ca * ct1 * (b * ct0 * ca * ca + b * st0 * ca + b * sa * sa)));

            H[1, 0] = Convert.ToInt32(commun21 + commun22);
            H[1, 1] = Convert.ToInt32(commun21 - commun22);

            H[1, 2] = Convert.ToInt32(ca * (sa * st1 * (b * ct0 - ca * b * st0) - ca * sa * (b * st0 - ca * b + ca * b * ct0)
                + sa * ct1 * (b * ct0 * ca * ca + b * st0 * ca + b * sa * sa)) - sa * ct2 * (sa * sa * (b * st0 - ca * b + ca * b * ct0)
                + ca * st1 * (b * ct0 - ca * b * st0) + ca * ct1 * (b * ct0 * ca * ca + b * st0 * ca + b * sa * sa))
                - sa * st2 * (ct1 * (b * ct0 - ca * b * st0) - st1 * (b * ct0 * ca * ca + b * st0 * ca + b * sa * sa)));

            float commun31 = b * (sa * (ca * ct0 * (ca * ca - 1) - sa * sa * st0 * st1 - ca * ca * ca
                + ca * ct1 * (ca * ca - 1) + ca * sa * sa * ct0 * ct1)
                + ca * sa * ct2 * (ca * ca - ca * ca * ct1 + sa * sa * ct0 + ca * ca * ct0 * ct1 - ca * st0 * st1)
                - ca * sa * st2 * (ct1 * st0 - ca * st1 + ca * ct0 * st1));

            float commun32 = b * (sa * ct2 * (ct1 * st0 - ca * st1 + ca * ct0 * st1)
                + sa * st2 * (ca * ca - ca * ca * ct1 + sa * sa * ct0 + ca * ca * ct0 * ct1 - ca * st0 * st1));

            H[2, 0] = Convert.ToInt32(commun31 - commun32);

            H[2, 1] = Convert.ToInt32(commun31 + commun32);

            H[2, 2] = Convert.ToInt32(ca * (ca * ct0 * (ca * ca - 1) - sa * sa * st0 * st1 - ca * ca * ca + ca * ct1 * (ca * ca - 1) + ca * sa * sa * ct0 * ct1)
                     - sa * sa * ct2 * (ca * ca - ca * ca * ct1 + sa * sa * ct0 + ca * ca * ct0 * ct1 - ca * st0 * st1)
                     + sa * sa * st2 * (ct1 * st0 - ca * st1 + ca * ct0 * st1));

            H[3, 0] = 0;
            H[3, 1] = 0;
            H[3, 2] = 0;
            H[3, 3] = 1;

            return H;
        }

        public static int[,] H_Transpose(int[,] H) //always 4x4
        {
            int[,] H_T = new int[4, 4];
            for (int i = 0; i < 3; i++)
                for (int j = 0; j < 3; j++)
                    H_T[i, j] = H[j, i];
            H_T[0, 3] = -(H_T[0, 0] * H[0, 3] + H_T[0, 1] * H[1, 3] + H_T[0, 2] * H[2, 3]);
            H_T[1, 3] = -(H_T[1, 0] * H[0, 3] + H_T[1, 1] * H[1, 3] + H_T[1, 2] * H[2, 3]);
            H_T[2, 3] = -(H_T[2, 0] * H[0, 3] + H_T[2, 1] * H[1, 3] + H_T[2, 2] * H[2, 3]);
            H_T[3, 3] = 1;
            H_T[3, 0] = 0;
            H_T[3, 1] = 0;
            H_T[3, 2] = 0;

            return H_T;

        }

        public int[] find_x_y_from_H(int[,] H)
        {
            int[] x_y = new int[2];
            for (int i = 0; i < 3; i++)
                if (Math.Abs(H[i, 0]) == 1)
                {
                    x_y[0] = i; // SO x=H[i,3]
                    break;
                }
            for (int j = 0; j < 3; j++)
                if (Math.Abs(H[j, 1]) == 1)
                {
                    x_y[1] = j; //SO y=H[j,3]
                    break;
                }
            return x_y;


        }
        public int[,] matrixMultiply(int[,] H, int[,] A) //@TODO: find if there is more otimum way to do matrix multiply 
        {
            int sizeH1 = H.GetLength(0);
            int sizeH2 = H.GetLength(1);
            int sizeA1 = A.GetLength(0);
            int sizeA2 = A.GetLength(1);

            int[,] result = new int[sizeH1, sizeA2];

            if (sizeA1 == sizeH2)
            {
                for (int i = 0; i < sizeH1; i++)
                {
                    for (int j = 0; j < sizeA2; j++)
                    {
                        int c = 0;
                        for (int k = 0; k < sizeA1; k++)
                        {
                            c += H[i, k] * A[k, j];
                        }
                        result[i, j] = c;
                    }
                }
            }
            else
                Console.WriteLine("ERROR: matrix multiplication error, sizes do not match");


            return result;
        }

        public float cos(int M)
        {
            float c;
            switch (M)
            {
                case 0:
                    c = 1;
                    break;
                case 120:
                    c = -0.5f;
                    break;
                case -120:
                    c = -0.5f;
                    break;
                case 90:
                    c = 0;
                    break;
                case 180:
                case -180:
                    c = -1;
                    break;
                case -90:
                    c = 0;
                    break;
                default:
                    c = 0;
                    break;
            }
            return c;
        }

        public float sin(int M)
        {
            float s;
            switch (M)
            {
                case 0:
                    s = 0;
                    break;
                case 120:
                    s = 0.8660f;
                    break;
                case -120:
                    s = -0.8660f;
                    break;
                case 90:
                    s = 1;
                    break;
                case 180:
                case -180:
                    s = 0;
                    break;
                case -90:
                    s = -1;
                    break;
                default:
                    s = 0;
                    break;
            }
            return s;
        }

        private string _KeyBuildforList(int[] Nk, int length)
        {
            string s = "";
            for (int i = 0; i < length; i++)
            {
                s = s + Nk[i].ToString();
            }
            return s;
        }

        private string _KeyBuildforCollision(ActionType type, bool acm, int[,] Nk, int size1, int size2, int m, int m1)
        {
            string s = Convert.ToInt16(acm).ToString();
            for (int i = 0; i < size1; i++)
                for (int j = 0; j < size2; j++)
                    s = s + Nk[i, j].ToString();
            s = s + m.ToString();
            if (type == ActionType.M0 || type == ActionType.M2 || type == ActionType.M0_M1)
            {
                s = s + m1.ToString();
            }

            return s;
        }
        private string _KeyBuildforSelfMMCollision(ActionType type, ClassNode Nk, int RB, int second, int m_before, int m_next, int mot_m1)
        {
            string s = "";
            if (second == -1)
            {
                Console.WriteLine("Error: Key build for SC - second is -1 ");
                return s;
            }
            int[] b_coord = (int[])Nk.RoombotModules[RB].ACM_Voxel[Nk.RoombotModules[RB].Base].Clone();
            int[] ee_coord = (int[])Nk.RoombotModules[RB].ACM_Voxel[Nk.RoombotModules[RB].EndEffector].Clone();
            int[] b2_coord, ee2_coord;
            //Console.WriteLine(ee_coord[0].ToString()+ ee_coord[1].ToString()+ ee_coord[2].ToString()+ee_coord[3].ToString()+ee_coord[4].ToString()+ ee_coord[5].ToString()+ ee_coord[6].ToString());
            b2_coord = (int[])Nk.RoombotModules[ee_coord[second]].ACM_Voxel[ee_coord[second + 1]].Clone();
            ee2_coord = (int[])Nk.RoombotModules[ee_coord[second]].ACM_Voxel[1 - ee_coord[second + 1]].Clone();

            switch (type)
            {
                case ActionType.M1:
                    for (int i = 0; i < 3; i++)
                        s = s + (ee_coord[i] - b_coord[i]).ToString();
                    for (int i = 0; i < 3; i++)
                        s = s + (b2_coord[i] - b_coord[i]).ToString();
                    for (int i = 0; i < 3; i++)
                        s = s + (ee2_coord[i] - b_coord[i]).ToString();
                    //Console.WriteLine("M1 SelfCollision " + s);
                    break;

                case ActionType.M0:
                    s = Nk.RoombotModules[RB].Base.ToString();
                    for (int i = 0; i < 3; i++)
                        for (int j = 0; j < 3; j++)
                            s = s + Nk.RoombotModules[RB].H_ACM[Nk.RoombotModules[RB].Base][i, j].ToString();
                    for (int i = 0; i < 3; i++)
                        s = s + (ee_coord[i] - b_coord[i]).ToString();
                    for (int i = 0; i < 3; i++)
                        s = s + (b2_coord[i] - b_coord[i]).ToString();
                    for (int i = 0; i < 3; i++)
                        s = s + (ee2_coord[i] - b_coord[i]).ToString();
                    //Console.WriteLine("M0 SelfCollision " + s);
                    break;
                case ActionType.M2:
                    s = Nk.RoombotModules[RB].Base.ToString();
                    for (int i = 0; i < 3; i++)
                        for (int j = 0; j < 3; j++)
                            s = s + Nk.RoombotModules[RB].H_ACM[Nk.RoombotModules[RB].Base][i, j].ToString();
                    s = s + mot_m1.ToString();
                    s = s + (m_next - m_before).ToString();

                    for (int i = 0; i < 3; i++)
                        s = s + (ee_coord[i] - b_coord[i]).ToString();
                    for (int i = 0; i < 3; i++)
                        s = s + (b2_coord[i] - b_coord[i]).ToString();
                    for (int i = 0; i < 3; i++)
                        s = s + (ee2_coord[i] - b_coord[i]).ToString();
                    //Console.WriteLine("M2 SelfCollision " + s);
                    break;
            }
            return s;
        }

        private string _KeyBuildforMMCollision(ActionType type, ClassNode Nk, int RB, int second, int m_before, int m_next, int mot_m1)
        {
            string s = "";
            if (second == -1)
            {
                Console.WriteLine("Error: Key build for Collision - second is -1 ");
                return s;
            }
            int[] b_coord = (int[])Nk.RoombotModules[RB].ACM_Voxel[Nk.RoombotModules[RB].Base].Clone();
            int[] ee_coord = (int[])Nk.RoombotModules[RB].ACM_Voxel[Nk.RoombotModules[RB].EndEffector].Clone();
            int[] b2_coord, ee2_coord;

            b2_coord = (int[])Nk.RoombotModules[ee_coord[second]].ACM_Voxel[ee_coord[second + 1]].Clone();
            ee2_coord = (int[])Nk.RoombotModules[ee_coord[second]].ACM_Voxel[1 - ee_coord[second + 1]].Clone();

            switch (type)
            {
                case ActionType.M1:
                    s = Nk.RoombotModules[RB].Base.ToString();
                    for (int i = 0; i < 3; i++)
                        for (int j = 0; j < 3; j++)
                            s = s + Nk.RoombotModules[RB].H_ACM[Nk.RoombotModules[RB].Base][i, j].ToString();
                    s = s + (m_next - m_before).ToString();
                    for (int i = 0; i < 3; i++)
                        s = s + (ee_coord[i] - b_coord[i]).ToString();
                    for (int i = 0; i < 3; i++)
                        s = s + (b2_coord[i] - b_coord[i]).ToString();
                    for (int i = 0; i < 3; i++)
                        s = s + (ee2_coord[i] - b_coord[i]).ToString();
                    break;

                case ActionType.M0:
                    s = Nk.RoombotModules[RB].Base.ToString();
                    for (int i = 0; i < 3; i++)
                        for (int j = 0; j < 3; j++)
                            s = s + Nk.RoombotModules[RB].H_ACM[Nk.RoombotModules[RB].Base][i, j].ToString();
                    s = s + (m_next - m_before).ToString();
                    //s = s + m_before.ToString() + m_next.ToString();

                    for (int i = 0; i < 3; i++)
                        s = s + (ee_coord[i] - b_coord[i]).ToString();
                    for (int i = 0; i < 3; i++)
                        s = s + (b2_coord[i] - b_coord[i]).ToString();
                    for (int i = 0; i < 3; i++)
                        s = s + (ee2_coord[i] - b_coord[i]).ToString();
                    break;
                case ActionType.M2:
                    s = Nk.RoombotModules[RB].Base.ToString();
                    for (int i = 0; i < 3; i++)
                        for (int j = 0; j < 3; j++)
                            s = s + Nk.RoombotModules[RB].H_ACM[Nk.RoombotModules[RB].Base][i, j].ToString();

                    s = s + (m_next - m_before).ToString();
                    s = s + mot_m1.ToString();

                    for (int i = 0; i < 3; i++)
                        s = s + (ee_coord[i] - b_coord[i]).ToString();
                    for (int i = 0; i < 3; i++)
                        s = s + (b2_coord[i] - b_coord[i]).ToString();
                    for (int i = 0; i < 3; i++)
                        s = s + (ee2_coord[i] - b_coord[i]).ToString();
                    break;

            }
            return s;
        }

        private SortedList<string, int[]> fill_List_OfPossibleConnection(ClassNode node)
        {
            SortedList<string, int[]> possible_list = new SortedList<string, int[]>();
            possible_list.Clear();
            //Fill the list of possible connectionsides 
            //1) all the ground and ceiling are ok. 
            //NOTE: this was commented bcz ceiling and floor can be checked with simple if z==0 or z==2*NodeSizeZ

            for (int i = 0; i < Program.ceiling.GetLength(0); i++)
                for (int j = 0; j < Program.ceiling.GetLength(1); j++)
                {
                    if (Program.ceiling[i, j])
                    {
                        int[] x = new int[] { 2 * i, 2 * j, 2 * node.NodeSizeZ, 5 };
                        possible_list.Add(_KeyBuildforList(x, 3), (int[])x.Clone());
                        //Console.WriteLine("ceil "+x[0].ToString()+x[1].ToString()+x[2].ToString());
                    }

                }
            for (int i = 0; i < Program.floor.GetLength(0); i++)
                for (int j = 0; j < Program.floor.GetLength(1); j++)
                {
                    if (Program.floor[i, j])
                    {
                        int[] x = new int[] { 2 * i, 2 * j, 0, 0 };
                        possible_list.Add(_KeyBuildforList(x, 3), (int[])x.Clone());
                        //Console.WriteLine(x[0].ToString() + x[1].ToString() + x[2].ToString());
                    }

                }
            for (int i = 0; i < Program.wall1.GetLength(0); i++)
                for (int j = 0; j < Program.wall1.GetLength(1); j++)
                {
                    if (Program.wall1[i, j])
                    {
                        int[] x = new int[] { -1, 2 * j, 2 * i + 1, 1 };
                        try
                        {
                            possible_list.Add(_KeyBuildforList(x, 3), (int[])x.Clone());
                        }
                        catch { }
                            //Console.WriteLine("wall 1 "+x[0].ToString() + x[1].ToString() + x[2].ToString());
                    }

                }
            for (int i = 0; i < Program.wall2.GetLength(0); i++)
                for (int j = 0; j < Program.wall2.GetLength(1); j++)
                {
                    if (Program.wall2[i, j])
                    {
                        int[] x = new int[] { 2 * j, -1, 2 * i + 1, 2 };
                        try
                        {
                            possible_list.Add(_KeyBuildforList(x, 3), (int[])x.Clone());
                        }
                        catch {
                            Console.WriteLine("wall 2"+x[0].ToString() + x[1].ToString() + x[2].ToString());
                        }
                    }

                }
            for (int i = 0; i < Program.wall3.GetLength(0); i++)
                for (int j = 0; j < Program.wall3.GetLength(1); j++)
                {
                    if (Program.wall3[i, j])
                    {
                        int[] x = new int[] { 2 * node.NodeSizeX - 1, 2 * j, 2 * i + 1, 3 };
                        try
                        {
                            possible_list.Add(_KeyBuildforList(x, 3), (int[])x.Clone());
                        }
                        catch
                        {
                            Console.WriteLine("wall 3" + x[0].ToString() + x[1].ToString() + x[2].ToString());
                        }
                    }

                }
            for (int i = 0; i < Program.wall4.GetLength(0); i++)
                for (int j = 0; j < Program.wall4.GetLength(1); j++)
                {
                    if (Program.wall4[i, j])
                    {

                        int[] x = new int[] { 2 * j, 2 * node.NodeSizeY - 1, 2 * i, 4 };
                        try
                        {
                            possible_list.Add(_KeyBuildforList(x, 3), (int[])x.Clone());
                        }
                        catch
                        {

                            Console.WriteLine("wall 4" + x[0].ToString() + x[1].ToString() + x[2].ToString());

                        }
                    }

                }

            //**Create a list of the present acm**//
            int[] n = new int[6];
            SortedList<string, int[]> acm_list = new SortedList<string, int[]>();
            acm_list.Clear();
            for (int a = 0; a < 2; a++)
                for (int i = 0; i < node.ModulesNb; i++)
                {
                    n[0] = node.RoombotModules[i].H_ACM[a][0, 3];
                    n[1] = node.RoombotModules[i].H_ACM[a][1, 3];
                    n[2] = node.RoombotModules[i].H_ACM[a][2, 3];
                    n[3] = i;
                    n[4] = a;
                    try
                    {
                        //Console.WriteLine("KEY: ACM : " +a.ToString()+" " + _KeyBuildforList(n,5));
                        acm_list.Add(_KeyBuildforList(n, 5), (int[])n.Clone());
                    }
                    catch { }//Console.WriteLine("ACM already present "); }
                }

            //2) all the sides of an occupied voxel except where are the acms
            //SortedList<string, string> acm_key_list = new SortedList<string, string>();
            //acm_key_list.Clear();
            for (int rb = 0; rb < node.ModulesNb; rb++)
                for (int v = 0; v < 2; v++)
                    for (int s = 0; s < 6; s++)
                    {

                        n[0] = node.RoombotModules[rb].ACM_Voxel[v][0] * 2 + side[s, 0];
                        n[1] = node.RoombotModules[rb].ACM_Voxel[v][1] * 2 + side[s, 1];
                        n[2] = node.RoombotModules[rb].ACM_Voxel[v][2] * 2 + side[s, 2];
                        n[3] = rb;
                        n[4] = v;
                        n[5] = s;
                        string key = _KeyBuildforList(n, 5);
                        try
                        {

                            //Console.WriteLine(key + " checking "+ s.ToString());
                            if (!acm_list.ContainsKey(key))//|| acm_key_list.ContainsKey(key)) //if not an acm add to the list  or if we already removed one do not remove the second. 
                            {
                                //Console.WriteLine(key + " added ");
                                possible_list.Add(_KeyBuildforList(n, 3), (int[])n.Clone());    //Always clone!!
                            }
                            else
                            {
                                //acm_key_list.Add(key, key);
                                //Console.WriteLine(key + " :this is an acm side so we do not add it for now ");
                            }

                        }
                        catch (ArgumentException)
                        {
                            //possible_list.Add(_KeyBuildforList(n, 3), n);
                            //Console.WriteLine(key + " catch: already in the list ");

                        }
                    }

            return possible_list;
        }
        public string _StateInString(ClassNode Nstr)       // Complementary function for _SaveSolvedPuzzle, ie. saving
        {
            string s = "";


            //1) Occupancy Grid // it is [z,y,x]

            for (int i = 0; i < NodeSizeZ; i++)
                for (int j = 0; j < NodeSizeY; j++)
                {
                    for (int k = 0; k < NodeSizeX; k++)
                    {
                        s = s + Convert.ToInt16(Nstr.OccupancyGrid[i, j, k]).ToString();
                        s = s + ",";
                    }
                }

            //s = s + "\r\n";
            //2) Roombot Statuses 
            //for (int i = 0; i < Nstr.ModulesNb; i++)
            //{
            SortedList<string, int> sortedRBs = new SortedList<string, int>();
            for (int i = 0; i < Nstr.ModulesNb; i++)
            {
                //string key = Nstr.RoombotModules[i].ACM_Voxel[0][0].ToString() + Nstr.RoombotModules[i].ACM_Voxel[0][1].ToString() + Nstr.RoombotModules[i].ACM_Voxel[0][2].ToString();
                string key = i.ToString();
                sortedRBs.Add(key, i);
                //Console.WriteLine(key + i.ToString());
            }

            foreach (int i in sortedRBs.Values)
            {
                s = s + i.ToString() + ",";
                //a) for each module the Homogenous matrix of both ACM0 and ACM1
                //ACM0
                for (int j = 0; j < 4; j++)
                    for (int k = 0; k < 4; k++)
                        s = s + Nstr.RoombotModules[i].H_ACM[0][j, k].ToString() + ",";
                //s = s + "\r\n";

                //ACM1
                for (int j = 0; j < 4; j++)
                    for (int k = 0; k < 4; k++)
                        s = s + Nstr.RoombotModules[i].H_ACM[1][j, k].ToString() + ",";

                //s = s + "\r\n";

                //ACM0 Voxel 
                for (int j = 0; j < 3; j++)
                    s = s + Nstr.RoombotModules[i].ACM_Voxel[0][j].ToString() + ",";

                //s = s + "\r\n";

                //ACM1 Voxel 
                for (int j = 0; j < 3; j++)
                    s = s + Nstr.RoombotModules[i].ACM_Voxel[1][j].ToString() + ",";

                //s = s + "\r\n";

                //Motor Angles
                for (int j = 0; j < 3; j++)
                    s = s + Nstr.RoombotModules[i].MotorAngles[j].ToString() + ",";

                //s = s + "\r\n";
                //Toggle ACM
                s = s + Convert.ToInt32(Nstr.RoombotModules[i].ToggleACM[0]).ToString() + ",";
                s = s + Convert.ToInt32(Nstr.RoombotModules[i].ToggleACM[1]).ToString() + ",";



            }
            //s = s + "\r\n";
            //Heuristic
            s = s + Nstr.Heur.ToString() + ", " + Nstr.Cost.ToString() + "\r\n";

            return s;
        }
        private int[] M0_2_Next(int a)
        {
            int[] next;
            switch (a)
            {
                case 0:
                    next = new int[2] { 120, -120 };
                    break;

                case 120:
                case -120:
                default:
                    next = new int[1] { 0 };
                    break;
            }
            return next;
        }
    }
}



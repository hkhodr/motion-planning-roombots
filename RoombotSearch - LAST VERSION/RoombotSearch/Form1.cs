using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace RoombotSearch
{

    public partial class Form1 : Form
    {
        static int nx ;
        static int ny ;
        static int nz;
        static int RBnb;
        volatile int concerned_RB;
        volatile int I_G=0;

        bool[][,] matrix;
        int[][][][,] H;
        int[][][][] vx= new int[2][][][];
        private ClassForm lePuzzle;
        internal ClassForm LePuzzle { get => lePuzzle; set => lePuzzle = value; }

        public Form1()
        {
            InitializeComponent();
            this.FormClosed += new FormClosedEventHandler(Form1_FormClosed);


        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            TextBox objTextBox = (TextBox)sender;
            string theText = objTextBox.Text;
            nx = Convert.ToInt32(theText);
            
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            TextBox objTextBox = (TextBox)sender;
            string theText = objTextBox.Text;
            ny = Convert.ToInt32(theText);
        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {
            TextBox objTextBox = (TextBox)sender;
            string theText = objTextBox.Text;
            nz = Convert.ToInt32(theText);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            matrix = new bool[6][,];
            int top = 250;
            int left = 50;
            int topp = 250;

            String[] labels = new string[] { "Floor","Ceiling","Wall1","Wall2","Wall3","Wall4"};
            int sizex, sizey;
            for (int z = 0; z < 6; z++)
            {
                switch (z)
                {
                    case 0:
                    case 1:
                    default:
                        sizex = nx; sizey = ny;
                        break;
                    case 2:
                    case 4:
                        sizex = nz; sizey = ny;
                        break;
                    case 3:
                    case 5:
                        sizex = nz; sizey = nx;
                        break;

                }
                matrix[z] = new bool[sizex, sizey];
                Label label = new Label();
                label.Text = labels[z];
                label.Font = new Font(label.Font, FontStyle.Bold);
                label.Top = topp - 25;
                label.Left = left;
                label.Width = 50;

                Button buttonf = new Button();
                buttonf.Left = left;
                buttonf.Top = topp - 10;
                buttonf.Width = 50;
                buttonf.Height = 10;
                buttonf.BackColor = Color.White;
                buttonf.Name = "B" + labels[z];
                buttonf.Click += new EventHandler(ON_OFF_Support_Button);
                this.Controls.Add(buttonf);

                this.Controls.Add(label);
                for (int y = 0; y < sizey; y++)
                {
                    for (int x = 0; x < sizex; x++)
                    {
                        Button button = new Button();
                        button.Left = left;
                        button.Top = top;
                        button.Width = button.Height = 20;
                        this.Controls.Add(button);
                        top += button.Height + 2;
                        // Add a Button Click Event handler
                        button.Click += new EventHandler(DynamicButton_Click);
                        button.Name = z.ToString()+x.ToString() + y.ToString();
                        matrix[z][x, y] = true;
                        button.BackColor = Color.Blue;
                    }
                    left = left + 22;
                    top = topp;
                }
                left = left + 20;

            }
            GroupBox Box = (GroupBox)this.Controls["Step2Box"];
            Box.Visible = true;

        }
        private void ON_OFF_Support_Button(object sender, EventArgs e)
        {
            Button buttonn = (Button)sender;
            if (buttonn.BackColor == Color.Black)
                buttonn.BackColor = Color.White;
            else
                buttonn.BackColor = Color.Black;
            int z=0;
            int sizex = 0, sizey = 0;
            switch (buttonn.Name)
            {
                case "BFloor":
                    z = 0;
                    sizex = nx; sizey = ny;
                    break;
                case "BCeiling":
                    z = 1;
                    sizex = nx; sizey = ny;
                    break;
                case "BWall1":
                    z = 2;
                    sizex = nz; sizey = ny;
                    break;
                case "BWall2":
                    z = 3;
                    sizex = nz; sizey = nx;
                    break;
                case "BWall3":
                    sizex = nz; sizey = ny;
                    z = 4;
                    break;
                case "BWall4":
                    sizex = nz; sizey = nx;
                    z = 5;
                    break;

            }
            for (int y = 0; y < sizey; y++)
            {
                for (int x = 0; x < sizex; x++)
                {

                    string Name = z.ToString() + x.ToString() + y.ToString();
                    Button button = (Button)this.Controls[Name];
                    matrix[z][x, y] = !matrix[z][x, y];
                    if (matrix[z][x, y])
                        button.BackColor = Color.Blue;
                    else
                        button.BackColor = Color.Red;
                }
            }
        }

            private void DynamicButton_Click(object sender, EventArgs e)
        {
            Button button = (Button)sender;
            int place = Convert.ToInt32(button.Name);
            int z = place / 100;
            int x = (-z*100+place) / 10;
            int y = (-z * 100 + place) % 10;
            
            matrix[z][x, y] = !matrix[z][x,y];
            if (!matrix[z][x, y])
            {
                button.BackColor = Color.Red;
            }
            else
                button.BackColor = Color.Blue;

        }
            private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void textBox4_TextChanged(object sender, EventArgs e)
        {
            TextBox objTextBox = (TextBox)sender;
            string theText = objTextBox.Text;
            RBnb = Convert.ToInt32(theText);
            vx[0] = new int[RBnb][][];
            vx[1] = new int[RBnb][][];

        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            NumericUpDown Numeric = (NumericUpDown)sender;
            //string theText = Numeric.Text;
            //concerned_RB = Convert.ToInt32(theText);
            concerned_RB = Convert.ToInt32(Numeric.Value);
            //MessageBox.Show(concerned_RB.ToString());

        }

        private void button2_Click(object sender, EventArgs e)
        {
            NumericUpDown numericUpDown1 = new NumericUpDown();
            int x_pos = 50 + ny * 22 * 2 + 50;
            int y_pos = 250 + nx * 22 + 35;
            numericUpDown1.Location = new Point(x_pos, y_pos);
            numericUpDown1.Name = "numericUpDown1";
            numericUpDown1.Size = new Size(65, 26);
            numericUpDown1.TabIndex = 11;
            numericUpDown1.ValueChanged += new EventHandler(this.numericUpDown1_ValueChanged);
            numericUpDown1.Maximum = RBnb-1;
            this.Controls.Add(numericUpDown1);

            DomainUpDown domainUpDown1 = new DomainUpDown();
            domainUpDown1.Items.Add("Init");
            domainUpDown1.Items.Add("Goal");
            domainUpDown1.Location = new System.Drawing.Point(x_pos, y_pos+25);
            domainUpDown1.Name = "domainUpDown1";
            domainUpDown1.Size = new System.Drawing.Size(65, 26);
            domainUpDown1.TabIndex = 21;
            domainUpDown1.Text = "Init";
            domainUpDown1.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            domainUpDown1.SelectedItemChanged += new System.EventHandler(this.domainUpDown1_SelectedItemChanged);
            this.Controls.Add(domainUpDown1);


            H = new int[2][][][,];
            H[0] = new int[RBnb][][,];
            H[1] = new int[RBnb][][,];
            for (int i=0;i<RBnb;i++)
            {
                H[0][i] = new int[2][,];
                H[0][i][0] = new int[4, 4];
                H[0][i][1] = new int[4, 4];

                H[1][i] = new int[2][,];
                H[1][i][0] = new int[4, 4];
                H[1][i][1] = new int[4, 4];

                H[0][i][0][0, 0] = 1;
                H[0][i][0][1, 0] = 0;
                H[0][i][0][2, 0] = 0;

                H[0][i][0][0, 1] = 0;
                H[0][i][0][1, 1] = 1;
                H[0][i][0][2, 1] = 0;

                H[0][i][0][0, 2] = 0;
                H[0][i][0][1, 2] = 0;
                H[0][i][0][2, 2] = 1;

                H[0][i][1][0, 0] = 0;
                H[0][i][1][1, 0] = 1;
                H[0][i][1][2, 0] = 0;

                H[0][i][1][0, 1] = 1;
                H[0][i][1][1, 1] = 0;
                H[0][i][1][2, 1] = 0;

                H[0][i][1][0, 2] = 0;
                H[0][i][1][1, 2] = 0;
                H[0][i][1][2, 2] = -1;





                H[1][i][0][0, 0] = 1;
                H[1][i][0][1, 0] = 0;
                H[1][i][0][2, 0] = 0;

                H[1][i][0][0, 1] = 0;
                H[1][i][0][1, 1] = 1;
                H[1][i][0][2, 1] = 0;

                H[1][i][0][0, 2] = 0;
                H[1][i][0][1, 2] = 0;
                H[1][i][0][2, 2] = 1;

                H[1][i][1][0, 0] = 0;
                H[1][i][1][1, 0] = 1;
                H[1][i][1][2, 0] = 0;

                H[1][i][1][0, 1] = 1;
                H[1][i][1][1, 1] = 0;
                H[1][i][1][2, 1] = 0;

                H[1][i][1][0, 2] = 0;
                H[1][i][1][1, 2] = 0;
                H[1][i][1][2, 2] = -1;

            }
            int top = nx*22+250+35;
            int left = 50;
            int topp = top;

            String[] labels = new string[] { "Floor", "Ceiling"};
            int sizex, sizey;
            
            //Initialization 
            for (int z = 0; z < 2; z++)
            {

                sizex = nx; sizey = ny;
                Label label = new Label();
                label.Text = labels[z];
                label.Font = new Font(label.Font, FontStyle.Bold);
                label.Top = topp - 25;
                label.Left = left;
                label.Width = 50;
                label.Height= 20;
                this.Controls.Add(label);
                for (int y = 0; y < sizey; y++)
                {
                    for (int x = 0; x < sizex; x++)
                    {

                        Button button = new Button();
                        button.Left = left;
                        button.Top = top;
                        button.Width = button.Height = 20;
                        this.Controls.Add(button);
                        top += button.Height + 2;
                        // Add a Button Click Event handler
                        button.Click += new EventHandler(DynamicButton_Click_RB);
                        button.Name = "0"+z.ToString() + x.ToString() + y.ToString();
                        if(!matrix[z][x,y])
                            button.BackColor = Color.Red;
                        else
                            button.BackColor = Color.Blue;
                    }
                    left = left + 22;
                    top = topp;
                }
                left = left + 20;

            }

            GroupBox Box = (GroupBox) this.Controls["Orientation"] ;
            Box.Visible = true;
            Box.Top = top+22*nx+4;
            Box.Left = 50;

        }
        private void DynamicButton_Click_RB(object sender, EventArgs e)
        {
            Button button = (Button)sender;
            int place = Convert.ToInt32(button.Name);
            int z = place / 100;
            int x = (-z * 100 + place) / 10;
            int y = (-z * 100 + place) % 10;

            if (false)//!matrix[z][x, y])
            {
                MessageBox.Show("This is an empty space! Please Choose another one", "Form Closing");
            }
            else
            {
                try
                {

                    string name = "0"+(vx[I_G][concerned_RB][0][2]/(nz - 1)).ToString() + vx[I_G][concerned_RB][0][0].ToString() + vx[I_G][concerned_RB][0][1].ToString();
                    Button old_button = (Button)this.Controls[name];

                    if (!matrix[vx[I_G][concerned_RB][0][2] / (nz - 1)][vx[I_G][concerned_RB][0][0], vx[I_G][concerned_RB][0][1]])
                        old_button.BackColor = Color.Red;
                    else
                        old_button.BackColor = Color.Blue;
                    //MessageBox.Show("new name is "+button.Name+" Old is " + name, "Form Closing");
                }
                catch { };
                H[I_G][concerned_RB][0][0, 3] = 2*x;
                H[I_G][concerned_RB][0][1, 3] = 2*y;
                H[I_G][concerned_RB][0][2, 3] = 2*z*(nz)+4*z*(1-2*z);
                H[I_G][concerned_RB][0][3, 3] = 1;
                H[I_G][concerned_RB][0][3, 0] = H[I_G][concerned_RB][0][3, 1] = H[I_G][concerned_RB][0][3, 2] = 0;

                vx[I_G][concerned_RB] = new int[2][];
                vx[I_G][concerned_RB][0] = new int[7];
                vx[I_G][concerned_RB][0][0] = x; 
                vx[I_G][concerned_RB][0][1] = y;
                vx[I_G][concerned_RB][0][2] = z * (nz - 1) + z * (1 - 2 * z);
                vx[I_G][concerned_RB][0][3] = -1;
                vx[I_G][concerned_RB][0][4] = -1;
                vx[I_G][concerned_RB][0][5] = -1;
                vx[I_G][concerned_RB][0][6] = -1;

                H[I_G][concerned_RB][1][0, 3] = 2*x;
                H[I_G][concerned_RB][1][1, 3] = 2*y;
                H[I_G][concerned_RB][1][2, 3] = 2*z*(nz)+ (1-z)*(-2*z+1)*4;
                H[I_G][concerned_RB][1][3, 3] = 1;
                H[I_G][concerned_RB][1][3, 0] = H[I_G][concerned_RB][1][3, 1] = H[I_G][concerned_RB][1][3, 2] = 0;

                vx[I_G][concerned_RB][1] = new int[7];
                vx[I_G][concerned_RB][1][0] = x;
                vx[I_G][concerned_RB][1][1] = y;
                vx[I_G][concerned_RB][1][2] = z * (nz - 1) + (1-z)*(-2 * z + 1);
                vx[I_G][concerned_RB][1][3] = -1;
                vx[I_G][concerned_RB][1][4] = -1;
                vx[I_G][concerned_RB][1][5] = -1;
                vx[I_G][concerned_RB][1][6] = -1;


                if (I_G == 0)
                    button.BackColor = Color.LimeGreen;
                else
                    button.BackColor = Color.Yellow;    
                

            }


        }

        private void domainUpDown1_SelectedItemChanged(object sender, EventArgs e)
        {
            DomainUpDown dom= (DomainUpDown)sender;
            I_G=dom.SelectedIndex;
            //MessageBox.Show(I_G.ToString());
            

        }


        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                H[I_G][concerned_RB][0][0, 0] = 1;
                H[I_G][concerned_RB][0][1, 0] = 0;
                H[I_G][concerned_RB][0][2, 0] = 0;

                H[I_G][concerned_RB][0][0, 1] = 0;
                H[I_G][concerned_RB][0][1, 1] = 1;
                H[I_G][concerned_RB][0][2, 1] = 0;

                H[I_G][concerned_RB][0][0, 2] = 0;
                H[I_G][concerned_RB][0][1, 2] = 0;
                H[I_G][concerned_RB][0][2, 2] = 1;

                H[I_G][concerned_RB][1][0, 0] = 0;
                H[I_G][concerned_RB][1][1, 0] = 1;
                H[I_G][concerned_RB][1][2, 0] = 0;

                H[I_G][concerned_RB][1][0, 1] = 1;
                H[I_G][concerned_RB][1][1, 1] = 0;
                H[I_G][concerned_RB][1][2, 1] = 0;

                H[I_G][concerned_RB][1][0, 2] = 0;
                H[I_G][concerned_RB][1][1, 2] = 0;
                H[I_G][concerned_RB][1][2, 2] = -1;
            }
            catch { }

        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                H[I_G][concerned_RB][0][0, 0] = 0;
                H[I_G][concerned_RB][0][1, 0] = -1;
                H[I_G][concerned_RB][0][2, 0] = 0;

                H[I_G][concerned_RB][0][0, 1] = 1;
                H[I_G][concerned_RB][0][1, 1] = 0;
                H[I_G][concerned_RB][0][2, 1] = 0;

                H[I_G][concerned_RB][0][0, 2] = 0;
                H[I_G][concerned_RB][0][1, 2] = 0;
                H[I_G][concerned_RB][0][2, 2] = 1;

                H[I_G][concerned_RB][1][0, 0] = 1;
                H[I_G][concerned_RB][1][1, 0] = 0;
                H[I_G][concerned_RB][1][2, 0] = 0;

                H[I_G][concerned_RB][1][0, 1] = 0;
                H[I_G][concerned_RB][1][1, 1] = -1;
                H[I_G][concerned_RB][1][2, 1] = 0;

                H[I_G][concerned_RB][1][0, 2] = 0;
                H[I_G][concerned_RB][1][1, 2] = 0;
                H[I_G][concerned_RB][1][2, 2] = -1;
            }
            catch { }

        }


        private void radioButton3_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                H[I_G][concerned_RB][0][0, 0] = -1;
                H[I_G][concerned_RB][0][1, 0] = 0;
                H[I_G][concerned_RB][0][2, 0] = 0;

                H[I_G][concerned_RB][0][0, 1] = 0;
                H[I_G][concerned_RB][0][1, 1] = -1;
                H[I_G][concerned_RB][0][2, 1] = 0;

                H[I_G][concerned_RB][0][0, 2] = 0;
                H[I_G][concerned_RB][0][1, 2] = 0;
                H[I_G][concerned_RB][0][2, 2] = 1;

                H[I_G][concerned_RB][1][0, 0] = 0;
                H[I_G][concerned_RB][1][1, 0] = -1;
                H[I_G][concerned_RB][1][2, 0] = 0;

                H[I_G][concerned_RB][1][0, 1] = -1;
                H[I_G][concerned_RB][1][1, 1] = 0;
                H[I_G][concerned_RB][1][2, 1] = 0;

                H[I_G][concerned_RB][1][0, 2] = 0;
                H[I_G][concerned_RB][1][1, 2] = 0;
                H[I_G][concerned_RB][1][2, 2] = -1;
            }
            catch { }
        }

        private void radioButton4_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                H[I_G][concerned_RB][0][0, 0] = 0;
                H[I_G][concerned_RB][0][1, 0] = 1;
                H[I_G][concerned_RB][0][2, 0] = 0;

                H[I_G][concerned_RB][0][0, 1] = -1;
                H[I_G][concerned_RB][0][1, 1] = 0;
                H[I_G][concerned_RB][0][2, 1] = 0;

                H[I_G][concerned_RB][0][0, 2] = 0;
                H[I_G][concerned_RB][0][1, 2] = 0;
                H[I_G][concerned_RB][0][2, 2] = 1;

                H[I_G][concerned_RB][1][0, 0] = -1;
                H[I_G][concerned_RB][1][1, 0] = 0;
                H[I_G][concerned_RB][1][2, 0] = 0;

                H[I_G][concerned_RB][1][0, 1] = 0;
                H[I_G][concerned_RB][1][1, 1] = 1;
                H[I_G][concerned_RB][1][2, 1] = 0;

                H[I_G][concerned_RB][1][0, 2] = 0;
                H[I_G][concerned_RB][1][1, 2] = 0;
                H[I_G][concerned_RB][1][2, 2] = -1;
            }
            catch { }

        }

        void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            LePuzzle = create_puzzle();
            MessageBox.Show("This form is closing");
        }

        private ClassForm create_puzzle()
        {
            RoombotStatus[] RoombotStatusesInit = new RoombotStatus[RBnb];
            RoombotStatus[] RoombotStatusesGoal = new RoombotStatus[RBnb];
            bool[,,] inputOccupancyGrid = new bool[nz, ny, nx];
            bool[,,] outputOccupancyGrid = new bool[nz, ny, nx];

            for (int ii = 0; ii < nz; ii++)
                for (int j = 0; j < ny; j++)
                    for (int k = 0; k < nx; k++)
                        inputOccupancyGrid[ii, j, k] = false;


            for (int i = 0; i < RBnb; i++)
            {

                int[] motors = new int[] { 0, 0, 0 };
                bool[][] acm=new bool [2][];
                int[] basee = new int[2];
                int [] ee=new int [2];
                for (int j = 0; j < 2; j++)
                {
                    if (vx[j][i][0][2] == 0)
                    {
                        acm[j] = new bool[] { true, false };
                        basee[j] = 0;
                        ee [j]= 1;
                    }
                    else
                    {
                        acm [j]= new bool[] { false, true };
                        basee[j] = 1;
                        ee [j]= 0;
                    }
                }


                bool M = true;
                bool MM = false;


                RoombotStatusesInit[i] = new RoombotStatus((int[][,])H[0][i].Clone(), (int[])motors.Clone(), (bool[])acm[0].Clone(), (int[][])vx[0][i].Clone(), basee[0], ee[0], M, MM, (bool[])acm[0].Clone());
                inputOccupancyGrid[vx[0][i][0][2], vx[0][i][0][1], vx[0][i][0][0]] = true;
                inputOccupancyGrid[vx[0][i][1][2], vx[0][i][1][1], vx[0][i][1][0]] = true;


                outputOccupancyGrid[vx[1][i][0][2], vx[1][i][0][1], vx[1][i][0][0]] = true;
                outputOccupancyGrid[vx[1][i][1][2], vx[1][i][1][1], vx[1][i][1][0]] = true;

                RoombotStatusesGoal[i] = new RoombotStatus((int[][,])H[1][i].Clone(), (int[])motors.Clone(), (bool[])acm[1].Clone(), (int[][])vx[1][i].Clone(), basee[1], ee[1], M, MM, (bool[])acm[1].Clone());

            }

            bool[,] floor = (bool [,])matrix[0].Clone();
            bool[,] ceiling = (bool[,])matrix[1].Clone();
            bool[,] wall1 = (bool[,])matrix[2].Clone();
            bool[,] wall2 = (bool[,])matrix[3].Clone();
            bool[,] wall3 = (bool[,])matrix[4].Clone();
            bool[,] wall4 = (bool[,])matrix[5].Clone();


            ClassForm Puzzle = new ClassForm(nx, ny, nz, RBnb, inputOccupancyGrid, outputOccupancyGrid, RoombotStatusesInit, RoombotStatusesGoal, floor, ceiling, wall1, wall2, wall3, wall4);
            return Puzzle;

        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void groupBox2_Enter(object sender, EventArgs e)
        {

        }
    }
}
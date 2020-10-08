using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Pac_Man
{
    public partial class Form1 : Form
    {
        static Pen BarrierPen = new Pen(Color.Blue,5);
        static Graphics FormGraphics, BackGraphics;
        static SizeF TileSize = new SizeF(10, 10);
        Bitmap Background;
        int Score;

        //2d Array of Tiles
        // 28W * 36T
        //2 Bottom Border, 2 Top

        public class Entity
        {
            Point TileCoordinates, CenterPoint;
            int MoveDirection; //1=up,2=down,3=left,4=right

        }

        public Form1()
        {
            InitializeComponent();   
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            FormGraphics = CreateGraphics();

            Height = int.Parse((40 * TileSize.Height).ToString());
            Width = int.Parse((28 * TileSize.Height).ToString());

            Background = new Bitmap(Width, Height);
            BackgroundImage = Background;
            BackGraphics = Graphics.FromImage(Background);

            DrawBoard(TileSize);
        }

        private void DrawBoard (SizeF TileSize)
        {
            float TopBound = (TileSize.Height * 2);
            float BottomBound = Height + (TileSize.Height*2);
            //Generate Points for top half of board


            //Gen Grid for Debug
            for (float Y = TopBound; Y <= BottomBound; Y +=10)
            {
                //Horiz Line
                BackGraphics.DrawLine(new Pen(Color.Red), new PointF(0, Y), new PointF(Width, Y));
            }
            for (float X = 0; X < Width; X += 10)
            {
                //Vert Line
                BackGraphics.DrawLine(new Pen(Color.Red), new PointF(X, TopBound), new PointF(X, BottomBound));
            }
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            //Change Direction on Key Press
        }
    }
}

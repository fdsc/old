using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace VinPlaning
{
    
    public partial class TreeTableView : UserControl
    {
        public TreeTableView()
        {
            InitializeComponent();

            this.SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.Opaque | ControlStyles.ResizeRedraw | ControlStyles.UserPaint, true);
            vw = new VirtualWindow(this);

            vw.repainted += new VirtualWindow.repaintEvent(repainted);
        }


        Bitmap vwb; Graphics g;
        SolidBrush blackBrush = new SolidBrush(Color.Black), whiteBrush = new SolidBrush(Color.White), yellowBrush = new SolidBrush(Color.YellowGreen);
        RectangleF fullRect, cellsRect;
        protected virtual void repainted(Bitmap vwb, Graphics g)
        {
            this.vwb = vwb;
            this.g   = g;
            return;
            /*drawCellsLines();
            drawCellsContent();*/
        }

        protected Font tableFont = new Font("Courier New", 12, FontStyle.Regular);

        public const float mainBorderLineWidth = 3;
        public const float oneBorderLineWidth = 1;
        private void drawCellsLines()
        {
            fullRect = new RectangleF(0, 0, vwb.Size.Width - vScrollBar.Width, vwb.Size.Height - hScrollBar.Height);

            g.FillRectangle(blackBrush, fullRect.X,                           fullRect.Y, fullRect.Width,      mainBorderLineWidth);
            g.FillRectangle(blackBrush, fullRect.X,                           fullRect.Y, mainBorderLineWidth, fullRect.Height);
            g.FillRectangle(blackBrush, fullRect.Right - mainBorderLineWidth, fullRect.Y, mainBorderLineWidth, fullRect.Height);


            g.FillRectangle(blackBrush, fullRect.Left, fullRect.Top + getLineHeight() + mainBorderLineWidth - oneBorderLineWidth, fullRect.Right, mainBorderLineWidth);
            cellsRect = new RectangleF(fullRect.X + mainBorderLineWidth,
                                       fullRect.Top + getHeaderHeight(),
                                       fullRect.Width - mainBorderLineWidth * 2,
                                       fullRect.Height - getHeaderHeight());

            // g.FillRectangle(yellowBrush, cellsRect); // отрисовать cellsRect - внутренний прямоугольник, в котором заключено всё белое пространство ниже заголовка
        }

        private float getLineHeight()
        {
            return tableFont.Height + oneBorderLineWidth + 2;
        }

        private float getHeaderHeight()
        {
            return getLineHeight() - oneBorderLineWidth + mainBorderLineWidth * 2;
        }

        VirtualWindow vw;

        private void TreeTableView_Resize(object sender, EventArgs e)
        {
            vScrollBar.Location = new Point(this.ClientSize.Width - vScrollBar.Width, 0);
            vScrollBar.Height   = this.ClientSize.Height - hScrollBar.Height;

            hScrollBar.Location = new Point(0, this.ClientSize.Height - hScrollBar.Height);
            hScrollBar.Width    = this.ClientSize.Width - vScrollBar.Width;
        }
    }
}

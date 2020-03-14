using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace VinPlaning
{
    public partial class TreeTableView
    {
        public delegate void getNode (byte[] nodeID, out Node Node);
        public delegate List<Node> getLinks(Node Node);
        public getNode  getNodeRequest;
        public getLinks getNodeLinkRequest;


        private void drawCellsContent()
        {
            if (getNodeRequest == null)
                return;

            Node currentRoot;
            getNodeRequest(null, out currentRoot);
            drawHeader(currentRoot);

            if (getNodeLinkRequest == null)
                return;

            int j = 0;
            drawCell(0, ref j, currentRoot, true);
        }

        private void drawHeader(Node currentRoot)
        {
            if (String.IsNullOrEmpty(currentRoot.NodeText))
                return;
            /*
            var SizeF = g.MeasureString(currentRoot.NodeText, tableFont);
            g.DrawString(currentRoot.NodeText, tableFont, blackBrush, new PointF((cellsRect.Width - SizeF.Width) / 2, (getLineHeight() - SizeF.Height) / 2 + mainBorderLineWidth));
             */

            drawStringOnLine(currentRoot.NodeText, tableFont, blackBrush, getLineHeight(), cellsRect.X, mainBorderLineWidth + getLineHeight(), cellsRect.Width);
        }

        public void drawStringOnLine(string text, Font font, Brush brush, float lineHeight, float x, float y, float width = -1)
        {
            var SizeF = g.MeasureString(text, font);
            if (width < 0)
                g.DrawString(text, font, brush, new PointF(x, y - (lineHeight - SizeF.Height) / 2 - SizeF.Height));
            else
                g.DrawString(text, font, brush, new PointF(x + (width - SizeF.Width)/2, y - (lineHeight - SizeF.Height) / 2 - SizeF.Height));
        }

        private void drawCell(int NestLevel, ref int j, Node node, bool isRoot = false)
        {
            if (node == null)
                return;

            if (NestLevel > cellsRect.Width)
                throw new Exception("Уровень вложенности раскрытых участков дерева выше разумных пределов (" + NestLevel + ")");

            if (!isRoot)
            {
                j++;
                drawCell(NestLevel, j, node);
            }

            var list = getNodeLinkRequest(node);
            for (int i = 0; i < list.Count; i++)
            {
                drawCell(NestLevel + 1, ref j, list[i]);
            }
        }

        private void drawCell(int NestLevel, int j, Node node)
        {
            drawCellLine(j);
            int w = drawPlus(NestLevel, j, j % 2 == 0, node);

            var h = getLineHeight();
            drawStringOnLine(node.NodeText, tableFont, blackBrush, h,  h * NestLevel + (w + 1) + h/3, cellsRect.Y + h * j);
        }

        private void drawCellLine(int j)
        {
            g.FillRectangle(blackBrush, cellsRect.X, cellsRect.Y + getLineHeight() * j, cellsRect.Right, oneBorderLineWidth);
        }

        static public readonly Bitmap plus  = VinPlaning.Properties.Resources.plus8_2;
        static public readonly Bitmap minus = VinPlaning.Properties.Resources.minus8_2;
        private int drawPlus(int NestLevel, int j, bool isExpanded, Node node, Bitmap expandedImage = null)
        {
            int x = (int) ( cellsRect.X + getLineHeight() * NestLevel + 1 + 1 );
            var y = cellsRect.Y + getLineHeight() * (j - 1) + 1;

            Bitmap p = expandedImage;
            if (isExpanded)
            {
                if (p == null)
                    p = minus;
            }
            else
            {
                if (p == null)
                    p = plus;
            }

            var h = p.Height;
            g.DrawImageUnscaled(p, x, (int) Math.Ceiling( y + (getLineHeight() - h) / 2.0 ) );

            return p.Width + 1;
        }
    }
}

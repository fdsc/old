using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using NumericalMethods.SystemLinearEqualizations;

namespace AttentionTests
{
    public partial class Attention2Number_Velocity : Form
    {
        private Attention2Number_Velocity()
        {
            InitializeComponent();
            this.SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.Opaque | ControlStyles.ResizeRedraw, true);
        }

        Attention2Number form;
        public Attention2Number_Velocity(Attention2Number mainForm): this()
        {
            form = mainForm;
        }

        private void draw()
        {
            var g       = this.CreateGraphics();

            g.FillRectangle(new SolidBrush(Color.WhiteSmoke), 0, 0, this.ClientSize.Width, this.ClientSize.Height);

            var V0      = 10;
            var VMax    = this.ClientSize.Height - V0 * 2;
            var VRange  = VMax - V0;
            var TMin    = form.minTime * 10000.0;   // Преобразуем из миллисекунд обратно в десятые микросекунд
            var TMax    = form.maxTime * 10000.0;
            //var TRange  = TMax - TMin;
            var p       = new Pen(Color.Black);
            var grPen   = new SolidBrush(Color.Green);
            var blPen   = new SolidBrush(Color.Blue);
            var red     = new SolidBrush(Color.Red);
            var mx      = new Pen(Color.BurlyWood);
            var scale   = this.ClientSize.Width / Attention2Number.maxCount;
            var st      = 2.0f;

            var rs = form.result_sigma;
            if (form.result_sigma < 150)
                rs = 150;

            float k = 1.5f;
            if (form.shifter == 2)
                k = 2.0f;

            double S  = (double) (rs*k     + form.result_time)*10000 / (double) TMax * VRange;
            double S3 = (double) (rs*k*2.0 + form.result_time)*10000 / (double) TMax * VRange;
            double V = 0;
            List<float> vs = new List<float>();
            vs.Add(1.0f);

            g.DrawLine(mx, 0, (int) (VMax - S ), this.ClientSize.Width, (int) (VMax - S ));
            g.DrawLine(mx, 0, (int) (VMax - S3), this.ClientSize.Width, (int) (VMax - S3));

            for (int i = 1; i < Attention2Number.maxCount; i++)
            {
                var x1 = i * scale;
                var x0 = x1 - scale;

                var reactionTime = form.inputNumbers[i].time - form.inputNumbers[i - 1].time;
                V = // (double) (TMax - reactionTime) / (double) TRange;
                    (double) reactionTime / TMax;
                if (form.inputNumbers[i].lastEnter)
                    V *= 0.5;

                V = VMax - V * VRange;

                // g.DrawLine(p, x0, (int) (VMax - oldV), x1, (int) (VMax - V));
                float y = (float) V;//(VMax - V);
                vs.Add(y);

                    //g.FillRectangle(red, x1, 0, scale, VRange);

                if (form.inputNumbers[i].number != form.testNumbers[i])
                    g.FillEllipse(red, x1 - st*2, y - st*2, st * 4 , st * 4);
                else
                if (form.inputNumbers[i].lastEnter)
                    g.FillEllipse(blPen, x1 - st*2, y - st*2, st * 4 , st * 4);
                else
                    g.FillEllipse(grPen, x1 - st, y - st, st * 2 , st * 2);
            }

            cb = null;
            float oldY = vs[0];
            for (int x = 1; x < this.ClientSize.Width; x++)
            {
                //float y = interpol(x, vs, scale, 5);
                float y = spline2(x, vs, scale);
                if (y < 0)
                    y = 0;
                if (y > this.ClientSize.Height)
                    y = this.ClientSize.Height;

                g.DrawLine(p, x - 1, oldY, x, y);
                oldY = y;
            }
        }

        private float interpol(int x, List<float> vs, int scale, int N = 1)
        {           // return vs[x/scale >= vs.Count ? vs.Count - 1 : x/scale];
            int x0 = x / scale - N/2;
            if (x0 < 0)
                x0 = 0;

            int x1 = x0 + N/2 + N%2;

            if (x1 >= vs.Count)
                x1 = vs.Count - 1;

            double result = 0.0f;

            for (int i = x0; i <= x1; i++)
            {
                double f = 1.0;
                for (int j = x0; j <= x1; j++)
                {
                    if (i != j)
                        f *= (double) (x - j*scale) / (double) ( i - j) / (double) scale;   // не убирать "/ (double) scale" в result или в f (идёт ведь и перемножение знаменателей)
                }

                result += vs[i] * f;
            }

            return (float) result;
        }

        CubicSpline cb = null;
        private float spline2(float x, List<float> vs, int scale)
        {
            if (cb == null)
            {
                cb = new CubicSpline();
                var xs = new double[vs.Count];
                var ys = new double[vs.Count];
                for (int i = 0; i < vs.Count; i++)
                {
                    xs[i] = i*scale;
                    ys[i] = vs[i];
                }

                cb.BuildSpline(xs, ys, xs.Length);
            }

            return (float) cb.Func(x);
        }

        // Не работает, гаусс возвращает null
        double [,] cofArray = null;
        Gaus gaus = null;
        private float spline(float x, List<float> vs, int scale)
        {
            double result = 0.0f;

            // a3*x^3 + a2*x^2 + a1*x + a0
            // 3*a3*x^2 + 2*a2*x + a1

            if (cofArray == null)
            {
                int n = vs.Count - 1;
                int N = n*4;
                cofArray = new double[N, N + 1];
                for (int k1 = 0; k1 < N; k1++)
                    for (int k2 = 0; k2 <= N; k2++)
                        cofArray[k1, k2] = 0;

                int j = 0;
                for (int i = 0; i < vs.Count - 1; i++)
                {
                    double curX1 = i*scale;
                    double curX2 = curX1*curX1;
                    double curX3 = curX2*curX1;
                    cofArray[j + 0, N] = vs[i];
                    cofArray[j + 1, N] = vs[i + 1];
                    cofArray[j + 2, N] = 0;
                    cofArray[j + 3, N] = 0;

                    cofArray[j + 0, j + 0] = curX3;
                    cofArray[j + 0, j + 1] = curX2;
                    cofArray[j + 0, j + 2] = curX1;
                    cofArray[j + 0, j + 3] = 1;

                    cofArray[j + 2, j + 0] = 3.0*curX2;
                    cofArray[j + 2, j + 1] = 2.0*curX1;
                    cofArray[j + 2, j + 2] = 1;

                    if (i > 0)
                    {
                        cofArray[j + 2, j - 4] = -3.0*curX2;
                        cofArray[j + 2, j - 3] = -2.0*curX1;
                        cofArray[j + 2, j - 2] = -1;
                    }

                    curX1 = (i + 1)*scale;
                    curX2 = curX1*curX1;
                    curX3 = curX2*curX1;

                    cofArray[j + 1, j + 0] = curX3;
                    cofArray[j + 1, j + 1] = curX2;
                    cofArray[j + 1, j + 2] = curX1;
                    cofArray[j + 1, j + 3] = 1;

                    cofArray[j + 3, j + 0] = 3.0*curX2;
                    cofArray[j + 3, j + 1] = 2.0*curX1;
                    cofArray[j + 3, j + 2] = 1;

                    if (i < vs.Count - 2)
                    {
                        cofArray[j + 3, j + 4] = -3.0*curX2;
                        cofArray[j + 3, j + 5] = -2.0*curX1;
                        cofArray[j + 3, j + 6] = -1;
                    }

                    j += 4;
                }

                gaus = new Gaus(cofArray);
            }

            double[] solved = gaus.GetSolution();

            int k = (int) (x / scale);
            if (k >= vs.Count)
                k = vs.Count - 1;

            result = solved[k*4 + 0]*x*x*x + solved[k*4 + 1]*x*x + solved[k*4 + 2]*x + solved[k*4 + 3];

            return (float) result;
        }

        private void Attention2Number_Velocity_Shown(object sender, EventArgs e)
        {
            cofArray = null;
            draw();
        }

        private void Attention2Number_Velocity_Paint(object sender, PaintEventArgs e)
        {
            cofArray = null;
            draw();
        }

        private void Attention2Number_Velocity_ResizeEnd(object sender, EventArgs e)
        {
            cofArray = null;
            draw();
        }

        private void Attention2Number_Velocity_MaximizedBoundsChanged(object sender, EventArgs e)
        {
            cofArray = null;
            draw();
        }
    }
}

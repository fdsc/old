using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using keccak;
using System.Threading;

namespace testKeccak
{
    partial class Program
    {
        public static void setBitInArray(byte[] array, int i, bool set = true)
        {
            var n = i & 7;
            i = i >> 3;

            var b = array[i];
            var k = 1 << n;

            if (set)
                array[i] = (byte) (b | k);
            else
                array[i] = (byte) (b & ~k);
        }

        public static bool getBitFromArray(byte[] array, int i)
        {
            var n = i & 7;
            i = i >> 3;
            var k = 1 << n;

            if ((array[i] & k) > 0)
                return true;
            else
                return false;
        }

        
        private static void getCountsResult(int sizeb, int[,] counts, out double[,] вероятности, out double минимальнаяВероятность, out double множительВероятностей, out double суммарнаяВероятность, out double отклонениеОтЭталона, out double отклонениеОтЭталонаВниз, out double отклонениеОтЭталонаВверх)
        {
            вероятности = new double[sizeb, sizeb];
            минимальнаяВероятность   = 1.0;
            отклонениеОтЭталона      = 0.0;
            отклонениеОтЭталонаВниз  = 0.0;
            отклонениеОтЭталонаВверх = 0.0;

            множительВероятностей = 1.0;
            List<double> сортированные = new List<double>(sizeb*sizeb);

            if (sizeb < 1)
                throw new ArgumentOutOfRangeException("sizeb");

            double commonS = 0;
            double pw = Math.Pow(sizeb<<1, 0.5);
            for (int i  = 0; i < sizeb; i++)
            {
                for (int j  = 0; j < sizeb; j++)
                {
                    double s = Math.Abs(counts[i, j] - sizeb);
                    s /= pw;
                    var v = erfc(s / 1.4142135623730950488016887242097, 1e-6);
                    вероятности[i, j] = v;

                    commonS += counts[i, j] - sizeb;

                    if (v < минимальнаяВероятность)
                        минимальнаяВероятность = v;

                    сортированные.Add(v);
                }
            }

            сортированные.Sort();

            int len = сортированные.Count;
            int k = 0;
            var ve = erfc(0.0, 1e-6);
            var lastVe = ve;
            double sup, d, a;
            /*
            using (var file = new FileStream("getCountsResult_" + DateTime.Now.ToFileTime() + ".txt", FileMode.CreateNew, FileAccess.Write))
            {
                int kl = 0;
                double dl = ve;
                for (int j = len - 1; j >= 0; j--)
                {
                    d = (double)(j+1)/len;
                    while (dl > d)
                    {
                        kl++;
                        dl = erfc(kl / pw / 1.4142135623730950488016887242097, 1e-6);
                    }

                    var str = Encoding.ASCII.GetBytes(dl + "\t" + сортированные[j] + "\r\n");
                    file.Write(str, 0, str.Length);
                }
            }*/

            for (int j = len - 1; j >= 0; j--)
            {
                d = (double)(j+1)/len;
                while (ve > d)
                {
                    lastVe = ve;
                    k++;
                    ve = erfc(k / pw / 1.4142135623730950488016887242097, 1e-6);
                }
                d = ve*0.999999;
                sup = lastVe*1.000001;

                a = сортированные[j];
                if (a*множительВероятностей < d)
                {
                    множительВероятностей = d / a;
                }

                if (a < d)
                {
                    /*if (сортированные[j] <= 0)
                        отклонениеОтЭталона = double.PositiveInfinity;
                    else*/
                    var val = Math.Pow((d - a)/d, 2);
                    отклонениеОтЭталона += val;
                    отклонениеОтЭталонаВниз += val;
                }
                else
                if (a > sup)
                {
                    /*if (сортированные[j] <= 0)
                        отклонениеОтЭталона = double.PositiveInfinity;
                    else*/
                    var val = Math.Pow(a - sup, 2);
                    отклонениеОтЭталона += val;
                    отклонениеОтЭталонаВверх += val;
                }
            }

            commonS = Math.Abs(commonS);
            commonS /= Math.Pow(sizeb, 3.0/2.0)*1.4142135623730950488016887242097; // n/sqrt(sizeb*sizeb*sizeb*2)
            суммарнаяВероятность = erfc(commonS / 1.4142135623730950488016887242097);

            отклонениеОтЭталона      /= len;
            отклонениеОтЭталонаВниз  /= len;
            отклонениеОтЭталонаВверх /= len;
        }


        private static void toResult(int sizeb, int index1, int index2, byte[] result, int[,] counts)
        {
            for (int i = 0; i < sizeb; i++)
            {
                if (getBitFromArray(result, i))
                {
                    counts[index1, i]++;
                    counts[index2, i]++;
                }
            }
        }

        //             ERFC function below
        // ----------------------------------------------

        private static double expt2(double t)
        {
            return Math.Exp(-t*t);
        }

        /// <summary>
        /// Определяет шаг для вычисления erfc2
        /// </summary>
        /// <param name="t">Параметр функции erfc2</param>
        /// <param name="tolerance">Параметр точности. Это не точность, но чем меньше, тем точнее получится результат.</param>
        /// <returns></returns>
        private static double _erfc2_tau(double t, double tolerance = 1e-3, double defTau = 5, double s = 0.0)
        {
            // const double c = 1e-6;

            //double s, s2, s3, tol0 = tolerance, T = t;
            double add, add2, inaccuracy2, tau = defTau, tauMin = 0.0, tauMax = 10, inaccuracy, valuet = expt2(t), a, kInacc, Z = s;

            if (t < -2.0)
            {
                tauMin = tolerance * 100;
                if (tauMin > 1.0)
                    tauMin = 1.0;
            }
            kInacc = 32*Math.Pow(1e-3 / tolerance, 0.25);

            int cnt = 0;
            do
            {
                var k1 = valuet * tau;
                var k2 = expt2(t + 0.5*tau) * tau;
                var k4 = expt2(t + tau) * tau;

                // var k2_3 = expt2(0, t + tau/3.0) * tau;
                // var k3_3 = expt2(0, t + tau*2.0/3.0) * tau;

                add  = 0.16666666666666666666666666666667 * (k1 + 2.0*k2 + 2.0*k2 + k4);
                //add2 = (k1+k2+k4) / 3.0; //0.25 * (k1+3.0*k3_3);
                add2 = (k1 + k2) / 2.0;
/*
                inaccuracy2 = Math.Abs(add-add2);
                inaccuracy3 = Math.Abs(add-add3);

                if (inaccuracy2 > inaccuracy3)
                    inaccuracy = inaccuracy2;
                else
                    inaccuracy = inaccuracy3;*/

                if (Z < tolerance && Z < add)
                    if (add > 0.0)
                        inaccuracy = Math.Abs((add-add2)/add);
                    else
                        inaccuracy = Math.Abs(add-add2);
                else
                    inaccuracy = Math.Abs((add-add2)/Z);

                if (inaccuracy > tolerance)
                {
                    if (tauMax > tau)
                        tauMax = tau;
                }
                if (inaccuracy < tolerance)
                {
                    if (tauMin < tau)
                        tauMin = tau;
                }

                a = tauMax - tauMin;
                if (inaccuracy < tolerance*1e-2)
                {
                    a /= 4.0;
                }
                else
                if (inaccuracy > tolerance*1e+1)
                {
                    a /= 4.0;
                }
                else
                    a /= 2.0;

                inaccuracy2 = Math.Abs((add-add2) / s);

                if (inaccuracy < tolerance*0.5 || inaccuracy2*kInacc < tolerance*1e-1)
                {
                    if (tau > 5)
                        return tau;

                    tau = tauMax - a;
                }
                else
                if (inaccuracy > tolerance)
                {
                    tau = tauMin + a;
                }
                else
                    return tau;

                // Это для случаев, когда tauMin = 1.0; или когда tolerance слишком мал
                if (tauMin > 0.0 && (tauMax - tauMin) / tauMin < 0.33)
                    return tauMin;

                if (cnt++ > 256)
                {
                    throw new Exception("_erfc2_tau");
                    // break;
                }
            }
            while (true);
        }

        private static double erfc(double s, double tolerance = 1e-3, bool isRecursive = false, double toleranceA = 1e-3)
        {
            if (tolerance < 1e-12)
                throw new ArgumentOutOfRangeException("erfc: tolerance < 1e-12");

            if (s == 0)
                return 1.0;

            if (s < 0)
                return 1.0 + erfc_(s, tolerance, 0, tolerance, 0.0);

            return erfc_(s, tolerance);
        }

        // erfc(s/2^0.5)
        private static double erfc_(double s, double tolerance = 1e-3, int isRecursive = 0, double toleranceA = 1e-3, double stop = 30)
        {
            if (s == stop)
                return 0.0;

            if (s < 0 && stop > 0.0)
            {
                if (stop >= 30)
                    return erfc_(s, tolerance, isRecursive, toleranceA, 0.0) + 1.0;

                return erfc_(s, tolerance, isRecursive, toleranceA, 0.0) + erfc_(0.0, tolerance, isRecursive, toleranceA, stop);
            }

            if (isRecursive > 256)
                throw new Exception("_erfc2 isRecursive > 256");

            var minusFlag = false;
            if (stop <= 0.0 && s < 0)
                minusFlag = true;

            // 2/sqrt(2*pi)
            const double k = 1.1283791670955125738961589031215;
            // exp(-t^2)

            if (isRecursive == 0)
                toleranceA = tolerance;

            // dx/dt = exp(-t^2);
            double result = 0.0, rs = 0.0, inaccuracy;

            if (s < -30.0)
                s = -30.0;
            if (s >= 30.0)
                return 0.0;

            /*
            if (estimatedCnt < 0)
                // (20-s)/40 убрал, т.к. впечатление, что это понижение только хуже делает
                estimatedCnt = (int) Math.Ceiling(   64.0 * Math.Pow(1e-3 / tolerance, 0.333333)   );
                
            if (estimatedCnt < 2)
                estimatedCnt = 2;
                */
            double add = 0, add2 = 0;
            double t = s;
            int cnt = 0, errorcnt = 0;
            double kTolerance = 64; // 1*Math.Pow(1e-3 / tolerance, 0.25);

            if (kTolerance < 1.0)
                kTolerance = 1.0;

            if (minusFlag)
            {
                if (stop == 0.0)
                {
                    t      = 0.0;
                    //summ   = 1.0;
                }
                else
                {
                    t      = stop;
                    //summ   = 1.0 - erfc_(stop, 1e-1, 0, 1e-1, 0.0);
                }
            }

            double tau = _erfc2_tau(t, tolerance, 5, result);
            double ntau = 0.0;
            if (minusFlag)
            {
                tau *= -1.0;
            }

            do
            {
                if (minusFlag)
                {
                    if (t+tau < s)
                    {
                        tau = s - t;
                    }
                }
                else
                {
                    if (t+tau > stop)
                    {
                        tau = stop - t;
                    }
                }

                var k1 = expt2(t) * tau;
                var k2 = expt2(t + 0.5*tau) * tau;
                var k4 = expt2(t + tau) * tau;

                var k1_2 = k1 * 0.5;
                var k2_2 = expt2(t + 0.25*tau) * tau*0.5;
                var k4_2 = k2 * 0.5;

                var k1_3 = k4_2;
                var k2_3 = expt2(t + 0.75*tau) * tau*0.5;
                var k4_3 = k4 * 0.5;

                add  = 0.16666666666666666666666666666667 * (k1_2+k1_3 + 4.0*(k2_2 + k2_3) + k4_2+k4_3);
                add2 = 0.16666666666666666666666666666667 * (k1 + 4.0*k2 + k4);

                if (double.IsNaN(add) || double.IsNaN(add2))
                    throw new Exception("erfc2");

                inaccuracy = Math.Abs(add-add2);
                if (result > 0.0)
                {
                    inaccuracy /= result;
                }
                else
                if (Math.Abs(add) > 0)
                {
                    inaccuracy /= Math.Abs(add);
                }

                ntau = tau;
                if (inaccuracy*kTolerance > tolerance)
                {
                    tau = tau * 0.5; //Math.Pow(inaccuracy/tolerance, 0.25)*2.0;
                    errorcnt++;

                    if (errorcnt > 256)
                        throw new Exception("_erfc2: inaccuracy");

                    if (minusFlag)
                    {
                        if (tau > -1e-15)
                            tau = -1e-15;
                    }
                    else
                    {
                        if (tau < 1e-15)
                            tau = 1e-15;
                    }

                    continue;
                }
                else
                if (inaccuracy*32.0*kTolerance < tolerance)
                {
                    ntau = tau*1.41;
                }
                
                if (minusFlag)
                {
                    if (t == s || t < -30)
                        break;
                }
                else
                {
                    if (t == stop || t > 30)
                        break;
                }

                errorcnt = 0;
                if (minusFlag)
                {
                    // add и add2 - отрицательные. Но мы их прибавляем
                    result -= add;
                    rs -= add2;
                    t += tau;
                }
                else
                {
                    result += add;
                    rs += add2;
                    t += tau;
                }

                //tau = _erfc2_tau(t, tolerance, tau, summ > result ? summ : result);
                cnt++;

                tau = ntau;
            }
            while (true);

            result *= k;
            rs *= k;
            
            inaccuracy = Math.Abs((result - rs) / result);
            if (result > 0 && inaccuracy > toleranceA)
                return erfc_(s, tolerance*tolerance/inaccuracy * 0.5, isRecursive+1, toleranceA, stop);

            if (result > 2.0)
                return 2.0;

            if (result < 0.0)
                return 0.0;

            return result;
        }
    }
}

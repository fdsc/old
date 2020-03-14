using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tests;

namespace options
{
    public partial class Random8
    {
        static int[] testStat, testStat2, testStat3;

        public static void setToNull(int[] stat)
        {
            for (int i = 0; i < stat.Length; i++)
                stat[i] = 0;
        }

        // [Test]
        public static TestMethodResult test()
        {// return new TestMethodResult(TestMethodResult.generalTestResult.skipped);
            testStat  = new int[256];
            testStat2 = new int[65536];
            testStat3 = new int[65536];
            setToNull(testStat);
            setToNull(testStat2);
            setToNull(testStat3);
            int N = 1024 * 10;

            var logFileName = typeof(Random8).FullName.Replace('.', '-') + ".log";

            System.IO.File.WriteAllText(logFileName, "");

            var rnd = Random8.Create();
            var r = new Random();
            int l = 0, l2 = 0;
            for (int i = 0; i < 256 * N; i++)
            {
                var t = // r.Next(0, 256);
                    rnd.nextByte();

                testStat [t]++;
                testStat2[(l  << 8) + t]++;
                testStat3[(l2 << 8) + t]++;
                l2 = l;
                l = t;
                /*
                System.IO.File.AppendAllText(logFileName, t.ToString("D3") + " ");

                if (i % 16 == 15)
                    System.IO.File.AppendAllText(logFileName, "\r\n");
                 * */
            }

            double s = 0, s2 = 0, s3 = 0; int si2 = 0;
            for (int i = 0; i < testStat.Length; i++)
            {
                s   = Math.Max(s, Math.Abs(testStat[i] - N) / (double) N);
            }

            for (int i = 0; i < testStat2.Length; i++)
            {
                var s2c = Math.Abs(testStat2[i] - (double) N / (double) 256) / (double) N * (double) 256;
                var s3c = Math.Abs(testStat3[i] - (double) N / (double) 256) / (double) N * (double) 256;
                if (s2c > s2)
                {
                    s2  = s2c;
                    si2 = i;
                }

                if (s3c > s3)
                {
                    s3  = s3c;
                    //si2 = i;
                }
            }
//                    return new TestMethodResult(TestMethodResult.generalTestResult.errorFound);

            testStat = null;
            var tmp = "" + si2;
            if (s > 0.045)
                return new TestMethodResult(TestMethodResult.generalTestResult.errorFound);

            return new TestMethodResult(TestMethodResult.generalTestResult.success);
        }
    }
}

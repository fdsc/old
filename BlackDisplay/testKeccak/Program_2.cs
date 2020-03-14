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
        private static void CheckGamma(byte[] gamma, out float cgr1, out float cgr2, out float cgr)
        {
            int[]  c  = new int[256];
            int[,] c1 = new int[256, 256];
            int[,] c2 = new int[256, 256];

            int gl = gamma.Length;

            int k = 0, t = 0;
            for (int i = 0; i < gl; i++)
            {
                c[gamma[i]]++;
                if (k + t >= gl)
                {
                    k = 0;
                    t++;
                }

                c1[gamma[i], gamma[(i+128)%gl]]++;
                c2[gamma[i], gamma[k+t]]++;

                k += 3;
            }

            cgr = 0; cgr1 = 0; cgr2 = 0;
            int CR  = gl / 256;
            int CR2 = gl / 65536;
            for (int i = 0; i < 256; i++)
            {
                float a = Math.Abs(   (float) (c[i] - CR) / (float) CR   );
                if (a > cgr)
                    cgr = a;

                for (int j = 0; j < 256; j++)
                {
                    a = Math.Abs(   (float) (c1[i, j] - CR2) / (float) CR2   );
                    if (a > cgr1)
                        cgr1 = a;

                    a = Math.Abs(   (float) (c2[i, j] - CR2) / (float) CR2   );
                    if (a > cgr2)
                        cgr2 = a;
                }
            }
        }

        private static void duplexModTest(ref int errorFlag)
        {
            var keccak = new SHA3(0);
            byte[] a, b, c, e;
            a = new byte[72*3];
            b = new byte[72*3];

            e = keccak.getDuplexMod(a, b);
            c = keccak.getDuplexMod(a, b);
            if (!BytesBuilder.Compare(c, e))
                goto error;

            c = keccak.getDuplexMod(a, b, true);
            if (e[0] == c[0])
                goto error;

            b[b.Length - 1] = 1;
            c = keccak.getDuplexMod(a, b);
            int i;
            BytesBuilder.Compare(c, e, out i);
            if (i != 72*2)
                goto error;

            b[b.Length - 1] = 0;
            b[b.Length - 72] = 1;
            c = keccak.getDuplexMod(a, b);
            BytesBuilder.Compare(c, e, out i);
            if (i != 72*2)
                goto error;

            b[b.Length - 72] = 0;
            b[b.Length - 144] = 1;
            c = keccak.getDuplexMod(a, b);
            BytesBuilder.Compare(c, e, out i);
            if (i != 72 || e[72*2] == c[72*2])
                goto error;

            b[b.Length - 144] = 0;
            b[b.Length - 145] = 1;
            c = keccak.getDuplexMod(a, b);
            BytesBuilder.Compare(c, e, out i);
            if (i != 0 || e[72*2] == c[72*2] || e[72] == c[72])
                goto error;

            b[b.Length - 145] = 0;
            b[0] = 1;
            c = keccak.getDuplexMod(a, b);
            BytesBuilder.Compare(c, e, out i);
            if (i != 0 || e[72*2] == c[72*2] || e[72] == c[72])
                goto error;

            return;

            error:
                Interlocked.Increment(ref errorFlag);
                Console.WriteLine("duplexModTest is incorrect");
        }

        private static void GammaTest(ref int errorFlag)
        {
            var keccak = new SHA3(0);
            byte[] k1= new byte[360], k2 = new byte[64], k3 = new byte[64], a = new byte[64], b = new byte[64], e1 = new byte[64], e2 = new byte[64];
            byte[][] g1 = new byte[10][];
            byte[][] g2 = new byte[10][];

            var keys = new List<byte[]>();
            var oi   = new List<byte[]>();
            keys.Add(k3);
            keys.Add(k2);
            keys.Add(k1);
            oi  .Add(e1);
            oi  .Add(e2);
            a[0]  = 1;
            b[63] = 1;

            int gr = 10;
            GetGammaForCheck(keccak, k2, k3, a, b, g1, g2, keys, oi, gr);

            byte[] numberse = new byte[] {0, 7};
            foreach (var n in numberse)
                foreach (var m in numberse)
                    if (n < m)
                        if (!BytesBuilder.Compare(g1[n], g1[m]))
                            goto error;

            byte[] numbersne = new byte[] {0, 1, 2, 3, 4, 5, 6, 8, 9};
            foreach (var n in numbersne)
                foreach (var m in numbersne)
                    if (n < m)
                        if (BytesBuilder.Compare(g1[n], g1[m]))
                            goto error;

            gr = 11;
            GetGammaForCheck(keccak, k2, k3, a, b, g1, g2, keys, oi, gr);

            numberse = new byte[] {0, 7};
            foreach (var n in numberse)
                foreach (var m in numberse)
                    if (n < m)
                        if (!BytesBuilder.Compare(g1[n], g1[m]))
                            goto error;

            numbersne = new byte[] {0, 1, 2, 3, 4, 5, 6, 8, 9};
            foreach (var n in numbersne)
                foreach (var m in numbersne)
                    if (n < m)
                        if (BytesBuilder.Compare(g1[n], g1[m]))
                            goto error;


            gr = 12;
            GetGammaForCheck(keccak, k2, k3, a, b, g1, g2, keys, oi, gr);

            numberse = new byte[] {0, 7};
            foreach (var n in numberse)
                foreach (var m in numberse)
                    if (n < m)
                        if (!BytesBuilder.Compare(g1[n], g1[m]))
                            goto error;

            numbersne = new byte[] {0, 1, 2, 3, 4, 5, 6, 8, 9};
            foreach (var n in numbersne)
                foreach (var m in numbersne)
                    if (n < m)
                        if (BytesBuilder.Compare(g1[n], g1[m]))
                            goto error;


            return;

            error:
                Interlocked.Increment(ref errorFlag);
                Console.WriteLine("GammaTest is incorrect");
        }

        private static void GetGammaForCheck(SHA3 keccak, byte[] k2, byte[] k3, byte[] a, byte[] b, byte[][] g1, byte[][] g2, List<byte[]> keys, List<byte[]> oi, int gr)
        {
            keccak.prepareGammaRegime10(keys, oi, gr, 64, out g1[0], out g2[0], null);
            keys[0] = a;
            keccak.prepareGammaRegime10(keys, oi, gr, 64, out g1[1], out g2[1], null);
            keys[0] = b;
            keccak.prepareGammaRegime10(keys, oi, gr, 64, out g1[2], out g2[2], null);
            keys[0] = k3;
            keys[1] = b;
            keccak.prepareGammaRegime10(keys, oi, gr, 64, out g1[3], out g2[3], null);
            keys[0] = k3;
            keys[1] = k2;
            keys[2][0] = 1;
            keccak.prepareGammaRegime10(keys, oi, gr, 64, out g1[4], out g2[4], null);
            keys[2][0] = 0;
            oi[0][0] = 1;
            keccak.prepareGammaRegime10(keys, oi, gr, 64, out g1[5], out g2[5], null);
            oi[0][0] = 0;
            oi[1][0] = 1;
            keccak.prepareGammaRegime10(keys, oi, gr, 64, out g1[6], out g2[6], null);
            oi[1][0] = 0;
            keccak.prepareGammaRegime10(keys, oi, gr, 64, out g1[7], out g2[7], null);
            keys[2][128] = 1;
            keccak.prepareGammaRegime10(keys, oi, gr, 64, out g1[8], out g2[8], null);
            keys[2][128] = 0;
            keys[2][359] = 1;
            keccak.prepareGammaRegime10(keys, oi, gr, 64, out g1[9], out g2[9], null);
            keys[2][359] = 0;
        }

        private static bool checkGammaByHash(SHA3 sha3)
        {
            byte[] keyAndMessage = new byte[72 * 8 - 1];
            byte[] key           = new byte[64];
            byte[] message       = new byte[72 * 7 - 1];

            var rnd = new Random(98732796);
            for (int i = 0; i < 64; i++)
                keyAndMessage[i] = (byte) rnd.Next(0, 255);

            keyAndMessage[64] = 0x01;
            keyAndMessage[71] = 0x80;

            for (int i = 72; i < keyAndMessage.Length; i++)
                keyAndMessage[i] = (byte) rnd.Next(0, 255);

            rnd = new Random(98732796);
            for (int i = 0; i < 64; i++)
                key[i] = (byte) rnd.Next(0, 255);
            for (int i = 0; i < message.Length; i++)
                message[i] = (byte) rnd.Next(0, 255);

            byte[] hash  = sha3.getHash512(keyAndMessage);
            sha3.prepareGamma(key, message);
            byte[] gamma = sha3.getGamma(64, true);

            bool errorFlag = false;
            for (int i = 0; i < hash.Length; i++)
                if (hash[i] != gamma[i])
                    errorFlag = true;

            if (hash.Length != gamma.Length)
                errorFlag = true;

            if (errorFlag)
                Console.WriteLine("Gamma and hash unequal, gamma is incorrect");
            else
                Console.WriteLine("Well. Gamma and hash equal, gamma is correct.");

            return !errorFlag;
        }

        private static bool checkDuplexByHash(SHA3 sha3)
        {
            byte[] keyAndMessage = new byte[72 * 8 - 1];

            var rnd = new Random(98732796);
            for (int i = 0; i < 64; i++)
                keyAndMessage[i] = (byte) rnd.Next(0, 255);

            keyAndMessage[64] = 0x01;
            keyAndMessage[71] = 0x80;

            for (int i = 72; i < keyAndMessage.Length; i++)
                keyAndMessage[i] = (byte) rnd.Next(0, 255);

            byte[] hash  = sha3.getHash512(keyAndMessage);
            byte[] gamma = sha3.getDuplex(keyAndMessage);

            bool errorFlag = false;
            for (int i = 0; i < hash.Length; i++)
                if (hash[i] != gamma[gamma.Length - 72 + i])
                    errorFlag = true;

            if (errorFlag)
                Console.WriteLine("Duplex and hash unequal, duplex is incorrect");
            else
                Console.WriteLine("Well. Duplex and hash equal, duplex is correct.");

            return !errorFlag;
        }

        private static bool checkInitDuplex(SHA3 sha3)
        {
            byte[] keyAndMessage = new byte[72 * 8 - 1];

            var rnd = new Random(98732796);
            for (int i = 0; i < 64; i++)
                keyAndMessage[i] = (byte) rnd.Next(0, 255);

            keyAndMessage[64] = 0x01;
            keyAndMessage[71] = 0x80;

            for (int i = 72; i < keyAndMessage.Length; i++)
                keyAndMessage[i] = (byte) rnd.Next(0, 255);

            var kam1 = new byte[64];
            var kam2 = new byte[keyAndMessage.Length - 72];
            BytesBuilder.CopyTo(keyAndMessage, kam1);
            BytesBuilder.CopyTo(keyAndMessage, kam2, 0, -1, 72);

            byte[] dupi1 = sha3.getDuplex(kam1);
            byte[] dupi2 = sha3.getDuplex(kam2, true);
            byte[] dupe  = sha3.getDuplex(keyAndMessage);

            bool errorFlag = false;
            int k;
            for (k = 0; k < dupi1.Length; k++)
                if (dupi1[k] != dupe[k])
                    errorFlag = true;
            for (int j = 0; j < dupi2.Length/* && k < dupe.Length*/; k++, j++)
                if (dupi2[j] != dupe[k])
                    errorFlag = true;

            if (errorFlag)
                Console.WriteLine("Duplex and init duplex unequal, duplex is incorrect");
            else
                Console.WriteLine("Well. Duplex and init duplex equal, duplex is correct.");

            return !errorFlag;
        }

        private static bool checkDuplexModByHash(SHA3 sha3)
        {
            byte[] m1 = new byte[72 * 8 - 1];
            byte[] m2 = new byte[72 * 8 - 1];
            byte[] m  = new byte[m1.Length + m2.Length + 1];

            var rnd = new Random(98732796);
            for (int i = 0; i < m.Length; i++)
                if (m.Length - i == 72)
                    m[i] = 0x81;
                else
                    m[i] = (byte) rnd.Next(0, 255);

            rnd = new Random(98732796);
            for (int i = 0, j = 0, k = 0; i < m.Length - 1; i++)
                if (i % 144 >= 72 || k >= m2.Length)
                {
                    m1[j++] = (byte) rnd.Next(0, 255);
                }
                else
                {
                    m2[k++] = (byte) rnd.Next(0, 255);
                }

            byte[] hash  = sha3.getHash512(m);
            byte[] gamma = sha3.getDuplexMod(m1, m2);
            byte[] tmp   = new byte[64];
            BytesBuilder.CopyTo(gamma, tmp, 0, -1, 72*7);

            bool errorFlag = false;
            for (int i = 0; i < hash.Length; i++)
                if (hash[i] != tmp[i])
                    errorFlag = true;

            if (errorFlag)
                Console.WriteLine("DuplexMod and hash unequal, duplexMod is incorrect");
            else
                Console.WriteLine("Well. DuplexMod and hash equal, duplexMod is correct.");

            return !errorFlag;
        }
    }
}

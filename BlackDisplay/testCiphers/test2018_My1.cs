using keccak;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace testCiphers
{
    class test2018_My1: test2018
    {
        public test2018_My1()
        {
            keyLen = 2;
        }

        int c1f = 0;
        int c1t = 0;
        public override void ProcessCryptoVariant(long variant)
        {
            var key = prepareKey(variant);
            var s = prepareOIV(variant);

            var c     = new VinHC1A(key, 4);
            var gamma = c.getGamma(numOfOutBytes);

            if (gamma[0] == 0)
            {
                if ((variant & 1) > 0)
                    c1f++;
                else
                    c1t++;
            }

            calcResult(gamma, variant, key);
        }
    }

    class VinHC1A
    {
        public static long[] SN;
        static VinHC1A()
        {
            var l = File.ReadAllLines("simplies.txt");
            SN = new long[l.Length];
            for (int i = 0; i < l.Length; i++)
                SN[i] = long.Parse(l[i]);
        }

        protected readonly byte[] key;

        protected class Automate
        {
            byte[,,] matrix;
            long[]   nodes;
            byte[]   key;

            int kl1;
            int kl2;
            public Automate(byte[] key, int nodeCount = -1)
            {
                this.key = key;

                kl1 = key.Length;
                kl2 = nodeCount == -1 ? kl1 : nodeCount;
                if (kl2 < 4)
                    kl2 = 4;

                kl2 >>= 1;
                kl2 <<= 1;

                // kl2 = (int) Math.Pow(key.Length << 2, 0.5);
                matrix = new byte[kl2, kl2, kl2];

                nodes = new long[kl2];

                int k = 0;
                int s = 0;
                for (int i = 0; i < kl2; i++)
                for (int j = 0; j < kl2; j++)
                for (int m = 0; m < kl2; m++)
                {
                    //matrix[i, j, m] = (byte) (getFromKey(key, k, s) + i + j + m);
                    matrix[i, j, m] = (byte) (getFromKey(key, k, s));
                    k++;
                    if (k >= key.Length)
                    {
                        k = 0;
                        s++;
                        if (s > 7)
                            s = 0;
                    }
                }

                for (int i = 0; i < kl2; i++)
                    nodes[i] = i;//SN[i];

                fullIterate();
            }
            /*
            private byte getFromKey(byte[] key, int k)
            {
                var k1 = k >> 2;
                var k2 = (byte) (k & 3);
                if (k1 < key.Length)
                {
                    var v =  key[k1];

                    var s = v >> (k2 << 1);
                    s &= 3;

                    return (byte) s;
                }

                return k2;
            }*/

            private byte getFromKey(byte[] key, int k, int s)
            {
                if (k > 0)
                {
                    var v =  (key[k-1] << 8) + key[k];

                    v >>= s;

                    return (byte) v;
                }
                else
                {
                    var v =  (key[key.Length - 1] << 8) + key[0];

                    v >>= s;

                    return (byte) v;
                }
            }
            /*
            public static long normalize(long a)
            {
                while (a >= 256)
                {
                    var t = a >> 8;
                    a -= t << 8;
                    a += t;
                }

                return a;
            }
            */
            public void fullIterate(int count = 0)
            {
                if (count <= 0)
                    count = nodes.Length;

                iterate();
                for (int i = 0; i < count; i++)
                {
                    // nodes[0] = normalize(nodes[0] + SN[i]);
                    iterate();
                }
            }

            public void iterate()
            {
                for (int p = 0; p < 256; p++)
                for (int i = 0; i < kl2; i++)
                for (int j = 0; j < kl2; j++)
                for (int m = 0; m < kl2; m++)
                {
                    if (i == j || i == m || j == m)
                        continue;

                    var op = matrix[i, j, m];
                        op = (byte) (op + i + j + m);

                    if (op != p)
                        continue;

                    var v1 = nodes[i];
                    var v2 = nodes[j];
                    var v3 = nodes[m];

                    if (i != j)
                    {
                        var vi = nodes[i];
                        var vj = nodes[j];
                        var vm = nodes[m];

                        nodes[m] = vi;
                        nodes[i] = vj;
                        nodes[j] = vm;
                    }
                    /*
                    if (op == 0)
                    {
                        var v = ~v2;
                        v1 ^= v;
                        v1 += v2;

                        nodes[i] = normalize(v1);
                    }
                    else
                    if (op == 1)
                    {
                        var m = j < i ? i - j : j - i;

                        v1 += v2 * SN[m];

                        nodes[i] = normalize(v1);
                    }
                    else
                    if (op == 2)
                    {
                        v1 += v1 * v2 + v1 + v2;

                        nodes[i] = normalize(v1);
                    }
                    else
                    if (op == 3)
                    {
                        var v = ~v2 & v1;

                        nodes[i] = normalize(v1 + v + v2);
                    }
                    else
                        throw new ArgumentOutOfRangeException("test2018_My1 MVZiG5ECHn8Z");*/
                }
            }

            public void addToState(byte newByte, int count = 0)
            {
                for (int i = 0; i < nodes.Length; i++)
                    nodes[i] ^= newByte;

                fullIterate(count);
            }

            int current = 0;
            protected byte getResult2()
            {
                long r = 0;
                /*for (int i = 0; i < kl2; i++)
                    r += nodes[i];*/

                r = nodes[current++];

                if (current >= nodes.Length)
                    current = 0;

                return (byte) (r & 1);
            }

            protected byte getResult8()
            {
                byte r = 0;
                r += getResult2();
                r <<= 1;
                iterate();

                r += getResult2();
                r <<= 1;
                iterate();

                r += getResult2();
                r <<= 1;
                iterate();

                r += getResult2();
                r <<= 1;
                iterate();

                r += getResult2();
                r <<= 1;
                iterate();

                r += getResult2();
                r <<= 1;
                iterate();

                r += getResult2();
                r <<= 1;
                iterate();

                r += getResult2();
                iterate();

                // return (byte) nodes[nodes.Length - 1];
                return r;
            }

            public void getResult(byte[] result, int start = 0, int end = -1)
            {
                if (end < 0)
                    end = result.Length;

                for (int i = start; i < end; i++)
                {
                    result[i] = getResult8();
                    fullIterate();
                    // iterate();
                }
            }
        }

        protected readonly Automate A;
        public VinHC1A(byte[] key, int nodeCount = -1)
        {
            this.key = key;

            key[0] = (3 << 4) + (2 << 2) + 1;

            A = new Automate(key, nodeCount);
        }

        public byte[] getGamma(int numOfOutBytes)
        {
            var b = new byte[numOfOutBytes];
            A.getResult(b);

            return b;
        }
    }
}

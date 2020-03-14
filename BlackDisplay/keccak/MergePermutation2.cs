using System;
using System.Threading;

namespace keccak
{
    public unsafe class MergePermutation2
    {
        public readonly static object staticSync = new object();
        protected static byte[] permutations = null;
        protected static int step = 0;

        protected static void initPermutation()
        {
            lock (staticSync)
            {
                if (permutations != null)
                    return;

                sbyte[] ac = { -1, -1, -1, -1, -1, -1, -1 };
                step = ac.Length;
                permutations = new byte[5040*step];

                int k = permutations.Length / step - 1;
                fixed (byte * perm = permutations)
                {
                    initPrm(perm, ref k, ac, 0);
                }
            }
        }

        private static void initPrm(byte * v1, ref int index, sbyte[] a, sbyte v2)
        {
            if (v2 == step)
            {
                var k = index--;
                for (sbyte i = 0; i < step; i++)
                {
                    v1[k*step + a[i]] = (byte) i;
                }
                return;
            }

            bool isRecurse = false;
            for (int i = 0; i < a.Length; i++)
            {
                if (a[i] == -1)
                {
                    a[i] = v2;
                    initPrm(v1, ref index, a, (sbyte) (v2 + 1));
                    a[i] = -1;
                    isRecurse = true;
                }
            }

            if (!isRecurse)
                initPrm(v1, ref index, a, (sbyte) (v2 + 1));
        }

        public static int permutationMergeBytes(byte[] key, byte[] target, int maxK = 0)
        {
            initPermutation();

            fixed (byte* k = key, t = target)
            {
                permutationMergeBytes(k, key.Length, t, target.Length);
            }

            return 0;
        }

        public static int permutationMergeBytes(byte * key, int keyLen, byte * target, int len, int maxK = 0)
        {
            if (len <= 1)
                throw new ArgumentOutOfRangeException("len", "len <= 1");

            if (keyLen <= 1)
                throw new ArgumentOutOfRangeException("keyLen", "keyLen <= 1");

            int tasks   = System.Environment.ProcessorCount;
            if (maxK > 0 && tasks > maxK)
                tasks = maxK;

            int cTasks = tasks;

            byte[] tmpManaged = new byte[step*tasks];
            int currentIndex = 0;
            int keyIndex = 0;
            object sync = new object();

            for (int k = 0; k < tasks; k++)
            {
                var k_ = k;
                ThreadPool.QueueUserWorkItem
                (delegate
                {
                    fixed (byte * tmp = tmpManaged, prm = permutations)
                    {
                        int threadStep = 2;
                        int threadIndex = 0;
                        int threadKeyIndex = 0;

                        do
                        {
                            lock (sync)
                            {
                                if (currentIndex <= len - step)
                                {
                                    threadIndex    = currentIndex;
                                    threadKeyIndex = keyIndex;
                                    currentIndex  += step << threadStep;
                                    keyIndex      += 2 << threadStep;

                                    if (keyIndex >= keyLen - 1)
                                        keyIndex = 0;
                                }
                                else
                                    break;
                            }

                            int ts = 1 << threadStep;
                            for (int i = 0; i < ts; i++)
                            {
                                if (threadKeyIndex >= keyLen - 1)
                                    threadKeyIndex = 0;
                                if (threadIndex > len - step)
                                    break;

                                perm(prm, key, threadKeyIndex, keyLen, target + 0, threadIndex, tmp, k_, 0);
                                threadKeyIndex += 2;
                                threadIndex    += step;
                            }
                        }
                        while (true);

                        lock (sync)
                        {
                            cTasks--;
                            Monitor.Pulse(sync);
                        }
                    }
                });
            }

            lock (sync)
            {
                while (cTasks > 0)
                    Monitor.Wait(sync);
            }

            return 0;
        }

        public static Int16 rol16(Int16 val, int n)
        {
            return (Int16) (   (val << n) | (val >> (16 - n))   );
        }

        private static void perm(byte * prm, byte* key, int j, int keyLen, byte* target, int i, byte * tmp, int k, int toRol)
        {
            var k0 = tmp + k*step;
            byte * i0 = key + j;
            BytesBuilder.CopyTo(step, step, target + i, k0);

            Int16 i1 = (Int16) (   i0[0] + (i0[1] << 8)   );
            if (toRol > 0)
                rol16(i1, toRol);

            i1 &= 4095;
            byte * prm0 = prm + i1*step;

            for (int p = 0; p < step; p++)
            {
                target[i + p] = k0[prm0[p]];
            }
        }
    }

    public class MergePermutation2Test: MergePermutation2
    {
        public static int initPermutation_Test()
        {
            int errors = 0;

            try
            {
                initPermutation();

                int[]  countOfValues    = new int[step];
                int[,] countOfPositions = new int[step, step];

                for (int i = 0; i < permutations.Length; i++)
                {
                    countOfValues[permutations[i]]++;
                    countOfPositions[permutations[i], i % step]++;
                }

                var val = countOfValues[0];
                for (int i = 1; i < countOfValues.Length; i++)
                {
                    if (countOfValues[i] != val)
                    {
                        errors++;
                        break;
                    }
                }

                val = countOfPositions[0, 0];
                for (int i = 0; i < countOfPositions.GetLength(0); i++)
                for (int j = 0; j < countOfPositions.GetLength(1); j++)
                {
                    if (countOfPositions[i, j] != val)
                    {
                        errors++;
                        break;
                    }
                }
            }
            catch
            {
                errors++;
            }

            return errors;
        }

        static byte[] permutationMergeBytes_Test_AA1, permutationMergeBytes_Test_AA2;
        public static int permutationMergeBytes_Test()
        {
            byte[] key  = new byte[64];
            byte[] trg1 = new byte[256];
            byte[] trg2 = new byte[256];
            permutationMergeBytes_Test_AA1 = trg1;
            permutationMergeBytes_Test_AA2 = trg2;

            for (int i = 0; i < trg1.Length; i++)
            {
                trg1[i] = (byte) i;
                trg2[i] = (byte) i;
                key[i % key.Length] = (byte) i;
            }

            permutationMergeBytes(key, trg1);
            permutationMergeBytes(key, trg2, 1);

            for (int i = 0; i < trg1.Length; i++)
            {
                if (trg1[i] != trg2[i])
                    return 1;
            }

            for (int i = 0; i < trg1.Length; i++)
            {
                if (trg1[i] != trg2[i])
                    return 1;
            }

            for (int j = 0; j < trg1.Length; j++)
            {
                int a = 0;
                for (int i = 0; i < trg1.Length; i++)
                {
                    if (trg1[i] == j)
                    {
                        a++;
                        if (a > 1)
                            return 1;
                    }
                }
            }

            return 0;
        }
    }
}

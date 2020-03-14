using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace keccak
{
    public unsafe class MergePermutation
    {
        
        public static int permutationMergeBytes(byte[] key, byte[] target, bool repeatKey = false)
        {
            int result;
            var tmp = new byte[target.Length];
            int kl = 0;
            fixed (byte * k = key, t = target, _tmp = tmp)
            {
                result = permutationMergeBytes(k, key.Length, t, _tmp, target.Length, ref kl, repeatKey);
                BytesBuilder.ToNull(tmp.Length, _tmp);
            }

            return result;
        }

        public static int permutationMergeBytes(byte * key, int keyLen, byte * target, byte * tmp, int len, ref int kl, bool repeatKey = false)
        {
            if (len <= 1)
                throw new ArgumentOutOfRangeException("len", "len <= 1");

            byte * a1 = tmp, a2 = target;
            int ended = 0;
            int counter = 0;
            for (int i = 1; i < len; i <<= 1)
            {
                counter += permutationMergeBytes_Iteration(i, a2, a1, len, key, keyLen, ref kl, ref ended, repeatKey);
                ended = 0;

                byte * a = a1;
                a1 = a2;
                a2 = a;
            }

            if (a2 != target)
                BytesBuilder.CopyTo(len, len, a2, a1);

            return counter;
        }

        private static int permutationMergeBytes_Iteration(int step, byte* a2, byte* a1, int len, byte * key, int keyLen, ref int kl, ref int ended, bool repeatKey)
        {
            int counter = 0;
            int j = 0;

            int step2 = step << 1;
            // выбираем два списка с началом i и i + step
            for (int i = 0; i < len; i += step2)
            {
                int i2 = i + step;
                int ie = i + step2;
                int j1 = i;
                int j2 = i2;

                if (i2 >= len)
                {
                    for (; j1 < len;)
                    {
                        a1[j++] = a2[j1++];
                    }
                    return counter;
                }

                if (ie > len)
                {
                    ie = len;
                }

                int C = 1;
                if ((kl & 7) > 0)
                    C <<= kl & 7;

                // a2 - здесь содержатся списки для сортировки
                while (true)
                {
                    counter++;
                    if (C >= 256)
                        C = 1;

                    int klb = kl >> 3;

                    if (ended == 1)
                        ended = 2;

                    if (klb >= keyLen)
                    {
                        if (repeatKey)
                        {
                            klb = 0;
                            kl  = 0;
                        }
                        else
                        {
                            kl = 0;
                            if (ended == 0)
                                ended = 1;
                        }
                    }
                    else
                        kl++;

                    if ((ended == 2 && (counter & 3) > 0) || (ended == 0 && (C & key[klb]) == 0))
                    {
                        a1[j++] = a2[j2++];
                    }
                    else
                    {
                        a1[j++] = a2[j1++];
                    }

                    if (j1 >= i2)
                    {
                        for (; j2 < ie;)
                        {
                            a1[j++] = a2[j2++];
                        }
                        break;
                    }

                    if (j2 >= ie)
                    {
                        for (; j1 < i2;)
                        {
                            a1[j++] = a2[j1++];
                        }
                        break;
                    }

                    C <<= 1;
                }
            }

            return counter;
        }

        // ~= log(n/2, 2)*n
        public static int permutationMergeCount(int len)
        {
            if (len <= 1)
                throw new ArgumentOutOfRangeException("len", "len <= 1");

            if (len < 64)
            {
                int counter = 0;
                for (int i = 1; i < len; i <<= 1)
                {
                    for (int y = 0; y < len; y += i << 1)
                    {
                        int i2 = y + i;
                        int ie = y + (i << 1);
                        int j1 = y;
                        int j2 = i2;

                        if (i2 >= len)
                        {
                            break;
                        }

                        if (ie > len)
                        {
                            ie = len;
                            counter += ((ie - i2) << 1) - 1;
                            counter += i - (ie - i2);
                        }
                        else
                            counter += ((ie - i2) << 1) - 1;
                    }
                }

                return counter;
            }

            return (int) Math.Ceiling( Math.Log(len >> 1, 2) * len + 1 );
        }

        public static int keyMergeCount(int klen, int e = 64)
        {
            int result = klen;
            int len, newResult;
            int count = 0;
            while (count < 12)
            {
                len    = permutationMergeCount(result) >> 3;

                newResult = result*klen/len;

                if (Math.Abs(newResult - result) < e)
                    return newResult > 0 ? newResult : 1;

                result = newResult;
            }

            throw new Exception();
        }

        /// <summary>
        /// Очень плохая гамма!
        /// </summary>
        /// <param name="key"></param>
        /// <param name="len"></param>
        /// <param name="stateLen"></param>
        /// <param name="notCopy"></param>
        /// <returns></returns>
        public static byte[] getGamma(byte[] key, int len, out int stateLen, bool notCopy = false)
        {
            if (key.Length > 65536)
                throw new ArgumentOutOfRangeException();

            var result = new byte[len];

            var kl = key.Length;
            var sl = 256;
            stateLen = sl;

            var k = key;
            var s = new byte[sl];
            var t = new byte[sl > kl ? sl : kl];

            if (!notCopy)
            {
                k = BytesBuilder.CloneBytes(key,  0, kl);
            }

            int C = kl >> 10;
            if (C < 2)
                C = 2;

            int kbit = 0;

            fixed (byte * kp = k, sp = s, tp = t, r = result)
            {
                for (int i = 0; i < sl; i++)
                    sp[i] = (byte) i;

                for (int i = 0; i < 4; i++)
                {
                    for (int j = 0; j < C; j++)
                    {
                        kbit = step(kl, sl, kbit, kp, sp, tp);
                    }
                }

                for (int i = 0; i < result.Length; i++)
                {
                    for (int j = 0; j < C; j++)
                    {
                        kbit = step(kl, sl, kbit, kp, sp, tp);
                    }

                    r[i] = getMask(sp, sl, sp[0]);
                }
            }

            return result;
        }

        private static int step(int kl, int sl, int kbit, byte* kp, byte* sp, byte* tp)
        {
            permutationMergeBytes(kp, kl, sp, tp, sl, ref kbit);

            permutationMergeBytes(sp, sl, sp, tp, sl, ref kbit);
            Gost28147Modified.addConstant(kp, kl);

            byte mask1 = getMask(sp, sl);

            for (int q = 0; q < sl; q++)
            {
                sp[q] ^= mask1;
            }

            return kbit;
        }

        private static unsafe byte getMask(byte* sp, int sl, byte nullMask = 0)
        {
            byte result = nullMask;
            for (int i = 0; i < sl; i += 3)
            {
                result ^= sp[i];
            }

            return result;
        }

        public static byte[] getHash(byte[] value, int count)
        {
            var result = new byte[count];

            var kl = value.Length;
            var sl = 256;
            var s = new byte[sl];
            var t = new byte[sl];
            var n = new byte[0];
            int kbit = 0, kn = 0;

            fixed (byte * kp = value, sp = s, tp = t, np = n)
            {
                for (int i = 0; i < sl; i++)
                    sp[i] = (byte) i;

                for (int i = 0; i < count; i++)
                {
                    permutationMergeBytes(kp, kl, sp, tp, sl, ref kbit, true);
                    permutationMergeBytes(kp, kl, sp, tp, sl, ref kbit, true);
                    permutationMergeBytes(kp, kl, sp, tp, sl, ref kbit, true);
                    permutationMergeBytes(kp, kl, sp, tp, sl, ref kbit, true);

                    permutationMergeBytes(np, 0, sp, tp, sl,  ref kn);
                    result[i] = sp[0];
                }

                BytesBuilder.ToNull(sl, tp);
                BytesBuilder.ToNull(sl, sp);
            }

            return result;
        }
    }
}

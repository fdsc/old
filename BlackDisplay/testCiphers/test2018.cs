using keccak;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace testCiphers
{
    class test2018
    {
        public int  keyLen = 64;
        public long numOfInVariants = -1;
        public int  numOfInBits = -1;
        public int  numOfOutBytes  = 1;

        protected object sync = new object();
        public virtual void test(bool wait = true)
        {
            InitializeTest();
            int count = 0;
            int ended = 0;

            var dt = DateTime.Now;
            Console.WriteLine(0 + "% " + dt.ToLongTimeString());

            var procCount = Environment.ProcessorCount;
            for (long variant_ = 0; variant_ < numOfInVariants; variant_++)
            {
                long variant = variant_;
                lock (sync)
                    count++;

                ThreadPool.QueueUserWorkItem
                (
                    delegate
                    {
                        try
                        {
                            ProcessCryptoVariant(variant);
                        }
                        finally
                        {
                            lock (sync)
                            {
                                count--;
                                ended++;
                                Monitor.Pulse(sync);
                            }
                        }
                    }
                );

                // Ожидание, чтобы не перегружать очередь пула потоков
                lock (sync)
                while (count > procCount << 1)
                {
                    Monitor.Wait(sync, 1000);
                    if ((DateTime.Now - dt).TotalMilliseconds > 1000)
                    {
                        // Выше аналогично
                        Console.WriteLine((100 * ended / numOfInVariants) + "% " + dt.ToLongTimeString());
                        dt = DateTime.Now;
                    }
                }
            }

            lock (sync)
            while (count > 0)
            {
                Monitor.Wait(sync, 1000);
                if ((DateTime.Now - dt).TotalMilliseconds > 1000)
                {
                    // Выше аналогично
                    Console.WriteLine((100 * ended / numOfInVariants) + "% " + dt.ToLongTimeString());
                    dt = DateTime.Now;
                }
            }

            printResult();

            Console.WriteLine("Press any key to exit");
            if (wait)
            Console.ReadLine();
        }

        public virtual void ProcessCryptoVariant(long variant)
        {
            SHA3 gs = new SHA3(0);
            var key = prepareKey(variant);
            var s = prepareOIV(variant);
            /*gs.prepareGamma(key, s);
            var gamma = gs.getGamma(numOfOutBytes);*/

            var v = numOfInVariants;
            int c = 0;
            for (c = 0; v > 1; c++)
                v >>= 1;
            for (; c > 0; c--)
                v <<= 1;

            var e = variant >> 1;

            byte[] gamma = new byte[numOfOutBytes];
            for (int gc = 0; gc < numOfOutBytes; gc++)
                if ((variant & v) > 0)
                    gamma[gc] = (byte) e;
                else
                    gamma[gc] = (byte) (e ^ 0xFFFF);

            calcResult(gamma, variant, key);
        }

        public virtual void InitializeTest()
        {
            numOfInVariants = keyLen*keyLen*8*8*2;
            numOfInBits = keyLen*8;
            result = new int[numOfInBits, numOfOutBytes, 256, 2];

            checkForKey  = new int[numOfInBits, 2];
            checkForKey2 = new int[256];
        }

        public virtual byte[] prepareKey(long variant)
        {
            var key = new byte[keyLen];

            if ((variant & 1) == 0)
                BytesBuilder.FillByBytes(0xFF, key);
            else
                BytesBuilder.ToNull(key);
            
            var v = variant >> 1;

            var i1 = v % (keyLen*8);
            var i2 = (v / (keyLen*8)) % (keyLen*8);

            setBit(key, i1, (int) variant & 1);
            setBit(key, i2, (int) variant & 1);

            return key;
        }

        public virtual void setBit(byte[] a, long index, int command)
        {
            var  i = index >> 3;
            byte s = (byte) (index & 7);
            byte v = (byte) (1 << s);

            if (command == 1)
                a[i] = (byte) (v | a[i]);
            else
            if (command == 2)
                a[i] = (byte) (v ^ a[i]);
            else
            if (command == 0)
            {
                v = (byte) ~v;
                a[i] = (byte) (v & a[i]);
            }
            else
                throw new ArgumentOutOfRangeException("setBit.command " + command);
        }

        public virtual int getBit(byte[] a, long index)
        {
            var  i = index >> 3;
            byte s = (byte) (index & 7);
            byte v = (byte) (1 << s);

            if ((v & a[i]) > 0)
                return 1;

            return 0;
        }

        protected byte[] nullBytes = null;
        public virtual byte[] prepareOIV(long variant)
        {
            lock (sync)
            {
                if (nullBytes == null)
                {
                    nullBytes = new byte[64];
                    BytesBuilder.ToNull(nullBytes);
                }
            }

            return nullBytes;
        }

        public int[,,,] result;  // result = new int[numOfInBits, numOfOutBytes, 256];
        public int[,]   checkForKey;
        public int[]    checkForKey2;
        public virtual void calcResult(byte[] resultOfVariant, long variant, byte[] key)
        {
            // result = new int[numOfInBits, numOfOutBytes, 256, 2];

            var a  = variant & 1;
            var v  = variant >> 1;
            var i1 = v % (keyLen*8);
            var i2 = (v / (keyLen*8)) % (keyLen*8);

            lock (result)
            {
                for (int i = 0; i < numOfInBits; i++)
                for (int j = 0; j < numOfOutBytes; j++)
                {
                    var s = getBit(key, i);
                    var r = resultOfVariant[j];

                    result[i, j, r, s]++;
                    checkForKey [i, s]++;
                    checkForKey2[r]++;
                }
            }
        }

        private void printResult()
        {
            var min = Int32.MaxValue;
            var max = 0;

            var minr = 0;
            var maxr = 0;

            for (int i = 0; i < numOfInBits; i++)
            for (int j = 0; j < numOfOutBytes; j++)
            for (int r = 0; r < 256; r++)
            for (int a = 0; a < 2; a++)
            {
                    if (max < result[i, j, r, a])
                    {
                        max  = result[i, j, r, a];
                        maxr = r;
                    }

                    if (min > result[i, j, r, a])
                    {
                        min  = result[i, j, r, a];
                        minr = r;
                    }
            }

            var minchk2 = Int32.MaxValue;
            var maxchk2 = 0;
            for (int r = 0; r < 256; r++)
            {
                if (minchk2 > checkForKey2[r])
                    minchk2 = checkForKey2[r];
                if (maxchk2 < checkForKey2[r])
                    maxchk2 = checkForKey2[r];
            }

            Console.WriteLine("min: " + min + " for r = " + minr);
            Console.WriteLine("max: " + max + " for r = " + maxr);
            var ideal = (numOfInVariants / 256 / 2);
            Console.WriteLine("ideal: " + ideal + ". Deviation: " + (float) max / (float) min);

            Console.WriteLine("Check2: min = " + minchk2 + ", max = " + maxchk2 + ", " + (float) maxchk2 / (float) minchk2);

            int b = checkForKey[0, 0];
            for (int i = 0; i < numOfInBits; i++)
            for (int j = 0; j < 2; j++)
            {
                if (b != checkForKey[i, j])
                {
                    Console.WriteLine("checkForKey == " + b);
                    Console.WriteLine("ERROR: checkForKey == " + checkForKey[i, j] + " for " + i + ", " + j);
                    goto end;
                }
            }
            end:;

        }
    }
}

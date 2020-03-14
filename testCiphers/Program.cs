#define AGOSTM
//#define AGOSTM2
//#define AGOSTM2E
//#define AGOST
//#define ASHA3
//#define AExHash
//#define AMH20
//#define AMACHASHMOD
//#define ARC4
//#define ARC4Plus
//#define AMD5
//#define APH
//#define AD
//#define ADerivato
//#define A20
//#define A22
//#define ARC4Mod
//#define Perm

#define fillKey1

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Threading;
using keccak;


namespace testCiphers
{
    class Program
    {
        static unsafe void Main(string[] args)
        {
            //new test2018().test();
            //new test2018_keccakGamma().test(false);
            //new test2018_magma().test();
            //new test2018_derivation2().test(false);
            //new test2018_keccakGamma3().test();
            //new test2018_RC4().test();
            //new test2018_magmaMod().test();
            //new test2018_keccakGammaBytes().test();
            //new test2018_RC4Bytes().test();
            // new test2018_keccakGammaBit2Mod().test();
            // new test2018_My1().test();
            //new test2018_md5().test();

            new test2018_keccakPwd().test();

            return;

            var a = new byte[10240];
            var c = new byte[1024];
            for (int i = 0; i < a.Length; i++)
                a[i] = 0x55;//0x55;

            Console.WriteLine("Count of 1024 bytes key merge {0} bits", MergePermutation.permutationMergeBytes(a, c));
            Console.WriteLine("Calculated count of 1024 bytes key merge {0} bits", MergePermutation.permutationMergeCount(c.Length));
            Console.WriteLine("Calculated count of 256 bytes key merge {0} bits", MergePermutation.permutationMergeCount(256));

            var b = new byte[256];
            var k = new byte[256];
            for (int i = 0; i < b.Length; i++)
            {
                b[i] = (byte)i;
                k[i] = 0x00;
            }

            File.WriteAllText("tmp.txt", "");
            File.AppendAllText("tmp.txt", BitConverter.ToString(b) + "\r\n\r\n\r\n");
            for (int i = 0; i < 16; i++)
            {
                MergePermutation.permutationMergeBytes(k, b);
                MergePermutation.permutationMergeBytes(b, k);

                if (i == 0)
                for (int j = 0; j < b.Length; j++)
                {
                    if (b[j] != 255-j)
                    {
                        Console.WriteLine("ERROR: permutationMergeBytes incorrect");
                        break;
                    }
                }
            }
            File.AppendAllText("tmp.txt", BitConverter.ToString(b) + "\r\n\r\n\r\n");


            int errorflag = 0;
            var sync = new object();
#if AMACHASHMOD || AMH20
            testPermutation(sync, 12, 0, 0, 72);
#elif AMD5
            testPermutation(sync, 12, 0, 0, 16);
#elif AExHash
            testPermutation(sync, 12, 0, 0, 64*4);
#elif Perm
            testPermutation(sync, 10, 0, 0, 1024+4);
#else
            testPermutation(sync, 12, 0, 0, 1024+4);
#endif
            lock (sync)
            {
                while (tgmgSuccess > 0)
                    Monitor.Wait(sync);

                errorflag += tgmgResultInt;
            }
            Console.WriteLine();
            




            if (errorflag > 0)
                Console.WriteLine("ERRORS found: " + errorflag);
            else
                Console.WriteLine("success");

            Console.ReadKey();
        }

#if !fillKey1
        private static void fillKey(int CK, int i, ref byte[] key)
        {
            for (int j = 0; j < key.Length - 4; j += 2)
                BytesBuilder.UIntToBytes((uint)i, ref key, j);
        }
#else
        private static void fillKey(int CK, int i, ref byte[] key)
        {
            BytesBuilder.UIntToBytes((uint)i, ref key, CK);
        }
#endif

        static int tgmgResultInt; static int tgmgSuccess;
        /*
                        gg.prepareGamma(key, s, Gost28147Modified.CryptoProA, Gost28147Modified.CryptoProB, Gost28147Modified.CryptoProC, Gost28147Modified.CryptoProD, Gost28147Modified.ESbox_A, Gost28147Modified.ESbox_B);
                        var gamma = gg.getGamma(GC + 1, true);*/
/*                        
                        gs.prepareGamma(key, s);
                        var gamma = gs.getGamma(GC + 1);
  */                      
                        //var gamma = gg.getGOSTGamma(key, s, Gost28147Modified.ESbox_B, 8);

                        /*gg.prepareGamma(key, s, Gost28147Modified.CryptoProA, Gost28147Modified.CryptoProB, Gost28147Modified.CryptoProC, Gost28147Modified.CryptoProD, Gost28147Modified.ESbox_A, Gost28147Modified.ESbox_B);
                        var gamma = gg.getGamma(512, true);*/
                        
                        /*gs.prepareGamma(key, s);
                        var gamma = gs.getGamma();*/
                        
                        //var gamma = gg.getGOSTGamma(key, s, Gost28147Modified.ESbox_B, 8);
                        //var gamma = gs.getDerivatoKey(key, s, hashCount, ref pc, 512, regime);

                        
                        /*gg.prepareGamma(key, s, Gost28147Modified.CryptoProA, Gost28147Modified.CryptoProB, Gost28147Modified.CryptoProC, Gost28147Modified.CryptoProD, Gost28147Modified.ESbox_A, Gost28147Modified.ESbox_B);
                        var gamma = gg.getGamma(512, true);*/
      /*                  
                        gs.prepareGamma(key, s);
                        var gamma = gs.getGamma();
       * */
    /*                    
                        var gamma = gg.getGOSTGamma(key, s, Gost28147Modified.ESbox_B, 8);
/*
                        byte[] gamma = new byte[72];
                        SHA3.getMultiHash20(key, out gamma, ref pc, hashCount, gs);
        */
                        /*
                        var lk = gs.GetKeysFromKeyData(key, s, hashCount, 20, ref pc);
                        var ls = gs.GetOIVectorsFromKeyData(lk.Count, s, hashCount);
                        var gamma = gs.getMACHashMod(s, lk, ls, pc, hashCount);
*/


        private static void testPermutation(object sync, int MC, int pc, int hashCount, int GC)
        {
            if (MC > 14)
                throw new ArgumentOutOfRangeException("MC", "MC must be <= 14");
#if !fillKey1
            tgmgSuccess = 1;
#else
            tgmgSuccess = 4;
#endif
            tgmgResultInt = 0;


#if AGOST || AD
            var SL = 32;
#elif AGOSTM
            var SL = 160;
#elif AGOSTM2 || AGOSTM2E
            var SL = 128;
#elif APH
            var SL = 256;
#else
            var SL = 64;
#endif

            for (int counter = 0; counter < tgmgSuccess; counter++)
            {
                int CK = 0;
                if (counter == 1)
                    CK = SL-4;
                else
                if (counter == 2)
                    CK = SL >> 1;
                else
                if (counter == 3)
                    CK = (SL >> 1) + (SL >> 2);

                int counterA = counter;

            ThreadPool.QueueUserWorkItem
            (
                delegate
                {
                    var error = 0;

                    int Max = 1 << (MC + 2);
                    var key = new byte[SL];
                    var s   = new byte[64];
                    var stat = new int[2, 16, 256, GC];
                    var stab = new int[2, 2, 16, 16, GC];

                    var gg = new Gost28147Modified();
                    var gs = new SHA3(0);
                    for (int i = 0; i < Max; i++)
                    {
                        fillKey(CK, i, ref key);

                        //byte[] gamma = MergePermutation.getGamma(key, 512, out t);

#if AGOSTM
                        gg.prepareGamma(key, s, Gost28147Modified.CryptoProA, Gost28147Modified.CryptoProB, Gost28147Modified.CryptoProC, Gost28147Modified.CryptoProD, Gost28147Modified.ESbox_A, Gost28147Modified.ESbox_B);
                        var gamma = gg.getGamma(GC + 1, 33-21);
#elif AGOSTM2
                        gg.prepareGamma(key, s, Gost28147Modified.CryptoProA, Gost28147Modified.CryptoProB, null, null, null, null, false);
                        var gamma = gg.getGamma(GC + 1, false);
#elif AGOSTM2E
                        gg.prepareGamma(key, s, Gost28147Modified.CryptoProA, Gost28147Modified.CryptoProB, null, null, null, null, true);
                        var gamma = gg.getGamma(GC + 1, 33-21);
#elif AGOST
                        var gamma = gg.getGOSTGamma(key, s, Gost28147Modified.ESbox_B, GC);
#elif ASHA3
                        gs.prepareGamma(key, s);
                        var gamma = gs.getGamma(GC);
#elif AExHash
                        var gamma = SHA3.getExHash(4, s, key);
#elif AMH20
                        byte[] gamma = new byte[GC];
                        SHA3.getMultiHash20(key, out gamma, ref pc, hashCount, gs);
#elif AMACHASHMOD
                        var lk = gs.GetKeysFromKeyData(key, s, hashCount, 20, ref pc);
                        var ls = gs.GetOIVectorsFromKeyData(lk.Count, s, hashCount);
                        var gamma = gs.getMACHashMod(s, lk, ls, pc, hashCount);
#elif ARC4
                        var RC4   = new RC4(key);
                        var gamma = RC4.getGamma(GC);
#elif ARC4Plus
                        var RC4   = new RC4(key);
                        var gamma = RC4.getGamma2(GC);
#elif AMD5
                        var md5 = System.Security.Cryptography.MD5.Create();
                        var gamma = md5.ComputeHash(key);
#elif APH
                        var gamma = MergePermutation.getHash(key, GC);
#elif AD
                        var gamma = SHA3.generateRandomPwdByDerivatoKey(key, GC);
#elif ADerivato
                        var gamma = gs.getDerivatoKey(key, /*new UTF8Encoding().GetBytes("passwordJjVjRyjcWwo7761qPBQUb3Sx8DACNpassword")*/Convert.FromBase64String("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"),
                                                           22 >= 20 ? 16 : 1024, ref pc, GC, 22 / 10);
#elif A20
                        var gamma = new byte[GC];
                        gamma = gs.multiCryptLZMA(gamma, key, s, 20, false, 19, 4);
                        gamma = BytesBuilder.CloneBytes(gamma, 4+s.Length, GC+4+s.Length);
#elif A22
                        var gamma = new byte[GC];
                        gamma = gs.multiCryptLZMA(gamma, key, s, 22, false, 19, 4);
                        var L = gamma.Length - s.Length - 4;
                        gamma = BytesBuilder.CloneBytes(gamma, gamma.Length - L, L);
#elif ARC4Mod
                        var RC4   = new RC4Mod(key);
                        var gamma = RC4.getGamma(GC);
#elif Perm
                        int slen = 256;
                        var gamma = MergePermutation.getGamma(key, GC, out slen);
#else
                        byte[] gamma = new byte[GC];
                        for (int gc = 0; gc < GC; gc++)
                            if ((i & 512) > 0)
                                gamma[gc] = (byte) i;
                            else
                                gamma[gc] = (byte) (i ^ 0xFFFF);
#endif

                        for (int gc = 0; gc < GC; gc++)
                        {
                            int C = 1;
                            for (int j = 0; j < MC; j++, C <<= 1)
                            {
                                stat[(i & C) > 0 ? 1 : 0, j, gamma[gc], gc]++;
                            }

                            C = 1;
                            for (int j = 0; j < MC; j++, C <<= 1)
                            {
                                int C2 = 1;
                                for (int j2 = 0; j2 < 8; j2++, C2 <<= 1)
                                    stab[(gamma[gc] & C2) > 0 ? 1 : 0, (i & C) > 0 ? 1 : 0, j, j2, gc]++;
                            }
                        }
                    }

                    int max  = 0;
                    int min  = int.MaxValue;
                    int summ = 0;

                    int max2  = 0;
                    int min2  = int.MaxValue;
                    int summ2 = 0;

                    for (int k01 = 0; k01 <= 1; k01++)
                    for (int gc = 0; gc < GC; gc++)
                    for (int i = 0; i < MC; i++)
                    {
                        for (int j = 0; j < 256; j++)
                        {
                            if (stat[k01, i, j, gc] > max)
                                max = stat[k01, i, j, gc];

                            if (stat[k01, i, j, gc] < min)
                                min = stat[k01, i, j, gc];

                            summ += stat[k01, i, j, gc];
                        }

                        for (int ib = 0; ib < 2; ib++)
                        for (int j2 = 0; j2 < 8; j2++)
                        {
                            if (stab[k01, ib, i, j2, gc] > max2)
                                max2 = stab[k01, ib, i, j2, gc];

                            if (stab[k01, ib, i, j2, gc] < min2)
                                min2 = stab[k01, ib, i, j2, gc];

                            summ2 += stab[k01, ib, i, j2, gc];
                        }
                    }
                    /*summ  /= Max*MC;
                    summ2 /= Max*MC*GC;
                     * */
                    float ms1 = Max*GC*MC/(2f*GC*MC*256f);
                    float ms2 = Max*GC*MC*8f/(2f*GC*MC*2f*8f);

                    if (GC < 2)
                    {
                        if ((float)max/(float)min >= 4.7)
                            error++;
                        if ((float)max/ms1 >= 1.8)
                            error++;
                        if (ms1/(float)min >= 2.7)
                            error++;

                        if ((float)max2/(float)min2 >= 1.07)
                            error++;
                        if ((float)max2/ms2 >= 1.04)
                            error++;
                        if ((float)ms2/(float)min2 >= 1.04)
                            error++;
                    }
                    else
                    if (GC < 73)
                    {
                        if ((float)max/(float)min >= 8.89)
                            error++;
                        if ((float)max/ms1 >= 2.0)
                            error++;
                        if (ms1/(float)min >= 4.58)
                            error++;

                        if ((float)max2/(float)min2 >= 1.11)
                            error++;
                        if ((float)max2/ms2 >= 1.06)
                            error++;
                        if ((float)ms2/(float)min2 >= 1.06)
                            error++;
                    }
                    else
                    {
                        if ((float)max/(float)min >= 11.51)
                            error++;
                        if ((float)max/ms1 >= 2.19)
                            error++;
                        if (ms1/(float)min >= 5.34)
                            error++;

                        if ((float)max2/(float)min2 >= 1.13)
                            error++;
                        if ((float)max2/ms2 >= 1.07)
                            error++;
                        if ((float)ms2/(float)min2 >= 1.07)
                            error++;
                    }

                    // GOST     9,29 2,03 4,57 1,11 1,05 1,06
                    //                                                         // fillKey1
                    // GOSTM    7,11 2,00 3,56 1,11 1,05 1,05
                    //          
                    // SHA3     10,8 2,03 5,33 1,10 1,05 1,05
                    //          7,88 1,97 4,00 1,10 1,05 1,05
                    //          9,29 2,03 4,57 1,11 1,05 1,06
                    //          7,67 2,16 3,56 1,12 1,06 1,06
                    //          
                    // MH20     6,20 1,94 3,20 1,09 1,04 1,04
                    //          7,63 1,91 4,00 1,09 1,04 1,05
                    //          6,30 1,97 3,20 1,09 1,04 1,04
                    // MACHASHMOD
                    //          
                    //          
                    //  RC4     8.11 2.28 3.56 1.10 1.05 1.05
                    //          1500
                    //  RC4+    --
                    //          44

                    lock (sync)
                    {
                        if (error == 0)
                        {
                            Console.WriteLine("success ({0}) EC 0 {1:F}\t{2:F}\t{3:F}\t{4:F}\t{5:F}\t{6:F}", counterA, ((float)max/(float)min), ((float)max/(float)ms1), ((float)ms1/(float)min), ((float)max2/(float)min2), ((float)max2/(float)ms2), ((float)ms2/(float)min2));
                        }
                        else
                        {
                            Console.WriteLine("FAILED ({0}) EC {7} {1:F}\t{2:F}\t{3:F}\t{4:F}\t{5:F}\t{6:F}", counterA, ((float)max/(float)min), ((float)max/(float)ms1), ((float)ms1/(float)min), ((float)max2/(float)min2), ((float)max2/(float)ms2), ((float)ms2/(float)min2), error);
                            tgmgResultInt++;
                        }

                        tgmgSuccess--;
                        Monitor.Pulse(sync);

                        System.Windows.Media.MediaPlayer player = new System.Windows.Media.MediaPlayer();
                        player.Open(new Uri("Resources/siren.wav", UriKind.Relative));
                        player.Position = new TimeSpan(0);
                        player.Play();

                    }
                }
            );
            }
        }

    }
}

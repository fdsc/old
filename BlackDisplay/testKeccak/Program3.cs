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
        private static bool testForBits_getMultiHash40(ref int pc, int hc, SHA3 sha)
        {
            var size  = 32;
            var sizeb = size << 3;
            byte[] result = null;
            byte[] inData = new byte[size];

            int[,] counts = new int[sizeb, sizeb];

            BytesBuilder.ToNull(inData);
            for (int i  = 0; i < sizeb; i++)
            {
                setBitInArray(inData, i);

                for (int j  = 0; j < sizeb; j++)
                {
                    if (i == j)
                        continue;

                    setBitInArray(inData, j);

                    SHA3.getMultiHash40(inData, out result, ref pc, hc, sha, size);
                    //SHA3.getMultiHash20(inData, out result, ref pc, hc, sha, size);
                    //result = sha.getHash512(inData);
                    //result = new testCiphers.RC4(inData).getGamma(size);
                    toResult(sizeb, i, j, result, counts);

                    setBitInArray(inData, j, false);
                }

                setBitInArray(inData, i, false);
            }

            double суммарнаяВероятность, минимальнаяВероятность, множительВероятностей, отклонениеОтЭталона, отклонениеОтЭталонаВниз, отклонениеОтЭталонаВверх;
            double[,] вероятность;
            getCountsResult(sizeb, counts, out вероятность, out минимальнаяВероятность, out множительВероятностей, out суммарнаяВероятность, out отклонениеОтЭталона, out отклонениеОтЭталонаВниз, out отклонениеОтЭталонаВверх);

            if (множительВероятностей > 1.01 || суммарнаяВероятность < 0.01 || отклонениеОтЭталонаВниз > 0.10 || отклонениеОтЭталонаВверх > 0.10 || минимальнаяВероятность < 0.001)
                return false;

            return true;
        }

        
        private static bool testForBits_keccak(SHA3 sha)
        {
            const int len = 32;
            var size  = len;
            var sizeb = size << 3;
            byte[] result = null;
            byte[] inData = new byte[len]; //BytesBuilder.CloneBytes(Encoding.ASCII.GetBytes("Ti8xb2bySHIQ13uiYSuLb2Dm8F2kUdNGezex8KOwKb7f0DMiIWIqvE2I3LwCBv3z"), 0, len);
            inData = BytesBuilder.CloneBytes(Encoding.ASCII.GetBytes("Ti8xb2bySHIQ13uiYSuLb2Dm8F2kUdNGezex8KOwKb7f0DMiIWIqvE2I3LwCBv3z"), 0, len);

            var pc = 0;
            //var sha2 = new SHA3(32);
            byte[] r;

            List<byte[]> keys = new List<byte[]>();
            keys.Add(new byte[144]);

            int[,] counts = new int[sizeb, sizeb];

            BytesBuilder.ToNull(inData);
            for (int i  = 0; i < sizeb; i++)
            {
                setBitInArray(inData, i);

                for (int j  = 0; j < sizeb; j++)
                {
                    if (i == j)
                        continue;

                    setBitInArray(inData, j);

                    //getExperiment(inData, out result, ref pc, 2);

                    // result = sha.getDuplex(inData, false, -1, true, size);
                    result = SHA3.generateRandomPwdByDerivatoKey(inData, 64);
                    //result = new System.Security.Cryptography.MD5Cng().ComputeHash(inData);
                    //result = sha.getHash224(inData);
                    //result = sha.getHash256(inData);
                    //result = sha.getHash512(inData);
                    //result = new Gost28147Modified().getGOSTGamma(inData, new byte[8], Gost28147Modified.CryptoProA, len);
                    //result = new testCiphers.RC4(inData).getGamma(len);
                    //result = new testCiphers.RC4Mod(inData).getGamma(len);
                    //result = new System.Security.Cryptography.SHA1Cng().ComputeHash(inData);
                    //result = new System.Security.Cryptography.SHA512Cng().ComputeHash(inData);
                    //result = NonRandomMethod(len, sizeb, i, j);
                    //result = GostGammaMod(len, inData);
                    //result = sha.getMACHashMod(inData, keys, keys, 4, 2, 40);

                    toResult(sizeb, i, j, result, counts);

                    setBitInArray(inData, j, false);
                }

                setBitInArray(inData, i, false);
            }

            double суммарнаяВероятность, минимальнаяВероятность, множительВероятностей, отклонениеОтЭталона, отклонениеОтЭталонаВниз, отклонениеОтЭталонаВверх;
            double[,] вероятность;
            getCountsResult(sizeb, counts, out вероятность, out минимальнаяВероятность, out множительВероятностей, out суммарнаяВероятность, out отклонениеОтЭталона, out отклонениеОтЭталонаВниз, out отклонениеОтЭталонаВверх);

            // Отклонение от эталона вверх на NonRandomMethod составляет 0.32 - это максимум, на который проверять уже нет смысла.
            // Отклонение вниз есть только у RC4
            if (множительВероятностей > 1.01/* || суммарнаяВероятность < 0.01*/ || отклонениеОтЭталонаВниз > 0.10 || отклонениеОтЭталонаВверх > 0.10/* || минимальнаяВероятность < 0.001*/)
                return false;

            return true;
        }

        public static void getExperiment(byte[] message, out byte[] result, ref int procCount, int hashCount = 12, SHA3 shaR = null, int resultLen = 72)
        {
            var rlen   = message.Length;
            SHA3[] sha = new SHA3[1];

            byte[][] bytes =
            {
                Encoding.ASCII.GetBytes("ZnEgo@6X5wkF8frXIegd74xlOHHTkhm(JUjOyeL+az9cRc4EiR_s%Dg(T6tHFOoOrKigO_h3I7QN$0(J+CIUW@36r6B4PBdGpFwp*&Lsi5qp53(M&A55RiWxt5kWi67prw5G7P#XzTQZMp7"),
                Encoding.ASCII.GetBytes("NT7mCC2SwouOINqS7dYCR1vhKToqpmZfXFutLWH8wRIZFhaIkIRHn0vAC4qwFfI2RlD1SHQZekzC2muf31g25jyU4m3dGSC0XNGvZnOCPrssaaEPajRoiqiJdrjDiBD2Jbc3Qf5LwLFFqJY"),
                Encoding.ASCII.GetBytes("CTMCekbueNI9N2N9Rc6MR4H3VJpXiuCvZtNyG0SRDmEh9NDcFCDRpEVJ3DKvM2DGhUV06aX1qXY2BPYvLc8knRI60TLQ9vvUk413lfoeuDA2DRbM9myIpTKWq6daSRboRQ2J02ckROrUmQS"),
                Encoding.ASCII.GetBytes("AgFsBdlNecqeXjcB9OXgs1rdiCwuGR5SMf0ynamUNM8zJ4PLQW9mZs8GFImTfuNIOPPtalcAN9npCFvi14typ22NcNb4RuJt0BEp9QTmKP4ROFNHXsVfpnjOo2MNsoQ7ulRTEKY4VRxgE4N")
            };

            for (int i = 0; i < sha.Length; i++)
            {
                sha[i] = new SHA3(rlen << hashCount);
                sha[i].getDuplex(bytes[i % bytes.Length], false, -1, false);
                sha[i].getDuplex(message, true);
            }
            /*
            var s3 = sha[3].getDuplex(message, true);
            var s2 = sha[2].getDuplexMod(message, s3, true);
            var s1 = sha[1].getDuplexMod(message, s2, true);

            result = sha[0].getDuplexMod(message, s1, true);*/

            result = sha[0].getDuplex(message, true);
        }

        private static byte[] GostGammaMod(int len, byte[] inData)
        {
            byte[] result;
            var dt = new byte[64 * 5];
            BytesBuilder.CopyTo(inData, dt);
            var g = new Gost28147Modified();
            g.prepareGamma(dt, new byte[160], Gost28147Modified.CryptoProA, Gost28147Modified.CryptoProC, Gost28147Modified.Sbox_Default, Gost28147Modified.ESbox_A, Gost28147Modified.ESbox_B, Gost28147Modified.ESbox_C);
            result = g.getGamma(len, 40);
            return result;
        }

        private static byte[] NonRandomMethod(int len, int sizeb, int i, int j)
        {
            byte[] result = new byte[len];
            for (int k = 0; k < sizeb; k++)
            {
                setBitInArray(result, k, ((i + j) & 1) > 0);
            }

            return result;
        }
    }
}

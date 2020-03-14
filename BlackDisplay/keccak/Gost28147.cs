using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace keccak
{
    public class Gost28147Modified
    {
        // http://tools.ietf.org/html/rfc4357
        /*
         * id-Gost28147-89-CryptoPro-A-ParamSet
		9  3  E  E  B  3  1  B
		6  7  4  7  5  A  D  A
		3  E  6  A  1  D  2  F
		2  9  2  C  9  C  9  5
		8  8  B  D  8  1  7  0
		B  A  3  1  D  2  A  C
		1  F  D  3  F  0  6  E
		7  0  8  9  0  B  0  8
		A  5  C  0  E  7  8  6
		4  2  F  2  4  5  C  2
		E  6  5  B  2  9  4  3
		F  C  A  4  3  4  5  9
		C  B  0  F  C  8  F  1
		0  4  7  8  7  F  3  7
		D  D  1  5  A  E  B  D
		5  1  9  6  6  6  E  4

		93 EE B3 1B 67 47 5A DA 3E 6A 1D 2F 29 2C 9C 95
        88 BD 81 70 BA 31 D2 AC 1F D3 F0 6E 70 89 0B 08
        A5 C0 E7 86 42 F2 45 C2 E6 5B 29 43 FC A4 34 59
        CB 0F C8 F1 04 78 7F 37 DD 15 AE BD 51 96 66 E4
		
        id-Gost28147-89-CryptoPro-B-ParamSet
		80 E7 28 50 41 C5 73 24 B2 00 C2 AB 1A AD F6 BE
        34 9B 94 98 5D 26 5D 13 05 D1 AE C7 9C B2 BB 31
        29 73 1C 7A E7 5A 41 42 A3 8C 07 D9 CF FF DF 06
        DB 34 6A 6F 68 6E 80 FD 76 19 E9 85 FE 48 35 EC
		
        id-Gost28147-89-CryptoPro-C-ParamSet
		10 83 8C A7 B1 26 D9 94 C7 50 BB 60 2D 01 01 85
        9B 45 48 DA D4 9D 5E E2 05 FA 12 2F F2 A8 24 0E
        48 3B 97 FC 5E 72 33 36 8F C9 C6 51 EC D7 E5 BB
        A9 6E 6A 4D 7A EF F0 19 66 1C AF C3 33 B4 7D 78

        id-Gost28147-89-CryptoPro-D-ParamSet
		FB 11 08 31 C6 C5 C0 0A 23 BE 8F 66 A4 0C 93 F8
        6C FA D2 1F 4F E7 25 EB 5E 60 AE 90 02 5D BB 24
        77 A6 71 DC 9D D2 3A 83 E8 4B 64 C5 D0 84 57 49
        15 99 4C B7 BA 33 E9 AD 89 7F FD 52 31 28 16 7E
         * */
        // ([0-9A-F])([0-9A-F])
        // 0x0\1, 0x0\2,
        static readonly public byte[] CryptoProA =
        {
            0x09, 0x03, 0x0E, 0x0E, 0x0B, 0x03, 0x01, 0x0B, 0x06, 0x07, 0x04, 0x07, 0x05, 0x0A, 0x0D, 0x0A, 0x03, 0x0E, 0x06, 0x0A, 0x01, 0x0D, 0x02, 0x0F, 0x02, 0x09, 0x02, 0x0C, 0x09, 0x0C, 0x09, 0x05,
            0x08, 0x08, 0x0B, 0x0D, 0x08, 0x01, 0x07, 0x00, 0x0B, 0x0A, 0x03, 0x01, 0x0D, 0x02, 0x0A, 0x0C, 0x01, 0x0F, 0x0D, 0x03, 0x0F, 0x00, 0x06, 0x0E, 0x07, 0x00, 0x08, 0x09, 0x00, 0x0B, 0x00, 0x08,
            0x0A, 0x05, 0x0C, 0x00, 0x0E, 0x07, 0x08, 0x06, 0x04, 0x02, 0x0F, 0x02, 0x04, 0x05, 0x0C, 0x02, 0x0E, 0x06, 0x05, 0x0B, 0x02, 0x09, 0x04, 0x03, 0x0F, 0x0C, 0x0A, 0x04, 0x03, 0x04, 0x05, 0x09,
            0x0C, 0x0B, 0x00, 0x0F, 0x0C, 0x08, 0x0F, 0x01, 0x00, 0x04, 0x07, 0x08, 0x07, 0x0F, 0x03, 0x07, 0x0D, 0x0D, 0x01, 0x05, 0x0A, 0x0E, 0x0B, 0x0D, 0x05, 0x01, 0x09, 0x06, 0x06, 0x06, 0x0E, 0x04
        };

        static readonly public byte[] CryptoProB =
        {
            0x08, 0x00, 0x0E, 0x07, 0x02, 0x08, 0x05, 0x00, 0x04, 0x01, 0x0C, 0x05, 0x07, 0x03, 0x02, 0x04, 0x0B, 0x02, 0x00, 0x00, 0x0C, 0x02, 0x0A, 0x0B, 0x01, 0x0A, 0x0A, 0x0D, 0x0F, 0x06, 0x0B, 0x0E,
            0x03, 0x04, 0x09, 0x0B, 0x09, 0x04, 0x09, 0x08, 0x05, 0x0D, 0x02, 0x06, 0x05, 0x0D, 0x01, 0x03, 0x00, 0x05, 0x0D, 0x01, 0x0A, 0x0E, 0x0C, 0x07, 0x09, 0x0C, 0x0B, 0x02, 0x0B, 0x0B, 0x03, 0x01,
            0x02, 0x09, 0x07, 0x03, 0x01, 0x0C, 0x07, 0x0A, 0x0E, 0x07, 0x05, 0x0A, 0x04, 0x01, 0x04, 0x02, 0x0A, 0x03, 0x08, 0x0C, 0x00, 0x07, 0x0D, 0x09, 0x0C, 0x0F, 0x0F, 0x0F, 0x0D, 0x0F, 0x00, 0x06,
            0x0D, 0x0B, 0x03, 0x04, 0x06, 0x0A, 0x06, 0x0F, 0x06, 0x08, 0x06, 0x0E, 0x08, 0x00, 0x0F, 0x0D, 0x07, 0x06, 0x01, 0x09, 0x0E, 0x09, 0x08, 0x05, 0x0F, 0x0E, 0x04, 0x08, 0x03, 0x05
        };

        static readonly public byte[] CryptoProC =
        {
            0x01, 0x00, 0x08, 0x03, 0x08, 0x0C, 0x0A, 0x07, 0x0B, 0x01, 0x02, 0x06, 0x0D, 0x09, 0x09, 0x04, 0x0C, 0x07, 0x05, 0x00, 0x0B, 0x0B, 0x06, 0x00, 0x02, 0x0D, 0x00, 0x01, 0x00, 0x01, 0x08, 0x05,
            0x09, 0x0B, 0x04, 0x05, 0x04, 0x08, 0x0D, 0x0A, 0x0D, 0x04, 0x09, 0x0D, 0x05, 0x0E, 0x0E, 0x02, 0x00, 0x05, 0x0F, 0x0A, 0x01, 0x02, 0x02, 0x0F, 0x0F, 0x02, 0x0A, 0x08, 0x02, 0x04, 0x00, 0x0E,
            0x04, 0x08, 0x03, 0x0B, 0x09, 0x07, 0x0F, 0x0C, 0x05, 0x0E, 0x07, 0x02, 0x03, 0x03, 0x03, 0x06, 0x08, 0x0F, 0x0C, 0x09, 0x0C, 0x06, 0x05, 0x01, 0x0E, 0x0C, 0x0D, 0x07, 0x0E, 0x05, 0x0B, 0x0B,
            0x0A, 0x09, 0x06, 0x0E, 0x06, 0x0A, 0x04, 0x0D, 0x07, 0x0A, 0x0E, 0x0F, 0x0F, 0x00, 0x01, 0x09, 0x06, 0x06, 0x01, 0x0C, 0x0A, 0x0F, 0x0C, 0x03, 0x03, 0x03, 0x0B, 0x04, 0x07, 0x0D, 0x07, 0x08
        };

        static readonly public byte[] CryptoProD =
        {
            0x0F, 0x0B, 0x01, 0x01, 0x00, 0x08, 0x03, 0x01, 0x0C, 0x06, 0x0C, 0x05, 0x0C, 0x00, 0x00, 0x0A, 0x02, 0x03, 0x0B, 0x0E, 0x08, 0x0F, 0x06, 0x06, 0x0A, 0x04, 0x00, 0x0C, 0x09, 0x03, 0x0F, 0x08,
            0x06, 0x0C, 0x0F, 0x0A, 0x0D, 0x02, 0x01, 0x0F, 0x04, 0x0F, 0x0E, 0x07, 0x02, 0x05, 0x0E, 0x0B, 0x05, 0x0E, 0x06, 0x00, 0x0A, 0x0E, 0x09, 0x00, 0x00, 0x02, 0x05, 0x0D, 0x0B, 0x0B, 0x02, 0x04,
            0x07, 0x07, 0x0A, 0x06, 0x07, 0x01, 0x0D, 0x0C, 0x09, 0x0D, 0x0D, 0x02, 0x03, 0x0A, 0x08, 0x03, 0x0E, 0x08, 0x04, 0x0B, 0x06, 0x04, 0x0C, 0x05, 0x0D, 0x00, 0x08, 0x04, 0x05, 0x07, 0x04, 0x09,
            0x01, 0x05, 0x09, 0x09, 0x04, 0x0C, 0x0B, 0x07, 0x0B, 0x0A, 0x03, 0x03, 0x0E, 0x09, 0x0A, 0x0D, 0x08, 0x09, 0x07, 0x0F, 0x0F, 0x0D, 0x05, 0x02, 0x03, 0x01, 0x02, 0x08, 0x01, 0x06, 0x07, 0x0E
        };

        static readonly public byte[] Sbox_Default = {
			0x4,0xA,0x9,0x2,0xD,0x8,0x0,0xE,0x6,0xB,0x1,0xC,0x7,0xF,0x5,0x3,
			0xE,0xB,0x4,0xC,0x6,0xD,0xF,0xA,0x2,0x3,0x8,0x1,0x0,0x7,0x5,0x9,
			0x5,0x8,0x1,0xD,0xA,0x3,0x4,0x2,0xE,0xF,0xC,0x7,0x6,0x0,0x9,0xB,
			0x7,0xD,0xA,0x1,0x0,0x8,0x9,0xF,0xE,0x4,0x6,0xC,0xB,0x2,0x5,0x3,
			0x6,0xC,0x7,0x1,0x5,0xF,0xD,0x8,0x4,0xA,0x9,0xE,0x0,0x3,0xB,0x2,
			0x4,0xB,0xA,0x0,0x7,0x2,0x1,0xD,0x3,0x6,0x8,0x5,0x9,0xC,0xF,0xE,
			0xD,0xB,0x4,0x1,0x3,0xF,0x5,0x9,0x0,0xA,0xE,0x7,0x6,0x8,0x2,0xC,
			0x1,0xF,0xD,0x0,0x5,0x7,0xA,0x4,0x9,0x2,0x3,0xE,0x6,0xB,0x8,0xC
		};

        // http://www.java2s.com/Open-Source/Java/Security/Bouncy-Castle/org/bouncycastle/crypto/engines/GOST28147Engine.java.htm
        static readonly public byte[] ESbox_D = {
			0xF,0xC,0x2,0xA,0x6,0x4,0x5,0x0,0x7,0x9,0xE,0xD,0x1,0xB,0x8,0x3,
			0xB,0x6,0x3,0x4,0xC,0xF,0xE,0x2,0x7,0xD,0x8,0x0,0x5,0xA,0x9,0x1,
			0x1,0xC,0xB,0x0,0xF,0xE,0x6,0x5,0xA,0xD,0x4,0x8,0x9,0x3,0x7,0x2,
			0x1,0x5,0xE,0xC,0xA,0x7,0x0,0xD,0x6,0x2,0xB,0x4,0x9,0x3,0xF,0x8,
			0x0,0xC,0x8,0x9,0xD,0x2,0xA,0xB,0x7,0x3,0x6,0x5,0x4,0xE,0xF,0x1,
			0x8,0x0,0xF,0x3,0x2,0x5,0xE,0xB,0x1,0xA,0x4,0x7,0xC,0x9,0xD,0x6,
			0x3,0x0,0x6,0xF,0x1,0xE,0x9,0x2,0xD,0x8,0xC,0x4,0xB,0xA,0x5,0x7,
			0x1,0xA,0x6,0x8,0xF,0xB,0x0,0x4,0xC,0x3,0x5,0x9,0x7,0xD,0x2,0xE
		};

        static readonly public byte[] ESbox_A = {
			0x9,0x6,0x3,0x2,0x8,0xB,0x1,0x7,0xA,0x4,0xE,0xF,0xC,0x0,0xD,0x5,
			0x3,0x7,0xE,0x9,0x8,0xA,0xF,0x0,0x5,0x2,0x6,0xC,0xB,0x4,0xD,0x1,
			0xE,0x4,0x6,0x2,0xB,0x3,0xD,0x8,0xC,0xF,0x5,0xA,0x0,0x7,0x1,0x9,
			0xE,0x7,0xA,0xC,0xD,0x1,0x3,0x9,0x0,0x2,0xB,0x4,0xF,0x8,0x5,0x6,
			0xB,0x5,0x1,0x9,0x8,0xD,0xF,0x0,0xE,0x4,0x2,0x3,0xC,0x7,0xA,0x6,
			0x3,0xA,0xD,0xC,0x1,0x2,0x0,0xB,0x7,0x5,0x9,0x4,0x8,0xF,0xE,0x6,
			0x1,0xD,0x2,0x9,0x7,0xA,0x6,0x0,0x8,0xC,0x4,0x5,0xF,0x3,0xB,0xE,
			0xB,0xA,0xF,0x5,0x0,0xC,0xE,0x8,0x6,0x2,0x3,0x9,0x1,0x7,0xD,0x4
		};

        static readonly public byte[] ESbox_B = {
         0x8,0x4,0xB,0x1,0x3,0x5,0x0,0x9,0x2,0xE,0xA,0xC,0xD,0x6,0x7,0xF,
         0x0,0x1,0x2,0xA,0x4,0xD,0x5,0xC,0x9,0x7,0x3,0xF,0xB,0x8,0x6,0xE,
         0xE,0xC,0x0,0xA,0x9,0x2,0xD,0xB,0x7,0x5,0x8,0xF,0x3,0x6,0x1,0x4,
         0x7,0x5,0x0,0xD,0xB,0x6,0x1,0x2,0x3,0xA,0xC,0xF,0x4,0xE,0x9,0x8,
         0x2,0x7,0xC,0xF,0x9,0x5,0xA,0xB,0x1,0x4,0x0,0xD,0x6,0x8,0xE,0x3,
         0x8,0x3,0x2,0x6,0x4,0xD,0xE,0xB,0xC,0x1,0x7,0xF,0xA,0x0,0x9,0x5,
         0x5,0x2,0xA,0xB,0x9,0x1,0xC,0x3,0x7,0x4,0xD,0x0,0x6,0xF,0x8,0xE,
         0x0,0x4,0xB,0xE,0x8,0x3,0x7,0x1,0xA,0x2,0x9,0x6,0xF,0xD,0x5,0xC
        };

        static readonly public byte[] ESbox_C = {
             0x1,0xB,0xC,0x2,0x9,0xD,0x0,0xF,0x4,0x5,0x8,0xE,0xA,0x7,0x6,0x3,
             0x0,0x1,0x7,0xD,0xB,0x4,0x5,0x2,0x8,0xE,0xF,0xC,0x9,0xA,0x6,0x3,
             0x8,0x2,0x5,0x0,0x4,0x9,0xF,0xA,0x3,0x7,0xC,0xD,0x6,0xE,0x1,0xB,
             0x3,0x6,0x0,0x1,0x5,0xD,0xA,0x8,0xB,0x2,0x9,0x7,0xE,0xF,0xC,0x4,
             0x8,0xD,0xB,0x0,0x4,0x5,0x1,0x2,0x9,0x3,0xC,0xE,0x6,0xF,0xA,0x7,
             0xC,0x9,0xB,0x1,0x8,0xE,0x2,0x4,0x7,0x3,0x6,0x5,0xA,0x0,0xF,0xD,
             0xA,0x9,0x6,0x8,0xD,0xE,0x2,0x0,0xF,0x3,0x5,0xB,0x4,0x1,0xC,0x7,
             0x7,0x4,0x0,0x5,0xA,0x2,0xF,0xE,0xC,0x6,0x1,0xB,0xD,0x9,0x3,0x8
        };

        private unsafe int Gost28147_mainStep(int n1, int key, byte* S)
		{
			int cm = (key + n1); // CM1

			// S-box replacing

			int om;

            om  = S[  0 + ((cm >> (0 * 4)) & 0xF)] << (0 * 4);
			om += S[ 16 + ((cm >> (1 * 4)) & 0xF)] << (1 * 4);
			om += S[ 32 + ((cm >> (2 * 4)) & 0xF)] << (2 * 4);
			om += S[ 48 + ((cm >> (3 * 4)) & 0xF)] << (3 * 4);
			om += S[ 64 + ((cm >> (4 * 4)) & 0xF)] << (4 * 4);
			om += S[ 80 + ((cm >> (5 * 4)) & 0xF)] << (5 * 4);
			om += S[ 96 + ((cm >> (6 * 4)) & 0xF)] << (6 * 4);
			om += S[112 + ((cm >> (7 * 4)) & 0xF)] << (7 * 4);


//			return om << 11 | om >>> (32-11); // 11-leftshift
			int omLeft = om << 11;
			int omRight = (int)(((uint) om) >> (32 - 11)); // Note: Casts required to get unsigned bit rotation

			return omLeft | omRight;
		}

        public readonly int BlockSize = 8;
        unsafe public int ProcessBlock(
			byte[]	input,
			int		inOff,
			byte[]	output,
			int		outOff,
            int[]   workingKey,
            byte *  S,
            bool    forEncryption = true
            )
		{
			if (workingKey == null)
			{
				throw new InvalidOperationException("Gost28147 engine not initialised");
			}

			if ((inOff + BlockSize) > input.Length)
			{
				throw new Exception("input buffer too short");
			}

			if ((outOff + BlockSize) > output.Length)
			{
				throw new Exception("output buffer too short");
			}

			Gost28147Func(workingKey, input, inOff, output, outOff, forEncryption, S);

			return BlockSize;
		}

        private static int bytesToint
                                    (
			                            byte[]  inBytes,
			                            int     inOff
                                    )
		{
			return  (int)((inBytes[inOff + 3] << 24) & 0xff000000) + ((inBytes[inOff + 2] << 16) & 0xff0000) +
					((inBytes[inOff + 1] << 8) & 0xff00) + (inBytes[inOff] & 0xff);
		}

		private static void intTobytes(
				                        int     num,
				                        byte[]  outBytes,
				                        int     outOff
                                        )
		{
				outBytes[outOff + 3] = (byte)(num >> 24);
				outBytes[outOff + 2] = (byte)(num >> 16);
				outBytes[outOff + 1] = (byte)(num >> 8);
				outBytes[outOff] =     (byte)num;
		}

        unsafe private void Gost28147Func(
			int[]   workingKey,
			byte[]  inBytes,
			int     inOff,
			byte[]  outBytes,
			int     outOff,
            bool    forEncryption,
            byte *  S)
		{
			int N1, N2, tmp;  //tmp -> for saving N1
			N1 = bytesToint(inBytes, inOff);
			N2 = bytesToint(inBytes, inOff + 4);

			if (forEncryption)
			{
			    for(int k = 0; k < 3; k++)  // 1-24 steps
			    {
				    for(int j = 0; j < 8; j++)
				    {
					    tmp = N1;
					    int step = Gost28147_mainStep(N1, workingKey[j], S);
					    N1 = N2 ^ step; // CM2
					    N2 = tmp;
				    }
			    }
			    for(int j = 7; j > 0; j--)  // 25-31 steps
			    {
				    tmp = N1;
				    N1 = N2 ^ Gost28147_mainStep(N1, workingKey[j], S); // CM2
				    N2 = tmp;
			    }
			}
			else //decrypt
			{
			    for(int j = 0; j < 8; j++)  // 1-8 steps
			    {
				    tmp = N1;
				    N1 = N2 ^ Gost28147_mainStep(N1, workingKey[j], S); // CM2
				    N2 = tmp;
			    }
			    for(int k = 0; k < 3; k++)  //9-31 steps
			    {
				    for(int j = 7; j >= 0; j--)
				    {
					    if ((k == 2) && (j==0))
					    {
						    break; // break 32 step
					    }
					    tmp = N1;
					    N1 = N2 ^ Gost28147_mainStep(N1, workingKey[j], S); // CM2
					    N2 = tmp;
				    }
			    }
			}

			N2 = N2 ^ Gost28147_mainStep(N1, workingKey[0], S);  // 32 step (N1=N1)

			intTobytes(N1, outBytes, outOff);
			intTobytes(N2, outBytes, outOff + 4);
		}

        private int[] generateWorkingKey(
			                            byte[]  userKey, int offset = 0
                                        )
		{
			int[] key = new int[8];
			for(int i = 0; i < 8; i++)
			{
				key[i] = bytesToint(userKey, (i << 2) + offset);
			}

			return key;
		}

        /// <summary>
        /// Подготовить вызов функции getGamma
        /// </summary>
        /// <param name="key"></param>
        /// <param name="syncro"></param>
        /// <param name="SBox"></param>
        /// <param name="SBox2"></param>
        /// <param name="errorCipher">Этот параметр всегда должен быть false. Требуется для совместимости со старыми версиями шифра. GOST5 в getGamma должен быть установлен в false</param>
        public void prepareGamma(byte[] key, byte[] syncro, byte[] SBox, byte[] SBox2, byte[] SBox3 = null, byte[] SBox4 = null, byte[] SBox5 = null, byte[] SBox6 = null, bool errorCipher = false)
        {
            int k = SBox6 == null ? 4 : 5;
            if (key.Length < 256*k/8)
                throw new ArgumentException("key must be 256*" + k + " bits size (or greater, but will use 256*k bits)");

            workingKey = new int[k][];
            if (errorCipher && SBox6 == null)
                for (int i = 0; i < k; i++)
                    workingKey[i] = generateWorkingKey(key, i << 3);
            else
            {
                for (int i = 0; i < k; i++)
                    workingKey[i] = generateWorkingKey(key, i << 5);

                workingKeyA = new byte[32*5];
                BytesBuilder.CopyTo(key, workingKeyA);
            }

            BytesBuilder.CopyTo(syncro, N1, 0, 4);
            BytesBuilder.CopyTo(syncro, N2, 0, 4, 4);

            snc = syncro;

            currentSBox  = SBox;
            currentSBox2 = SBox2;
            currentSBox3 = SBox3;
            currentSBox4 = SBox4;
            currentSBox5 = SBox5;
            currentSBox6 = SBox6;

            if (currentSBox3 == null)
                currentSBox3 = Sbox_Default;
            if (currentSBox4 == null)
                currentSBox4 = ESbox_D;
            if (currentSBox5 == null)
                currentSBox5 = ESbox_B;
            if (currentSBox6 == null)
                currentSBox6 = ESbox_C;
        }

        int[][]  workingKey  = null;
        byte[]   workingKeyA = null;
        byte[]   currentSBox, currentSBox2, currentSBox3, currentSBox4, currentSBox5, currentSBox6;
        readonly byte[] N1 = new byte[4], N2 = new byte[4], _N3 = new byte[8], _N4 = new byte[8];
        byte[] snc;
        keccak.SHA3.SHA3Random shaP = null, shaPP = null;
        // GOST5 = GostRegime - 21
        public unsafe byte[] getGamma(long gammaLength, int GOST5 = 0)
        {
            if (workingKey == null)
                throw new Exception("GOST 28147 mod. gamma not prepared");

            if (gammaLength <= 0)
                throw new ArgumentOutOfRangeException("GOST 28147 mod. gammaLenght is incorrect");

            var bb = new BytesBuilder();
            int SL = snc.Length & 0x7FFFFFF8;
            int BL = SL;
            if (SL > 72)
                BL = 64;
            bool BL72 = false;

            byte[] S = null;
            byte[] SA = null;
            int tmp72 = 0;
            int kl    = 0;

            byte[] tmpp = null;

            keccak.SHA3 sha = null, shaR = null;

            if (GOST5 > 0)
            {
                BL72  = (SL % 72 > 0) && (SL > 71);
                if (GOST5 < 13)
                    tmp72 = (SL / 72) * (GOST5 == 1 ? 64 : 16);
                else
                {
                    tmp72 =  (SL / 72) * 8;    // Хеш возвращает только 8 байтов
                }

                S     = BytesBuilder.CloneBytes(snc, 0, SL);
                SA    = new byte[SL];
                tmpp  = new byte[SL+72];
                sha   = new SHA3(SL);
                shaP  = new SHA3.SHA3Random(snc);    // это не лишнее, получает ключ для инициализирующих перестановок
                if (GOST5 >= 13)
                {
                    shaPP = new SHA3.SHA3Random(shaP.nextBytes64());
                    shaR  = new SHA3(SL);
                }

                if (SL < 64)
                    throw new ArgumentException("GOST 28147 mod. sync lenght is incorrect");
            }
            snc = null;

            fixed (  byte* S1 = currentSBox, S2 = currentSBox2, S3 = currentSBox3, S4 = currentSBox4, S5 = currentSBox5, S6 = currentSBox6  )
            {
                BytesBuilder.CopyTo(N1, _N3);
                BytesBuilder.CopyTo(N2, _N3, 4);

                byte[]  k = {0, 1, 2, 3, 4};
                byte*[] b = {S1, S2, S3, S4, S5, S6};

                if (GOST5 > 0)
                {
                    if (GOST5 <= 2)
                    {
                        processBlocksForGamma(SL, S,  SA, k, b);
                        processBlocksForGamma(SL, SA, S,  k, b, true);
                        processBlocksForGamma(SL, S,  SA, k, b, true);
                        processBlocksForGamma(SL, SA, S,  k, b, true);
                    }
                    else
                    {
                        processBlocksForGamma2(SL, S,  SA, k, b, false, GOST5);
                        processBlocksForGamma2(SL, SA, S,  k, b, true,  GOST5);
                        processBlocksForGamma2(SL, S,  SA, k, b, true,  GOST5);
                        processBlocksForGamma2(SL, SA, S,  k, b, true,  GOST5);
                    }
                    sha .getDuplex(S, false, -1, false);
                    if (GOST5 >= 13)
                    shaR.getDuplex(S, false, -1, false);
                }
                else
                {
                    // Делаем несколько раз шифрование, на всяк пожарный, хотя это и не по ГОСТу
                    ProcessBlock(_N3, 0, _N4, 0, workingKey[0], S2);
                    ProcessBlock(_N4, 0, _N3, 0, workingKey[1], S3);
                    ProcessBlock(_N3, 0, _N4, 0, workingKey[2], S4);
                    ProcessBlock(_N4, 0, _N3, 0, workingKey[3], S1);

                    ProcessBlock(_N3, 0, _N4, 0, workingKey[2], S3);
                    ProcessBlock(_N4, 0, _N3, 0, workingKey[1], S1);
                    ProcessBlock(_N3, 0, _N4, 0, workingKey[3], S2);
                    ProcessBlock(_N4, 0, _N3, 0, workingKey[0], S4);
                }

                BytesBuilder.CopyTo(_N3, N1);
                BytesBuilder.CopyTo(_N3, N2, 0, 4, 4);

                long n1 = bytesToint(N1, 0), n2 = bytesToint(N2, 0);
                int  _n1, _n2;

                int ik = 0;
                do
                {
                    
                    if (GOST5 > 0)
                    {
                        byte[] tmp1, tmp2, tmp3;
                        fixed (byte* s = S, sa = SA, tmpp_ = tmpp)
                        {
                            if (GOST5 <= 2)
                                processBlocksForGamma(SL, S,  SA, k, b);
                            else
                                processBlocksForGamma2(SL, S,  SA, k, b, false, GOST5);

                            if (GOST5 < 13)
                                tmp1 = sha .getDuplex(SA, true);
                            else
                                tmp1 = shaR.getDuplex(SA, true);
                            addConstant(s, S.Length);
                            tmp3 = BytesBuilder.CloneBytes(sa, 0, SA.Length);

                            if (GOST5 <= 2)
                                processBlocksForGamma(SL, S,  SA, k, b);
                            else
                                processBlocksForGamma2(SL, S,  SA, k, b, false, GOST5);

                            if (GOST5 < 13)
                            fixed (byte * tmp1_ = tmp1, tmp3_ = tmp3)
                            {
                                if (GOST5 > 1)
                                    MergePermutation.permutationMergeBytes(tmp3_, tmp3.Length, sa, tmpp_, SL, ref kl, true);

                                SHA3.xorBytesWithGamma(SA, tmp1);

                                MergePermutation.permutationMergeBytes(tmp1_, tmp1.Length, sa, tmpp_, SL, ref kl, true);
                                BytesBuilder.ToNull(tmp1.Length, tmp1_);
                                BytesBuilder.ToNull(tmp3.Length, tmp3_);
                            }

                            if (GOST5 < 13)
                                tmp2 = sha.getDuplex(SA, true, GOST5 == 1 ? 64 : 16);
                            else
                            {
                                tmp2 = sha.getDuplexMod(SA, tmp1, true, 8, 0);
                                BytesBuilder.ToNull(tmp1);
                                BytesBuilder.ToNull(tmp3);
                            }
                            addConstant(s, S.Length);
                        }

                        if (BL72)
                        {
                            bb.add(BytesBuilder.CloneBytes(tmp2, tmp2.Length - tmp72, tmp72));
                            BytesBuilder.ToNull(tmp2);
                        }
                        else
                            bb.add(tmp2);
                    }
                    else
                    {
                        n1 += 0x1010101;
                        n2 += 0x1010104;

                        if (n2 >= 0x100000000L)
                            n2 -= 0xFFFFFFFFL;
                        if (n1 >= 0x100000000L)
                            n1 -= 0x100000000L;

                        _n1 = (int) n1;
                        _n2 = (int) n2;

                        intTobytes( _n1, _N3, 0 );
                        intTobytes( _n2, _N3, 4 );

                        // Получаем новые значения N1; опять не по ГОСТу - 4 преобразования вместо одного
                        ProcessBlock(_N3, 0, _N4, 0, workingKey[2], S1);
                        ProcessBlock(_N4, 0, _N3, 0, workingKey[(ik+1) & 1], S3);
                        ProcessBlock(_N3, 0, _N4, 0, workingKey[ik & 1], S4);
                        ProcessBlock(_N4, 0, _N3, 0, workingKey[3], S2);

                        bb.addCopy(_N3);
                    }

                    ik++;
                }
                while (bb.Count < gammaLength);

                 n1 = 0;
                 n2 = 0;
                _n1 = 0;
                _n2 = 0;
            }

            for (int i = 0; i < workingKey.Length; i++)
                for (int j = 0; j < workingKey[i].Length; j++)
                    workingKey[i][j] = 0;
            workingKey = null;
            if (workingKeyA != null)
            {
                BytesBuilder.ToNull(workingKeyA);
                workingKeyA = null;
            }

            BytesBuilder.ToNull(N1);
            BytesBuilder.ToNull(N2);
            BytesBuilder.ToNull(_N3);
            BytesBuilder.ToNull(_N4);

            var result = bb.getBytes(gammaLength);
            bb.clear();
            if (S != null)
            {
                BytesBuilder.ToNull(S);
                BytesBuilder.ToNull(SA);
                BytesBuilder.ToNull(tmpp);
            }

            if (sha != null)
            {
                sha .Clear(true);
                shaP.Clear();
                sha  = null;
                shaP = null;

                if (shaPP != null)
                {
                    shaPP.Clear();
                    shaPP = null;
                }
            }

            return result;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="SL"></param>
        /// <param name="block"></param>
        /// <param name="outblock"></param>
        /// <param name="S1"></param>
        /// <param name="S2"></param>
        /// <param name="S3"></param>
        /// <param name="S4"></param>
        /// <param name="S5"></param>
        /// <param name="k">byte[]  k = {0, 1, 2, 3}</param>
        /// <param name="b">byte*[] b = {S1, S2, S3, S4, S5};</param>
        protected unsafe void processBlocksForGamma(int SL, byte[] block, byte[] outblock, byte[] k, byte*[] b, bool permutation = false)
        {
            for (int i = 0, j = 0; i < SL; i += 8, j++)
            {
                int N = bytesToint(block, i);
                for (int i1 = 0;      i1 < b.Length; i1++)
                for (int i2 = i1 + 1; i2 < b.Length; i2++)
                {
                    if ((N & 1) > 0)
                    {
                        byte * t = b[i1];
                        b[i1] = b[i2];
                        b[i2] = t;
                    }
                    
                    N >>= 1;
                }

                N = bytesToint(block, i + 4);
                for (int i1 = 0;      i1 < k.Length; i1++)
                for (int i2 = i1 + 1; i2 < k.Length; i2++)
                {
                    if ((N & 1) > 0)
                    {
                        byte t = k[i1];
                        k[i1] = k[i2];
                        k[i2] = t;
                    }
                    
                    N >>= 1;
                }

                if ((j & 1) > 0)
                {
                    /*ProcessBlock(block, i, outblock, i, workingKey[k[3]], b[0], true);
                    ProcessBlock(block, i, outblock, i, workingKey[k[1]], b[1], false);
                    ProcessBlock(block, i, outblock, i, workingKey[k[0]], b[2], true);
                    ProcessBlock(block, i, outblock, i, workingKey[k[2]], b[3], false);*/
                    ProcessBlock(block, i, outblock, i, workingKey[k[3]], b[4], true);
                }
                else
                {
                    /*ProcessBlock(block, i, outblock, i, workingKey[k[0]], b[4], true);
                    ProcessBlock(block, i, outblock, i, workingKey[k[3]], b[3], false);
                    ProcessBlock(block, i, outblock, i, workingKey[k[2]], b[2], true);
                    ProcessBlock(block, i, outblock, i, workingKey[k[1]], b[1], false);*/
                    ProcessBlock(block, i, outblock, i, workingKey[k[0]], b[0], true);
                }
            }

            if (permutation)
                BlocksPermutation(SL, outblock, block, k, b);
        }

        protected unsafe void pbfg2Perm(byte[] k, int[,] p, int n)
        {
            //byte[] kn = (byte[]) k.Clone();
            byte[] kn = { k[0], k[1], k[2], k[3], k[4] };

            for (int i = 0; i < k.Length; i++)
            {
                k[i] = kn[p[n, i]];
            }
        }

        protected unsafe void pbfg2PPerm(byte*[] b, int[,] p, int n)
        {
            byte*[] bn = (byte*[]) b.Clone();

            for (int i = 0; i < b.Length; i++)
            {
                b[i] = bn[p[n, i]];
            }
        }

        int[,] ka = { { 4, 2, 1, 0, 3 },
            { 1, 0, 2, 4, 3 },
            { 1, 2, 0, 3, 4 },
            { 2, 0, 1, 3, 4 },
            { 2, 1, 0, 4, 3 },
            { 3, 0, 4, 2, 1 },
            { 0, 4, 3, 1, 2 },
            { 0, 3, 1, 4, 2 } };

        int[,] ba = { { 0, 2, 1, 4, 3, 5 },
            { 1, 0, 2, 4, 3, 5 },
            { 5, 2, 0, 3, 4, 1 },
            { 2, 5, 1, 3, 4, 0 },
            { 2, 1, 5, 4, 3, 0 },
            { 3, 0, 4, 1, 2, 5 },
            { 0, 4, 3, 1, 2, 5 },
            { 0, 3, 1, 4, 2, 5 } };

        byte[] bytes1 = Encoding.ASCII.GetBytes("PW+)q2C5jwJhIrm+WU3M&9AYjQkZGFV!$CS4g9eesJ&Ykyyk8YGt5acVAHh0ly*Yym0ZS$m");
        byte[] bytes2 = Encoding.ASCII.GetBytes("#zBKaWTYa5bMOE85+^4gH3E(6GOHs)RhD5ucCmvFWUYO$fO6kVAUig%JysYaXM^6zLP8Ihy");
        byte[] bytes3 = Encoding.ASCII.GetBytes("CdxVJ6%WSfDJDMUZOSepvvYrGRf4HKM(Tsh+novbrw*mGnukeyIcnzmK(%98q+co^CB2PKM");
        byte[] bytes4 = Encoding.ASCII.GetBytes("TA93Q)WRJLV^s3F0NRi*x9v*IO+c7HW5#)DkWL(3yaHaY2Zyd_WrSu#%Jtpo*H&0YrVgM6X");

        protected unsafe void processBlocksForGamma2(int SL, byte[] block, byte[] outblock, byte[] k, byte*[] b, bool permutation, int GOST5)
        {
            if (b.Length != 6 || k.Length != 5)
                throw new ArgumentException("processBlocksForGamma2 b.Length != 6 || k.Length != 5");

            long works = 0;
            var  sync  = new Object();

            SHA3 sha = null;
            if (GOST5 >= 13)
            {
                shaPP.randomize(block);

                sha = new SHA3(block.Length);
                sha.getDuplex(block, false, -1, false);
            }

            if (GOST5 < 13)
            {
                if ((SL & 7) > 0)
                    throw new ArgumentException("processBlocksForGamma2 SL & 7 > 0");
            }
            else
            {
                if ((SL & 15) > 0)
                    throw new ArgumentException("processBlocksForGamma2 SL & 15 > 0: " + SL);
            }

            byte[] Na = (GOST5 >= 13) ? shaPP.nextBytes64() : null; //bytesToint(block, i);
            int nk2 = 0;
            int bStep = (GOST5 >= 13) ? 16 : 8;
            for (int i = 0, j = 0; i < SL; i += bStep, j++)
            {
                if (i + bStep > SL)
                    throw new ArgumentOutOfRangeException("i + bStep >= SL");

                if (GOST5 < 13)
                {
                    int N = bytesToint(block, i);
                    for (int i1 = 0;      i1 < b.Length; i1++)
                    for (int i2 = i1 + 1; i2 < b.Length; i2++)
                    {
                        if ((N & 1) > 0)
                        {
                            byte * t = b[i1];
                            b[i1] = b[i2];
                            b[i2] = t;
                        }
                        N >>= 1;
                    }

                    N = bytesToint(block, i + 4);
                    for (int i1 = 0;      i1 < k.Length; i1++)
                    for (int i2 = i1 + 1; i2 < k.Length; i2++)
                    {
                        if ((N & 1) > 0)
                        {
                            byte t = k[i1];
                            k[i1] = k[i2];
                            k[i2] = t;
                        }

                        N >>= 1;
                    }

                }
                else
                {
                    if (i > SL - 15)
                        break;

                    byte _L = (byte) (j & 7);

                    if (nk2 > Na.Length - 8)
                    {
                        Na = shaPP.nextBytes64();
                        nk2 = 0;
                    }

                    for (int nk2i = nk2; nk2i < nk2 + 8; nk2i++)
                    {
                        pbfg2Perm (k, ka, (Na[nk2i] & 7) ^ _L);
                        Na[nk2i] >>= 3;
                        pbfg2PPerm(b, ba, (Na[nk2i] & 7) ^ _L);
                    }
                    nk2 += 8;
                }

                var _i = i;
                var _j = j & 1;
                var k0 = k[0];
                var k1 = k[1];
                var k2 = k[2];
                var k3 = k[3];
                var k4 = GOST5 >= 13 ? k[4] : 0;
                var b0 = b[0];
                var b1 = b[1];
                var b2 = b[2];
                var b3 = b[3];
                var b4 = b[4];
                var sh = GOST5 >= 13 ? sha.Clone() : null;

                Interlocked.Increment(ref works);
                ThreadPool.QueueUserWorkItem
                (
                    delegate
                    {
                        try
                        {
                            if (GOST5 >= 13)
                            {
                                var tmp0 = new byte[16];
                                var tmp1 = new byte[16];
                                var tmp2 = new byte[16];

                                BytesBuilder.CopyTo(block, tmp0, 0, -1, _i);
                                BytesBuilder.CopyTo(tmp0, tmp1);
                                BytesBuilder.CopyTo(tmp0, tmp2);
                                BytesBuilder.ToNull(tmp0);


                                tmp1 = sh.getDuplex(tmp2, true, 16, true, 16);
                                ProcessBlock(tmp1, 0, tmp2, 0, workingKey[k0], b4, true);
                                ProcessBlock(tmp1, 8, tmp2, 8, workingKey[k0], b0, true);
                                BytesBuilder.ToNull(tmp1);

                                //sh.getDuplex(bytes1, true, 0, false);
                                tmp1 = sh.getDuplex(tmp2, true, 16, true, 16);
                                ProcessBlock(tmp1, 0, tmp2, 0, workingKey[k1], b0, true);
                                ProcessBlock(tmp1, 8, tmp2, 8, workingKey[k1], b1, true);
                                BytesBuilder.ToNull(tmp1);

                                tmp1 = sh.getDuplex(tmp2, true, 16, true, 16);
                                ProcessBlock(tmp1, 0, tmp2, 0, workingKey[k2], b1, true);
                                ProcessBlock(tmp1, 8, tmp2, 8, workingKey[k2], b2, true);
                                BytesBuilder.ToNull(tmp1);
                                
                                tmp1 = sh.getDuplex(tmp2, true, 16, true, 16);
                                ProcessBlock(tmp1, 0, tmp2, 0, workingKey[k3], b2, true);
                                ProcessBlock(tmp1, 8, tmp2, 8, workingKey[k3], b3, true);
                                BytesBuilder.ToNull(tmp1);

                                tmp1 = sh.getDuplex(tmp2, true, 16, true, 16);
                                ProcessBlock(tmp1, 0, tmp2, 0, workingKey[k4], b3, true);
                                ProcessBlock(tmp1, 8, tmp2, 8, workingKey[k4], b4, true);
                                BytesBuilder.ToNull(tmp1);

                                BytesBuilder.CopyTo(tmp2, outblock, _i);
                                BytesBuilder.ToNull(tmp2);
                            }
                            else
                            if (_j > 0)
                            {
                                ProcessBlock(block,    _i, outblock, _i, workingKey[k3], b0, true);
                                ProcessBlock(outblock, _i, block,    _i, workingKey[k1], b1, false);
                                ProcessBlock(block,    _i, outblock, _i, workingKey[k0], b2, true);
                                ProcessBlock(outblock, _i, block,    _i, workingKey[k2], b3, false);
                                ProcessBlock(block,    _i, outblock, _i, workingKey[k3], b4, true);
                            }
                            else
                            {
                                ProcessBlock(block,    _i, outblock, _i, workingKey[k0], b4, true);
                                ProcessBlock(outblock, _i, block,    _i, workingKey[k3], b3, false);
                                ProcessBlock(block,    _i, outblock, _i, workingKey[k2], b2, true);
                                ProcessBlock(outblock, _i, block,    _i, workingKey[k1], b1, false);
                                ProcessBlock(block,    _i, outblock, _i, workingKey[k0], b0, true);
                            }
                        }
                        finally
                        {
                            lock (sync)
                            {
                                Interlocked.Decrement(ref works);
                                Monitor.Pulse(sync);
                            }
                        }
                    }
                );
            }

            lock (sync)
                while (Interlocked.Read(ref works) > 0)
                    Monitor.Wait(sync);

            if (permutation)
                BlocksPermutation(SL, outblock, block, k, b, GOST5);
        }

        protected unsafe void BlocksPermutation(int SL, byte[] targetBlock, byte[] keyBlock, byte[] k, byte*[] b, int GOST5 = 0)
        {
            int s = 0;
            if (GOST5 >= 13)
            {
                /*shaP.randomize(keyBlock);
                MergePermutation2.permutationMergeBytes(shaP.nextBytes64(), targetBlock);*/
            }
            else
            {
                shaP.wrongRandomize(keyBlock);
                
                ulong N = shaP.nextLong();
                for (int i = SL - 1; i > 0; i--)
                {
                    for (int i2 = 0; i2 < i; i2++, s++)
                    {
                        if (s >= 64)
                        {
                            s = 0;
                            N = shaP.nextLong();
                        }

                        if ((N & 1) > 0)
                        {
                            byte t = targetBlock[i];
                            targetBlock[i] = targetBlock[i2];
                            targetBlock[i2] = t;
                        }

                        N >>= 1;
                    }
                }
            }
        }

        public static unsafe void addConstant(byte* data, int l, int k = 0x55)
        {
            int f = 0;
            //bool s = false;
            for (int i = 0; i < l; i++)
            {
                int a = data[i];
                a += k + f;

                data[i] = (byte) a;
                f = a >> 8;
            }
        }

        /// <summary>
        /// Гамма по ГОСТ 28147-89. Не требует вызова prepareGamma
        /// </summary>
        /// <param name="key">Ключ шифрования</param>
        /// <param name="syncro">Синхропосылка (берутся первые 8-мь байт)</param>
        /// <param name="SBox">Таблица перестановок (см. статические S-таблицы)</param>
        /// <param name="gammaLength">Длинна гаммы в байтах</param>
        /// <returns>Гамма по ГОСТ 28147-89</returns>
        public unsafe byte[] getGOSTGamma(byte[] key, byte[] syncro, byte[] SBox, long gammaLength)
        {
            if (key.Length < 32)
                throw new ArgumentException("key must be 256 bit size (or greater, but will use 256*4 bits)");

            byte[] N1 = new byte[4], N2 = new byte[4], _N3 = new byte[8], _N4 = new byte[8];

            int[] workingKey = generateWorkingKey(key);
            BytesBuilder.CopyTo(syncro, N1, 0, 4);
            BytesBuilder.CopyTo(syncro, N2, 0, 4, 4);

            byte[] currentSBox  = SBox;

            if (gammaLength <= 0)
                throw new ArgumentOutOfRangeException("GOST 28147 gammaLenght is incorrect");

            var bb = new BytesBuilder();

            fixed (byte* S1 = SBox)
            {
                BytesBuilder.CopyTo(N1, _N3);
                BytesBuilder.CopyTo(N2, _N3, 4);

                ProcessBlock(_N3, 0, _N4, 0, workingKey, S1);

                BytesBuilder.CopyTo(_N4, N1);
                BytesBuilder.CopyTo(_N4, N2, 0, 4, 4);

                long n1 = bytesToint(N1, 0), n2 = bytesToint(N2, 0);
                int  _n1, _n2;

                do
                {
                    n1 += 0x1010101;
                    n2 += 0x1010104;

                    if (n2 >= 0x100000000L)
                        n2 -= 0xFFFFFFFFL;
                    if (n1 >= 0x100000000L)
                        n1 -= 0x100000000L;

                    _n1 = (int) n1;
                    _n2 = (int) n2;

                    intTobytes( _n1, _N3, 0 );
                    intTobytes( _n2, _N3, 4 );

                    ProcessBlock(_N3, 0, _N4, 0, workingKey, S1);

                    bb.addCopy(_N4);
                }
                while (bb.Count < gammaLength);

                 n1 = 0;
                 n2 = 0;
                _n1 = 0;
                _n2 = 0;
            }

            for (int i = 0; i < workingKey.GetLongLength(0); i++)
                    workingKey[i] = 0;

            BytesBuilder.ToNull(N1);
            BytesBuilder.ToNull(N2);
            BytesBuilder.ToNull(_N3);
            BytesBuilder.ToNull(_N4);

            var result = bb.getBytes(gammaLength);
            bb.clear();
            return result;
        }
    }
}

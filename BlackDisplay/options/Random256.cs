using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tests;

namespace options
{
    public partial class Random256
    {
        public readonly int    seed;
        public readonly byte[] bytes = new byte[256];
        public Random256(int seed)
        {
            this.seed = seed;

            setStartBytes();
            setByteSeed();
        }

        public static Random256 Create()
        {
            var dt = DateTime.Now.Ticks;
            int c    = (int) (dt & 0x1F);
            int seed = (int) (  dt ^ (dt >> c)  );

            return new Random256(seed);
        }

        public int current = 0;
        public int ct      = 0;
        byte m1 = 0xA5;
        byte m2 = 0xCC;
        byte m3 = 0x49;
        byte a = 1;
        byte b = 127;
        byte d = 255;
        public byte nextByte()
        {
            int c1  = bytes[current];
            if (c1 == current)
                c1  = bytes[current ^ 0xA5];

            int c2  = bytes[c1];

            int mc  = (c2 >> 6) + ct & 1;
            int c26 = (c2 & 0x3F) + ct & 3;
            int c   = (  (c26 << (8 - mc)) + (c1 ^ b) >> mc  ) & 0xFF;

            var r = 0;
            for (int i = -8; i < (c & 0x0F); i++)
            {
                getPermutation3();

                r = (r << 1) + ((d & 8) >> 3);
                if (i >= 0)
                {
                    r += c & 1;
                    c  = c >> 1;
                }
            }

            ct++;
            /*if ((ct & 0xF) == 0)
                xor((byte) (DateTime.Now.Ticks));*/

            /*permutByte(c1, current);
            permutByte(c ^ 0xAA, c ^ 0x55);*/

            xor((byte) (r));
            current = bytes[c2];

            return (byte) r;
        }

        private void getPermutation3()
        {
            a = bytes[b ^ m2];
            b = bytes[d ^ m1];
            d = bytes[a ^ m3];
            permutByte(a, b);

            m2 ^= d;
            m3 ^= b;
            m1 ^= a;
        }

        public void xor(byte mask)
        {
            for (int i = 0; i < bytes.Length; i++)
                bytes[i] ^= mask;
        }

        public void setStartBytes()
        {
            byte c = 0;
            for (int i = 0; i < 128; i++, c++)
            {
                bytes[i] = c;
            }

            for (int i = 255; i >= 128; i--, c++)
            {
                bytes[i] = c;
            }
        }

        public void setByteSeed()
        {
            byte mask = (byte) (seed >> 24);
            xor(mask);
            current = (seed >> 16) & 0xFF;

            int count = seed & 0xFF;
            for (int i = 0; i < count; i++)
                nextByte();

            mask = (byte) ((seed >> 8) & 0xFF);
            xor(mask);
        }

        public void permutByte(int num1, int num2)
        {
            byte b = bytes[num1];

            bytes[num1] = bytes[num2];
            bytes[num2] = b;
        }
    }
}

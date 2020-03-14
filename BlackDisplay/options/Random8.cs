using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tests;

namespace options
{
    public partial class Random8
    {
        public readonly long   seed;
        public readonly long[] bytes = new long[256];
        public Random8(long seed)
        {
            this.seed = seed;

            setStartBytes();
        }

        public static Random8 Create()
        {
            return new Random8(DateTime.Now.Ticks);
        }

        public byte current = 0;
        public long ct      = 0;
        public byte nextByte()
        {
            long c1 = bytes[current];
            if (c1 == current)
                c1 = bytes[current ^ 0xFF];

            byte a = (byte) c1;
            byte b = 0;
            byte[] bs = new byte[] {(byte) c1, (byte) (c1 >> 8), (byte) (c1 >> 16), (byte) (c1 >> 24), (byte) (c1 >> 32), (byte) (c1 >> 40), (byte) (c1 >> 48), (byte) (c1 >> 56)};
            if (c1 < 16)
                b = (byte) ( bs[0] ^ bs[1] ^ bs[3] );
            else
            if (c1 < 31)
                b = (byte) (  (bs[0])  );

            xor(c1);
            int c = bs[0] + bs[1] + bs[2] + bs[3] + bs[4] + bs[5] + bs[6] + bs[7];
            current = (byte)( ((byte) c) ^ b );

            return (byte) bytes[current ^ 0xA5];
        }

        public void xor(long mask)
        {
            for (int i = 0; i < bytes.Length; i++)
                bytes[i] ^= mask;
        }

        public void setStartBytes()
        {
            long c = 0;
            long t = 1;
            for (int i = 0; i < 256; i++)
            {
                c += (t >> 8) ^ seed + i;
                t += seed ^ c;

                bytes[i] = c;
            }
        }
    }
}

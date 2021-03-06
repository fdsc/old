﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace testCiphers
{
    class RC4
    {
        byte[] S = new byte[256];
        byte x = 0;
        byte y = 0;

        public RC4(byte[] key)
        {
            if (key.Length > 128)
                init(key, 128);
            else
                init(key, key.Length);
        }

        private void init(byte[] key, int len)
        {
          int keyLength = len;

          for (int i = 0; i < 256; i++)
          {
            S[i] = (byte)i;
          }

          int j = 0;
          for (int i = 0; i < 256; i++)
          {
            j = (j + S[i] + key[i % keyLength]) % 256;
            swap(S, i, j);
          }
        }

        private byte keyItem()
        {
          x++; // x = (x + 1) % 256;
          y += S[x]; //y = (y + S[x]) % 256;

          swap(S, x, y);

          return S[(byte)(S[x] + S[y])];
        }

       
        // RC4+
        private byte keyItem2()
        {
           //i := i + 1
            x++;

            //a := S[i]
            byte a = S[x];
            //j := j + a
            y += a;

            //b := S[j]
            byte b = S[y];
            //S[i] := b     (поменяли местами S[i] и S[j])
            S[x] = b;
            //S[j] := a
            S[y] = a;
            //c := S[i<<5 ⊕ j>>3] + S[j<<5 ⊕ i>>3]
            byte c = (byte) ( S[(byte)((x << 5) ^ (y >> 3))] + S[(byte)((y << 5) ^ (x >> 3))] );

            //  (S[a+b] + S[c⊕0xAA]) ⊕ S[j+b]
            byte ab = (byte) ( a + b );
            return (byte) (   ( S[ab] + S[c^0xAA] ) ^ S[(byte)(y+b)]   );
        }

        private void swap(byte[] A, int i, int j)
        {
            byte t = A[i];
            A[i] = A[j];
            A[j] = t;
        }

        public byte[] getGamma(int len)
        {
            var result = new byte[len];
            for (int i = 0; i < len; i++)
                result[i] = keyItem();

            return result;
        }

        public byte[] getGamma2(int len)
        {
            var result = new byte[len];
            for (int i = 0; i < len; i++)
                result[i] = keyItem2();

            return result;
        }
    }
}

using keccak;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace testCiphers
{
    class test2018_magmaMod: test2018
    {
        public override void InitializeTest()
        {
            keyLen = 2;
            base.InitializeTest();
        }

        int c1f = 0;
        int c1t = 0;
        public override void ProcessCryptoVariant(long variant)
        {
            var g = new Gost28147Modified();
            var key = prepareKey(variant);
            var s = prepareOIV(variant);
            g.prepareGamma(key, s, Gost28147Modified.CryptoProB, Gost28147Modified.CryptoProC, Gost28147Modified.CryptoProA, Gost28147Modified.CryptoProD, Gost28147Modified.ESbox_A, Gost28147Modified.ESbox_B, false);
            var gamma = g.getGamma(1024*128, 12);   // или 13

            if (gamma[0] == 1)
            {
                if ((variant & 1) > 0)
                    c1f++;
                else
                    c1t++;
            }

            calcResult(gamma, variant, key);
        }

        public override byte[] prepareKey(long variant)
        {
            var k = base.prepareKey(variant);

            var key = new byte[256*5];
            BytesBuilder.ToNull(key);
            BytesBuilder.CopyTo(k, key);

            return key;
        }
    }
}

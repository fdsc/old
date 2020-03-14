using keccak;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace testCiphers
{
    // Заполнение не по битам, а по байтам
    class test2018_keccakGammaBytes: test2018
    {
        int c1f = 0;
        int c1t = 0;
        public override void ProcessCryptoVariant(long variant)
        {
            SHA3 gs = new SHA3(0);
            var key = prepareKey(variant);
            var s = prepareOIV(variant);
            gs.prepareGamma(key, s);
            var gamma = gs.getGamma(numOfOutBytes);

            if (gamma[0] == 1)
            {
                if ((variant & 1) > 0)
                    c1f++;
                else
                    c1t++;
            }

            calcResult(gamma, variant, key);
        }

        public virtual void InitializeTest()
        {
            keyLen = 2;
            base.InitializeTest();
        }

        public override byte[] prepareKey(long variant)
        {
            var k = new byte[2];
            k[0] = (byte) variant;
            k[1] = (byte) (variant >> 8);

            var key = new byte[64];
            BytesBuilder.ToNull(key);
            BytesBuilder.CopyTo(k, key);

            return key;
        }
    }
}

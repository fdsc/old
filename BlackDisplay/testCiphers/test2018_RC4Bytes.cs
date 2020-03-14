using keccak;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace testCiphers
{
    class test2018_RC4Bytes: test2018
    {
        int c1f = 0;
        int c1t = 0;
        public override void ProcessCryptoVariant(long variant)
        {
            var key = prepareKey(variant);
            var s = prepareOIV(variant);

            var RC4   = new RC4(key);
            var gamma = RC4.getGamma(numOfOutBytes);

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

using keccak;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace testCiphers
{
    class test2018_derivation2: test2018
    {
        public test2018_derivation2()
        {
            keyLen = 24;
        }

        int c1 = 0;
        public override void ProcessCryptoVariant(long variant)
        {
            SHA3 gs = new SHA3(0);
            var key = prepareKey(variant);
            var s = prepareOIV(variant);
            int pc = 1;
            var gamma = gs.getDerivatoKey(key, s, 2, ref pc, 1, 4);

            if (gamma[0] == 0)
                c1++;

            calcResult(gamma, variant, key);
        }

        public override byte[] prepareOIV(long variant)
        {
            lock (sync)
            {
                if (nullBytes == null)
                {
                    nullBytes = new byte[1];
                    BytesBuilder.ToNull(nullBytes);
                }
            }

            return nullBytes;
        }
    }
}

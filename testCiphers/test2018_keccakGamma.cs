using keccak;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace testCiphers
{
    class test2018_keccakGamma: test2018
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
    }
}

using keccak;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace testCiphers
{
    class test2018_keccakGamma3: test2018
    {
        int c1f = 0;
        int c1t = 0;
        public override void ProcessCryptoVariant(long variant)
        {
            SHA3 gs1 = new SHA3(0);
            SHA3 gs2 = new SHA3(0);
            //SHA3 gs3 = new SHA3(0);

            var key = prepareKey(variant);
            var s = prepareOIV(variant);

            gs1.prepareGamma(key, s);
            var gamma = gs1.getGamma(71);
            gs2.prepareGamma(gamma, s);
            gamma = gs1.getDuplex(gamma, true);
            gamma = gs2.getDuplex(gamma, true);

            gamma = gs1.getDuplex(gamma, true);
            gamma = gs2.getDuplex(gamma, true);

            gamma = gs1.getDuplex(gamma, true);
            gamma = gs2.getDuplex(gamma, true);

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

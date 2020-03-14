using keccak;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace testCiphers
{
    class test2018_magma: test2018
    {
        int c1f = 0;
        int c1t = 0;
        public override void ProcessCryptoVariant(long variant)
        {
            var gg = new Gost28147Modified();
            var key = prepareKey(variant);
            var s = prepareOIV(variant);
            var gamma = gg.getGOSTGamma(key, s, Gost28147Modified.ESbox_B, numOfOutBytes);
            //var gamma = gg.getGOSTGamma(key, s, Gost28147Modified.CryptoProB, numOfOutBytes);

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

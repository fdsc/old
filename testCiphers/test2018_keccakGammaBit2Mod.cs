using keccak;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace testCiphers
{
    class test2018_keccakGammaBit2Mod: test2018
    {
        int c1f = 0;
        int c1t = 0;
        public override void ProcessCryptoVariant(long variant)
        {
            SHA3 gs1 = new SHA3(0);
            SHA3 gs2 = new SHA3(0);
            var key = prepareKey(variant);
            var s = prepareOIV(variant);

            var bt = Convert.FromBase64String("GL4C/c+hpZrVQcIg771ujdjHN/2Jqoj+UpS6cZ5fbG7zU1qwXMMg5P3YgnLY9byB1laMMu557wK1J7EewmkKJ0wPjA8u3uVO7oOhmtVSc6rnGMCaTrRHtLVLv9a9dBFnNxGjxbCLOcaGkZZYeBT1KUraXh/5reXVQmedmkijKvEwJ4d3CPSMUoPGca+lv7I=");
            var g1 = gs1.getDuplex(bt);
            var g2 = gs2.getDuplex(key);

            g1 = gs1.getDuplex(g2, true);
            g2 = gs2.getDuplex(g1, true);

            g1 = gs1.getDuplex(g2, true);
            g2 = gs2.getDuplex(g1, true);
            g1 = gs1.getDuplex(g2, true);
            g2 = gs2.getDuplex(g1, true);
            g1 = gs1.getDuplex(g2, true);
            g2 = gs2.getDuplex(g1, true);
            g1 = gs1.getDuplex(g2, true);
            g2 = gs2.getDuplex(g1, true);

            var gamma = gs1.getDuplexMod(g1, g2);


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

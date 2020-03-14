using keccak;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace testCiphers
{
    class test2018_md5: test2018
    {
        int c1f = 0;
        int c1t = 0;
        public override void ProcessCryptoVariant(long variant)
        {
            var key = prepareKey(variant);
            //var s = prepareOIV(variant);

            var md5 = System.Security.Cryptography.MD5.Create();
            var gamma = md5.ComputeHash(key);

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

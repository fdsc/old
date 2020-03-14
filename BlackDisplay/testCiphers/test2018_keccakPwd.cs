using keccak;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace testCiphers
{
    class test2018_keccakPwd: test2018
    {
        public test2018_keccakPwd(): base()
        {
            keyLen = 16;
        }

        int c1f = 0;
        int c1t = 0;
        public override void ProcessCryptoVariant(long variant)
        {
            var key = prepareKey(variant);
            var gamma = SHA3.generateRandomPwdByDerivatoKey(key, keyLen);

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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace keccak
{/*
    public unsafe class SymmetricSigner: IDisposable
    {
        protected readonly SymmetricSignerFuncs ss;
        public SymmetricSigner(byte[][] Keys)
        {
            ss = new SymmetricSignerFuncs(Keys);
        }

        public SymmetricSigner(SymmetricSignerFuncs Keys)
        {
            ss = Keys;
        }

        public SymmetricSignerFuncs.SignerKeys generateNewKeys(byte[] input, byte[] OIV, int numberOfKeys, int keyLength, byte mul = 0)
        {
            var result = SymmetricSignerFuncs.SignerKeys();

            int counter = numberOfKeys;
            for (int i = 0; i < numberOfKeys; i++)
            {
                uint index = (uint) i;
                ThreadPool.QueueUserWorkItem
                (
                    delegate
                    {
                        var keys  = new SymmetricSignerFuncs.SignerKeys();
                        SHA3[] sh = null;
                        try
                        {
                            var num  = new byte[8];
                            var s    = new byte[OIV.Length + 8];
                            var sha  = new SHA3(input.Length);
                            if (mul <= 1)
                                sha.getDuplex(input, false, -1, false);

                            for (uint k = 0; k < keyLength; k++)
                            {
                                BytesBuilder.UIntToBytes(index, ref num);
                                BytesBuilder.UIntToBytes(k,     ref num, 4);

                                if (mul > 1)
                                {
                                    BytesBuilder.CopyTo(num, s);
                                    BytesBuilder.CopyTo(OIV, s, 8);

                                    var hashMul = SHA3.getExHash((byte)(mul+1), OIV, input, ref sh);
                                    keys.add(hashMul);
                                }
                                else
                                {
                                    sha.getDuplex(input, true, -1, false);
                                    sha.getDuplex(OIV,   true, -1, false);
                                    sha.getDuplex(num,   true, -1, false);
                                    keys.add(sha.getGamma(64, true));
                                }
                            }
                            
                            if (mul > 1)
                            {
                                foreach (var sc in sh)
                                {
                                    sc.Clear();
                                }
                            }
                            else
                                sha.Clear(true);

                            result.Add(keys);
                        }
                        finally
                        {
                            lock (result)
                            {
                                counter--;
                                Monitor.Pulse(result);
                            }
                        }
                    }
                );
            }

            return result;
        }

        public void Dispose()
        {
            ss.Dispose();
        }
    }*/
}

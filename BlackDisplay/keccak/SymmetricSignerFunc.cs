using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace keccak
{/*
    public unsafe class SymmetricSignerFuncs: IDisposable
    {
        [System.Runtime.InteropServices.DllImport("kernel32.dll")]
        public static extern byte* VirtualAlloc(byte* adress, int size, uint allocType, uint protect);

        [System.Runtime.InteropServices.DllImport("kernel32.dll")]
        public static extern Int32 VirtualLock(byte* adress, int size);

        [System.Runtime.InteropServices.DllImport("kernel32.dll")]
        public static extern Int32 VirtualUnlock(byte* adress, int size);

        [System.Runtime.InteropServices.DllImport("kernel32.dll")]
        public static extern Int32 VirtualProtect(byte* adress, int size, uint newProtect, out int oldProtect);

        [System.Runtime.InteropServices.DllImport("kernel32.dll")]
        public static extern Int32 VirtualFree(byte* adress, int size, uint freeType);

        [System.Runtime.InteropServices.DllImport("kernel32")]
		public extern static int GetLastError();

        public class SignerKey: IDisposable
        {
            public readonly byte* key;
            public readonly int   keyLen;
            public SignerKey(byte[] keyValue)
            {
                this.keyLen = keyValue.Length;

                if (keyLen < 1)
                    throw new ArgumentOutOfRangeException("keyValue.Length");

                // MEM_COMMIT 0x1000 | MEM_RESERVE 0x2000
                // MEM_PHYSICAL 0x00400000
                // PAGE_READWRITE 0x04
                // PAGE_READONLY 0x02
                // PAGE_NOACCESS 0x01
                key = VirtualAlloc(null, keyLen, 0x1000 | 0x2000, 0x04);
                if (key == null)
                    throw new OutOfMemoryException("SymmetricSigner.SignerKey.SignerKey: VirtualAlloc failed " + GetLastError());

                if (VirtualLock(key, keyLen) == 0)
                    throw new OutOfMemoryException("SymmetricSigner.SignerKey.SignerKey: VirtualLock failed " + GetLastError());

                fixed (byte* kv = keyValue)
                {
                    BytesBuilder.CopyTo(keyLen, keyLen, kv, key);
                    BytesBuilder.ToNull(keyLen, kv);
                }

                int old;
                VirtualProtect(key, keyLen, 0x01, out old);
            }

            unsafe public byte[] getObjectValue()
            {
                var pwdObject = new byte[keyLen];

                fixed (byte * target = pwdObject)
                {
                    int old;
                    VirtualProtect(key, keyLen, 0x02, out old);
                    byte* source = (byte*) key;
                    BytesBuilder.CopyTo(keyLen, keyLen, source, target);
                    VirtualProtect(key, keyLen, 0x01, out old);
                }

                return pwdObject;
            }

            protected bool disposed = false;
            public void Dispose()
            {
                if (disposed)
                    return;

                int old;
                VirtualProtect(key, keyLen, 0x04, out old);
                BytesBuilder.ToNull(keyLen, key);

                // MEM_RELEASE
                VirtualUnlock(key, keyLen);
                VirtualFree(key, 0, 0x8000);
                disposed = true;
            }
        }

        public class SignerKeys: IDisposable
        {
            protected readonly List<SymmetricSignerFuncs> list = new List<SymmetricSignerFuncs>();
            public int Count
            {
                get
                {
                    return list.Count;
                }
            }

            public SignerKeys(): this(null)
            {
            }

            public SignerKeys(byte[][] keys)
            {
                list.Add(new SymmetricSignerFuncs(keys));
            }

            public SignerKey this[int i]
            {
                get
                {
                    return list[i];
                }
            }

            public void Dispose()
            {
                foreach (var s in list)
                {
                    s.Dispose();
                }

                list.Clear();
            }
        }

        protected readonly SignerKeys keys;
        public SymmetricSignerFuncs(byte[][] Keys)
        {
            keys = new SignerKeys(Keys);
        }

        public SymmetricSignerFuncs(SignerKeys Keys)
        {
            keys = Keys;
        }

        public byte[][] getPublicKeys(byte[] parameter, byte[] keyNumber, int[] ValueTo, byte mul = 8)
        {
            if (ValueTo.Length < keys.Count)
                throw new ArgumentOutOfRangeException("ValueTo");

            var publicKeys = new byte[keys.Count][];

            int counter = keys.Count;
            for (int i = 0; i < keys.Count; i++)
            {
                var number = i;
                ThreadPool.QueueUserWorkItem
                (
                    delegate
                    {
                        try
                        {
                            publicKeys[number] = getPublicKey(keys[number], parameter, keyNumber, ValueTo[number], mul);
                        }
                        finally
                        {
                            lock (publicKeys)
                            {
                                counter--;
                                Monitor.Pulse(publicKeys);
                            }
                        }
                    }
                );
            }

            lock (publicKeys)
                while (counter > 0)
                    Monitor.Wait(publicKeys);

            return publicKeys;
        }

        public byte[] getReservedPrivateKey(SignerKey privateKey, byte[] parameter, byte[] keyNumber, SHA3 sh)
        {
            byte[] key1;
            sh.getDuplex(parameter, false, -1, false);
            sh.getDuplex(keyNumber, true,  -1, false);
            var sk  = new byte[64];
            var val = privateKey.getObjectValue();

            try
            {
                BytesBuilder.CopyTo(val, sk);
                sh.getDuplex(sk, true,  -1, false);
                key1 = sh.getGamma(64, true);
            }
            finally
            {
                BytesBuilder.ToNull(val);
                BytesBuilder.ToNull(sk);
            }

            return key1;
        }

        public byte[] getPublicKey(SignerKey privateKey, byte[] parameter, byte[] keyNumber, int ValueTo = 257, byte mul = 8)
        {
            byte[] key1;
            var sh = new SHA3(parameter.Length);
            key1 = getReservedPrivateKey(privateKey, parameter, keyNumber, sh);

            for (int i = 0; i < ValueTo; i++)
            {
                sh.getDuplex(parameter, false, -1, false);
                sh.getDuplex(keyNumber, true,  -1, false);
                sh.getDuplex(key1,      true,  -1, false);
                BytesBuilder.ToNull(key1);
                key1 = sh.getGamma(64, true);
            }
            if (mul == 0)
                return key1;

            byte[] result = new byte[64*mul+64];
            BytesBuilder.CopyTo(key1, result, 64*mul);
            BytesBuilder.ToNull(key1);
            sh.Clear(true);

            var bb = new BytesBuilder();
            bb.addVariableULong((ulong) parameter.LongLength);
            bb.add(parameter);
            bb.add(keyNumber);
            var pkn = bb.getBytes();

            var val = privateKey.getObjectValue();
            byte[] longKey;
            SHA3[] sha = null;
            try
            {
                longKey = SHA3.getExHash(mul, pkn, val, ref sha);
            }
            finally
            {
                BytesBuilder.ToNull(val);
            }

            for (int i = 0; i < ValueTo; i++)
            {
                var k = longKey;
                longKey = SHA3.getExHash(mul, pkn, k, ref sha);
                BytesBuilder.ToNull(k);
            }
            BytesBuilder.CopyTo(longKey, result);
            BytesBuilder.ToNull(longKey);

            return result;
        }

        public byte[] getClosedKey(byte[] signKey, byte[] parameter, byte[] keyNumber, int ValueTo = 257, byte mul = 8)
        {
            byte[] key1   = new byte[64];
            var sh = new SHA3(parameter.Length);
            BytesBuilder.CopyTo(signKey, key1, 0, -1, 64*mul);

            for (int i = 0; i < ValueTo; i++)
            {
                sh.getDuplex(parameter, false, -1, false);
                sh.getDuplex(keyNumber, true,  -1, false);
                sh.getDuplex(key1,      true,  -1, false);
                BytesBuilder.ToNull(key1);
                key1 = sh.getGamma(64, true);
            }
            if (mul == 0)
                return key1;

            byte[] result = new byte[64*mul+64];
            BytesBuilder.CopyTo(key1, result, 64*mul);
            BytesBuilder.ToNull(key1);
            sh.Clear(true);

            var bb = new BytesBuilder();
            bb.addVariableULong((ulong) parameter.LongLength);
            bb.add(parameter);
            bb.add(keyNumber);
            var pkn = bb.getBytes();

            byte[] val = new byte[64*mul];
            BytesBuilder.CopyTo(signKey, val);
            SHA3[] sha = null;
            for (int i = 0; i < ValueTo; i++)
            {
                var k = val;
                val = SHA3.getExHash(mul, pkn, k, ref sha);
                BytesBuilder.ToNull(k);
            }
            BytesBuilder.CopyTo(val, result);
            BytesBuilder.ToNull(val);

            return result;
        }

        public static byte[][] getKeysFromArray(byte[] signKeys, int keysCount)
        {
            var mul = ((signKeys.Length / keysCount) >> 6) - 1;
            if (64*(mul+1)*keysCount != signKeys.Length)
                throw new ArgumentOutOfRangeException("signKeys");

            var signKeysArray = new byte[keysCount][];
            for (int i = 0; i < keysCount; i++)
            {
                signKeysArray[i] = new byte[64*(mul+1)];
                BytesBuilder.CopyTo(signKeys, signKeysArray[i], 0, -1, 64*(mul+1)*i);
            }

            return signKeysArray;
        }

        public static byte[] getArrayFromKeys(byte[][] signKeys)
        {
            if (signKeys.Length < 1)
                throw new ArgumentOutOfRangeException("signKeys");

            var result = new byte[signKeys.Length*signKeys[0].Length];
            for (int i = 0; i < signKeys.Length; i++)
            {
                BytesBuilder.CopyTo(signKeys[i], result, signKeys[0].Length*i);
            }

            return result;
        }

        public byte[][] getClosedKeys(byte[][] signKeys, byte[] parameter, byte[] keyNumber, int[] ValueTo, byte mul = 8)
        {
            if (ValueTo.Length < keys.Count)
                throw new ArgumentOutOfRangeException("ValueTo");

            var closedKeys = new byte[keys.Count][];

            int counter = keys.Count;
            for (int i = 0; i < keys.Count; i++)
            {
                var number = i;
                ThreadPool.QueueUserWorkItem
                (
                    delegate
                    {
                        try
                        {
                            closedKeys[number] = getClosedKey(signKeys[number], parameter, keyNumber, ValueTo[number], mul);
                        }
                        finally
                        {
                            lock (closedKeys)
                            {
                                counter--;
                                Monitor.Pulse(closedKeys);
                            }
                        }
                    }
                );
            }

            lock (closedKeys)
                while (counter > 0)
                    Monitor.Wait(closedKeys);

            return closedKeys;
        }

        public void Dispose()
        {
            keys.Dispose();
        }
    }*/
}

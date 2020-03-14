        public unsafe void CFB(byte[] key, byte[] oiv, byte[] compressedOpenText, bool encrypt)
        {
            var sha  = new SHA3(compressedOpenText.Length);

            var init = sha.getDuplex(key, false, -1, oiv == null);
            if (oiv != null)
            {
                init = sha.getDuplex(oiv, true);
            }

            var block = new byte[71];
            BytesBuilder.CopyTo(init, block, 0, 71, init.Length < 71 ? 0 : init.Length - 71);

            fixed (byte * o_ = compressedOpenText)
            {
                UInt64 * o  = (UInt64 *) o_;
                UInt16 * o2 = (UInt16 *) o_;

                for (int i = 0; i < compressedOpenText.Length; i += 72)
                {
                    var c = sha.getDuplex(block, true);
                    BytesBuilder.ToNull(block);
                    if (!encrypt)
                        BytesBuilder.CopyTo(compressedOpenText, block, 0, 71, i);

                    if (i+71 < compressedOpenText.Length)
                    fixed (byte * b_ = c)
                    {
                        UInt64 * b  = (UInt64 *) b_;
                        UInt16 * b2 = (UInt16 *) b_;

                        o[0] ^= b[0];
                        o[1] ^= b[1];
                        o[2] ^= b[2];
                        o[3] ^= b[3];

                        o[4] ^= b[4];
                        o[5] ^= b[5];
                        o[6] ^= b[6];
                        o[7] ^= b[7];

                        o2[32] ^= b2[32];
                        o2[33] ^= b2[33];
                        o2[34] ^= b2[34];

                        o_[70] ^= b_[70];
                    }
                    else
                    for (int j = 0; j + i < compressedOpenText.Length && j < 71; j++)
                    {
                        compressedOpenText[j] ^= c[j];
                    }

                    if (encrypt)
                        BytesBuilder.CopyTo(compressedOpenText, block, 0, 71, i);
                }
            }
        }

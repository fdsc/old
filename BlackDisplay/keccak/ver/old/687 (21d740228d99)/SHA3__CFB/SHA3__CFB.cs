public unsafe void CFB(byte[] key, byte[] oiv, byte[] compressedOpenText, bool encrypt)
{
    if (compressedOpenText.Length <= 0)
        return;

    var sha  = new SHA3(compressedOpenText.Length);

    var init = sha.getDuplex(key, false, -1, oiv == null);
    if (oiv != null)
    {
        init = sha.getDuplex(oiv, true);
    }

    var block = new byte[71];
    BytesBuilder.CopyTo(init, block, 0, 71);
    BytesBuilder.ToNull(init);

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

            if (i+70 < compressedOpenText.Length)
            fixed (byte * b_ = c)
            {
                UInt64 * b  = (UInt64 *) (b_ + i);
                UInt16 * b2 = (UInt16 *) (b_ + i);

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

                o_[70+i] ^= b_[70];

                b[0]   = 0;
                b[1]   = 0;
                b[2]   = 0;
                b[3]   = 0;
                b[4]   = 0;
                b[5]   = 0;
                b[6]   = 0;
                b[7]   = 0;
                b2[32] = 0;
                b2[33] = 0;
                b2[34] = 0;
                b_[70] = 0;
            }
            else
            {
                for (int j = 0; j + i < compressedOpenText.Length && j < 71; j++)
                {
                    compressedOpenText[j+i] ^= c[j];
                }
                BytesBuilder.ToNull(c);
            }

            if (encrypt)
                BytesBuilder.CopyTo(compressedOpenText, block, 0, 71, i);
        }
    }

    sha.Clear();
    BytesBuilder.ToNull(block);
}

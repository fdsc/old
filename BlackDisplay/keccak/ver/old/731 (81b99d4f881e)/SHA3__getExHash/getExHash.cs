
public static byte[] getExHash(int count, byte[] data, byte[] key)
{
var result = new byte[64 * count];

var dt = new byte[count][];
var sh = new SHA3[count];
for (int i = 0; i < count; i++)
{
    dt[i] = new byte[data.Length];
    byte val  = (byte) i;
    byte val2 = (byte) (   i + (i << 2) + (i << 4) + (i << 6)   );
    for (int j = 0; j < dt[i].Length; j++)
    {
        if ((j & 1) == 0)
        {
            val += 0x55;
            dt[i][j]= val;
        }
        else
        {
            val2 += 0x55;
            dt[i][j]= val2;
        }
    }

    sh[i] = new SHA3(data.Length);
    sh[i].getDuplex(key, false, -1, false);
    var s = dt[i];
    dt[i] = sh[i].getDuplexMod(data, s, true);
    BytesBuilder.ToNull(s);
}

for (int j = 0; j < count; j++)
{
    for (int i = 0; i < count; i++)
    {
        var s = dt[i];
        dt[i] = sh[i].getDuplexMod(s, dt[(i + 1) % count], true);
        BytesBuilder.ToNull(s);
    }
}

long index = 0;
for (int i = 0; i < count; i++)
{
    BytesBuilder.CopyTo(dt[i], result, index, 64, dt[i].LongLength - 64);
    BytesBuilder.ToNull(dt[i]);
}

return result;
}

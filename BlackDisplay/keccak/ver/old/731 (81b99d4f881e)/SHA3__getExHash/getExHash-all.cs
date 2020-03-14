/*
 * count - любой
 * e11  count < 2
 * e12  count = 2
 * e13  count = 3
 * e14  count = 4
 * e15  count > 4
 * e16  count > 255
 * 
 * data != null
 * key  != null
 * 
 * e21  data.length = 0
 * e22  data.length > 0
 * 
 * e31  key.length = 0
 * e32  key.length > 0
 * 
 * */

public static byte[] getExHash(int count, byte[] data, byte[] key)
{
-   // Нет проверки на count > 255
-   // Нет проверки на count < 2

    // Если data или key равны null, то при обращении к ним будет вызвано исключение (key пройдёт прямо из keccak padding)
    // Результат - набор хешей по 64-ре байта, всего count хешей
    var result = new byte[64 * count];

    // Объект dt готовится для дальнейшей инициализации по количеству запрошенных хешей
    // Объект sh также инициализируюется по количеству запрошенных хешей
    var dt = new byte[count][];     // (1)
    var sh = new SHA3[count];       // (2)

    for (int i = 0; i < count; i++) // (4)
    {
        // Объект dt[i] инициализируется длинной входного массива data
        // Объект dt является массивом массивов и инициализирован конструкцией (1)
        // dt[i] не выходит за пределы массива, т.к. по (4) i < count, а по (1) размер dt как раз является count
        dt[i] = new byte[data.Length]; // (3)

        // val и val2 инициализируются
        // val - никогда не повторяется для разных i
+       // !1 - val  должен быть использован
+       // !2 - val2 должен быть использован
        byte val  = (byte) i;
        byte val2 = (byte) (   i + (i << 2) + (i << 4) + (i << 6)   );

        // j пробегает всю длинну первоначального массива dt[i]
        // Массив dt[i] инициализирован выражением (3)
        for (int j = 0; j < dt[i].Length; j++)  // (5)
        {
            if ((j & 1) == 0)
            {
                val += 0x55;
                // !1+ val использован
                // индекс i лежит в пределах массива i (1)/(4) и недавно был инициализирован (3)
                // индекс j лежит в пределах массива, т.к. (5)
                dt[i][j]= val;
            }
            else
            {
                val2 += 0x55;
                // !2+ val2 использован
                // индекс i лежит в пределах массива i (1)/(4) и недавно был инициализирован (3)
                // индекс j лежит в пределах массива, т.к. (5)
                dt[i][j]= val2;
            }
        }

        // Инициализация sh[i]
        // i внутри массива, т.к. (2), (4)
        sh[i] = new SHA3(data.Length);
        // Инициализация объекта шифрования sh[i]
        // ключ использован, дуплекс ещё не инициализирован
        sh[i].getDuplex(key, false, -1, false);   // (6)
        // в пределах массива
        var s = dt[i];
        // дуплекс модифицирует значениями data с помощью s
        // d[i] очищается ниже
        dt[i] = sh[i].getDuplexMod(data, s, true);
        // s очищено
        BytesBuilder.ToNull(s);
    }

    for (int j = 0; j < count; j++)
    {
        for (int i = 0; i < count; i++)
        {
            var s = dt[i];
            // dt[i] очищается ниже
            // (i + 1) % count не совпадает с i для любых count > 1
            // sh[i] считается инициализированным, т.к. (6)
            dt[i] = sh[i].getDuplexMod(s, dt[(i + 1) % count], true);
            BytesBuilder.ToNull(s);
        }
    }

    long index = 0;
    for (int i = 0; i < count; i++)
    {
-        // Нет приращения index
        BytesBuilder.CopyTo(dt[i], result, index, 64, dt[i].LongLength - 64);
        BytesBuilder.ToNull(dt[i]);
    }

    return result;
}

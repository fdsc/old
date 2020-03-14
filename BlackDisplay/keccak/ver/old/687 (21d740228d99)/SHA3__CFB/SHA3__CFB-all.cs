﻿// Одну ошибку при анализе не нашёл: UInt64 * b  = (UInt64 *) (b_ + i) не нужно приплюсовывать i, это к o нужно плюсовать, но там как раз этого нет
/*
 * key
 * e11  недопустим null
 * e12  пустой массив
 * e13  массив с длинной больше нуля
 * 
 * oiv
 * e21  null
 * e22  пустой массив
 * e23  массив с длинной больше нуля
 * 
 * compressedOpenText
 * e31  недопустим null
 * e32  пустой массив
 * e33  массив с длиной больше нуля
 * e34  массив с длиной 1
 * e35  массив с длиной 71
 * e36  массив с длиной 72
 * e37  массив с длиной 73
 * 
 * encrypt
 * e41  encrypt==true
 * e42  encrypt==false
 * 
 * */

/*
 * key должен содержать ключ и не быть null, по использованию не уничтожаетя
 * compressedOpenText не null и должен содержать шифруемый текст
 * 
 * */
public unsafe void CFB(byte[] key, byte[] oiv, byte[] compressedOpenText, bool encrypt)
{
    // Если не требуется шифровать, операции не производятся
(2-1)
    if (compressedOpenText.Length <= 0)
        return;

    // инициализация объекта шифрования SHA3
    // параметр коструктора не имеет решающего значения
+   // !4 объект шифрования должен быть безопасно уничтожен функцией clear
    var sha  = new SHA3(compressedOpenText.Length);


    // инициализация ключём объекта шифрования
    // объект шифрования sha инициализируется значением ключа key
    // второй параметр false говорит, что объект sha пока не инициализирован
(1) // результат возвращается, только если oiv == null, то есть если не задан вектор инициализации
    // вектор инициализации может быть не задан, при этом тогда инициализирующий блок init действительно должен быть инициализирован
    // по схеме CFB init должен пойти в первый блок гаммы и затем более нигде не используется

+   //!1 ключ key более нигде не используется, т.к. вместо него используется состояние объекта шифрования sha. key не уничтожается
+   //!2 объект шифрования sha не должен потерять инициализацию
+   //!3 объект шифрования sha должен использовать своё состояние в при шифровании всегда
+   //!5 объект init должен быть безопасно уничтожен
    var init = sha.getDuplex(key, false, -1, oiv == null);

    // !1+: объект key более нигде не используется, копий объекта в других указателях или объектах нет
    if (oiv != null)
    {
        // !2+: объект используется с условием, что он уже инициализирован (второй параметр - isInitialized = true)
        // !5+: объект init перезаписывается, однако только в том случае, если oiv != null , а значит, если объект не создан см. (1)
        init = sha.getDuplex(oiv, true);
    }

    // создание блока для шифрования
    // !6 исключено
+   // !7 объект block имеет индексы на запись не более [0; +1; 70] (71 элемент)
    var block = new byte[71];

    // объект block инициализируется
    // при создании выше он уже инициализирован нулями
    // объект init не может быть менее 72-х байт, поэтому проверки на то, что он меньше не нужны
    // объект init инициализирован в (2), если oiv == null
    // объект init инициализирован в (3), если oiv != null
    // oiv == null или oiv != null равно true, следовательно объект init инициализирован всегда
    // объект init - инициализирован безусловно
    // объект block имеет ненулевую длинну, готов принять в себя информацию
    // длина объекта init больше длины block:
    //      1. весь block будет проинициализирован
    //      2. не будет нарушена память за block, т.к. функция CopyTo защищает от этого
    // !7+: выполнено, т.к. верен пункт 2
    // !6=
+   // !8 объект block должен быть использован
+   // !9 текущее значение объекта block должно быть безопасно уничтожено
    BytesBuilder.CopyTo(init, block, 0, 71);
    // !5+
    BytesBuilder.ToNull(init);

    // Переменная compressedOpenText имеет копию указателя
    fixed (byte * o_ = compressedOpenText)
    {
        UInt64 * o  = (UInt64 *) o_;
        UInt16 * o2 = (UInt16 *) o_;

        // создаётся переменная i
        // compressedOpenText инициализирован
        // compressedOpenText.Length >= 0
        // цикл сработает хотя бы один раз, если compressedOpenText.Length > 0, то есть точно сработает хотя бы один раз, так как (2-1)
(5)     // i в начале цикла всегда в границах диапазона индексов compressedOpenText
        for (int i = 0; i < compressedOpenText.Length; i += 72)
        {
            // !2+: объект используется с условием, что он уже инициализирован (второй параметр - isInitialized = true)
            // !2+: далее в коде объект не используется
            // !3+: объект используется только с функцией getDuplex с предварительной инициализацией, не теряет инициализацию
+           // !9с: объект c должен быть безопасно уничтожен
            // происходит шифрование с ключевым внутренним состоянием (второй параметр - true)
            // шифруется block
            // block содержит синхропосылку, если это первая итерация цикла
            // если это следующие итерации цикла то:
            // если !encrypt, то block содержит кусок шифротекста предудыщего блока
            // если encrypt, то block содержит кусок шифротекста предыдущего блока
            // алгоритм CFB в данной позиции верен
            // !8+
            var c = sha.getDuplex(block, true);
            // !9+
            BytesBuilder.ToNull(block);


            // По алгоритму CFB при шифровании блок шифротекста шифруется с ключём (в данном случае, с внутренним состоянием)
            // При расшифровании блок шифротекста также должен быть зашифрован с ключём
            // При шифровани шифротекст содержится в блоке compressedOpenText после прменения к нему операции xor
            // При расшифровании шифротекст содержится в блоке compressedOpenText перед расшифрованием соответствующего блока

            // !11: предыдущий блок шифротекста должен быть зашифрован и передан на вход операции xor
+           // !12: синхропосылка (вектор инициализации) должна быть зашифрована, это сделано в (2) или в (3)
            // Строки (4) копируют 71 байт из compressedOpenText в block
            // Учитывая, что encrypt - наличие зашифрования
            // !encrypt - условие расшифрования
            // Таким образом, операция осуществляется только при шифровании
            // В compressedOpenText при шифровании расположен блок шифротекста
            // В функции CopyTo выход за пределы диапазона индексов невозможен
            // block длиной 71 байт может принять блок длиной 71 байт
            // compressedOpenText не обязательно содержит 71 байт данных, тем более, начиная с индекса i
            // индекс i остался в пределах диапазона индексов compressedOpenText, как указано в (5), так как не изменялся ни i, ни длина compressedOpenText
+           // !14: в ходе цикла длина compressedOpenText не изменяется
            // таким образом, при отсутствии в compressedOpenText достаточной информации, block будет заполнен не полностью, однако хотя бы один байт будет записан из compressedOpenText
            // block ранее обнулён, поэтому на месте недостающих байт будут нули
+           // !15: учесть при зашифровании, что на месте недостающих байт шифротекста также должны быть нули
+           // !13: объект block должен быть безопасно уничтожен

            if (!encrypt)
                BytesBuilder.CopyTo(compressedOpenText, block, 0, 71, i);

            // !14+: чтение compressedOpenText не изменяет его длину, в том числе, чтение по указателю o и o_
            // Если после индекса i имеется блок данных не менее 71-ого байта в длинну
            // действительно, если i < compressedOpenText.Length, то имеется не менее 1 байта, включая i-ый
            // если i+1 - не менее 2-х байт
            // если i+70 - не менее 71-ого байта
            if (i+70 < compressedOpenText.Length)
            fixed (byte * b_ = c)
            {
                UInt64 * b  = (UInt64 *) (b_ + i);
                UInt16 * b2 = (UInt16 *) (b_ + i);

                // !7+
                o[0] ^= b[0];               // перезапись байтов 0-7
                o[1] ^= b[1];               // байтов 8-15
                o[2] ^= b[2];               // байтов 16-23
                o[3] ^= b[3];               // байтов 23-31

                o[4] ^= b[4];               // байтов 32-39
                o[5] ^= b[5];               // байтов 40-47
                o[6] ^= b[6];               // 48-55
                o[7] ^= b[7];               // 56-63

                o2[32] ^= b2[32];           // 64-65
                o2[33] ^= b2[33];           // 66-67
                o2[34] ^= b2[34];           // 68-69

                o_[70+i] ^= b_[70];         // 70

                // 9с+
                // !7+
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


                // итого наложено xor из b_, b2 и b на o_, o2 и o 70 байт

                // в b есть нужное количество байт, так как в нём 71-н байт, а индекс 70 есть как раз максимальный индекс этого массива
                // в o есть нужно количество байт, так как условие (7) выполняется
            }
            else
            {
                // !14+: чтение compressedOpenText не изменяет его длину, в том числе, чтение по указателю o и o_
                for (int j = 0; j + i < compressedOpenText.Length && j < 71; j++)
                {
                    // в объекте c 72 байта, j всегда менее 71, так как блок содержит лишь 71-н байт
                    // верный индекс j+i, он всегда менее длинны массива, а значит, не более максимального индекса массива
                    // цикл завершается, так как j растёт до 71-ого
                    // все байты массива compressedOpenText будут наложены
                    compressedOpenText[j+i] ^= c[j];
                }
                // 9с+
                BytesBuilder.ToNull(c);
            }

            // !13=: объект block в данном случае не уничтожается, т.к. условие создания !13 противоположно условию перезаписи
 !-         // !10: объект block должен быть безопасно уничтожен
            // !14+: чтение compressedOpenText не изменяет его длину, в том числе, чтение по указателю o и o_
            // После применения xor при зашифровании compressedOpenText в данной части содержит шифротекст, который записывается в block
            // !15+: на месте недостающих байт будут нули, так как выше block безусловно очищается
            if (encrypt)
                BytesBuilder.CopyTo(compressedOpenText, block, 0, 71, i);
        }
    }

    // !4+
    sha.Clear();
    // !9+, 13+, !10+
    BytesBuilder.ToNull(block);
}

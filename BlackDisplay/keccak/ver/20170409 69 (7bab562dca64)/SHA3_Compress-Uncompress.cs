
// Подготовить криптографическое состояние для объекта this
1: prepareGamma(keys[keys.Count - 1], oivectors[0]);
// Получить случайную последовательность из объекта this. Внутреннее состояние подготовлено на 1:
// Обнуление gm идёт на 3:
2: var gm = getGamma(71, true);
// Подготовить объект rg, инициализировав его gm. gm инициализированно на 2:
// Обнуление идёт на 4:
5: var rg = new SHA3Random(gm);
// Обнулить gm
3: BytesBuilder.ToNull(gm);
// Сгенерировать случайное число, характеризующее количество правых элементов для записи
// rg инициализирован на 5:
// Ошибка с обнулением LR и LR2 устранена
6: int LR = (int)((((ulong)rg.nextLong()) % 200560490130L) % 2221) + 97;
// GostRegime инициализирован вне блока
// rg инициализированно на 5:
// LR инициализированно на 6:
// bytes не применяется, если GostRegime < 34. Ещё как применяется. Исправлено
7: var bytes = GostRegime >= 34 ? rg.nextBytes(LR) : null;
// Инициализированно, но пока плохо. Для GostRegime < 34 должно быть корректно в виде нуля.
int LR2 = 0;
// Не инициализированно
byte[] bytes2 = null;
// GostRegime инициализирован вне блока
if (GostRegime >= 34)
{
    // rg инициализированно на 5:
    // Дополнительная инициализация состояния прошла выше на 6:
    LR2 = (int)((rg.nextLong() % 200560490130L) % 2221) + 97;
    // LR2 инициализирована строкой выше
    // rg инициализированна на 5:, 6: и строкой выше
8:    bytes2 = rg.nextBytes(LR2);
}
// rg далее не применяется и очищен
4: rg.Clear();
// GostRegime инициализирован вне блока
if (GostRegime >= 34)
{
    // compressedText инициализирован вне блока
    // bytes инициализирован на 7:
    // bytes должен быть расположен в самом начале compressedText, т.к. параметр index1 = 0
    if (!BytesBuilder.Compare(compressedText, bytes, bytes.Length))
    {
    // Если bytes не равно содержимому начала массива, то это ошибка
    // bytes инициализирован на 7:
        BytesBuilder.ToNull(bytes);
    // bytes2 инициализирован на 8:
    // Причём условие для его инициализации точно такое же, как и в этой ветке кода
        BytesBuilder.ToNull(bytes2);
    // Очищаем объект this
        Clear(true);
// Возврат
        return null;
    }
}

// Производим запись в служебный поток
// inStream инициализирован вне блока кода
// Для GostRegime >= 34 значение, с которого начинается чтение = LR (пропускаем bytes)
// Для режимов ниже 34 - значение 0 (с начала массива)
// Длина bytes действительно равна LR, см. 7: ( rg.nextBytes(LR) )
// Длина информации для записи равна compressedText.LongLength - LR - LR2 - 8
// Это действительно так, так как в compressedText записано:
// 1. bytes длиной LR
// 2. bytes2 длиной LR2. Если bytes2 не записан, то его длина LR2 = 0
// 3. 8 байтов информации о длине заархивированного текста
// 4. Собственно, открытый текст
// 5. Больше там ничего не записано
inStream.Write(compressedText, GostRegime >= 34 ? LR : 0, (int)(compressedText.LongLength - LR - LR2 - 8));

// GostRegime инициализирован вне блока
if (GostRegime >= 34)
{
    // compressedText инициализирован вне блока
    // bytes2 инициализирован обязательно, т.к. условие на инициализацию 8: точно такое же, как и на эту ветку кода
    // Сравниваем последние байты compressedText, они должны быть равны с bytes2, т.к. именно туда записываются bytes2
    // Третий параметр - длина сравнения. Она равна bytes2. Это верно, т.к. мы сравниваем именно полный участок bytes2
    // LR2 == bytes2.Length, т.к. инициализирован на 8: с тем же самым условием, что и эта ветка кода ( bytes2 = rg.nextBytes(LR2) )
    // Четвёртый параметр - индекс, с которого начинается сравнение.
    // Это должен быть индекс от конца массива, ровно на LR2 байтов меньше
    // Действительно, если LR2 == 1, то мы получим последний индекс массива, который и будет сравнен
    if (!BytesBuilder.Compare(compressedText, bytes2, bytes2.Length, (int)compressedText.LongLength - LR2))
    {
        // Если сравнение не удалось, очищаем bytes и bytes2. Они точно инициализированны на 7: и 8:
        BytesBuilder.ToNull(bytes);
        BytesBuilder.ToNull(bytes2);
        Clear(true);
        return null;
    }

// очищаем bytes и bytes2
    BytesBuilder.ToNull(bytes);
    BytesBuilder.ToNull(bytes2);
}


// uncLen не очищена. Ошибка исправлена.
ulong uncLen;
// Получаем в переменную uncLen, которая пока не инициализированна, значение из compressedText
// Это значение записано перед bytes2
// То есть на расстоянии LR2 и ещё 8-мь байтов, т.к. значение ULong занимает 8 байтов
BytesBuilder.BytesToULong(out uncLen, compressedText, compressedText.LongLength - 8 - LR2);

try
{
    // Приготавляем поток inStream к чтению
    inStream.Position = 0;
    // inStream, outStream - инициализированны вне блока кода
    // Читаем ровно uncLen плюс хеш размером 200 байтов
    // Прогресс не принимаем
    decoder.Code(inStream, outStream, inStream.Length, (long) uncLen + 200, null);
}
catch
{
    // Если исключение, просто очищаем потоки и возвращаем null
    Clear(true);
    ClearStreams(inStream, outStream);
    return null;
}
// Берём результат
OpenText = outStream.ToArray();














inStream .Write(openText, 0, openText.Length);

hash = getMACHashMod(openText, keys, oivectors, pc, hashCount - 8, GostRegime);
inStream.Write(hash, 0, hash.Length);

BytesBuilder.ToNull(hash);
lenOT = null;

// ниже - аналогично
prepareGamma(keys[keys.Count - 1], oivectors[0]);
var rg    = new SHA3Random(getGamma(71, true));
int LR    = (int) (  ( ((ulong) rg.nextLong()) % /*2 * 3 * 5 * 7 * 11 * 13 * 17 * 19 * 23 * 29 * 31*/ 200560490130L) % 2221  ) + 97;
var bytes = rg.nextBytes(LR);
int LR2   = 0;
byte[] bytes2 = null;
if (GostRegime >= 34)
{
    LR2   = (int) (  (rg.nextLong() % /*2 * 3 * 5 * 7 * 11 * 13 * 17 * 19 * 23 * 29 * 31*/ 200560490130L) % 2221  ) + 97;
    bytes2 = rg.nextBytes(LR2);
}

outStream.Write(bytes, 0, bytes.Length);
BytesBuilder.ToNull(bytes);

inStream.Position = 0;
encoder.Code(inStream, outStream, inStream.Length, -1L, null);

BytesBuilder.ULongToBytes((ulong) openText.LongLength, ref lenOT);
outStream.Write(lenOT, 0, lenOT.Length);

outStream.Write(bytes2, 0, bytes2.Length);
BytesBuilder.ToNull(bytes2);

compressedOpenText = outStream.ToArray();

BytesBuilder.ToNull(lenOT);
lenOT = null;
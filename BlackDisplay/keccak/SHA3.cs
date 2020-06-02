using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using SevenZip;
using System.IO;
using System.Threading;

namespace keccak
{

    // http://www.itmesh.ru/archives/2706
    // Переработано для увеличения производительности (Виноградов С.В., 2012 ноябрь)
    // http://www.di-mgt.com.au/crypto.html
    // тестирование D:\Arcs\yaDisk\Prg\ЯзыкПрограммирования\Keccack\ConsoleApplication1
    public class SHA3
    {
        //константы раундов, всего их 24
        //применяются на шаге ι
        /*
        private static readonly ulong[] RC ={0x0000000000000001,
        0x0000000000008082,
        0x800000000000808A,
        0x8000000080008000,
        0x000000000000808B,
        0x0000000080000001,

        0x8000000080008081,
        0x8000000000008009,
        0x000000000000008A,
        0x0000000000000088,


        0x0000000080008009,
        0x000000008000000A,
        0x000000008000808B,
        0x800000000000008B,
        0x8000000000008089,

        0x8000000000008003,
        0x8000000000008002,
        0x8000000000000080,
        0x000000000000800A,
        0x800000008000000A,


        0x8000000080008081,
        0x8000000000008080,
        0x0000000080000001,
        0x8000000080008008};

        // матрица смещений, применяется при каждом раунде на шаге θ
        // матрица r в алгоритме не используется - она вынесена в алгоритм явно
        private static readonly int[,] r = {{0,    36,     3,    41,    18}    ,
                          {1,    44,    10,    45,     2}    ,
                          {62,    6,    43,    15,    61}    ,
                          {28,   55,    25,    21,    56}    ,
                          {27,   20,    39,     8,    14}    };
         * */
        private const int w = 64, l = 6, n = 24;
        //в конструкторе устанавливаем параметры функции b=1600
        /// <summary>
        /// Создаёт класс (использовать один объект только в одном потоке одновременно), позволяющий рассчитывать хеши
        /// Можно использовать повторно
        /// </summary>
        /// <param name="maxCapacity">Инициализационное значение максимального размера хешируемого сообщения.
        /// Можно хешировать и более, но будет дополнительно выделятся память.</param>
        public SHA3(long maxCapacity)
        {
            /*w = 64; // b = 1600; b / 25;
            l = 6; //Convert.ToInt32(Math.Log(w, 2));
            n = 24; //12 + 2 * l;
            */

            zBytes = new byte [maxCapacity + 144 + 1];   // + максимальный размер блока (для 224-хбитной суммы) +1
            arrayM = new ulong[maxCapacity / 72 + 1, 25]; // минимальный размер блока (для 512-хбитной суммы)
        }

        public SHA3 Clone()
        {
            var result = new SHA3(zBytes.Length - 144 - 1);

            for (int i = 0; i < S.GetLength(0); i++)
                for (int j = 0; j < S.GetLength(1); j++)
                    result.S[i, j] = S[i, j];

            return result;
        }

        public void Clear(bool AllClear = true)
        {
            for (int i = 0; i < arrayM.GetLongLength(0); i++)
                for (int j = 0; j < arrayM.GetLongLength(1); j++)
                    arrayM[i, j] = 0x55AA55AA55AA55AAL;

            if (AllClear)
            {
                BytesBuilder.ToNull(zBytes);

                for (int i = 0; i < C.Length; i++)
                    C[i] = 0;

                for (int i = 0; i < B.GetLength(0); i++)
                    for (int j = 0; j < B.GetLength(1); j++)
                        B[i, j] = 0;

                for (int i = 0; i < S.GetLength(0); i++)
                    for (int j = 0; j < S.GetLength(1); j++)
                        S[i, j] = 0;

                this.d = 0;
            }
        }

        private unsafe void Keccackf(ulong * a, ulong * c, ulong * b)
        {
            roundB(a, c, b);
            //шаг ι
            *a ^= 0x0000000000000001;

            roundB(a, c, b); *a ^= 0x0000000000008082;
            roundB(a, c, b); *a ^= 0x800000000000808A;
            roundB(a, c, b); *a ^= 0x8000000080008000;
            roundB(a, c, b); *a ^= 0x000000000000808B;
            roundB(a, c, b); *a ^= 0x0000000080000001;
            roundB(a, c, b); *a ^= 0x8000000080008081;
            roundB(a, c, b); *a ^= 0x8000000000008009;
            roundB(a, c, b); *a ^= 0x000000000000008A;
            roundB(a, c, b); *a ^= 0x0000000000000088;

            roundB(a, c, b); *a ^= 0x0000000080008009;
            roundB(a, c, b); *a ^= 0x000000008000000A;
            roundB(a, c, b); *a ^= 0x000000008000808B;
            roundB(a, c, b); *a ^= 0x800000000000008B;
            roundB(a, c, b); *a ^= 0x8000000000008089;
            roundB(a, c, b); *a ^= 0x8000000000008003;
            roundB(a, c, b); *a ^= 0x8000000000008002;
            roundB(a, c, b); *a ^= 0x8000000000000080;
            roundB(a, c, b); *a ^= 0x000000000000800A;
            roundB(a, c, b); *a ^= 0x800000008000000A;

            roundB(a, c, b); *a ^= 0x8000000080008081;
            roundB(a, c, b); *a ^= 0x8000000000008080;
            roundB(a, c, b); *a ^= 0x0000000080000001;
            roundB(a, c, b); *a ^= 0x8000000080008008;
        }

        readonly ulong[] C  = new ulong[5];
        ulong[,] B = new ulong[5, 5];
        ulong d;
        private unsafe void roundB(ulong * a, ulong * c, ulong * b)
        {
            //шаг θ
            *(c + 0) = *(a +  0) ^ *(a +  1) ^ *(a +  2) ^ *(a +  3) ^ *(a +  4);
            *(c + 1) = *(a +  5) ^ *(a +  6) ^ *(a +  7) ^ *(a +  8) ^ *(a +  9);
            *(c + 2) = *(a + 10) ^ *(a + 11) ^ *(a + 12) ^ *(a + 13) ^ *(a + 14);
            *(c + 3) = *(a + 15) ^ *(a + 16) ^ *(a + 17) ^ *(a + 18) ^ *(a + 19);
            *(c + 4) = *(a + 20) ^ *(a + 21) ^ *(a + 22) ^ *(a + 23) ^ *(a + 24);

            d = *(c + 4) ^ ((*(c + 1) << 1) | (*(c + 1) >> 63));
            *(a +  0) ^= d; // D[0];
            *(a +  1) ^= d; // D[0];
            *(a +  2) ^= d; // D[0];
            *(a +  3) ^= d; // D[0];
            *(a +  4) ^= d; // D[0];

            d = *(c + 0) ^ ((*(c + 2) << 1) | (*(c + 2) >> 63));
            *(a +  5) ^= d; // D[1];
            *(a +  6) ^= d; // D[1];
            *(a +  7) ^= d; // D[1];
            *(a +  8) ^= d; // D[1];
            *(a +  9) ^= d; // D[1];

            d = *(c + 1) ^ ((*(c + 3) << 1) | (*(c + 3) >> 63));
            *(a + 10) ^= d; // D[2];
            *(a + 11) ^= d; // D[2];
            *(a + 12) ^= d; // D[2];
            *(a + 13) ^= d; // D[2];
            *(a + 14) ^= d; // D[2];

            d = *(c + 2) ^ ((*(c + 4) << 1) | (*(c + 4) >> 63));
            *(a + 15) ^= d; // D[3];
            *(a + 16) ^= d; // D[3];
            *(a + 17) ^= d; // D[3];
            *(a + 18) ^= d; // D[3];
            *(a + 19) ^= d; // D[3];

            d = *(c + 3) ^ ((*(c + 0) << 1) | (*(c + 0) >> 63));
            *(a + 20) ^= d; // D[4];
            *(a + 21) ^= d; // D[4];
            *(a + 22) ^= d; // D[4];
            *(a + 23) ^= d; // D[4];
            *(a + 24) ^= d; // D[4];
            

            //шаги ρ и π

            *(b +  0) =  *(a +  0);                             // rot(A[0, 0], r[0, 0]);
            *(b +  8) = (*(a +  1) << 36) | (*(a +  1) >> 28);  // rot(A[0, 1], r[0, 1]);
            *(b + 11) = (*(a +  2) <<  3) | (*(a +  2) >> 61);  // rot(A[0, 2], r[0, 2]);
            *(b + 19) = (*(a +  3) << 41) | (*(a +  3) >> 23);  // rot(A[0, 3], r[0, 3]);
            *(b + 22) = (*(a +  4) << 18) | (*(a +  4) >> 46);  // rot(A[0, 4], r[0, 4]);

            *(b +  2) = (*(a +  5) <<  1) | (*(a +  5) >> 63);  // rot(A[1, 0], r[1, 0]);
            *(b +  5) = (*(a +  6) << 44) | (*(a +  6) >> 20);  // rot(A[1, 1], r[1, 1]);
            *(b + 13) = (*(a +  7) << 10) | (*(a +  7) >> 54);  // rot(A[1, 2], r[1, 2]);
            *(b + 16) = (*(a +  8) << 45) | (*(a +  8) >> 19);  // rot(A[1, 3], r[1, 3]);
            *(b + 24) = (*(a +  9) <<  2) | (*(a +  9) >> 62);  // rot(A[1, 4], r[1, 4]);

            *(b +  4) = (*(a + 10) << 62) | (*(a + 10) >>  2);  // rot(A[2, 0], r[2, 0]);
            *(b +  7) = (*(a + 11) <<  6) | (*(a + 11) >> 58);  // rot(A[2, 1], r[2, 1]);
            *(b + 10) = (*(a + 12) << 43) | (*(a + 12) >> 21);  // rot(A[2, 2], r[2, 2]);
            *(b + 18) = (*(a + 13) << 15) | (*(a + 13) >> 49);  // rot(A[2, 3], r[2, 3]);
            *(b + 21) = (*(a + 14) << 61) | (*(a + 14) >>  3);  // rot(A[2, 4], r[2, 4]);

            *(b +  1) = (*(a + 15) << 28) | (*(a + 15) >> 36);  // rot(A[3, 0], r[3, 0]);
            *(b +  9) = (*(a + 16) << 55) | (*(a + 16) >>  9);  // rot(A[3, 1], r[3, 1]);
            *(b + 12) = (*(a + 17) << 25) | (*(a + 17) >> 39);  // rot(A[3, 2], r[3, 2]);
            *(b + 15) = (*(a + 18) << 21) | (*(a + 18) >> 43);  // rot(A[3, 3], r[3, 3]);
            *(b + 23) = (*(a + 19) << 56) | (*(a + 19) >>  8);  // rot(A[3, 4], r[3, 4]);

            *(b +  3) = (*(a + 20) << 27) | (*(a + 20) >> 37);  // rot(A[4, 0], r[4, 0]);
            *(b +  6) = (*(a + 21) << 20) | (*(a + 21) >> 44);  // rot(A[4, 1], r[4, 1]);
            *(b + 14) = (*(a + 22) << 39) | (*(a + 22) >> 25);  // rot(A[4, 2], r[4, 2]);
            *(b + 17) = (*(a + 23) <<  8) | (*(a + 23) >> 56);  // rot(A[4, 3], r[4, 3]);
            *(b + 20) = (*(a + 24) << 14) | (*(a + 24) >> 50);  // rot(A[4, 4], r[4, 4]);

            //шаг χ

            *(a +  0) = *(b +  0) ^ ((~*(b +  5)) & *(b + 10));
            *(a +  1) = *(b +  1) ^ ((~*(b +  6)) & *(b + 11));
            *(a +  2) = *(b +  2) ^ ((~*(b +  7)) & *(b + 12));
            *(a +  3) = *(b +  3) ^ ((~*(b +  8)) & *(b + 13));
            *(a +  4) = *(b +  4) ^ ((~*(b +  9)) & *(b + 14));

            *(a +  5) = *(b +  5) ^ ((~*(b + 10)) & *(b + 15));
            *(a +  6) = *(b +  6) ^ ((~*(b + 11)) & *(b + 16));
            *(a +  7) = *(b +  7) ^ ((~*(b + 12)) & *(b + 17));
            *(a +  8) = *(b +  8) ^ ((~*(b + 13)) & *(b + 18));
            *(a +  9) = *(b +  9) ^ ((~*(b + 14)) & *(b + 19));

            *(a + 10) = *(b + 10) ^ ((~*(b + 15)) & *(b + 20));
            *(a + 11) = *(b + 11) ^ ((~*(b + 16)) & *(b + 21));
            *(a + 12) = *(b + 12) ^ ((~*(b + 17)) & *(b + 22));
            *(a + 13) = *(b + 13) ^ ((~*(b + 18)) & *(b + 23));
            *(a + 14) = *(b + 14) ^ ((~*(b + 19)) & *(b + 24));

            *(a + 15) = *(b + 15) ^ ((~*(b + 20)) & *(b +  0));
            *(a + 16) = *(b + 16) ^ ((~*(b + 21)) & *(b +  1));
            *(a + 17) = *(b + 17) ^ ((~*(b + 22)) & *(b +  2));
            *(a + 18) = *(b + 18) ^ ((~*(b + 23)) & *(b +  3));
            *(a + 19) = *(b + 19) ^ ((~*(b + 24)) & *(b +  4));

            *(a + 20) = *(b + 20) ^ ((~*(b +  0)) & *(b +  5));
            *(a + 21) = *(b + 21) ^ ((~*(b +  1)) & *(b +  6));
            *(a + 22) = *(b + 22) ^ ((~*(b +  2)) & *(b +  7));
            *(a + 23) = *(b + 23) ^ ((~*(b +  3)) & *(b +  8));
            *(a + 24) = *(b + 24) ^ ((~*(b +  4)) & *(b +  9));

            //шаг ι - выполняется во внешнйе подпрограмме
        }

        //циклический сдвиг переменной x на n бит
        /*private ulong rot(ulong x, int n)
        {
            // n = n & 0x3F;    // здесь в алгоритме нет чисел n, превышающих w=64
            return ((x << n) | (x >> (w - n)));
        }*/

        byte[] zBytes; // = new byte[64000];
        ulong[,] arrayM; // = new ulong[1, 25];
        long arrayMSize;
        //функция дополняет 16-чную строку до размер r-байт и преобразует ее в матрицу 64-битных слов
        private unsafe ulong[,] padding(byte[] M, long len, int r, bool paddedInOtherArray = false, bool lastBlock = true)
        {
            int sr;
            int sb;
            long zLength;
            PaddedBytes(M, len, r, out sr, out sb, out zLength, ref zBytes, lastBlock);

            //получаем из скольких блоков длиной r-бит состоит сообщение
            arrayMSize  = zLength / sr;

            ulong[,] arrayMOrOther;
            if (paddedInOtherArray)
            {
                arrayMOrOther = new ulong[arrayMSize, 25];

                if (arrayMSize <= 0)
                    return arrayMOrOther;
            }
            else
            {
                if (arrayM.GetLength(0) < arrayMSize)
                {
                    Clear(false);
                    arrayM = new ulong[arrayMSize, 25];
                }

                if (arrayMSize <= 0)
                    return arrayM;

                arrayMOrOther = arrayM;
            }

            fixed (ulong * am = arrayMOrOther)
            {
                /*
                for (int x = 0, xi = 0; x < arrayMSize; x++, xi += 25)
                    for (int y = 0; y < 25; y++)
                        *(am + xi + y) = 0; //arrayM[x, y] = 0;
                */
                long j = 0, i = 0;

                var sb1 = sb - 1;
                long count = 0;
                while (true)
                {
                    BytesBuilder.BytesToULong(/*out arrayM[i, j]*/ out *(am + i + j), zBytes, count);
                    count += 8;

                    if (j >= sb1)
                    {
                        i += 25;
                        j = 0;
                        if (count >= zLength)
                        {
                            if (count == zLength)
                                break;
                            else
                                throw new InternalBufferOverflowException("SHA3.padding buffer overflow fatal error");
                        }
                    }
                    else
                        j++;

                }
            }

            return arrayMOrOther;
        }

        /// <summary>
        /// Дополняет сообщение
        /// </summary>
        /// <param name="M">Сообщение</param>
        /// <param name="r">Параметр r, для 512 бит - 576</param>
        /// <param name="sr">r >> 3</param>
        /// <param name="sb">sr >> 3</param>
        /// <param name="zLength">Длинна возвращённого массива. Должна быть равна sr</param>
        /// <param name="zBytes">Массив-результат</param>
        long last_zLength = 0;
        public void PaddedBytes(byte[] M, long len, int r, out int sr, out int sb, out long zLength, ref byte[] zBytes, bool lastBlock = true)
        {
            long l = len; //M.LongLength;
            long k = 0;

            sr = r >> 3;   // число байт
            sb = sr >> 3;  // число 8-мибайтовых слов

            if (!lastBlock)
                zLength = l;
            else
            {
                k = sr - l % sr;
                zLength = l + k;
            }

            if (zLength > zBytes.LongLength)
            {
                BytesBuilder.ToNull(zBytes);
                zBytes = new byte[zLength];
            }
            
            if (last_zLength > zLength)
                BytesBuilder.BytesToNull(zBytes, last_zLength, len);
            else
                BytesBuilder.BytesToNull(zBytes, zLength, len);
            last_zLength = zLength;

            BytesBuilder.CopyTo(M, zBytes, 0, len);

            if (lastBlock)
            {
                zBytes[l - 0]     ^= 0x01;
                zBytes[l + k - 1] ^= 0x80;
            }
        }

        /// <summary>
        /// Выдаёт 512-битный хэш от строки
        /// </summary>
        /// <param name="message">Строка для хэширования</param>
        /// <returns>64 байта хэша</returns>
        public byte[] getHash512(string message)
        {
            return getHash512(  Encoding.UTF8.GetBytes(message)  );
        }

        /// <summary>
        /// Выдаёт 384-битный хэш от строки
        /// </summary>
        /// <param name="message">Строка для хэширования</param>
        /// <returns>48 байта хэша</returns>
        public byte[] getHash384(string message)
        {
            return getHash384(  Encoding.UTF8.GetBytes(message)  );
        }

        /// <summary>
        /// Выдаёт 256-битный хэш от строки
        /// </summary>
        /// <param name="message">Строка для хэширования</param>
        /// <returns>32 байта хэша</returns>
        public byte[] getHash256(string message)
        {
            return getHash256(  Encoding.UTF8.GetBytes(message)  );
        }

        /// <summary>
        /// Выдаёт 224-битный хэш от строки
        /// </summary>
        /// <param name="message">Строка для хэширования</param>
        /// <returns>28 байта хэша</returns>
        public byte[] getHash224(string message)
        {
            return getHash224(  Encoding.UTF8.GetBytes(message)  );
        }

        public static readonly int[] rNumbers = {1152, 1088, 832, 576};

        // Преобразование hash в строку 16-битных значений BitConverter.ToString(sha3.getHash512(text[i])).Replace("-", "");
        public byte[] getHash512(byte[] message, long len = -1, bool isInitialized = false, bool lastBlock = true, byte[] result = null)
        {
            return getHash(message, len, 576, 64, false, isInitialized, lastBlock, result);
        }

        public byte[] getHash384(byte[] message, long len = -1, bool isInitialized = false, bool lastBlock = true, byte[] result = null)
        {
            return getHash(message, len, 832, 48, false, isInitialized, lastBlock, result);
        }

        public byte[] getHash256(byte[] message, long len = -1, bool isInitialized = false, bool lastBlock = true, byte[] result = null)
        {
            return getHash(message, len, 1088, 32, false, isInitialized, lastBlock, result);
        }

        public byte[] getHash224(byte[] message, long len = -1, bool isInitialized = false, bool lastBlock = true, byte[] result = null)
        {
            return getHash(message, len, 1152, 28, false, isInitialized, lastBlock, result);
        }

        ulong[,] S = new ulong[5, 5];
        protected unsafe byte[] getHash(byte[] message, long len, int r, int d, bool toClearHelperHash = false, bool isInitialized = false, bool lastBlock = true, byte[] resultA = null)
        {
            isGammaGenerator = false;
            if (len == -1)
                len = message.LongLength;

            if (!lastBlock && len % (r >> 3) != 0)
                throw new ArgumentOutOfRangeException("len and lastBlock", "error: !lastBlock && len % (r >> 3) != 0; (r>>3) = " + (r >> 3));


            BytesBuilder z = new BytesBuilder();
            fixed (ulong * s = S)
            {
                ulong * sb = s;
                //Забиваем начальное значение матрицы S=0
                if (!isInitialized)
                for (int i = 0; i < 25; i++, sb++)
                        *sb = 0;

                int sr = r >> 6;
                int rw = sr; //r / w;

                ulong * Pmii;
                ulong * si5;

                fixed (ulong * c = C, b = B)
                {
                    fixed (ulong * P = padding(message, len, r, false, lastBlock))
                    {
                        Pmii = P;

                        //Сообщение P представляет собой массив элементов Pi, 
                        //каждый из которых в свою очередь является массивом 64-битных элементов 
                        for (int Mi = 0; Mi < /*P.GetLength(0)*/ arrayMSize; Mi++, Pmii += 25)
                        {
                            si5  = s;
                            for (int i = 0; i < 5; i++, si5 += 5)
                            {
                                for (int j = 0, paddingCounter = i; paddingCounter < sr; j++, paddingCounter += 5)
                                {
                                    *(si5 + j) ^= *(Pmii + paddingCounter);     // S[i, j] = S[i, j] ^ Pi[j, i];
                                }
                            }

                            Keccackf(s, c, b);
                        }
                    }

                    //добавляем к возвращаемой строке значения, пока не достигнем нужной длины
                    do
                    {
                        for (int i = 0; i < 5; i++)
                            for (int j = 0; j < 5; j++)
                                if ((5 * i + j) < rw)
                                {
                                    z.addULong(S[j, i]);
                                }

                        if (z.Count < d)
                            Keccackf(s, c, b);
                        else break;
                    }
                    while (true);
                }
            }

            var result = z.getBytes(d, resultA);
            if (toClearHelperHash)
                z.clear();

            return result;
        }


        /// <summary>
        /// Берёт хэш hashCount раз с модификацией промежуточными хэшами. Хеш изменяет внутреннее состояние объекта
        /// </summary>
        /// <param name="message">Хэшируемое сообщение</param>
        /// <param name="hashCount">Количество взятий хэшей</param>
        /// <returns></returns>
        public byte[] getMultiHash(byte[] message, long hashCount = 16384)
        {
            if (hashCount < 3)
                hashCount = 3;

            byte[] preResult = new byte[hashCount * 71 + message.LongLength];
            byte[] curMsg    = null;
            BytesBuilder.CopyTo(message, preResult);

            curMsg = getHash512(message);

            long j = message.LongLength;
            for (long i = 0; i < hashCount; i++)
            {
                prepareGamma(message, curMsg);
                curMsg = getGamma(71, true);
                BytesBuilder.CopyTo(curMsg, preResult, j);

                j += curMsg.LongLength;
            }

            var result = getHash512(preResult);
            BytesBuilder.ToNull(preResult);
            BytesBuilder.ToNull(curMsg);

            return result;
        }

        /// <summary>
        /// Вычисляет множественный хэш с использованием потоков
        /// </summary>
        /// <param name="message">Сообщение, от которого берётся хэш</param>
        /// <param name="result">Результат (72 байта)</param>
        /// <param name="procCount">Если 0 - procCount = Environment.ProcessorCount, иначе - количество потоков, которые участвуют в хешировании</param>
        /// <param name="hashCount">Количество итераций хеширования; не менее 3-х</param>
        public static void getMultiHash20(byte[] message, out byte[] result, ref int procCount, int hashCount /* = 16384*/, SHA3 shaR = null, int resultLen = 72)
        {
            if (hashCount < 4)
                hashCount = 4;
            if ((hashCount & 3) > 0)
                hashCount += 4 - (hashCount & 3);

            if (procCount < 1)
                procCount = Environment.ProcessorCount;
            var PC = procCount;

            object sync = new object(), syncr = new object();
            byte[] hashes = new byte[71*procCount];

            int thc = hashCount; // / procCount + hashCount % procCount;

            var rmessage = new byte[message.LongLength];
            BytesBuilder.CopyTo(message, rmessage);
            //Array.Reverse(rmessage);
            MergePermutation.permutationMergeBytes(message, rmessage);


            int  threads = procCount;
            int  errors  = 0;
            int  len     = message.Length;
            for (int i = 0; i < procCount; i++)
            {
                int threadNumber = i;

                ThreadPool.QueueUserWorkItem
                (
                    delegate
                    {
                        Interlocked.Increment(ref errors);
                        try
                        {
                            int counter   = 0x5A5A5A5A;
                            byte[] rmessage_ = new byte[rmessage.Length];
                            BytesBuilder.CopyTo(rmessage, rmessage_);
                            long A    = 0;

                            var sha   = new SHA3(thc*71 + len + 72);
                            var shag  = new SHA3(len);
                            var sham  = new SHA3(thc*71 + len + 72);
                            byte[] firstKey = new byte[message.LongLength < 16 ? 16 : message.LongLength];
                            BytesBuilder.CopyTo(message, firstKey);
                            A = (threadNumber * 97) % (firstKey.LongLength - 8);
                            BytesBuilder.VariableULongToBytes((ulong) (threadNumber*thc*PC + threadNumber*7 + thc + PC), ref firstKey, A);

                            int CA = (int) ( (PC*threadNumber*thc*97) % (firstKey.LongLength - A - 3) );

                            if (CA >= 3)
                                Array.Reverse(firstKey, (int) (A+2),
                                                CA
                                              );

                            BytesBuilder.VariableULongToBytes((ulong) (threadNumber*thc*PC + threadNumber*11 + thc + PC), ref firstKey, threadNumber % (firstKey.LongLength - 8));
                            Array.Reverse(firstKey, 0, (firstKey.Length >> 1) + ((threadNumber % firstKey.Length) >> 1));

                            sha.getDuplex(message, false, -1, false);
                            byte[] key  = sha.getDuplex(firstKey, true);
                            byte[] fkey = new byte[key.LongLength];
                            BytesBuilder.CopyTo(key, fkey);
                            Array.Reverse(fkey);

                            BytesBuilder.ToNull(firstKey);

                            shag.prepareGamma(rmessage, key);
                            sham.prepareGamma(rmessage, fkey);

                            byte[] bb = null;
                            byte[] curkey = null;
                            for (int k = 0; k < thc; k++)
                            {
                                if ((k & 1) == 0)
                                {
                                    if (bb == null)
                                    {
                                        bb = sha.getDuplex(message, true);
                                    }
                                    else
                                    {
                                        curkey = sha.getDuplex(bb, true);
                                        //Array.Reverse(curkey);

                                        if (thc - k > 2)
                                        {
                                            MergePermutation.permutationMergeBytes(curkey, bb);

                                            var newbb = new byte[curkey.LongLength + bb.LongLength];
                                            if ((k & 2) == 0)
                                            {
                                                BytesBuilder.CopyTo(curkey, newbb, 0);
                                                BytesBuilder.CopyTo(bb,     newbb, curkey.Length);
                                            }
                                            else
                                            {
                                                BytesBuilder.CopyTo(curkey, newbb, bb.LongLength);
                                                BytesBuilder.CopyTo(bb,     newbb, 0);
                                            }

                                            BytesBuilder.ToNull(bb);
                                            BytesBuilder.ToNull(curkey);
                                            bb = newbb;
                                        }
                                    }

                                    if (thc - k > 2)
                                    {
                                        var g      = shag.getGamma(bb.LongLength);
                                        var newbbm = sham.getDuplexMod(bb, g, true);

                                        BytesBuilder.BytesToNull(bb);
                                        BytesBuilder.BytesToNull(g);
                                        bb = newbbm;
                                    }
                                }
                                else
                                {
                                    if ((k & 2) == 0)
                                    {
                                        counter += 0x05050505;
                                        xorBytesWithInitVector(rmessage_, counter);
                                        MergePermutation.permutationMergeBytes(fkey, rmessage_);
                                        sha.getDuplex(rmessage_, true, -1, false);
                                        sha.getDuplex(key,       true, -1, false);
                                    }
                                    else
                                    {
                                        BytesBuilder.VariableULongToBytes((ulong) (k*threadNumber + k + threadNumber*7), ref fkey, fkey.LongLength - 8);

                                        sha.getDuplex(fkey,     true, -1, false);
                                        sha.getDuplex(message,  true, -1, false);
                                    }
                                }
                            }

                            BytesBuilder.ToNull(key);

                            lock (syncr)
                            {
                                BytesBuilder.VariableULongToBytes((ulong) (thc + threadNumber), ref fkey, fkey.LongLength - 8);
                                var lastKey = BytesBuilder.CloneBytes(fkey, 0, 72);
                                curkey = sha.getDuplex(lastKey, true);
                                //curkey = sha.getDuplex(fkey, true);
                                BytesBuilder.CopyTo(curkey, hashes, 71*threadNumber, 71);
                            }

                            sha .Clear(true);
                            shag.Clear(true);
                            sham.Clear(true);

                            BytesBuilder.ToNull(fkey);
                            BytesBuilder.ToNull(curkey);
                            BytesBuilder.ToNull(rmessage_);
                            Interlocked.Decrement(ref errors);
                        }
                        finally
                        {
                            lock (sync)
                            {
                                threads--;
                                Monitor.Pulse(sync);
                            }
                        }
                    }
                );
            }

            lock (sync)
            {
                while (threads > 0)
                    Monitor.Wait(sync);
            }

            if (errors > 0)
            {
                BytesBuilder.ToNull(hashes);
                result = null;
                return;
            }

            if (shaR == null)
                shaR = new SHA3(message.Length);

            shaR.prepareGamma(message, hashes);
            result = shaR.getGamma(resultLen);

            BytesBuilder.ToNull(hashes);
            BytesBuilder.ToNull(rmessage);
            shaR.Clear(true);
        }

        /// <summary>
        /// Вычисляет множественный хэш с использованием потоков
        /// </summary>
        /// <param name="message">Сообщение, от которого берётся хэш</param>
        /// <param name="result">Результат (72 байта)</param>
        /// <param name="procCount">Если 0 - procCount = Environment.ProcessorCount, иначе - количество потоков, которые участвуют в хешировании</param>
        /// <param name="hashCount">Количество итераций хеширования; не менее 2-х, кратно 2-ум (если некратно, то будет округлено вверх)</param>
        public static void getMultiHash40(byte[] message, out byte[] result, ref int procCount, int hashCount = 12, SHA3 shaR = null, int resultLen = 72)
        {
            if (hashCount < 2)
                hashCount = 2;
            if ((hashCount & 1) > 0)
                hashCount += 2 - (hashCount & 1);

            if (procCount < 1)
                procCount = Environment.ProcessorCount;
            var PC = procCount;

            object sync = new object();
            var hashes  = new BytesBuilder(); //new byte[71*procCount];
            long[] hSeq = new long[PC];

            int thc = hashCount;
            if (shaR == null)
                shaR = new SHA3(message.Length);

            var bytes1 = Encoding.ASCII.GetBytes("7aXq@))RFbuGG@YJUZi+5)WQuIUDl)eh+R784YTT)+fK7Z!d)BjGQ$#*cbn$hkEioThKFR&3cbD+3Y5gepdWzP4ZPeZhLIIB6qh@2(3X+1QgNN+9ZtlKK%BKzN6ka3D9&oFb6XxfHPfA3PR");
            var bytes2 = Encoding.ASCII.GetBytes("axx82N$#PEO1w()g_+YO_IOpjNwdq2CWaaofqiq_sB_+)YK7lDeP9%n+ukyXEN6eOhU$ZL%q^!X0dQ!yehy#Tn#mCQuAZ(y(RA#SLoC9nG(0Wun3xFU0c5Nm1S(scnYWDbHKsW*HLWkCg6H");
            var bytes3 = Encoding.ASCII.GetBytes("H%eHsfY8BC(T3_13YTlj4n!fLpXIXK5KChm)J&2OX27(vvNPL_A7695dGoOph%jlU+DbbI0uVv_4p)JyjEA@+^O+p@WJElWDT1qH2A3+ulENbHsXEgvR&)Bt6+d^90U3RB@AF*pHV8!XUnL");
            shaR.getDuplex(bytes1, false, -1, false);
            var rmessage = shaR.getDuplex(message, true);

            int  threads = procCount;
            int  errors  = 0;
            for (int i = 0; i < procCount; i++)
            {
                int threadNumber = i;

                if ((i & 1) == 0)
                    shaR.getDuplex(bytes2, true, -1, false);
                else
                    shaR.getDuplex(bytes1, true, -1, false);

                var threadKey = shaR.getDuplex(message, true);

                ThreadPool.QueueUserWorkItem
                (
                    delegate
                    {
                        Interlocked.Increment(ref errors);
                        try
                        {
                            var rlen  = rmessage.Length;
                            var sha   = new SHA3(rlen << thc);
                            var shag  = new SHA3(rlen);
                            var sham  = new SHA3(rlen);

                            var key = sha.getDuplex(threadKey);

                            shag.getDuplex(key,      false, -1, false);
                            var a = shag.getDuplex(message, true);
                            sham.getDuplex(key,      false, -1, false);
                            sham.getDuplex(rmessage, true,  -1, false);

                            byte[] bb = sha.getDuplexMod(message, a, true), newbb = null;
                            byte[] cur = null;
                            for (int k = 0; k < thc; k++)
                            {
                                cur   = sha.getDuplex(bb, true);
                                newbb = new byte[bb.LongLength + cur.LongLength];

                                if ((k & 1) == 0)
                                {
                                    BytesBuilder.CopyTo(cur, newbb, 0);
                                    BytesBuilder.CopyTo(bb,  newbb, cur.Length);
                                }
                                else
                                {
                                    BytesBuilder.CopyTo(cur, newbb, bb.LongLength);
                                    BytesBuilder.CopyTo(bb,  newbb, 0);
                                }
                                BytesBuilder.ToNull(bb);
                                bb = newbb;

                                newbb = sham.getDuplex(threadKey, true);
                                sha.getDuplex(newbb, true, -1, false);
                                BytesBuilder.ToNull(newbb);
                                newbb = shag.getDuplexMod(threadKey, cur, true);
                                BytesBuilder.ToNull(cur);
                                BytesBuilder.ToNull(threadKey);
                                threadKey = newbb;
                            }

                            BytesBuilder.ToNull(key);
                            BytesBuilder.ToNull(cur);
                            sham.Clear(true);
                            shag.Clear(true);

                            cur = sha.getDuplex(threadKey, true);
                            BytesBuilder.ToNull(threadKey);
                            sha.Clear(true);
                            lock (hashes)
                            {
                                hashes.add(cur);
                                hSeq[threadNumber] = hashes.countOfBlocks - 1;
                            }

                            Interlocked.Decrement(ref errors);
                        }
                        finally
                        {
                            lock (sync)
                            {
                                threads--;
                                Monitor.Pulse(sync);
                            }
                        }
                    }
                );
            }

            lock (sync)
            {
                while (threads > 0)
                    Monitor.Wait(sync);
            }

            BytesBuilder.ToNull(rmessage);
            if (errors > 0)
            {
                hashes.clear();
                shaR.Clear(true);
                result = null;
                return;
            }

            var hs = new BytesBuilder();
            for (int i = 0; i < hashes.countOfBlocks; i++)
            {
                hs.add(hashes.getBlock((int) hSeq[i]));
            }

            var h = hs.getBytes();
            hs.clear();
            hashes.clear(); // hashes можно очищать только после использования hs
            shaR.getDuplex(bytes3, true, -1, false);
            var hd = shaR.getDuplex(h, true);
            BytesBuilder.ToNull(h);
            /*if (hd.LongLength > message.Length)
            {
                h = BytesBuilder.CloneBytes(hd, 0, message.LongLength);
                BytesBuilder.ToNull(hd);
                hd = h;
            }*/
            result = shaR.getDuplexMod(message, hd, true, -1, resultLen);

            BytesBuilder.ToNull(hd);
            shaR.Clear(true);
        }


        public static int getHashCountForMultiHash()
        {
            byte[] t = new byte[71];
            for (byte i = 0; i < t.Length; i++)
                t[i] = i;

            var t1 = DateTime.Now.Ticks;
            int c  = 10000;
            new SHA3(72).getMultiHash(t, c);
            var t2 = DateTime.Now.Ticks;

            double lt = (t2 - t1) / (100.0 * 10000.0);  // время в 0.1 секундах (100 милисекунд)
            c = (int) (c / lt); // вычисление размера так, чтобы хэш вычислялся приблизительно 0.1 секунды
            if (c < 4)
                c = 4;

            int k = 0;
            while (c > 0)
            {
                c >>= 1;
                k++;
            }
            k--;

            c = 1;
            for (; k > 0; k--)
                c <<= 1;

            if (c < 4)
                throw new Exception("fatal error in SHA3.getHashCountForMultiHash (c < 4, but c >= 4)");

            return c;
        }

        public static long getHashCountForMultiHash20(int count = 71, int procCount = 0, int targetTime = 300, long ltInit = 0, int algorithm = 0)
        {
            byte[] t = new byte[count];
            for (int i = 0; i < t.Length; i++)
                t[i] = (byte) i;

            int c  = 0;
            byte[] result;
            int pc  = procCount;
            //int pc1 = 1;

            int maxC = 1;
            if (targetTime < 40)
            {
                maxC = 40 / targetTime;
            }

            long t1, t2, lt = ltInit;
            var sha = new SHA3(count);
            if (algorithm == 0)
                getMultiHash20(t, out result, ref pc, c, sha);   // для того, чтобы инициализировать объект sha - инициализация более долгая, чем повторное исполнение
            else
                getMultiHash40(t, out result, ref pc, c, sha);
            /*do
            {*/
                do
                {
                    if (algorithm == 0)
                    {
                        if (lt < (targetTime >> 4))
                            c += 16;
                        else
                        if (lt < (targetTime >> 3))
                            c += 12;
                        else
                        if (lt < (targetTime >> 2))
                            c += 8;
                        else
                            c += 4;
                    }
                    else
                    {
                        var tt = targetTime;
                        while (lt < tt)
                        {
                            tt >>= 1;
                            c += 1;
                        }
                    }

                    t1 = DateTime.Now.Ticks;
                    for (int i = 0; i < maxC; i++)
                        if (algorithm == 0)
                            getMultiHash20(t, out result, ref pc, c, sha);
                        else
                            getMultiHash40(t, out result, ref pc, c, sha);

                    t2 = DateTime.Now.Ticks;
                    lt = (t2 - t1) / 10000 / maxC;
                }
                while (lt < targetTime); // миллисекунды * 10000
            /*
                t1 = DateTime.Now.Ticks;
                sha.getMultiHash20(t, out result, ref pc1, c);
                t2 = DateTime.Now.Ticks;
            }
            while (t2 - t1 < 150 * 10000);*/
            /*
            var countA = 5;
            t1 = DateTime.Now.Ticks;
            for (int i = 0; i < countA; i++)
            {
                sha.getMultiHash20(t, out result, out pc, c);
            }
            t2 = DateTime.Now.Ticks;

            double lt = (t2 - t1) / (100.0 * 10000.0);  // время в 0.1 секундах (100 милисекунд)
            lt /= countA;
            c = (int) (c / lt); // вычисление размера так, чтобы хэш вычислялся приблизительно 0.1 секунды
             */
            if (c < 4)
                c = 4;

            return c;
        }

        public static byte[] getExHash(byte count, byte[] data, byte[] key)
        {
            SHA3[] sh = null;
            return getExHash(count, data, key, ref sh);
        }

        public static byte[] getExHash(byte count, byte[] data, byte[] key, ref SHA3[] sh)
        {
            if (count < 2)
                throw new ArgumentOutOfRangeException("count");
            var result = new byte[64 * count];

            var dt = new byte[count][];
            if (sh == null)
                sh = new SHA3[count];
            else
            {
                var newSh = new SHA3[count];
                for (int i = 0; i < sh.Length; i++)
                    newSh[i] = sh[i];

                sh = newSh;
            }

            for (int i = 0; i < count; i++)
            {
                dt[i] = new byte[data.Length <= 64 ? 64 : data.Length];
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

                if (sh[i] == null)
                    sh[i] = new SHA3(data.Length);

                sh[i].getDuplex(key, false, -1, false);
                var s = dt[i];
                dt[i] = sh[i].getDuplexMod(data, s, true, -1, 64);
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
            for (int i = 0; i < count; i++, index += 64)
            {
                BytesBuilder.CopyTo(dt[i], result, index, 64, dt[i].LongLength - 64);
                BytesBuilder.ToNull(dt[i]);
            }

            return result;
        }


        private bool isGammaGenerator = false;

        /// <summary>
        /// Подготавливает алгоритм к генерации гаммы с ключём key и открытым (публичным) вектором инициализации.
        /// Может быть также использована для подписи сообщений. Key - секретный ключ, OpenInitVector - сообщение.
        /// getGamma - хэш сообщения с секретным ключём.
        /// </summary>
        /// <param name="key">Секретный ключ</param>
        /// <param name="OpenInitVector">Публичный вектор инициализации</param>
        public unsafe void prepareGamma(byte[] key, byte[] OpenInitVector, bool AcceptEmptyInitVector = false)
        {
            if (key.Length <= 0 || (!AcceptEmptyInitVector && OpenInitVector.Length <= 0))
                throw new ArgumentException("Gamma generator fatal error (parameters length must be greater zero)");

            int r = 576;
            fixed (ulong * s = S)
            {
                ulong * sb = s;
                //Забиваем начальное значение матрицы S=0
                for (int i = 0; i < 25; i++, sb++)
                        *sb = 0;

                int sr = r >> 6;
                int rw = sr; //r / w;

                ulong * Pmii;
                ulong * si5;

                int tempInt1, tempInt2;
                long length1;
                var tempBytes1 = new byte[0];
                PaddedBytes(key, key.LongLength, r, out tempInt1, out tempInt2, out length1, ref tempBytes1);

                if (tempBytes1.Length % (r >> 3) != 0 || length1 != tempBytes1.Length)
                    throw new Exception("Gamma generator fatal error (padding calculates incorrect)");

                var message = new byte[tempBytes1.Length + OpenInitVector.Length];
                BytesBuilder.CopyTo(tempBytes1,     message, 0);
                BytesBuilder.CopyTo(OpenInitVector, message, length1);

                BytesBuilder.ToNull(tempBytes1);

                fixed (ulong * c = C, b = B)
                {
                    fixed (ulong * P = padding(message, message.Length, r))
                    {
                        Pmii = P;

                        //Сообщение P представляет собой массив элементов Pi, 
                        //каждый из которых в свою очередь является массивом 64-битных элементов 
                        for (int Mi = 0; Mi < /*P.GetLength(0)*/ arrayMSize; Mi++, Pmii += 25)
                        {
                            si5  = s;
                            for (int i = 0; i < 5; i++, si5 += 5)
                            {
                                for (int j = 0, paddingCounter = i; paddingCounter < sr; j++, paddingCounter += 5)
                                {
                                    *(si5 + j) ^= *(Pmii + paddingCounter);     // S[i, j] = S[i, j] ^ Pi[j, i];
                                }
                            }

                            Keccackf(s, c, b);
                        }
                    }
                }

                BytesBuilder.ToNull(message);
            }

            isGammaGenerator = true;
        }

        /// <summary>
        /// Выдаёт отрезок гаммы шифра (sha-3 512 bit)
        /// Выдаст продолжение гаммы при повторном вызове, однако часть гаммы может быть упущена, если length не кратна (r >> 3) или length не кратна 8
        /// </summary>
        /// <param name="length">Длинна выдаваемой гаммы</param>
        /// <param name="isEnd">true - если продолжение гаммы не потребуется (быстрее отработает)</param>
        /// <param name="r">Параметр r шифрования (576 для 512-тибитной гаммы; 832 - 384-битная гамма; 1088 - 256-битная гамма; 1152 - 224-битная гамма)</param>
        /// <returns>Отрезок гаммы</returns>
        public unsafe byte[] getGamma(long length = 72, bool isEnd = false, int r = 576)
        {
            if (!isGammaGenerator)
                throw new Exception();

            BytesBuilder z = new BytesBuilder();
            fixed (ulong * s = S)
            {
                int rw = 576 / w;
                fixed (ulong * c = C, b = B)
                {
                    long start = 0;
                    //добавляем к возвращаемой строке значения, пока не достигнем нужной длины
                    do
                    {
                        start = z.Count;
                        for (int i = 0; i < 5; i++)
                            for (int j = 0; j < 5; j++)
                                if ((5 * i + j) < rw)           // 576 832 1088 1152
                                {
                                    z.addULong(S[j, i]);
                                }

                        if (!isEnd || z.Count < length)
                            Keccackf(s, c, b);
                    }
                    while (z.Count < length);
                }
            }

            if (isEnd)
            {
                isGammaGenerator = false;
            }

            var result = z.getBytes(length);
            z.clear();
            return result;
        }

        public bool useOldDuplex = false;
        /// <summary>
        /// К заданному сообщению выдаёт duplex (r после каждого применения keccak) для 512-тибитной версии keccak
        /// </summary>
        /// <param name="message">Сообщение</param>
        /// <param name="isInitialized">Если false, то начальное значение инициализируется нулём. Если дуплекс вызывается повторно как продолжение одного сообщения, то в следующих за первым сообщениях isInitialized = true</param>
        /// <param name="count72">Количество байтов, которые берёт duplex из каждого применения keccak, по-умолчанию все байты count72 = -1, либо count72 = 72</param>
        /// <returns>Возвращает массив, прочитанный из состояния функции keccak после каждого её применения</returns>
        public unsafe byte[] getDuplex(byte[] message, bool isInitialized = false, int count72 = -1, bool returnResult = true, long resultCount = -1)
        {
            BytesBuilder z      = new BytesBuilder();
            BytesBuilder result = new BytesBuilder();

            int r = 576;
            if (count72 == -1 || count72 > (r >> 3))
                count72 = r >> 3;

            fixed (ulong * s = S)
            {
                ulong * sb = s;
                //Забиваем начальное значение матрицы S=0
                if (!isInitialized)
                {
                    for (int i = 0; i < 25; i++, sb++)
                            *sb = 0;
                }

                int sr = r >> 6;
                int rw = sr; //r / w;

                ulong * Pmii;
                ulong * si5;

                fixed (ulong * c = C, b = B)
                {
                    fixed (ulong * P = padding(message, message.LongLength, r))
                    {
                        Pmii = P;

                        //Сообщение P представляет собой массив элементов Pi, 
                        //каждый из которых в свою очередь является массивом 64-битных элементов 
                        for (int Mi = 0; Mi < /*P.GetLength(0)*/ arrayMSize; Mi++, Pmii += 25)
                        {
                            si5  = s;
                            for (int i = 0; i < 5; i++, si5 += 5)
                            {
                                for (int j = 0, paddingCounter = i; paddingCounter < sr; j++, paddingCounter += 5)
                                {
                                    *(si5 + j) ^= *(Pmii + paddingCounter);     // S[i, j] = S[i, j] ^ Pi[j, i];
                                }
                            }

                            Keccackf(s, c, b);

                            if (returnResult)
                            {
                                for (int i = 0; i < 5; i++)
                                for (int j = 0; j < 5; j++)
                                    if ((5 * i + j) < rw)
                                    {
                                        if (useOldDuplex)
                                            z.addULong(s[5 * i + j]);
                                        else
                                            z.addULong(s[1 * i + 5*j]);
                                    }

                                result.add(z.getBytes(count72));
                                z.clear();
                            }
                        }
                    }
                }
            }

            isGammaGenerator = true;

            byte[] resultbytes;
            if (returnResult)
                resultbytes = result.getBytes(resultCount);
            else
                resultbytes = null;

            result.clear();
            return resultbytes;
        }


        /// <summary>
        /// Модифицирует message модифицирующим сообщением mod (необратимое преобразование)
        /// </summary>
        /// <param name="message">Модифицируемое сообщение</param>
        /// <param name="mod">Модифицирующее сообщение</param>
        /// <param name="isInitialized">Если false, то начальное значение инициализируется нулём. Если дуплекс вызывается повторно как продолжение одного сообщения, то в следующих за первым сообщениях isInitialized = true</param>
        /// <param name="count72">Количество байтов, которые берёт duplex из каждого применения keccak, по-умолчанию все байты count72 = -1, либо count72 = 72</param>
        /// <param name="minCount">Количество возвращённой информации; -1 - message.LongLength; 0 - какое есть</param>
        /// <returns></returns>
        public unsafe byte[] getDuplexMod(byte[] message, byte[] mod, bool isInitialized = false, int count72 = -1, long minCount = -1)
        {
            BytesBuilder z      = new BytesBuilder();
            BytesBuilder result = new BytesBuilder();

            int r = 576;
            if (count72 == -1 || count72 > (r >> 3))
                count72 = r >> 3;

            if (message.LongLength > mod.LongLength)
                throw new ArgumentOutOfRangeException("mod", "mod length must be > message length");

            fixed (ulong * s = S)
            {
                ulong * sb = s;
                //Забиваем начальное значение матрицы S=0
                if (!isInitialized)
                {
                    for (int i = 0; i < 25; i++, sb++)
                            *sb = 0;
                }

                int sr = r >> 6;
                int rw = sr; //r / w;

                ulong * Pmii, PMii2;
                ulong * si5;

                fixed (ulong * c = C, b = B)
                {
                    var p2 = padding(mod, mod.LongLength, r, true);
                    fixed (ulong * P  = padding(message, message.LongLength, r))
                    fixed (ulong * P2 = p2)
                    {
                        Pmii  = P;
                        PMii2 = P2;
                        //Сообщение P представляет собой массив элементов Pi, 
                        //каждый из которых в свою очередь является массивом 64-битных элементов 
                        for (int Mi = 0; Mi < /*P.GetLength(0)*/ arrayMSize; Mi++, Pmii += 25, PMii2 += 25)
                        {
                            si5  = s;
                            for (int i = 0; i < 5; i++, si5 += 5)
                            {
                                for (int j = 0, paddingCounter = i; paddingCounter < sr; j++, paddingCounter += 5)
                                {
                                    *(si5 + j) ^= *(PMii2 + paddingCounter);     // S[i, j] = S[i, j] ^ Pi[j, i];
                                }
                            }
                            Keccackf(s, c, b);

                            si5  = s;
                            for (int i = 0; i < 5; i++, si5 += 5)
                            {
                                for (int j = 0, paddingCounter = i; paddingCounter < sr; j++, paddingCounter += 5)
                                {
                                    *(si5 + j) ^= *(Pmii + paddingCounter);     // S[i, j] = S[i, j] ^ Pi[j, i];
                                }
                            }
                            Keccackf(s, c, b);


                            for (int i = 0; i < 5; i++)
                            for (int j = 0; j < 5; j++)
                                if ((5 * i + j) < rw)
                                {
                                    z.addULong(s[i + j * 5]);
                                }

                            result.add(z.getBytes(count72));
                            z.clear();
                        }
                    }

                    for (int i = 0; i < p2.GetLongLength(0); i++)
                        for (int j = 0; j < p2.GetLongLength(1); j++)
                            p2[i, j] = 0;
                }
            }

            if (minCount <= 0)
            {
                if (minCount == -1)
                    minCount = message.LongLength;
                else
                    minCount = -1;
            }
            else
            if (minCount < message.LongLength)
                minCount = message.LongLength;

            var resultArray = result.getBytes(minCount);
            result.clear();

            if (resultArray.LongLength < minCount)
                throw new ArgumentOutOfRangeException("minCount");

            return resultArray;
        }




        /// <summary>
        /// Генерирует ключ по случайному входу и текущему времени. Устаревшая
        /// </summary>
        /// <param name="input">Случайный вход с высокой степенью закономерности (например, ввод с клавиатуры или мыши)</param>
        /// <param name="count72">Количество байтов, которые будут взяты в качестве случайных из каждых 72-х байтов состояния</param>
        /// <returns>Случайная последовательность, зависящая от времени применения функции</returns>
        public static byte[] generateRandomPwd(byte[] input, int count72)
        {
            var keccak = new keccak.SHA3(64 * 1024);

            // инициализация начального состояния, сам duplex при этом пропадает
            keccak.getDuplex(Encoding.Default.GetBytes(Environment.WorkingSet.ToString() + Environment.UserName + System.Threading.Thread.CurrentThread.ManagedThreadId));
            var duplex1 = keccak.getDuplex(input,  true);
            var duplex2 = keccak.getDuplex(duplex1, true);
            BytesBuilder.ToNull(duplex1);

            // нам нужно получить 1 байт из 72 байт
            duplex1 = keccak.getDuplex(duplex2, true, count72);
            BytesBuilder.ToNull(duplex2);


            keccak.prepareGamma(Encoding.Default.GetBytes(Environment.ProcessorCount.ToString() + "|" + Environment.TickCount.ToString()), Encoding.Default.GetBytes(DateTime.Now.ToString() + Environment.MachineName));
            var result = keccak.getGamma(duplex1.Length, true);

            xorBytesWithGamma(duplex1, result);

            BytesBuilder.ToNull(result);
            keccak.Clear(true);
            return duplex1;
        }

        /// <summary>
        /// Генерирует ключ по случайному входу и текущему времени
        /// </summary>
        /// <param name="input">Случайный вход с высокой степенью закономерности (например, ввод с клавиатуры или мыши)</param>
        /// <param name="count72">Количество байтов, которые будут взяты в качестве случайных из каждых 72-х байтов состояния</param>
        /// <returns>Случайная последовательность, зависящая от времени применения функции</returns>
        public unsafe static byte[] generateRandomPwdByDerivatoKey(byte[] input, int pwdLen, bool service = false, int regime = 0)
        {
            var keccak = new keccak.SHA3(input.Length > 64*1024 ? input.Length : 64*1024);

            var oivC = service ? Encoding.Default.GetBytes(Environment.WorkingSet.ToString() + Environment.UserName + System.Threading.Thread.CurrentThread.ManagedThreadId + DateTime.Now.Ticks)
                               : keccak.CreateInitVector(0, pwdLen >> 1, regime);

            if (regime < 40)
                MergePermutation.permutationMergeBytes(input, oivC);
            else
            {
                var tmp = keccak.getDuplex(oivC);
                BytesBuilder.ToNull(oivC);
                oivC = tmp;
            }

            var OIv = keccak.getDuplex(oivC, regime >= 40);
            BytesBuilder.ToNull(oivC);

            if (regime >= 40)
            {
                var bt = Convert.FromBase64String("GL4C/c+hpZrVQcIg771ujdjHN/2Jqoj+UpS6cZ5fbG7zU1qwXMMg5P3YgnLY9byB1laMMu557wK1J7EewmkKJ0wPjA8u3uVO7oOhmtVSc6rnGMCaTrRHtLVLv9a9dBFnNxGjxbCLOcaGkZZYeBT1KUraXh/5reXVQmedmkijKvEwJ4d3CPSMUoPGca+lv7I=");
                keccak.getDuplex(bt, true);
            }

            var duplex1 = keccak.getDuplex(input, regime >= 40);
            var duplex2 = keccak.getDuplex(duplex1, true);
            if (regime < 40)
            MergePermutation.permutationMergeBytes(duplex1, duplex2);
            BytesBuilder.ToNull(duplex1);

            int pc = 0;
            duplex1 = keccak.getDerivatoKey(duplex2, OIv, regime >= 40 ? 8 : 1024, ref pc, pwdLen, regime >= 40 ? regime / 10 : 1);
            BytesBuilder.ToNull(duplex2);

            if (service)
                keccak.prepareGamma(Encoding.Default.GetBytes(Environment.ProcessorCount.ToString() + "|" + Environment.TickCount.ToString()), Encoding.Default.GetBytes(DateTime.Now.ToString() + Environment.MachineName));
            else
                keccak.prepareGamma(keccak.CreateInitVector(0, pwdLen >> 1, regime), keccak.CreateInitVector(0, pwdLen >> 1, regime));

            var result = keccak.getGamma(duplex1.Length, true);

            xorBytesWithGamma(duplex1, result);

            BytesBuilder.ToNull(result);
            BytesBuilder.ToNull(OIv);
            keccak.Clear(true);
            return duplex1;
        }

        /// <summary>
        /// Генерирует пароль с приблизительно равновероятными символами из allowedChars по заданой случайной последовательности (используйте generateRandomPwd для получения последовательности)
        /// </summary>
        /// <param name="duplex">Случайная последовательность</param>
        /// <param name="allowedChars">Допустимые символы в пароле</param>
        /// <param name="count">Количество символов в пароле (count = -1 для duplex.Length)</param>
        /// <returns></returns>
        public static string generatePwd(byte[] duplex, string allowedChars, int count = -1)
        {
            if (count <= 0 || count > duplex.Length)
                count = duplex.Length;

            char[] result = new char[count];

            for (int i = 0; i < count; i++)
            {
                int i0 = i;
                int i1 = (i + 1) % duplex.Length;
                int i2 = (i + 2) % duplex.Length;
                int i3 = (i + 3) % duplex.Length;
                int i4 = (i + 4) % duplex.Length;

                result[i] = 
                        allowedChars[(duplex[i0] + duplex[i1] * 3 + duplex[i2] * 5 + duplex[i3] * 7 + duplex[i4] * 11) % (allowedChars.Length)];
            }

            var str = new string(result);
            for (int i = 0; i < result.LongLength; i++)
                result[i] = '\x0';

            return str;
        }

        public struct PwdCheckResult
        {
            public double pwdStrength;
            public double maxPwdStrength;
            public double minPwdStrength;
            public bool   isLowerLatin;
            public bool   isUpperLatin;
            public bool   isDigit;
            public bool   isOther;
            public bool   nonLULDO;
            public int    nonFirstCount;
            public int    nonFirstCountCharsCount;
            public bool   isVeryLess;
            public int    doublesCount;
            public int    MultCount;

            public double maxPwdStrengthInTimes;
            public double minPwdStrengthInTimes;
            public double maxPwdStrengthInLogDateK;
            public double maxPwdStrengthInLogDate;
            public double minPwdStrengthInLogDate;

            public double minAbsoluteCount;
            public double maxAbsoluteCount;
            public double AbsoluteYearCount;
            public double minAbsoluteYearCount;
        }

        static readonly string[] unsecureCharSecuence = {"01234567890123",
                                         "qwertyuiopasdfghjklzxcvbnmqwer", 
                                         "qazwsxedcrfvtgbyhnujmik,ol.qaz",
                                         "qwertyuiop[]asdfghjkl;'zxcvbnm,./qwer",
                                         "zaqxswcdevfrbgtnhymju,ki.lo/;p'[]zaq",
                                         "password",
                                         "gfhjkm" /*"пароль"*/,
                                         "!@#$%^&*()_+|!@#",
                                         "~!@#$%^&*()_+|~!@#",
                                         "```111222333444555666777888999000---===\\\\\\~~~!!!@@@###$$$%%%^^^&&&***((()))___+++|||qqqwwweeerrrtttyyyuuuiiioooppp[[[]]]{{{}}}aaasssdddfffggghhhjjjkkklll;;;'''zzzxxxcccvvvbbbnnnmmm,,,...///:::\"\"\"<<<>>>???",
                                         "abcdefghijklmnopqrstuvwxyz"};

        /// <summary>
        /// Проверяет пароль pwd на стойкость (без учёта словарного перебора)
        /// </summary>
        /// <param name="pwd"></param>
        /// <param name="checkResult"></param>
        public static void checkPwd(string pwd, out PwdCheckResult checkResult)
        {
            checkResult = new PwdCheckResult();

            int characterMightiness = 0;
            int minCharacterMightiness = 0;
            var upperLatinRegex = new Regex("[A-Z]");
            var lowerLatinRegex = new Regex("[a-z]");
            var      digitRegex = new Regex("[0-9]");
            var LULatDigitRegex = new Regex("[ !\"#$%&'()*+,-./\\\\:;<=>?@\\[\\]^_`~|]");
            var nonLULDO        = new Regex("[^ !-z]");

            if (upperLatinRegex.IsMatch(pwd))
            {
                characterMightiness += 26;
                checkResult.isUpperLatin = true;
            }
            else
                checkResult.isUpperLatin = false;

            if (lowerLatinRegex.IsMatch(pwd))
            {
                characterMightiness += 26;
                checkResult.isLowerLatin = true;
            }
            else
                checkResult.isLowerLatin = false;

            if (digitRegex.IsMatch(pwd))
            {
                characterMightiness += 10;
                checkResult.isDigit = true;
            }
            else
                checkResult.isDigit = false;

            if (LULatDigitRegex.IsMatch(pwd))
            {
                characterMightiness += 10;
                checkResult.isOther = true;
            }
            else
                checkResult.isOther = false;

            if (nonLULDO.IsMatch(pwd))
            {
                characterMightiness += 40;
                checkResult.nonLULDO = true;
            }
            else
                checkResult.nonLULDO = false;

            checkResult.doublesCount = 0;
            checkResult.MultCount    = 0;


            SortedList<char, int> characters         = new SortedList<char, int>();
            SortedList<char, int> nonFirstcharacters = new SortedList<char, int>();
            char last1 = '\x0', last2 = '\x0';
            foreach (char a in pwd)
            {
                if (!characters.ContainsKey(a))
                    characters.Add(a, 0);
                else
                    if (!nonFirstcharacters.ContainsKey(a))
                        nonFirstcharacters.Add(a, 0);

                if (!checkForUnsecureCharSequence(ref checkResult.MultCount, a, last1, last2))
                if (last1 == a)
                {
                    if (last2 != a)
                        checkResult.doublesCount++;
                    else
                        checkResult.MultCount++;
                }


                last2 = last1;
                last1 = a;
            }
            minCharacterMightiness    = characters.Count - checkResult.MultCount;
            checkResult.nonFirstCount = pwd.Length - characters.Count;
            characters = null;

            if (minCharacterMightiness < 1)
                minCharacterMightiness = 1;

            checkResult.nonFirstCountCharsCount = nonFirstcharacters.Count;
            nonFirstcharacters = null;

            double correct2012     = Math.Pow(2, (DateTime.Now - new DateTime(2012, 1, 1)).TotalDays / 365.0 / 2.0);
            double correct2012A    = Math.Pow(2, (DateTime.Now - new DateTime(2012, 1, 1)).TotalDays / 365.0 / 3.0);
            //double correct2012A100 = Math.Pow(2, (DateTime.Now - new DateTime(2012, 1, 1) + new TimeSpan(365*100, 0, 0, 0)).TotalDays / 365.0 / 2.0);
            double maxBaseCount     = 1000D * 1000D * 1000D * 1000D /*(GPU-модуль в секунду)*/ * 3600D * 24D * 365D;
            double maxAbsoluteCount = maxBaseCount * 100000D /* 100 000 модулей */ * correct2012;
            double minAbsoluteCount = 180D * 1000D * 1000D * 1000D /*(малый GPU-модуль в секунду)*/ * 3600D * 24D * 90D /* 90-сто дней */ * correct2012A;
            checkResult.minAbsoluteYearCount = 1000D * 1000D /* только миллион переборов в секунду, это больше, чем sha-2 на GPU */ * 3600D * 24D * 365; // без коррекции

            checkResult.maxAbsoluteCount = maxAbsoluteCount;
            checkResult.minAbsoluteCount = minAbsoluteCount;
            checkResult.AbsoluteYearCount = maxAbsoluteCount;   // годовая мощность для 2012 года

            int nonFirstExpCount = checkResult.nonFirstCount - checkResult.doublesCount - checkResult.MultCount + 1;
            if (nonFirstExpCount < 0)
                nonFirstExpCount = 0;
            int nonFirstBaseCount = checkResult.nonFirstCountCharsCount + 1;

            // /2.0 - чтобы учесть вероятность 50% подбора пароля; ниже также деление на 100 000 в одном из расчётов
            checkResult.maxPwdStrengthInTimes = Math.Pow(characterMightiness,    minCharacterMightiness)*Math.Pow(nonFirstBaseCount, nonFirstExpCount) / 2.0;
            checkResult.minPwdStrengthInTimes = Math.Pow(minCharacterMightiness, pwd.Length) / 2.0;

            var min = Math.Log(minAbsoluteCount, maxAbsoluteCount);
            checkResult.pwdStrength    = Math.Min(   Math.Max(Math.Log(checkResult.maxPwdStrengthInTimes, maxAbsoluteCount), min) - min, 1.0   );
            checkResult.maxPwdStrength = Math.Log(checkResult.maxPwdStrengthInTimes, minAbsoluteCount);
            checkResult.minPwdStrength = Math.Log(checkResult.minPwdStrengthInTimes, minAbsoluteCount);


            // Каждый день идёт перебор Math.Pow(2, Years/1.5)*maxLogDateCount*24*3600 паролей.
            // По закону Мура удвоение идёт каждый два года (берём каждый полтора года, т.к. Дэвид Хаус из Intel предложил именно такой прогноз)
            // Соответственно, интеграл от 0 до T Math.Pow(2, Years/1.5)*maxLogDateCount*24*3600*365 по Years даёт количество даёт общее количество перебранных вариантов
            // C = maxLogDateCount*24*3600*365
            // Интеграл 2^(t/1.5)*C*dt = 2.1640425613334451110398870215028*C*exp(0.46209812037329687294482141430545*x) даёт общее количество перебранных вариантов
            // Следовательно, 2.1640425613334451110398870215028*exp(0.46209812037329687294482141430545*x)*C
            // 2.1640425613334451110398870215028*exp(0.46209812037329687294482141430545*x)*C == A, где A - количество варинатов к перебору
            // 2.1640425613334451110398870215028*log((0.46209812037329687294482141430546*A)/C) - количество лет к перебору
            /*
             *  Более правильно, однако не удаётся найти аналитического решения 
                Интеграл (C+2^((t-1)/1.5)*C)*dt = 2.1640425613334451110398870215028*C*exp(0.46209812037329687294482141430545*x) даёт общее количество перебранных вариантов
                C*(x + 1.3632613879462123093183482150833*2.0^(0.66666666666666666666666666666667*x))
                C*(x + 1.3632613879462123093183482150833*2.0^(0.66666666666666666666666666666667*x)) == A, где A - количество варинатов к перебору
             * */
            var maxLogDateCount = 8D /* процессорных ядер с помощью GPU (считаем, что GPU количество ядер удваивает) */ * 100D * 1000D * 1000D /* 100 млн. переборов*/ * 50D * 1000D * 1000D /* компьютеров, возможных в бот-сети */;
            var C = maxLogDateCount * 3600 * 24 * 365 * Math.Pow(2, (DateTime.Now - new DateTime(2012, 1, 1)).TotalDays / 365.0 / 1.5);
            // maxPwdStrengthInTimes/K - делим на 100000, чтобы учесть случайность при переборе паролей
            long K = 100L * 1000L /* вероятность наступления недопустимого события 1/100 000 */ * 365L * 80L; // учитывая количество ключей, равное количеству дней в жизни
            checkResult.maxPwdStrengthInLogDateK =  2.1640425613334451110398870215028*Math.Log((0.46209812037329687294482141430546*checkResult.maxPwdStrengthInTimes)/C/K);
            if (checkResult.maxPwdStrengthInLogDateK < 2)
            {
                checkResult.maxPwdStrengthInLogDateK = checkResult.maxPwdStrengthInTimes/C/K;
                if (checkResult.maxPwdStrengthInLogDateK > 2)
                    checkResult.maxPwdStrengthInLogDateK = 2;
            }

            checkResult.maxPwdStrengthInLogDate =  2.1640425613334451110398870215028*Math.Log((0.46209812037329687294482141430546*checkResult.maxPwdStrengthInTimes)/C/100000);
            if (checkResult.maxPwdStrengthInLogDate < 2)
            {
                checkResult.maxPwdStrengthInLogDate = checkResult.maxPwdStrengthInTimes/C/100000;
                if (checkResult.maxPwdStrengthInLogDate > 2)
                    checkResult.maxPwdStrengthInLogDate = 2;
            }

            C = 100D*3600D*24D*365D;
            checkResult.minPwdStrengthInLogDate =  2.1640425613334451110398870215028*Math.Log((0.46209812037329687294482141430546*checkResult.maxPwdStrengthInTimes)/C/K);
            if (checkResult.minPwdStrengthInLogDate < 2)
            {
                checkResult.minPwdStrengthInLogDate = checkResult.maxPwdStrengthInTimes/C/K;
                if (checkResult.minPwdStrengthInLogDate > 2)
                    checkResult.minPwdStrengthInLogDate = 2;
            }

            checkResult.isVeryLess     = checkResult.maxPwdStrength < 1.0;
        }

        public static bool checkForUnsecureCharSequence(ref int p, char a, char last1, char last2)
        {
            string af = (a.ToString() + last1.ToString() + last2.ToString()).ToLowerInvariant(),
                   ar = (last2.ToString() + last1.ToString() + a.ToString()).ToLowerInvariant();

            for (int i = 0; i < unsecureCharSecuence.Length; i++)
            {
                if (unsecureCharSecuence[i].Contains(af) || unsecureCharSecuence[i].Contains(ar))
                {
                    p++;
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Осуществляет операцию xor с сообщением и вектором модификации любой длинны. Если сообщение меньше вектора модификации, вектор будет применён повторно
        /// </summary>
        /// <param name="message">Сообщение и результат</param>
        /// <param name="Modificator">Вектор модификации</param>
        /// <param name="start">Начальный элемент вектора модификации (после достижения конца вектора вектор будет использован сначала)</param>
        public static unsafe void xorBytesWithInitVector(byte[] message, byte[] Modificator, byte start = 0)
        {
            long meLen = message    .LongLength;
            long moLen = Modificator.LongLength;
            long maLen = meLen > moLen ? meLen : moLen;

            fixed (byte * me = message, mod = Modificator)
            {
                for (long i = 0; i < maLen; i++)
                    *(me + i % meLen) ^= *(mod + (i + start) % moLen);
            }
        }

        /// <summary>
        /// Осуществляет xor сообщения с байтом модификации
        /// </summary>
        /// <param name="message">Сообщение и результат</param>
        /// <param name="Modificator">Байт модификации</param>
        public static unsafe void xorBytesWithInitVector(byte[] message, byte Modificator)
        {
            long meLen = message.LongLength;

            fixed (byte * me = message)
            {
                byte * m = me;
                for (long i = 0; i < meLen; i++, m++)
                    *m ^= Modificator;
            }
        }

        public static unsafe void xorBytesWithInitVector(byte[] message, Int32 Modificator)
        {
            long meLen = message.LongLength >> 2;

            fixed (byte * me = message)
            {
                Int32 * m = (Int32 *) me;
                for (long i = 0; i < meLen; i++, m++)
                    *m ^= Modificator;
            }
        }

        /// <summary>
        /// Применяет побайтовое xor к гамме и сообщению
        /// </summary>
        /// <param name="message">Кодируемое сообщение и результат</param>
        /// <param name="gamma">Гамма. Не менее размера сообщения - gammaIndex</param>
        /// <param name="gammaIndex">Начальный номер символа, с которого применяется гамма</param>
        public static unsafe void xorBytesWithGamma(byte[] message, byte[] gamma, long gammaIndex = 0)
        {
            if (message.LongLength + gammaIndex > gamma.LongLength)
                throw new ArgumentException();

            long mLen = message.Length;
            fixed (byte * mf = message, gf = gamma)
            {
                byte * m = mf, g = gf + gammaIndex;
                for (long i = 0; i < mLen; i++, m++, g++)
                    *m ^= *g;
            }
        }


        public unsafe void CFB(byte[] key, byte[] oiv, byte[] compressedOpenText, bool encrypt, int GostRegime = 0)
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
            BytesBuilder.CopyTo(init, block, 0, 71, init.Length > 71 ? init.Length - 71 : 0);
            BytesBuilder.ToNull(init);

            int L1 = 71, L2 = 72;
            if (GostRegime >= 34)
            {
                L1 = 64;
                L2 = 64;
            }

            fixed (byte * o_ = compressedOpenText)
            {

                for (int i = 0; i < compressedOpenText.Length; i += L2)
                {
                    var c = sha.getDuplex(block, true, L1);
                    BytesBuilder.ToNull(block);
                    if (!encrypt)
                        BytesBuilder.CopyTo(compressedOpenText, block, 0, L1, i);


                    if (i+L1 <= compressedOpenText.Length)
                    fixed (byte * b_ = c)
                    {
                        UInt64 * b  = (UInt64 *) b_;
                        UInt64 * o  = (UInt64 *) (o_ + i);

                        o[0] ^= b[0];
                        o[1] ^= b[1];
                        o[2] ^= b[2];
                        o[3] ^= b[3];

                        o[4] ^= b[4];
                        o[5] ^= b[5];
                        o[6] ^= b[6];
                        o[7] ^= b[7];

                        if (GostRegime < 34)
                        {
                            UInt16 * o2 = (UInt16 *) o;
                            UInt16 * b2 = (UInt16 *) b_;

                            o2[32] ^= b2[32];
                            o2[33] ^= b2[33];
                            o2[34] ^= b2[34];

                            o_[70+i] ^= b_[70];

                            // Обнуление блока для безопасности
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
                            // Обнуление блока для безопасности
                            b[0]   = 0;
                            b[1]   = 0;
                            b[2]   = 0;
                            b[3]   = 0;
                            b[4]   = 0;
                            b[5]   = 0;
                            b[6]   = 0;
                            b[7]   = 0;
                        }
                    }
                    else
                    {
                        for (int j = 0; j + i < compressedOpenText.Length && j < L1; j++)
                        {
                            compressedOpenText[j+i] ^= c[j];
                        }
                        BytesBuilder.ToNull(c);
                    }

                    if (encrypt)
                        BytesBuilder.CopyTo(compressedOpenText, block, 0, L1, i);
                }
            }

            sha.Clear(true);
            BytesBuilder.ToNull(block);
        }

        public delegate void cryptCallBack(byte[] result, Exception e);

        public void multiCryptLZMA(cryptCallBack callback, byte[] openText, byte[] key, byte[] OpenInitVector, byte GostRegime = 3, bool DoCompress = true, byte DictSize = 19, int hashCount = -1)
        {
            if (callback == null)
                throw new ArgumentNullException();

            ThreadPool.QueueUserWorkItem
            (
                delegate
                {
                    try
                    {
                        var result = multiCryptLZMA(openText, key, OpenInitVector, GostRegime, DoCompress, DictSize, hashCount);
                        callback(result, null);
                    }
                    catch (Exception e)
                    {
                        callback(null, e);
                    }
                }
            );
        }

        public static bool isR22(int GostRegime)
        {
            if (GostRegime == 22 || GostRegime == 23 || GostRegime == 32 || GostRegime == 33 || GostRegime == 41)
                return true;

            return false;
        }

        /// <summary>
        /// Шифрует ключём произвольной длины key открытый текст
        /// </summary>
        /// <param name="openText">Открытый текст для закрытия</param>
        /// <param name="key">Секретный ключ симметричного шифрования</param>
        /// <param name="OpenInitVector">Открытый вектор инициализации гаммы; может быть null; не менее 8-ми байт на 64-ре байта массива key плюс 8 байт</param>
        /// <param name="DoCompress">true - если открытый текст можно подвергнуть сжатию</param>
        /// <param name="hashCount">Вторичный ключ для усиления гаммы вычисляется путём извлечения хэша; параметр задаёт кратность извлечения хэша</param>
        /// <returns>Зашифрованный текст</returns>
        public byte[] multiCryptLZMA(byte[] openText, byte[] key, byte[] OpenInitVector, byte GostRegime = 3, bool DoCompress = true, byte DictSize = 19, int hashCount = -1)
        {
            // Дублируется в другой функции, см. выше
            if (GostRegime < 20)
            {
                if (hashCount <= 0)
                    hashCount = SHA3.getHashCountForMultiHash();

                if (GostRegime > 9)
                {
                    if (GostRegime > 12)
                        throw new ArgumentOutOfRangeException();
                }
                else
                    if (GostRegime < 0 || GostRegime > 4)
                        throw new ArgumentOutOfRangeException();
            }
            else
            {
                if (hashCount <= 0)
                    if (GostRegime >= 34)
                        hashCount = (int) SHA3.getHashCountForMultiHash20(71, 0, 300, 0, 1);
                    else
                        hashCount = (int) SHA3.getHashCountForMultiHash20();

                if (GostRegime > 23 && GostRegime < 30)
                    throw new ArgumentOutOfRangeException();

                if (GostRegime > 33 && GostRegime < 40)
                    throw new ArgumentOutOfRangeException();

                if (GostRegime > 41)
                    throw new ArgumentOutOfRangeException();
            }

            byte[] result = null;
            try
            {
                useOldDuplex = GostRegime < 10;

                bool ovNull = false;
                if (OpenInitVector == null)
                {
                    ovNull = true;
                    if (!isR22(GostRegime))
                        OpenInitVector = CreateInitVector(key.LongLength * 71 / 64, 64, GostRegime);
                    else
                    {
                        var oilen = key.LongLength * 71 / 64;
                        oilen &= 0x7FFFFFF0;
                        if (oilen < 1024)
                            oilen = 1024;

                        OpenInitVector = CreateInitVector(oilen, 64, GostRegime);
                    }
                }

                if (!DoCompress)
                    DictSize = 0;

                int pc = 0;
                List<byte[]> keys      = GetKeysFromKeyData     (key,          OpenInitVector, hashCount, GostRegime, ref pc);
                List<byte[]> oivectors = GetOIVectorsFromKeyData(keys.Count-1, OpenInitVector, hashCount);

                byte[] compressedOpenText = compress(openText, key, keys, oivectors, DoCompress, DictSize, GostRegime, pc, hashCount);

                if (GostRegime < 10)
                {
                    if (GostRegime == 0 || GostRegime == 3 || GostRegime == 4)
                    {
                        simpleCrypt(compressedOpenText, getHash512(keys[keys.Count - 1]), getHash512(oivectors[keys.Count - 2]));
                        simpleCrypt(compressedOpenText,            keys[keys.Count - 1],  getHash384(oivectors[keys.Count - 2]));
                    }

                    if (GostRegime != 0)
                        cryptBy28147(compressedOpenText, keys, oivectors, true, GostRegime);

                    for (int i = keys.Count - 2; i >= 0; i--)   // -2 - потому что последний ключ уже использован в алгоритме по ГОСТ 28147 (модифицированный)
                    {
                        simpleCrypt(compressedOpenText, keys[i], oivectors[i]);
                    }
                }
                else
                {
                    var CFBEnded = 0;
                    Exception CFBE = null;
                    var sync = new Object();
                    ThreadPool.QueueUserWorkItem
                    (
                        delegate
                        {
                            try
                            {
                                if (GostRegime >= 20)
                                    CFB(key, OpenInitVector, compressedOpenText, true, GostRegime);

                                lock (sync)
                                {
                                    CFBEnded = 1;
                                    Monitor.Pulse(sync);
                                }
                            }
                            catch (Exception e)
                            {
                                CFBE = e;

                                lock (sync)
                                {
                                    CFBEnded = 2;
                                    Monitor.Pulse(sync);
                                }
                            }
                            finally
                            {
                                lock (sync)
                                {
                                    Monitor.Pulse(sync);
                                }
                            }
                        }
                    );

                    byte[] keyGamma1, keyGamma2;
                    prepareGammaRegime10(keys, oivectors, GostRegime, compressedOpenText.LongLength, out keyGamma1, out keyGamma2, OpenInitVector);

                    lock (sync)
                    {
                        while (CFBEnded == 0)
                            Monitor.Wait(sync);
                    }

                    if (CFBEnded == 2)
                        throw CFBE;

                    xorBytesWithGamma(compressedOpenText, keyGamma1);
                    if (keyGamma2 != null)
                        xorBytesWithGamma(compressedOpenText, keyGamma2);
                }

                var bb = new BytesBuilder();

                bb.addByte(DictSize);
                bb.addByte(GostRegime);
                if (GostRegime >= 20)
                    bb.addVariableULong((ulong) pc);

                bb.addVariableULong((ulong) hashCount);
                bb.addVariableULong((ulong) OpenInitVector.LongLength);
                bb.add(OpenInitVector);
                bb.add(compressedOpenText);
                result = bb.getBytes();

                foreach (var keyPart in keys)
                {
                    BytesBuilder.ToNull(keyPart);
                }

                BytesBuilder.ToNull(compressedOpenText);
                if (ovNull)
                    bb.clear();
                    //BytesBuilder.ToNull(OpenInitVector);
                else
                {
                    BytesBuilder.ToNull(compressedOpenText);    // bb.clear нельзя, т.к. очистится OpenInitVector
                }

                keys      = null;
                oivectors = null;
                openText  = null;
                bb        = null;
                OpenInitVector = null;
                compressedOpenText = null;
                Clear(true);
            }
            finally
            {
                useOldDuplex = false;
            }

            return result;
        }

        
        public void multiDecryptLZMA(byte[] closedText, byte[] key, cryptCallBack callback)
        {
            if (callback == null)
                throw new ArgumentNullException();

            ThreadPool.QueueUserWorkItem
            (
                delegate
                {
                    try
                    {
                        var result = multiDecryptLZMA(closedText, key);
                        callback(result, null);
                    }
                    catch (Exception e)
                    {
                        callback(null, e);
                    }
                }
            );
        }


        public byte[] multiDecryptLZMA(byte[] closedText, byte[] key)
        {
            try
            {
                ulong openInitVectorLen;
                byte  DictSize, GostRegime;
                ulong hashCount, pcl = 0;
                int pc;

                DictSize   = closedText[0];
                GostRegime = closedText[1];
                useOldDuplex = GostRegime < 10;

                bool DoCompress = DictSize > 0;

                // см. выше дублирован код
                if (GostRegime < 20)
                {
                    if (GostRegime > 9)
                    {
                        if (GostRegime > 12)
                            throw new ArgumentOutOfRangeException();
                    }
                    else
                        if (GostRegime < 0 || GostRegime > 4)
                            throw new ArgumentOutOfRangeException();
                }
                else
                {
                    if (GostRegime > 23 && GostRegime < 30)
                        throw new ArgumentOutOfRangeException();

                    if (GostRegime > 33 && GostRegime < 40)
                        throw new ArgumentOutOfRangeException();

                    if (GostRegime > 41)
                        throw new ArgumentOutOfRangeException();
                }

                var s = 2;
                if (GostRegime >= 20)
                s += BytesBuilder.BytesToVariableULong(out pcl,         closedText, 2);

                s += BytesBuilder.BytesToVariableULong(out hashCount,         closedText, s);
                s += BytesBuilder.BytesToVariableULong(out openInitVectorLen, closedText, s);

                byte[] openText = new byte[closedText.LongLength - s - (long) openInitVectorLen];
                byte[] openInit = new byte[openInitVectorLen];
                BytesBuilder.CopyTo(closedText, openInit, 0, -1, s);
                BytesBuilder.CopyTo(closedText, openText, 0, -1, s + (long) openInitVectorLen);

                pc = (int) pcl;
                List<byte[]> keys      = GetKeysFromKeyData     (key,          openInit, (int) hashCount, GostRegime, ref pc);
                List<byte[]> oivectors = GetOIVectorsFromKeyData(keys.Count-1, openInit, (int) hashCount);

                if (GostRegime < 10)
                {
                    for (int i = 0; i < keys.Count - 1; i++)
                    {
                        simpleCrypt(openText, keys[i], oivectors[i]);
                    }

                    if (GostRegime != 0)
                        cryptBy28147(openText, keys, oivectors, false, GostRegime);

                    if (GostRegime == 0 || GostRegime == 3 || GostRegime == 4)
                    {
                        simpleCrypt(openText,            keys[keys.Count - 1],  getHash384(oivectors[keys.Count - 2]));
                        simpleCrypt(openText, getHash512(keys[keys.Count - 1]), getHash512(oivectors[keys.Count - 2]));
                    }
                }
                else
                {
                    byte[] keyGamma1, keyGamma2;
                    prepareGammaRegime10(keys, oivectors, GostRegime, openText.LongLength, out keyGamma1, out keyGamma2, openInit);

                    xorBytesWithGamma(openText, keyGamma1);
                    if (keyGamma2 != null)
                        xorBytesWithGamma(openText, keyGamma2);

                    if (GostRegime >= 20)
                        CFB(key, openInit, openText, false, GostRegime);
                }

                try
                {
                    openText = uncompress(openText, key, keys, oivectors, DoCompress, DictSize, GostRegime, pc, (int) hashCount);
                }
                catch
                {
                    openText = null;
                }

                foreach (var keyPart in keys)
                {
                    BytesBuilder.ToNull(keyPart);
                }

                keys      = null;
                oivectors = null;
                key       = null;
                Clear(true);
                useOldDuplex = false;

                return openText;
            }
            finally
            {
                useOldDuplex = false;
            }
        }

        public void prepareGammaRegime10(List<byte[]> keys, List<byte[]> oivectors, int GostRegime, long len, out byte[] g1, out byte[] g2, byte[] OpenInitVector)
        {
            int gr = GostRegime % 10;
            byte[] gamma1 = null, gamma2 = null;
            g1 = null; g2 = null;

            var key3 = new byte[64];
            BytesBuilder.CopyTo(keys[keys.Count - 1], key3, 0, 64, keys[keys.Count - 1].Length - 64);

            if (gr >= 1 && gr <= 2 || isR22(GostRegime))
            {
                var g = new Gost28147Modified();

                if (gr == 2 || isR22(GostRegime))
                {
                    byte[] key1, key2;
                    if (GostRegime >= 20)
                    {
                        key1 = new byte[160];
                        BytesBuilder.CopyTo(keys[keys.Count - 1], key1, 0, 160, 0);
                        key2 = new byte[160];
                        BytesBuilder.CopyTo(keys[keys.Count - 1], key2, 0, 160, 160);

                        var oiv = BytesBuilder.CloneBytes(OpenInitVector, 0, OpenInitVector.Length);
                        Array.Reverse(oiv);
                        oiv = getDuplex(oiv);
                        var oiv1 = getDuplex(OpenInitVector, true);

                        if (GostRegime >= 34)
                        {
                            if ((oiv.Length & 15) > 0)
                            {
                                var oivt = BytesBuilder.CloneBytes(oiv, 0, oiv.Length & 0x7FFFFFF0);
                                BytesBuilder.ToNull(oiv);
                                oiv = oivt;
                            }
                            if ((oiv1.Length & 15) > 0)
                            {
                                var oiv1t = BytesBuilder.CloneBytes(oiv1, 0, oiv1.Length & 0x7FFFFFF0);
                                BytesBuilder.ToNull(oiv1);
                                oiv1 = oiv1t;
                            }
                        }

                        g.prepareGamma(key1, oiv1, Gost28147Modified.CryptoProA, Gost28147Modified.CryptoProC, Gost28147Modified.Sbox_Default, Gost28147Modified.ESbox_A, Gost28147Modified.ESbox_B, Gost28147Modified.ESbox_C);
                        gamma1 = g.getGamma(len, GostRegime < 22 ? 1 : GostRegime - 21);
                        g.prepareGamma(key2, oiv, Gost28147Modified.CryptoProB, Gost28147Modified.CryptoProD, Gost28147Modified.Sbox_Default, Gost28147Modified.ESbox_D, Gost28147Modified.ESbox_C, Gost28147Modified.ESbox_A);
                        gamma2 = g.getGamma(len, GostRegime < 22 ? 1 : GostRegime - 21);
                    }
                    else
                    {
                        key1 = new byte[128];
                        BytesBuilder.CopyTo(keys[keys.Count - 1], key1, 0, 128, 0);
                        key2 = new byte[128];
                        BytesBuilder.CopyTo(keys[keys.Count - 1], key2, 0, 128, 128);

                        g.prepareGamma(key1, getHash224(oivectors[keys.Count - 2]), Gost28147Modified.CryptoProA, Gost28147Modified.CryptoProC, null, null, null, null, true);
                        gamma1 = g.getGamma(len);
                        g.prepareGamma(key2, getHash384(oivectors[keys.Count - 2]), Gost28147Modified.CryptoProB, Gost28147Modified.CryptoProD, null, null, null, null, true);
                        gamma2 = g.getGamma(len);
                    }

                    BytesBuilder.ToNull(key1);
                    BytesBuilder.ToNull(key2);
                }

                var k3 = getHash512(key3);
                g1 = g.getGOSTGamma(k3, getHash256(oivectors[keys.Count - 2]), Gost28147Modified.ESbox_A,    len);
                g2 = g.getGOSTGamma(k3, getHash256(oivectors[keys.Count - 2]), Gost28147Modified.CryptoProA, len);

                BytesBuilder.ToNull(k3);
            }


            var k = getHash512(keys[keys.Count - 1]);
            prepareGamma(k, getHash512(oivectors[keys.Count - 2]));
            if (g1 == null)
            {
                g1 = getGamma(len, true);
            }
            else
            {
                var ga = g1;
                var g  = getGamma(len, true);
                g1 = getDuplexMod(ga, g);

                BytesBuilder.ToNull(ga);
                BytesBuilder.ToNull(g);
            }

            for (int i = keys.Count - 2; i >= 0; i--)   // -2 - потому что последний ключ уже использован
            {
                prepareGamma(keys[i], oivectors[i]);
                var g = getGamma(len, true);

                var ga = g1;
                g1 = getDuplexMod(ga, g);

                BytesBuilder.ToNull(ga);
                BytesBuilder.ToNull(g);
            }

            if (gamma1 != null)
            {
                var ga = g1;
                g1 = getDuplexMod(ga, gamma1);

                BytesBuilder.ToNull(ga);
                BytesBuilder.ToNull(gamma1);
            }

            if (gamma2 != null)
            {
                xorBytesWithGamma(g2, gamma2);
                BytesBuilder.ToNull(gamma2);
            }
        }

        private void cryptBy28147(byte[] openText, List<byte[]> keys, List<byte[]> oivectors, bool encrypt, int GostRegime = 2)
        {
            byte[] gamma1 = null, gamma2 = null, gamma3;
            var g = new Gost28147Modified();

            var key3 = new byte[32];
            BytesBuilder.CopyTo(keys[keys.Count - 1], key3, 0, 32, keys[keys.Count - 1].Length - 32);
            if (GostRegime == 2 || GostRegime == 4)
            {
                var key1 = new byte[128];
                BytesBuilder.CopyTo(keys[keys.Count - 1], key1, 0, 128, 0);
                var key2 = new byte[128];
                BytesBuilder.CopyTo(keys[keys.Count - 1], key2, 0, 128, 128);

                g.prepareGamma(key1, getHash224(oivectors[keys.Count - 2]), Gost28147Modified.CryptoProA, Gost28147Modified.CryptoProC, null, null, null, null, true);
                gamma1 = g.getGamma(openText.LongLength);
                g.prepareGamma(key2, getHash384(oivectors[keys.Count - 2]), Gost28147Modified.CryptoProB, Gost28147Modified.CryptoProD, null, null, null, null, true);
                gamma2 = g.getGamma(openText.LongLength);

                BytesBuilder.ToNull(key1);
                BytesBuilder.ToNull(key2);
            }

            var k3 = getHash512(key3);
            gamma3 = g.getGOSTGamma(k3, getHash256(oivectors[keys.Count - 2]), Gost28147Modified.ESbox_A, openText.LongLength);
            BytesBuilder.ToNull(k3);
            BytesBuilder.ToNull(key3);

            if (GostRegime == 2 || GostRegime == 4)
            {
                xorBytesWithGamma(openText, gamma2);
                xorBytesWithGamma(openText, gamma1);

                BytesBuilder.ToNull(gamma1);
                BytesBuilder.ToNull(gamma2);
            }

            xorBytesWithGamma(openText, gamma3);
            BytesBuilder.ToNull(gamma3);
        }

        class getRandomValues_ended
        {
            public volatile bool ended = false;
        }
        public static string getRandomValues(long count)
        {
            var sb = new StringBuilder();
            Int16 CNT = 0;
            var ended = new getRandomValues_ended();

            ThreadPool.QueueUserWorkItem
            (
            delegate
            {
                while (!ended.ended)
                {
                    CNT++;
                }

                CNT++;
            }
            );

            long lastCNT = CNT;
            for (long i = 0; i < count; i++)
            {
                while (lastCNT == CNT)
                    Thread.Sleep(0);

                lastCNT = CNT;
                sb.Append(CNT);
            }

            ended.ended = true;

            return sb.ToString();
        }

        /// <summary>
        /// Создаёт вектор инициализации, длинной Size + 64
        /// </summary>
        /// <returns>Вектор инициализации, длинной Size + 64</returns>
        static long CreateInitVectorCounter = 1;
        public byte[] CreateInitVector(long Size, long rnd = 64, int regime = 0)
        {
            Size += 64;

            var str = SHA3.getRandomValues(rnd);
            try
            {
                var dr = DriveInfo.GetDrives();
                foreach (var d in dr)
                {
                    try
                    {
                       str += "|" + d.TotalSize + d.VolumeLabel + d.DriveFormat + d.TotalFreeSpace;
                    }
                    catch (Exception e)
                    {
                        str += "|" + e.Message;
                    }
                }
            }
            catch
            {}

            var initVector = Encoding.Default.GetBytes(Environment.WorkingSet.ToString() + "|" + System.Threading.Thread.CurrentThread.ManagedThreadId + "|" + Environment.ProcessorCount.ToString() + "|" + DateTime.Now.ToString() + "!" + (CreateInitVectorCounter++) + "|" + Environment.CurrentDirectory + "|" + Environment.TickCount.ToString() + "|" + Environment.MachineName + "|" + Environment.UserName + str);
            /*
            prepareGamma(key, initVector);
            var result = getGamma(Size, true);
            */

            var result = generateRandomPwdByDerivatoKey(initVector, (int) Size, true, regime);

            BytesBuilder.ToNull(initVector);

            return result;
        }

        /// <summary>
        /// Сжимает, если DoCompress = true, подписывает сообщение хэшем с секретным ключём
        /// Вызывается multiCryptLZMA перед шифрованием, если multiCryptLZMA.DoCompress = true.
        /// </summary>
        /// <param name="openText">Открытый текст</param>
        /// <param name="key">Секретный ключ</param>
        /// <param name="DoCompress">Если true, сжимает открытый текст; подписывается, в любом случае, открытый текст</param>
        /// <param name="dictSize">Устанавливает показатель степени 2 при вычислении размера словаря; если DoCompress, dictSize должен быть не менее 10, иначе ArgumentOutOfRangeException</param>
        /// <returns></returns>
        public byte[] compress(byte[] openText, byte[] key, List<byte[]> keys, List<byte[]> oivectors, bool DoCompress, int dictSize, int GostRegime, int pc, int hashCount)
        {
            if (DoCompress && dictSize < 10)
                throw new ArgumentOutOfRangeException("dictSize", "dictSize < 10");

            byte[] compressedOpenText;
            if (DoCompress)
            {
                var encoder = GetLZMAEncoder(dictSize);

                byte[] lenOT = null;
                byte[] hash  = null;

                using (var inStream  = new MemoryStream(openText.Length + 64 + 16))
                using (var outStream = new MemoryStream(openText.Length + 64 + 16 + 4))
                {
                    if (GostRegime < 20)
                    {
                        BytesBuilder.VariableULongToBytes((ulong)openText.LongLength, ref lenOT);

                        outStream.Write(lenOT,    0, lenOT   .Length);
                        inStream .Write(openText, 0, openText.Length);

                        hash = getMACHash(key, openText);
                        inStream.Write(hash, 0, 72);

                        BytesBuilder.ToNull(lenOT);

                        inStream.Position = 0;
                        encoder.Code(inStream, outStream, inStream.Length, -1L, null);

                        compressedOpenText = outStream.ToArray();

                        BytesBuilder.ToNull(hash);
                    }
                    else
                    {
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
                            LR2    = (int) (  (rg.nextLong() % /*2 * 3 * 5 * 7 * 11 * 13 * 17 * 19 * 23 * 29 * 31*/ 200560490130L) % 2221  ) + 97;
                            bytes2 = rg.nextBytes(LR2);
                        }


                        if (GostRegime < 34)
                        {
                            inStream.Position = 0;
                            encoder.Code(inStream, outStream, inStream.Length, -1L, null);

                            outStream.Write(bytes, 0, bytes.Length);
                            BytesBuilder.ToNull(bytes);

                            BytesBuilder.ULongToBytes((ulong) openText.LongLength, ref lenOT);
                            outStream.Write(lenOT, 0, lenOT.Length);
                        }
                        else
                        {
                            outStream.Write(bytes, 0, bytes.Length);
                            BytesBuilder.ToNull(bytes);

                            inStream.Position = 0;
                            encoder.Code(inStream, outStream, inStream.Length, -1L, null);

                            BytesBuilder.ULongToBytes((ulong) openText.LongLength, ref lenOT);
                            outStream.Write(lenOT, 0, lenOT.Length);

                            outStream.Write(bytes2, 0, bytes2.Length);
                            BytesBuilder.ToNull(bytes2);
                        }

                        compressedOpenText = outStream.ToArray();

                        BytesBuilder.ToNull(lenOT);
                        lenOT = null;
                        LR  = 0;
                        LR2 = 0;
                    }

                    ClearStreams(inStream, outStream);
                }
            }
            else
            {
                byte[] hash;
                if (GostRegime < 20)
                    hash = getMACHash(key, openText);
                else
                {
                    hash = getMACHashMod(openText, keys, oivectors, pc, hashCount - 8, GostRegime);
                }

                int LR = 0, LR2 = 0;
                byte[] bytes = null, bytes2 = null;

                if (GostRegime >= 20)
                {
                    // выше - аналогично
                    prepareGamma(keys[keys.Count - 1], oivectors[0]);
                    var rg    = new SHA3Random(getGamma(71, true));
                        LR    = (int) (  (rg.nextLong() % /*2 * 3 * 5 * 7 * 11 * 13 * 17 * 19 * 23 * 29 * 31*/ 200560490130L) % 2221  ) + 97;
                    bytes = rg.nextBytes(LR);

                    if (GostRegime >= 34)
                    {
                        LR2    = (int) (  (rg.nextLong() % /*2 * 3 * 5 * 7 * 11 * 13 * 17 * 19 * 23 * 29 * 31*/ 200560490130L) % 2221  ) + 97;
                        bytes2 = rg.nextBytes(LR2);
                    }
                }

                if (GostRegime >= 20)
                {
                    compressedOpenText = new byte[openText.Length + /*hash.Length*/200 + LR + LR2];
                    if (GostRegime < 34)
                    {
                        BytesBuilder.CopyTo(openText, compressedOpenText, 0);
                        BytesBuilder.CopyTo(hash,     compressedOpenText, openText.LongLength);
                        BytesBuilder.CopyTo(bytes,    compressedOpenText, openText.LongLength + 200);
                        BytesBuilder.ToNull(bytes);
                    }
                    else
                    {
                        BytesBuilder.CopyTo(bytes,    compressedOpenText, 0);
                        BytesBuilder.CopyTo(openText, compressedOpenText, bytes.Length);
                        BytesBuilder.CopyTo(hash,     compressedOpenText, bytes.Length+openText.LongLength);
                        BytesBuilder.CopyTo(bytes2,   compressedOpenText, bytes.Length+openText.LongLength+hash.Length);
                        BytesBuilder.ToNull(bytes);
                        BytesBuilder.ToNull(bytes2);
                    }
                }
                else
                {
                    byte[] lenOT = null;
                    BytesBuilder.VariableULongToBytes((ulong)openText.LongLength, ref lenOT);
                    compressedOpenText = new byte[openText.Length + 72 + lenOT.Length];
                    BytesBuilder.CopyTo(lenOT,    compressedOpenText, 0);
                    BytesBuilder.CopyTo(openText, compressedOpenText, lenOT.LongLength);
                    BytesBuilder.CopyTo(hash,     compressedOpenText, compressedOpenText.LongLength - 72);
                    BytesBuilder.ToNull(lenOT);
                }

                BytesBuilder.ToNull(hash);
                LR  = 0;
                LR2 = 0;
            }

            return compressedOpenText;
        }

        /// <summary>
        /// Даёт модифицированный MAC-хеш
        /// </summary>
        /// <param name="openText">Тест для хеширования</param>
        /// <param name="keys">Ключи подписи, инициализируют состояние криптографической функции перед каждым проходом</param>
        /// <param name="oivectors">Дополнительные модификаторы</param>
        /// <param name="pc">Параметр для getMultiHash20</param>
        /// <param name="hashCount">Параметр для getMultiHash20</param>
        /// <returns>200 байт MAC-хеша</returns>
        public byte[] getMACHashMod(byte[] openText, List<byte[]> keys, List<byte[]> oivectors, int pc, int hashCount, int GostRegime)
        {
            if (keys.Count <= 0)
                throw new ArgumentOutOfRangeException("keys", "keys.count <= 0");
            if (oivectors.Count <= 0)
                throw new ArgumentOutOfRangeException("oivectors", "oivectors.count <= 0");

            int L = 32;
            if (openText.Length > 72*32)
                L = 1;

            var shaH = new SHA3(openText.Length);

            var k = new BytesBuilder();
            var t = new BytesBuilder();
            for (int i = 0; i < keys.Count; i++)
            {
                shaH.getDuplex(keys[i], i > 0, -1, false);

                getDuplex(keys[i], i > 0, -1, false);
                t.add(getDuplex(openText, true, L, true));

                if (i + 1 < oivectors.Count)
                {
                    k.add(getDuplex(oivectors[i + 1], true, -1, true));
                }
            }

            for (int i = 0; i < oivectors.Count; i++)
            {
                shaH.getDuplex(oivectors[i], true, -1, false);
            }

            shaH.getDuplex(openText, true, -1, false);

            k.add(oivectors[0], -1, true);
            var kb = k.getBytes();
            k.clear();

            if (kb.Length < 72*2-1)
                throw new ArgumentException("keys.Length + oivectors.Length");

            byte[] keyHash;
            if (GostRegime < 34)
                getMultiHash20(kb, out keyHash, ref pc, hashCount, null, 72*2-1);
            else
                getMultiHash40(kb, out keyHash, ref pc, hashCount, null, 72*2-1);
            BytesBuilder.ToNull(kb);
            kb = null;

            byte[] hashResult = new byte[200];

            var hash = getDuplex(keyHash, true, -1, true);
            var tmp  = t.getBytes();
            t.clear();

            var tmph = getHash512(tmp);

            var hashS = shaH.getGamma();

            BytesBuilder.CopyTo(hash,  hashResult, 0,   72, hash.Length - 72);
            BytesBuilder.CopyTo(tmph,  hashResult, 72,  128-72);
            BytesBuilder.CopyTo(hashS, hashResult, 128);

            BytesBuilder.ToNull(tmp);
            BytesBuilder.ToNull(tmph);
            BytesBuilder.ToNull(hashS);
            BytesBuilder.ToNull(keyHash);

            shaH.Clear(true);

            return hashResult;
        }

        public byte[] getMACHashMod40(byte[] openText, List<byte[]> keys, List<byte[]> oivectors, int pc, int hashCount, int GostRegime)
        {
            if (keys.Count <= 0)
                throw new ArgumentOutOfRangeException("keys", "keys.count <= 0");
            if (oivectors.Count <= 0)
                throw new ArgumentOutOfRangeException("oivectors", "oivectors.count <= 0");

            int L = 32;
            if (openText.Length > 72*32)
                L = 1;

            var shaH = new SHA3(openText.Length);

            var k = new BytesBuilder();
            var t = new BytesBuilder();
            for (int i = 0; i < keys.Count; i++)
            {
                shaH.getDuplex(keys[i], i > 0, -1, false);

                getDuplex(keys[i], i > 0, -1, false);
                t.add(getDuplex(openText, true, L, true));

                if (i + 1 < oivectors.Count)
                {
                    k.add(getDuplex(oivectors[i + 1], true, -1, true));
                }
            }

            for (int i = 0; i < oivectors.Count; i++)
            {
                shaH.getDuplex(oivectors[i], true, -1, false);
            }

            shaH.getDuplex(openText, true, -1, false);

            k.add(oivectors[0], -1, true);
            var kb = k.getBytes();
            k.clear();

            if (kb.Length < 72*2-1)
                throw new ArgumentException("keys.Length + oivectors.Length");

            byte[] keyHash;
            if (GostRegime < 34)
                getMultiHash20(kb, out keyHash, ref pc, hashCount, null, 72*2-1);
            else
                getMultiHash40(kb, out keyHash, ref pc, hashCount, null, 72*2-1);
            BytesBuilder.ToNull(kb);
            kb = null;

            byte[] hashResult = new byte[200];

            var hash = getDuplex(keyHash, true, -1, true);
            var tmp  = t.getBytes();
            t.clear();

            var tmph = getHash512(tmp);

            var hashS = shaH.getGamma();

            BytesBuilder.CopyTo(hash,  hashResult, 0,   72, hash.Length - 72);
            BytesBuilder.CopyTo(tmph,  hashResult, 72,  128-72);
            BytesBuilder.CopyTo(hashS, hashResult, 128);

            BytesBuilder.ToNull(tmp);
            BytesBuilder.ToNull(tmph);
            BytesBuilder.ToNull(hashS);
            BytesBuilder.ToNull(keyHash);

            shaH.Clear(true);

            return hashResult;
        }

        public byte[] getMACHash(byte[] key, byte[] openText)
        {
            prepareGamma(key, openText, true);
            var hash = getGamma(72, true);
            return hash;
        }

        public byte[] uncompress(byte[] compressedText, byte[] key, List<byte[]> keys, List<byte[]> oivectors, bool DoUncompress, int dictSize, int GostRegime, int pc, int hashCount)
        {
            if (DoUncompress && dictSize < 10)
                throw new ArgumentOutOfRangeException("dictSize", "dictSize < 10");

            byte[] OpenText, text, hash;
            if (DoUncompress)
            {
                var decoder = GetLZMADecoder(dictSize);

                using (var inStream  = new MemoryStream(compressedText.Length))
                using (var outStream = new MemoryStream(compressedText.Length >> 1))
                {
                    if (GostRegime < 20)
                    {
                        ulong uncLen;
                        int s = BytesBuilder.BytesToVariableULong(out uncLen, compressedText, 0);

                        //inStream.Write(lenOT,    0, lenOT.Length);
                        inStream.Write(compressedText, s, compressedText.Length - s);

                        inStream.Position = 0;
                        decoder.Code(inStream, outStream, inStream.Length, (long)uncLen + 72, null);

                        OpenText = outStream.ToArray();
                    }
                    else
                    {
                        // ниже - аналогично
                        prepareGamma(keys[keys.Count - 1], oivectors[0]);
                        var gm = getGamma(71, true);
                        var rg = new SHA3Random(gm);
                        BytesBuilder.ToNull(gm);
                        int LR = (int)((((ulong)rg.nextLong()) % 200560490130L) % 2221) + 97;
                        var bytes = GostRegime >= 34 ? rg.nextBytes(LR) : null;
                        int LR2 = 0;
                        byte[] bytes2 = null;
                        if (GostRegime >= 34)
                        {
                            LR2 = (int)((rg.nextLong() % 200560490130L) % 2221) + 97;
                            bytes2 = rg.nextBytes(LR2);
                        }
                        rg.Clear();

                        if (GostRegime >= 34)
                        {
                            if (!BytesBuilder.Compare(compressedText, bytes, bytes.Length))
                            {
                                BytesBuilder.ToNull(bytes);
                                BytesBuilder.ToNull(bytes2);
                                Clear(true);
                                LR  = 0;
                                LR2 = 0;
                                return null;
                            }
                        }

                        inStream.Write(compressedText, GostRegime >= 34 ? LR : 0, (int)(compressedText.LongLength - LR - LR2 - 8));

                        if (GostRegime >= 34)
                        {
                            if (!BytesBuilder.Compare(compressedText, bytes2, LR2, (int)compressedText.LongLength - LR2))
                            {
                                BytesBuilder.ToNull(bytes);
                                BytesBuilder.ToNull(bytes2);
                                Clear(true);
                                LR  = 0;
                                LR2 = 0;
                                return null;
                            }

                            BytesBuilder.ToNull(bytes);
                            BytesBuilder.ToNull(bytes2);
                        }
                        

                        ulong uncLen;

                        BytesBuilder.BytesToULong(out uncLen, compressedText, compressedText.LongLength - 8 - LR2);

                        try
                        {
                            inStream.Position = 0;
                            decoder.Code(inStream, outStream, inStream.Length, (long) uncLen + 200, null);
                        }
                        catch
                        {
                            Clear(true);
                            ClearStreams(inStream, outStream);
                            LR  = 0;
                            LR2 = 0;
                            uncLen = 0;
                            return null;
                        }

                        OpenText = outStream.ToArray();
                        LR  = 0;
                        LR2 = 0;
                        uncLen = 0;
                    }

                    Clear(true);
                    ClearStreams(inStream, outStream);
                }
            }
            else
            {
                if (GostRegime < 20)
                {
                    ulong uncLen;
                    int s = BytesBuilder.BytesToVariableULong(out uncLen, compressedText, 0);
                    OpenText = null;

                    if ((long) uncLen == (compressedText.LongLength - 72 - s))
                    {
                        OpenText = new byte[uncLen + 72];
                        BytesBuilder.CopyTo(compressedText, OpenText, 0, -1, s);
                    }
                    else
                    {
                        return null;
                    }
                }
                else
                {/*
                    BytesBuilder.CopyTo(bytes,    compressedOpenText, 0);
                    BytesBuilder.CopyTo(openText, compressedOpenText, bytes.Length);
                    BytesBuilder.CopyTo(hash,     compressedOpenText, bytes.Length+openText.LongLength);
                    BytesBuilder.CopyTo(bytes2,   compressedOpenText, bytes.Length+openText.LongLength+hash.Length);
                    */
                    // выше - аналогично
                    prepareGamma(keys[keys.Count - 1], oivectors[0]);
                    var rg    = new SHA3Random(getGamma(71, true));
                    int LR    = (int) (  ( ((ulong) rg.nextLong()) % 200560490130L) % 2221  ) + 97;
                    var bytes = GostRegime >= 34 ? rg.nextBytes(LR) : null;
                    int LR2   = 0;
                    byte[] bytes2 = null;
                    if (GostRegime >= 34)
                    {
                        LR2    = (int) (  (rg.nextLong() % 200560490130L) % 2221  ) + 97;
                        bytes2 = rg.nextBytes(LR2);
                    }
                    rg.Clear();

                    if (GostRegime >= 34)
                    {
                        if (!BytesBuilder.Compare(compressedText, bytes, bytes.Length))
                        {
                            BytesBuilder.ToNull(bytes);
                            BytesBuilder.ToNull(bytes2);
                            LR  = 0;
                            LR2 = 0;
                            return null;
                        }

                        if (!BytesBuilder.Compare(compressedText, bytes2, bytes2.Length, (int) compressedText.LongLength - LR2))
                        {
                            BytesBuilder.ToNull(bytes);
                            BytesBuilder.ToNull(bytes2);
                            LR  = 0;
                            LR2 = 0;
                            return null;
                        }

                        BytesBuilder.ToNull(bytes);
                        BytesBuilder.ToNull(bytes2);
                    }

                    ulong uncLen = (ulong) (compressedText.LongLength - LR - LR2);
                    if (uncLen > (ulong) compressedText.LongLength)
                    {
                        LR  = 0;
                        LR2 = 0;
                        uncLen = 0;
                        return null;
                    }

                    OpenText = new byte[uncLen];
                    BytesBuilder.CopyTo(compressedText, OpenText, 0, (long) uncLen, GostRegime >= 34 ? LR : 0);
                    LR  = 0;
                    LR2 = 0;
                    uncLen = 0;
                }
            }

            int hashLen = GostRegime < 20 ? 72 : 200;

            hash = new byte[hashLen];
            text = new byte[OpenText.LongLength - hashLen];
            BytesBuilder.CopyTo(OpenText, text);
            BytesBuilder.CopyTo(OpenText, hash, 0, -1, OpenText.LongLength - hashLen);

            var wellHash = GostRegime >= 20 ? getMACHashMod(text, keys, oivectors, pc, hashCount - 8, GostRegime) : getMACHash(key, text);
            if (!BytesBuilder.Compare(wellHash, hash))
            {
                BytesBuilder.ToNull(wellHash);
                BytesBuilder.ToNull(hash);
                BytesBuilder.ToNull(text);
                BytesBuilder.ToNull(OpenText);
                return null;
            }

            BytesBuilder.ToNull(hash);
            BytesBuilder.ToNull(wellHash);
            BytesBuilder.ToNull(OpenText);

            return text;
        }

        private static void ClearStreams(MemoryStream inStream, MemoryStream outStream)
        {
            inStream.Position = 0;
            outStream.Position = 0;

            for (long i = 0; i < inStream.Length; i++)
                inStream.WriteByte(0);
            for (long i = 0; i < outStream.Length; i++)
                outStream.WriteByte(0);
        }

        static readonly byte[] GetKeysFromKeyData_bytes = Convert.FromBase64String("+qm1GLrcWsdPLM/Y/uEb2IC2Hzkrshp58P/aVWqJR96yANFcmz9lFmUVu8y+dJ/s1LXIHMMNMZJ2vqb8k9WQGdD8+er7IehBFMQzypb34KSfeWPwfQUs+H30ndZVf6K7MIqXvwzGb9N0YsqxkjC7AJn0ajjIu2sprSxNHnajjPnkHqCqU5cesVzlI55oWkM=");
        public List<byte[]> GetKeysFromKeyData(byte[] key, byte[] OpenInitVector, int hashCount, int GostRegime, ref int pc)
        {
            List<byte[]> keys = new List<byte[]>((key.Length >> 6) + 1); // бьём ключ на 64-байтные эпизоды
            var sha  = new SHA3(OpenInitVector.Length);
            var hash = sha.getDuplex(OpenInitVector);
                       sha.getDuplex(key, true, -1, false);

            int j = 0;
            do
            {
                int curLen = 64;
                if (key.Length - j < 72)
                    curLen = key.Length - j;

                byte[] tmp = new byte[curLen];
                BytesBuilder.CopyTo(key, tmp, 0, curLen, j);
                if (GostRegime < 20)
                    keys.Add(tmp);
                else
                {
                    keys.Add(getDerivatoKey(tmp, hash, hashCount, ref pc, 71, GostRegime >= 34 ? 4 : 2));
                    sha.getDuplex(tmp, true, -1, false);
                    BytesBuilder.ToNull(tmp);

                    if (GostRegime < 40)
                    {
                        Array.Reverse(hash);
                        var h = sha.getDuplex(hash, true);
                        BytesBuilder.ToNull(hash);
                        hash = h;
                    }
                    else
                    {
                        sha.getDuplex(GetKeysFromKeyData_bytes, true, -1, false);
                        var h = sha.getDuplex(hash, true);
                        BytesBuilder.ToNull(hash);
                        hash = h;
                    }
                }

                j += curLen;
            }
            while (j < key.Length);

            sha.Clear(true);

            var L = 360;
            if (GostRegime >= 10 && GostRegime <= 19)
                L = 320;
            else
            if (GostRegime >= 20)
                L = 160*2 + 64; // 384

            keys.Add(getDerivatoKey(key, OpenInitVector, hashCount, ref pc, L, GostRegime / 10));
            return keys;
        }

        public byte[] getDerivatoKey(byte[] key, byte[] OpenInitVector, int hashCount, ref int pc, int DerivatoKeyLen = 320, int regime = 1)
        {
            if (DerivatoKeyLen < 72)
                DerivatoKeyLen = 72;

            BytesBuilder lastKey = new BytesBuilder();
            BytesBuilder duplex  = new BytesBuilder();
            byte[] keyDup;
            if (regime < 2)
            {
                byte[] reversedKey = (byte[])key.Clone();
                Array.Reverse(reversedKey);

                duplex.add(key, -1, true);              // true - иначе BytesBuilder обнулит key при вызове Clear
                duplex.add(OpenInitVector, -1, true);

                keyDup = duplex.getBytes();
                duplex.add(getDuplex(keyDup), 0);
                duplex.add(reversedKey);                // reversedKey будет обнулён при duplex.clear() - не использовать параметр true, иначе не будет обнулён
                BytesBuilder.ToNull(keyDup);

                keyDup = duplex.getBytes();

                duplex.clear();                         // reversedKey - обнулён, результат getDuplex(keyDup) - обнулён
                duplex.add(getDuplex(keyDup, regime > 0));
                BytesBuilder.ToNull(keyDup);
            }
            else
            {
                keyDup = new byte[key.LongLength + OpenInitVector.LongLength];
                BytesBuilder.CopyTo(key,            keyDup, 0);
                BytesBuilder.CopyTo(OpenInitVector, keyDup, key.LongLength);

                var kd = getDuplex(keyDup);
                duplex.add(getDuplex(kd, true));
                BytesBuilder.ToNull(keyDup);
                BytesBuilder.ToNull(kd);
            }

            int k1 = 1;
            int hc;
            if (regime >= 2)
            {
                if (DerivatoKeyLen <= 72)
                    hc = hashCount;
                else
                    if (regime < 4)
                        hc = (int) Math.Ceiling(  hashCount - (4.0*Math.Log(DerivatoKeyLen / 72.0) / Math.Log(2.0))  );
                    else
                    {
                        var cl = DerivatoKeyLen > duplex.Count ? DerivatoKeyLen : duplex.Count;
                        hc = (int) Math.Ceiling(  hashCount - (Math.Log(cl / 72.0) / Math.Log(2.0))  );
                    }
            }
            else
                hc = hashCount * 72 / DerivatoKeyLen;

            byte[] hash;
            while (lastKey.Count < DerivatoKeyLen)
            {
                var t  = duplex.getBytes();
                keyDup = getDuplex(t, regime > 0);
                

                if (regime < 2)
                {
                    var ph = getHash512(keyDup);
                    hash = getMultiHash(ph, hc);
                    BytesBuilder.ToNull(ph);
                }
                else
                {
                    if (regime < 4)
                        getMultiHash20(keyDup, out hash, ref pc, hc);
                    else
                        getMultiHash40(keyDup, out hash, ref pc, hc);
                }

                lastKey.add(hash);
                duplex.add(hash, k1);
                k1 *= -1;

                BytesBuilder.ToNull(t);
                BytesBuilder.ToNull(keyDup);
            }

            var result = lastKey.getBytes();
            lastKey.clear();
            duplex.clear(); // очищать после использования lastKey, т.к. hash и там, и там идёт без копирования

            if (regime > 0)
            {
                var result1 = getDuplex(result, true);
                BytesBuilder.ToNull(result);

                result = getDuplex(result1, true);
                BytesBuilder.ToNull(result1);
            }

            lastKey    = null;
            duplex     = null;
            Clear(true);

            if (regime > 1 && result.LongLength != DerivatoKeyLen)
            {
                var r = result;
                result = new byte[DerivatoKeyLen];
                BytesBuilder.CopyTo(r, result);
                BytesBuilder.ToNull(r);
            }

            return result;
        }

        public List<byte[]> GetOIVectorsFromKeyData(int keysCount, byte[] OpenInitVector, long hashCount)
        {
            List<byte[]> oivectorts = new List<byte[]>(keysCount);

            int k = OpenInitVector.Length / keysCount;

            if (k > 71)
                k = 71;
            if (k < 8)
                throw new Exception("keccak.crypt: OpenInitVector is small");

            int j = 0;
            do
            {
                int curLen = k;
                if (OpenInitVector.Length - j < k * 2)
                    curLen = OpenInitVector.Length - j;

                if (curLen > 71)
                    curLen = 71;

                byte[] tmp = new byte[curLen];
                BytesBuilder.CopyTo(OpenInitVector, tmp, 0, curLen, j);
                oivectorts.Add(tmp);

                j += curLen;
            }
            while (oivectorts.Count < keysCount);

            return oivectorts;
        }

        public void simpleCrypt(byte[] openText, byte[] key, byte[] OpenInitVector, bool DoCompress = true)
        {
            prepareGamma(key, OpenInitVector);
            var keyGamma  = getGamma(openText.LongLength, true);

            xorBytesWithGamma(openText, keyGamma);
        }

        public static SevenZip.Compression.LZMA.Encoder GetLZMAEncoder(int dictSize = 19)
        {
            CoderPropID[] propIDs = 
				{
					CoderPropID.DictionarySize,
					CoderPropID.PosStateBits,
					CoderPropID.LitContextBits,
					CoderPropID.LitPosBits,
					CoderPropID.Algorithm,
					CoderPropID.NumFastBytes,
					CoderPropID.MatchFinder,
					CoderPropID.EndMarker
				};
            object[] properties = 
				{
					(Int32)(1 << dictSize),
					(Int32) 2,
					(Int32) 3,
					(Int32) 0,
					(Int32) 2,
					(Int32) 128,
					"bt4",
					false
				};

            var encoder = new SevenZip.Compression.LZMA.Encoder();
            encoder.SetCoderProperties(propIDs, properties);
            return encoder;
        }

        public static SevenZip.Compression.LZMA.Decoder GetLZMADecoder(int dictSize = 19)
        {
            var encoder = GetLZMAEncoder(dictSize);
            var decoder = new SevenZip.Compression.LZMA.Decoder();
            using (var ms = new MemoryStream())
            {
                encoder.WriteCoderProperties(ms);
                decoder.SetDecoderProperties(ms.ToArray());
            }

            return decoder;
        }


        private void CryptBlock(byte[] bytesO, List<byte[]> keys, long TN, SHA3 sha)
        {
            var tcounter = new byte[8];
            BytesBuilder.ULongToBytes((ulong) TN, ref tcounter);

            sha.prepareGamma(keys[0], tcounter);
            var gamma = sha.getGamma(bytesO.Length, true);
            for (int i = 1; i < keys.Count; i++)
            {
                var gamma1 = gamma;

                sha.prepareGamma(keys[i], tcounter);
                var gamma2 = sha.getGamma(bytesO.Length, true);

                gamma = sha.getDuplexMod(gamma1, gamma2);
            }

            xorBytesWithGamma(bytesO, gamma);
        }
        /*
        public class ProgressObject
        {
            public long progress = 0;
        }

        public unsafe void parallelCryptT(string FileName, string CryptFileName, bool decrypt, byte[] key, byte[] OpenInitVector, byte regime, ProgressObject progress, int hashCount)
        {
            var thr = new Thread
                ((ThreadStart) delegate
                {
                    parallelCryptT(FileName, CryptFileName, decrypt, key, OpenInitVector, regime, progress, hashCount);
                });

            thr.Start();
        }

        public unsafe void parallelCrypt(string FileName, string CryptFileName, bool decrypt, byte[] key, byte[] OpenInitVector, byte regime, ProgressObject progress, int hashCount)
        {
            useOldDuplex = false;
            if (regime != 23)
                throw new ArgumentOutOfRangeException();

            lock (progress)
            {
                progress.progress = 0;
            }

            int pc = Environment.ProcessorCount;
            if (hashCount < 0)
	            hashCount = (int) SHA3.getHashCountForMultiHash20();

            bool ovNull = false;
            if (OpenInitVector == null)
            {
	            ovNull = true;
	            OpenInitVector = CreateInitVector(key.LongLength * 71 / 64);
            }

            List<byte[]> keys      = GetKeysFromKeyData     (key,         OpenInitVector, hashCount, regime, ref pc);
            List<byte[]> oivectors = GetOIVectorsFromKeyData(keys.Count, OpenInitVector, hashCount);


            var bb = new BytesBuilder();

            bb.addByte(0);
            bb.addByte(regime);

            bb.addVariableULong((ulong) hashCount);
            bb.addVariableULong((ulong) OpenInitVector.LongLength);
            bb.add(OpenInitVector);
            byte[] result = bb.getBytes();
            bb.clear();

            File.WriteAllBytes(CryptFileName, result);

            var fi = new FileInfo(FileName);
            long AN      = fi.Length / pc;
            if (decrypt)
            {
                long A = AN & 65535;
                AN -= A;
            }

            long BNC     = 0;
            long ANCM    = fi.Length % AN;
            long tnc     = 0;
            var  sync    = new object();
            var  threads = pc;

            for (int i = 0; i < pc; i++)
            {
                var tnumber = i;
                var TN      = BNC;
                var LN      = AN;
                BNC += AN;

                long TNC0 = 0;
                if (decrypt)
                {
                }
                else
                {
                    TNC0 = (AN / 65461) << 16;
                    if ((AN % 65461) > 0)
                        TNC0 += 65536;
                }

                long TNC = tnc;
                tnc += TNC0;

                if (i == pc - 1)
                {
                    LN  += ANCM;
                    // BNC += ANCM;
                }

                var thr = new Thread
                ((ThreadStart) delegate
                {
                    try
                    {
                        var last = TNC + LN;
                        var  b3  = new byte[3];
                        var  b8  = new byte[8];
                        var sha  = new SHA3(65536+512);
                        List<byte[]> tkeys = new List<byte[]>(keys.Count);
                        lock (keys)
                        {
                            for (int j = 0; j < keys.Count; j++)
                                tkeys[j] = sha.getDuplexMod(keys[j], oivectors[j]);
                        }

                        var  bk = new byte[tkeys[tkeys.Count].LongLength + 8];
                        BytesBuilder.CopyTo(tkeys[tkeys.Count], bk, 8);

                        var LNC = LN;
                        byte[] bytesOF = new byte[65536+512], bytesO;
                        using (FileStream fs = new FileStream(FileName,      FileMode.Open, FileAccess.Read,  FileShare.Read,  0, FileOptions.WriteThrough),
                                          wf = new FileStream(CryptFileName, FileMode.Open, FileAccess.Write, FileShare.Write, 0, FileOptions.WriteThrough)
                               )
                        {
                            while (TN < last)
                            {
                                int  start = (int) (TN & 512);
                                long TNS = TN;
                                if (start > 0)
                                {
                                    TNS = TN - start;
                                }

                                fs.Position = TNS;
                                var L = fs.Read(bytesOF, 0, 65536 + 512);
                                fixed (byte * BytesOF = bytesOF)
                                    if (decrypt)
                                        bytesO = BytesBuilder.CloneBytes(BytesOF, start, (L - start) < 65536 ? (L - start) : 65536);
                                    else
                                        bytesO = BytesBuilder.CloneBytes(BytesOF, start, (L - start) < 65461 ? (L - start) : 65461);

                                if (decrypt)
                                {
                                    sha.CryptBlock(bytesO, tkeys, TN, sha);
                                    BytesBuilder.VariableULongToBytes((ulong) TN, ref bk);
                                    var hash = sha.getMACHash(bk, bytesO);
                                }
                                else
                                {
                                    BytesBuilder.VariableULongToBytes((ulong) TN, ref bk);
                                    var hash = sha.getMACHash(bk, bytesO);
                                    sha.CryptBlock(bytesO, tkeys, TN, sha);

                                    lock (progress)
                                    {
                                        progress.progress += bytesO.LongLength;
                                    }

                                    wf.Position = TNC;
                                    BytesBuilder.VariableULongToBytes((ulong) bytesO.LongLength, ref b3);
                                    BytesBuilder.ULongToBytes((ulong) TN, ref b8);
                                    wf.Write(b3,     0, 3);
                                    wf.Write(b8,     0, 8);
                                    wf.Write(bytesO, 0, bytesO.Length);
                                    wf.Position = TNC + 65461 + 3 + 8;
                                    wf.Write(hash,   0, hash.Length);

                                    TN  += 65461;
                                    LNC -= 65461;
                                    TNC += 65536; // 65461 + 3 + 8 + 64
                                }

                                BytesBuilder.ToNull(bytesO);
                            }
                        }

                        BytesBuilder.ToNull(bytesOF);
                        sha.Clear();
                        for (int j = 0; j < tkeys.Count; j++)
                            BytesBuilder.ToNull(tkeys[j]);
                    }
                    finally
                    {
                        lock (sync)
                        {
                            threads--;
                            Monitor.Pulse(sync);
                        }
                    }
                });

                thr.Start();
            }

            lock (sync)
            {
                while (threads > 0)
                    Monitor.Wait(sync);
            }

            foreach (var keyPart in keys)
            {
	            BytesBuilder.ToNull(keyPart);
            }

            if (ovNull)
	            BytesBuilder.ToNull(OpenInitVector);

            keys      = null;
            oivectors = null;
            bb        = null;
            Clear(true);
            useOldDuplex = false;

            lock (progress)
            {
                progress.progress = long.MaxValue;
            }
        }
        */

        public class SHA3Random
        {
            readonly SHA3 sha, shahash;
            byte[] hash;
            public SHA3Random(byte[] seed)
            {
                sha = new SHA3(seed.Length);
                hash = sha.getDuplex(seed);

                shahash = new SHA3(seed.Length);
            }

            public SHA3Random(string seed, Encoding en = null)
            {
                if (en == null)
                    en = new UTF32Encoding();

                sha = new SHA3(seed.Length);
                var bytes = en.GetBytes(seed);
                hash = sha.getDuplex(bytes);

                BytesBuilder.ToNull(bytes);

                shahash = new SHA3(seed.Length);
            }

            public void randomize(byte[] randomizeSeed)
            {
                sha.getDuplex(randomizeSeed, true, -1, false);
            }

            public void wrongRandomize(byte[] randomizeSeed)
            {
                sha.getDuplex(hash, true, -1, false);
            }

            public byte[] nextBytes64()
            {
                var t = sha.getDuplex(hash, true);
                BytesBuilder.ToNull(hash);
                hash = t;

                if (hash.Length >= 144)
                {
                    t = shahash.getHash512(hash);
                    BytesBuilder.ToNull(hash);
                    hash = t;
                }

                return shahash.getHash512(hash);
            }

            public uint nextInt()
            {
                var bytes = nextBytes64();
                //uint result = bytes[0] + (bytes[1] << 8) + (bytes[2] << 16) + (bytes[3] << 24);
                uint result = (uint) bytes[3]; result <<= 8;
                result |= (uint) bytes[2]; result <<= 8;
                result |= (uint) bytes[1]; result <<= 8;
                result |= (uint) bytes[0];

                BytesBuilder.ToNull(bytes);

                return result;
            }

            public ulong nextLong()
            {
                var bytes = nextBytes64();
                //ulong result = bytes[0] + (bytes[1] << 8) + (bytes[2] << 16) + (bytes[3] << 24) + (bytes[4] << 32) + (bytes[5] << 40) + (bytes[6] << 48) + (bytes[7] << 56);
                ulong result = bytes[7]; result <<= 8;
                result |= bytes[6]; result <<= 8;
                result |= bytes[5]; result <<= 8;
                result |= bytes[4]; result <<= 8;
                result |= bytes[3]; result <<= 8;
                result |= bytes[2]; result <<= 8;
                result |= bytes[1]; result <<= 8;
                result |= bytes[0];

                BytesBuilder.ToNull(bytes);

                return result;
            }

            public Guid nextGUID()
            {
                var bytes = nextBytes64();
                var b16 = new byte[16];
                BytesBuilder.CopyTo(bytes, b16, 0, 16);
                BytesBuilder.ToNull(bytes);

                return new Guid(b16);
            }

            public byte[] nextBytes(int count)
            {
                var result = new byte[count];
                var left   = count;

                int cnt;
                for (int i = 0; i < count; i += cnt)
                {
                    var bytes  = nextBytes64();
                    cnt = left % 64;
                    if (cnt == 0)
                        cnt = 64;

                    left -= cnt;

                    BytesBuilder.CopyTo(bytes, result, i, cnt);
                    BytesBuilder.ToNull(bytes);
                }

                return result;
            }

            public void Clear()
            {
                sha    .Clear(true);
                shahash.Clear(true);
                BytesBuilder.ToNull(hash);
            }
        }

        public static byte[] getPasswordCypherTable(byte[] initVector, byte count = 32, byte count2 = 32)
        {
            var b = new byte[count*count2];

            for (byte i = 0; i < count;  i++)
            for (byte j = 0; j < count2; j++)
            {
                b[i*count2 + j] = j;
            }

            var COUNT = 1;
            var sc = 3*COUNT*count*count2;

            var sha = new SHA3(initVector.Length);
            int pc = 0;
            var d = sha.getDerivatoKey(initVector, sha.CreateInitVector(initVector.Length, 64, 40), 8, ref pc, sc, 4);

            //for (int k = 0; k < d.Length; k += 6)
            int k = 0;
            for (int kc = 0; kc < COUNT; kc++)
            for (byte i = 0; i < count;  i++)
            for (byte j = 0; j < count2; j++)
            {
                int a  = i*count2;
                int a1 = a + (d[k++] % count2);
                int a2 = a + (d[k++] % count2);
                int a3 = a + (d[k++] % count2);

                if (a1 == a2 || a1 == a3 || a2 == a3)
                {
                    if (a1 == a2)
                        a2 = a3;

                    if (a1 == a2)
                        continue;

                    byte b1 = b[a1];
                    b[a1] = b[a2];
                    b[a2] = b1;
                }
                else
                {
                    byte b1 = b[a1];
                    b[a1] = b[a2];
                    b[a2] = b[a3];
                    b[a3] = b1;
                }
            }

            return b;
        }
    }










    // Копия класса в SymmetricSigner
    public class PasswordSecure: IDisposable
    {
        [System.Runtime.InteropServices.DllImport("kernel32.dll")]
        public static extern Int32 VirtualAlloc(int adress, int size, uint allocType, uint protect);

        [System.Runtime.InteropServices.DllImport("kernel32.dll")]
        public static extern Int32 VirtualLock(int adress, int size);

        [System.Runtime.InteropServices.DllImport("kernel32.dll")]
        public static extern Int32 VirtualUnlock(int adress, int size);

        [System.Runtime.InteropServices.DllImport("kernel32.dll")]
        public static extern Int32 VirtualProtect(int adress, int size, uint newProtect, out int oldProtect);

        [System.Runtime.InteropServices.DllImport("kernel32.dll")]
        public static extern Int32 VirtualFree(int adress, int size, uint freeType);

        readonly int pwdAddress;
        readonly int len;
        private bool disposed = false;
        unsafe public PasswordSecure(byte[] pwdObject)
        {
            len = pwdObject.Length;
            // MEM_COMMIT 0x1000 | MEM_RESERVE 0x2000
            // MEM_PHYSICAL = 0x00400000
            // PAGE_READWRITE 0x04
            // PAGE_NOACCESS 0x01
            pwdAddress = VirtualAlloc(0, len, 0x1000 | 0x2000, 0x04);

            if (pwdAddress == 0)
                throw new OutOfMemoryException("SHA3.PasswordSecure.PasswordSecure: VirtualAlloc failed");

            if (VirtualLock(pwdAddress, len) == 0)
                throw new OutOfMemoryException("SHA3.PasswordSecure.PasswordSecure: VirtualLock failed");

            fixed (byte * source = pwdObject)
            {
                byte* destination = (byte*) pwdAddress;
                BytesBuilder.CopyTo(len, len, source, destination);
                BytesBuilder.ToNull(len, source);
            }

            int old;
            VirtualProtect(pwdAddress, len, 0x01, out old);
        }

        unsafe public byte[] getObjectValue()
        {
            var pwdObject = new byte[len];

            fixed (byte * target = pwdObject)
            {
                int old;
                VirtualProtect(pwdAddress, len, 0x02, out old);
                byte* source = (byte*) pwdAddress;
                BytesBuilder.CopyTo(len, len, source, target);
                VirtualProtect(pwdAddress, len, 0x01, out old);
            }

            return pwdObject;
        }

        unsafe public void Dispose()
        {
            if (disposed)
                return;

            int old;
            VirtualProtect(pwdAddress, len, 0x04, out old);
            BytesBuilder.ToNull(len, (byte *) pwdAddress);

            // MEM_RELEASE
            VirtualUnlock(pwdAddress, len);
            VirtualFree(pwdAddress, 0, 0x8000);
            disposed = true;
        }
    }














    // Копия в updator
    public class BytesBuilder
    {
        public List<byte[]> bytes = new List<byte[]>();

        public long Count
        {
            get
            {
                return count;
            }
        }

        public long countOfBlocks
        {
            get
            {
                return bytes.Count;
            }
        }

        public byte[] getBlock(int number)
        {
            return bytes[number];
        }

        long count = 0;
        public void add(byte[] bytesToAdded, int index = -1, bool isNoConstant = false)
        {
            if (isNoConstant)
            {
                var b = new byte[bytesToAdded.LongLength];
                BytesBuilder.CopyTo(bytesToAdded, b);
                bytesToAdded = b;
            }

            if (index == -1)
                bytes.Add(bytesToAdded);
            else
                bytes.Insert((int) index, bytesToAdded);

            count += bytesToAdded.LongLength;
        }

        public void addCopy(byte[] bytesToAdded, int index = -1)
        {
            add(bytesToAdded, index, true);
        }

        public void addByte(byte number, int index = -1)
        {
            var n = new byte[1];
            n[0] = number;
            add(n, index);
        }

        public void addUshort(ushort number, int index = -1)
        {
            var n = new byte[2];
            n[1] = (byte) (number >> 8);
            n[0] = (byte) (number     );
            add(n, index);
        }

        public void addInt(int number, int index = -1)
        {
            var n = new byte[4];
            n[3] = (byte) (number >> 24);
            n[2] = (byte) (number >> 16);
            n[1] = (byte) (number >> 8);
            n[0] = (byte) (number     );

            add(n, index);
        }

        public void addULong(ulong number, int index = -1)
        {
            var n = new byte[8];
            n[7] = (byte) (number >> 56);
            n[6] = (byte) (number >> 48);
            n[5] = (byte) (number >> 40);
            n[4] = (byte) (number >> 32);
            n[3] = (byte) (number >> 24);
            n[2] = (byte) (number >> 16);
            n[1] = (byte) (number >> 8);
            n[0] = (byte) (number     );

            add(n, index);
        }

        public void addVariableULong(ulong number, int index = -1)
        {
            byte[] target = null;
            BytesBuilder.VariableULongToBytes(number, ref target);

            add(target, index);
        }

        public void add(string utf8String, int index = -1)
        {
            add(UTF8Encoding.UTF8.GetBytes(utf8String), index);
        }

        public void clear(bool fast = false)
        {
            if (!fast)
            {
                foreach (byte[] e in bytes)
                    BytesBuilder.ToNull(e);
            }

            count = 0;
            bytes.Clear();
        }

        public long RemoveLastBlock()
        {
            if (count == 0)
                return 0;
            
            long removedLength = bytes[bytes.Count - 1].LongLength;
            var tmp = bytes[bytes.Count - 1];
            bytes.RemoveAt(bytes.Count - 1);
            BytesBuilder.BytesToNull(tmp);

            count -= removedLength;
            return removedLength;
        }

        public long RemoveBlockAt(int position)
        {
            if (count == 0 || position < 0 || position >= bytes.Count)
                return 0;

            long removedLength = bytes[position].LongLength;
            var tmp = bytes[position];
            bytes.RemoveAt(position);
            BytesBuilder.BytesToNull(tmp);

            count -= removedLength;
            return removedLength;
        }

        public long RemoveBlocks(int position, int endPosition)
        {
            if (count == 0 || position < 0 || position >= bytes.Count || position > endPosition || endPosition >= bytes.Count)
                return 0;

            long removedLength = 0;

            for (int i = position; i <= endPosition; i++)
            {
                var tmp = bytes[position];
                removedLength += RemoveBlockAt(position);
                BytesBuilder.BytesToNull(tmp);
            }

            return removedLength;
        }

        public byte[] getBytes(long resultCount = -1, byte[] resultA = null)
        {
            if (resultCount == -1 || resultCount > count)
                resultCount = count;

            if (resultA != null && resultA.Length < resultCount)
                throw new System.ArgumentOutOfRangeException("resultA", "resultA is too small");

            byte[] result = resultA == null ? new byte[resultCount] : resultA;

            long cursor = 0;
            for (int i = 0; i < bytes.Count/*result.LongLength*/; i++)
            {
                if (cursor >= result.LongLength)
                    break;

                CopyTo(bytes[i], result, cursor);
                cursor += bytes[i].LongLength;
            }

            return result;
        }

        public byte[] getBytes(long resultCount, long index)
        {
            if (resultCount == -1 || resultCount > count)
                resultCount = count;

            byte[] result = new byte[resultCount];

            long cursor = 0;
            long tindex = 0;
            for (int i = 0; i < bytes.Count; i++)
            {
                if (cursor >= result.Length)
                    break;

                if (tindex + bytes[i].LongLength < index)
                {
                    tindex += bytes[i].LongLength;
                    continue;
                }

                CopyTo(bytes[i], result, cursor, resultCount - cursor, index - tindex);
                cursor += bytes[i].LongLength;
                tindex += bytes[i].LongLength;
            }

            return result;
        }

        public static unsafe byte[] CloneBytes(byte[] B, long start, long PostEnd)
        {
            var result = new byte[PostEnd - start];
            fixed (byte * r = result, b = B)
                BytesBuilder.CopyTo(PostEnd, PostEnd - start, b, r, 0, -1, start);

            return result;
        }

        public static unsafe byte[] CloneBytes(byte * b, long start, long PostEnd)
        {
            var result = new byte[PostEnd - start];
            fixed (byte * r = result)
                BytesBuilder.CopyTo(PostEnd, PostEnd - start, b, r, 0, -1, start);

            return result;
        }

        /// <summary>
        /// Копирует массив source в массив target. Если запрошенное количество байт скопировать невозможно, копирует те, что возможно
        /// </summary>
        /// <param name="source">Источник копирования</param>
        /// <param name="target">Приёмник</param>
        /// <param name="targetIndex">Начальный индекс копирования в приёмник</param>
        /// <param name="count">Максимальное количество байт для копирования (-1 - все доступные)</param>
        /// <param name="index">Начальный индекс копирования из источника</param>
        public unsafe static long CopyTo(byte[] source, byte[] target, long targetIndex = 0, long count = -1, long index = 0)
        {
            long sl = source.LongLength;
            if (count < 0)
                count = sl;

            /*
            long firstUncopied = index + count;
            if (firstUncopied > source.Length)
                firstUncopied = source.Length;*/

            fixed (byte * s = source, t = target)
            {
                return CopyTo(sl, target.LongLength, s, t, targetIndex, count, index);
            }
        }

        unsafe public static long CopyTo(long sourceLength, long targetLength, byte* s, byte* t, long targetIndex = 0, long count = -1, long index = 0)
        {
            byte* se = s + sourceLength;
            byte* te = t + targetLength;

            if (count == -1)
            {
                count = Math.Min(sourceLength - index, targetLength - targetIndex);
            }

            byte* sec = s + index + count;
            byte* tec = t + targetIndex + count;

            byte* sbc = s + index;
            byte* tbc = t + targetIndex;

            if (sec > se)
            {
                tec -= sec - se;
                sec = se;
            }

            if (tec > te)
            {
                sec -= tec - te;
                tec = te;
            }

            if (tbc < t)
                throw new ArgumentOutOfRangeException();

            if (sbc < s)
                throw new ArgumentOutOfRangeException();

            if (sec - sbc != tec - tbc)
                throw new OverflowException("BytesBuilder.CopyTo: fatal algorithmic error");


            ulong* sbw = (ulong*)sbc;
            ulong* tbw = (ulong*)tbc;

            ulong* sew = sbw + ((sec - sbc) >> 3);

            for (; sbw < sew; sbw++, tbw++)
                *tbw = *sbw;

            byte toEnd = (byte)(((int)(sec - sbc)) & 0x7);

            byte* sbcb = (byte*)sbw;
            byte* tbcb = (byte*)tbw;
            byte* sbce = sbcb + toEnd;

            for (; sbcb < sbce; sbcb++, tbcb++)
                *tbcb = *sbcb;


            return sec - sbc;
        }

        public static void FillByBytes(byte value, byte[] t, long index = 0, long count = -1)
        {
            if (count < 0)
                count = t.LongLength - index;

            var ic = index + count;
            for (long i = index; i < ic; i++)
                t[i] = value;
        }

        unsafe public static long ToNull(byte[] t, long index = 0, long count = -1)
        {
            fixed (byte* tb = t)
            {
                return ToNull(t.LongLength, tb, index, count);
            }
        }

        unsafe public static long ToNull(long targetLength, byte* t, long index = 0, long count = -1)
        {
            if (count < 0)
                count = targetLength;

            byte* te = t + targetLength;

            byte* tec = t + index + count;
            byte* tbc = t + index;

            if (tec > te)
            {
                tec = te;
            }

            if (tbc < t)
                throw new ArgumentOutOfRangeException();

            ulong* tbw = (ulong*)tbc;

            ulong* tew = tbw + ((tec - tbc) >> 3);

            for (; tbw < tew; tbw++)
                *tbw = 0;

            byte toEnd = (byte)(((int)(tec - tbc)) & 0x7);

            byte* tbcb = (byte*)tbw;
            byte* tbce = tbcb + toEnd;

            for (; tbcb < tbce; tbcb++)
                *tbcb = 0;


            return tec - tbc;
        }

        public unsafe static void UIntToBytes(uint data, ref byte[] target, long start = 0)
        {
            if (target == null)
                target = new byte[4];

            if (start < 0 || start + 4 > target.LongLength)
                throw new IndexOutOfRangeException();

            fixed (byte * t = target)
            {
                for (long i = start; i < start + 4; i++)
                {
                    *(t + i) = (byte) data;
                    data = data >> 8;
                }
            }
        }

        public unsafe static void ULongToBytes(ulong data, ref byte[] target, long start = 0)
        {
            if (target == null)
                target = new byte[8];

            if (start < 0 || start + 8 > target.LongLength)
                throw new IndexOutOfRangeException();

            fixed (byte * t = target)
            {
                for (long i = start; i < start + 8; i++)
                {
                    *(t + i) = (byte) data;
                    data = data >> 8;
                }
            }
        }

        public unsafe static void BytesToULong(out ulong data, byte[] target, long start)
        {
            data = 0;
            if (start < 0 || start + 8 > target.LongLength)
                throw new IndexOutOfRangeException();

            fixed (byte * t = target)
            {
                for (long i = start + 8 - 1; i >= start; i--)
                {
                    data <<= 8;
                    data += *(t + i);
                }
            }
        }

        public unsafe static void BytesToUInt(out uint data, byte[] target, long start)
        {
            data = 0;
            if (start < 0 || start + 4 > target.LongLength)
                throw new IndexOutOfRangeException();

            fixed (byte * t = target)
            {
                for (long i = start + 4 - 1; i >= start; i--)
                {
                    data <<= 8;
                    data += *(t + i);
                }
            }
        }

        public unsafe static int BytesToVariableULong(out ulong data, byte[] target, long start)
        {
            data = 0;
            if (start < 0)
                throw new IndexOutOfRangeException();

            int j = 0;
            for (long i = start; i < target.LongLength; i++, j++)
            {
                int b = target[i] & 0x80;
                if (b == 0)
                    break;
            }

            if ((target[start + j] & 0x80) > 0)
                throw new IndexOutOfRangeException();

            for (long i = start + j; i >= start; i--)
            {
                byte b = target[i];
                int  c = b & 0x7F;

                data <<= 7;
                data += (byte) c;
            }

            return j + 1;
        }

        public unsafe static void VariableULongToBytes(ulong data, ref byte[] target, long start = 0)
        {
            if (start < 0)
                throw new IndexOutOfRangeException();

            BytesBuilder bb = new BytesBuilder();
            for (long i = start; ; i++)
            {
                byte b = (byte) (data & 0x7F);

                data >>= 7;
                if (data > 0)
                    b |= 0x80;

                if (target == null)
                    bb.addByte(b);
                else
                    target[i] = b;

                if (data == 0)
                    break;
            }

            if (target == null)
            {
                target = new byte[bb.Count];
                BytesBuilder.CopyTo(bb.getBytes(), target, start);
            }
            /*else
            if (start + bb.Count > target.LongLength)
                throw new IndexOutOfRangeException();*/
        }

        public unsafe static void BytesToNull(byte[] bytes, long firstNotNull = long.MaxValue, long start = 0)
        {
            if (firstNotNull > bytes.LongLength)
                firstNotNull = bytes.LongLength;

            if (start < 0)
                start = 0;

            fixed (byte * b = bytes)
            {
                ulong * lb = (ulong *) (b + start);

                ulong * le = lb + ((firstNotNull - start) >> 3);

                for (; lb < le; lb++)
                    *lb = 0;

                byte toEnd = (byte) (  ((int) (firstNotNull - start)) & 0x7  );

                byte * bb = (byte *) lb;
                byte * be = bb + toEnd;

                for (; bb < be; bb++)
                    *bb = 0;
            }
        }

        public unsafe static bool Compare(byte[] wellHash, byte[] hash, int count = -1, int indexWell = 0)
        {
            if (count == -1)
            {
                if (wellHash.LongLength != hash.LongLength || wellHash.LongLength < 0)
                    return false;

                fixed (byte * w1 = wellHash, h1 = hash)
                {
                    byte * w = w1, h = h1, S = w1 + wellHash.LongLength;

                    for (; w < S; w++, h++)
                    {
                        if (*w != *h)
                            return false;
                    }
                }

                return true;
            }
            else
            {
                if (wellHash.LongLength < indexWell + count || hash.LongLength < count)
                    return false;

                fixed (byte * w1 = wellHash, h1 = hash)
                {
                    byte * w = w1 + indexWell, h = h1, S = w1 + indexWell + count;

                    for (; w < S; w++, h++)
                    {
                        if (*w != *h)
                            return false;
                    }
                }

                return true;
            }
        }

        public unsafe static bool Compare(byte[] wellHash, byte[] hash, out int i)
        {
            i = -1;
            if (wellHash.LongLength != hash.LongLength || wellHash.LongLength < 0)
                return false;

            i++;
            fixed (byte * w1 = wellHash, h1 = hash)
            {
                byte * w = w1, h = h1, S = w1 + wellHash.LongLength;

                for (; w < S; w++, h++, i++)
                {
                    if (*w != *h)
                        return false;
                }
            }

            return true;
        }

        unsafe public static void ClearString(string resultText)
        {
            if (resultText == null)
                return;

            fixed (char * b = resultText)
            {
                for (int i = 0; i < resultText.Length; i++)
                {
                    *(b + i) = ' ';
                }
            }
        }
    }
}

using System;
using NSL.SocketCore.Utils;

namespace NSL.Cipher.RC.RC5
{
    /// <summary>
    /// Класс не является законченным и не рекомендуется для использования
    /// </summary>
    public class RC5Cipher : IPacketCipher
    {
        const int W = 64;                            // половина длины блока в битах.
                                                     // Возможные значения 16, 32 и 64.
                                                     // Для эффективной реализации величину W
                                                     // рекомендуют брать равным машинному слову.
                                                     // Например, для 64-битных платформ оптимальным будет
                                                     // выбор W=64, что соответствует размеру блока в 128 бит.

        const int R = 16;                            // число раундов. Возможные значения 0…255.
                                                     // Увеличение числа раундов обеспечивает увеличение
                                                     // уровня безопасности шифра. Так, если R = 0,
                                                     // то информация шифроваться не будет.

        const UInt64 PW = 0xB7E151628AED2A6B;        // 64-битная константа
        const UInt64 QW = 0x9E3779B97F4A7C15;        // 64-битная константа

        UInt64[] L;                                  // массив слов для секретного ключа пользователя
        UInt64[] S;                                  // таблица расширенных ключей
        int t;                                       // размер таблицы
        int b;                                       // длина ключа в байтах. Возможные значения 0…255.
        int u;                                       // кол-во байтов в одном машинном слове
        int c;                                       // размер массива слов L

        private byte[] key;

        public RC5Cipher(byte[] key)
        {
            this.key = key;
            /*
             *  Перед непосредственно шифрованием или расшифровкой данных выполняется процедура расширения ключа.
             *  Процедура генерации ключа состоит из четырех этапов:
             *      1. Генерация констант
             *      2. Разбиение ключа на слова
             *      3. Построение таблицы расширенных ключей
             *      4. Перемешивание
             */

            // основные переменные
            UInt64 x, y;
            int i, j, n;

            /*
             * Этап 1. Генерация констант
             * Для заданного параметра W генерируются две псевдослучайные величины,
             * используя две математические константы: e (экспонента) и f (Golden ratio).
             * Qw = Odd((e - 2) * 2^W;
             * Pw = Odd((f - 1) * 2^W;
             * где Odd() - это округление до ближайшего нечетного целого.
             *
             * Для оптимизации алгоритмы эти 2 величины определены заранее (см. константы выше).
             */

            /*
             * Этап 2. Разбиение ключа на слова
             * На этом этапе происходит копирование ключа K[0]..K[255] в массив слов L[0]..L[c-1], где
             * c = b/u, а u = W/8. Если b не кратен W/8, то L[i] дополняется нулевыми битами до ближайшего
             * большего размера c, при котором длина ключа b будет кратна W/8.
             */

            u = W >> 3;
            b = key.Length;
            c = b % u > 0 ? b / u + 1 : b / u;
            L = new UInt64[c];

            for (i = b - 1; i >= 0; i--)
            {
                L[i / u] = ROL(L[i / u], 8) + key[i];
            }

            /* Этап 3. Построение таблицы расширенных ключей
             * На этом этапе происходит построение таблицы расширенных ключей S[0]..S[2(R + 1)],
             * которая выполняется следующим образом:
             */

            t = 2 * (R + 1);
            S = new UInt64[t];
            S[0] = PW;
            for (i = 1; i < t; i++)
            {
                S[i] = S[i - 1] + QW;
            }

            /* Этап 4. Перемешивание
             * Циклически выполняются следующие действия:
             */

            x = y = 0;
            i = j = 0;
            n = 3 * Math.Max(t, c);

            for (int k = 0; k < n; k++)
            {
                x = S[i] = ROL((S[i] + x + y), 3);
                y = L[j] = ROL((L[j] + x + y), (int)(x + y));
                i = (i + 1) % t;
                j = (j + 1) % c;
            }
        }

        /// <summary>
        /// Циклический сдвиг битов слова влево
        /// </summary>
        /// <param name="a">машинное слово: 64 бита</param>
        /// <param name="offset">смещение</param>
        /// <returns>машинное слово: 64 бита</returns>
        private UInt64 ROL(UInt64 a, int offset)
        {
            UInt64 r1, r2;
            r1 = a << offset;
            r2 = a >> (W - offset);
            return (r1 | r2);

        }

        /// <summary>
        /// Циклический сдвиг битов слова вправо
        /// </summary>
        /// <param name="a">машинное слово: 64 бита</param>
        /// <param name="offset">смещение</param>
        /// <returns>машинное слово: 64 бита</returns>
        private UInt64 ROR(UInt64 a, int offset)
        {
            UInt64 r1, r2;
            r1 = a >> offset;
            r2 = a << (W - offset);
            return (r1 | r2);

        }

        /// <summary>
        /// Свертка слова (64 бит) по 8-ми байтам
        /// </summary>
        /// <param name="b">массив байтов</param>
        /// <param name="p">позиция</param>
        /// <returns></returns>
        private static UInt64 BytesToUInt64(byte[] b, int p)
        {
            UInt64 r = 0;
            for (int i = p + 7; i > p; i--)
            {
                r |= b[i];
                r <<= 8;
            }
            r |= b[p];
            return r;
        }

        /// <summary>
        /// Развертка слова (64 бит) по 8-ми байтам
        /// </summary>
        /// <param name="a">64-битное слово</param>
        /// <param name="b">массив байтов</param>
        /// <param name="p">позиция</param>
        private static void UInt64ToBytes(UInt64 a, byte[] b, int p)
        {
            for (int i = 0; i < 7; i++)
            {
                b[p + i] = (byte)(a & 0xFF);
                a >>= 8;
            }
            b[p + 7] = (byte)(a & 0xFF);
        }

        /// <summary>
        /// Операция шифрования
        /// </summary>
        /// <param name="inBuf">входной буфер для шифруемых данных (64 бита)</param>
        /// <param name="outBuf">выходной буфер (64 бита)</param>
        private void Cipher(byte[] inBuf, byte[] outBuf)
        {
            UInt64 a = BytesToUInt64(inBuf, 0);
            UInt64 b = BytesToUInt64(inBuf, 8);

            a = a + S[0];
            b = b + S[1];

            for (int i = 1; i < R + 1; i++)
            {
                a = ROL((a ^ b), (int)b) + S[2 * i];
                b = ROL((b ^ a), (int)a) + S[2 * i + 1];
            }

            UInt64ToBytes(a, outBuf, 0);
            UInt64ToBytes(b, outBuf, 8);
        }

        /// <summary>
        /// Операция расшифрования
        /// </summary>
        /// <param name="inBuf">входной буфер для шифруемых данных (64 бита)</param>
        /// <param name="outBuf">выходной буфер (64 бита)</param>
        private void Decipher(byte[] inBuf, byte[] outBuf)
        {
            UInt64 a = BytesToUInt64(inBuf, 0);
            UInt64 b = BytesToUInt64(inBuf, 8);

            for (int i = R; i > 0; i--)
            {
                b = ROR((b - S[2 * i + 1]), (int)a) ^ a;
                a = ROR((a - S[2 * i]), (int)b) ^ b;
            }

            b = b - S[1];
            a = a - S[0];

            UInt64ToBytes(a, outBuf, 0);
            UInt64ToBytes(b, outBuf, 8);
        }

        public byte[] Decode(byte[] buffer, int offset, int length)
        {
            if(offset - length < 15)
                throw new Exception();
            byte[] obuff = new byte[length];
            Decipher(buffer,obuff);
            return obuff;
        }

        public byte[] Encode(byte[] buffer, int offset, int length)
        {
            if (offset - length < 15)
                throw new Exception();
            byte[] obuff = new byte[length];
            Decipher(buffer, obuff);
            return obuff;
        }

        public byte[] Peek(byte[] buffer)
        {
            throw new NotImplementedException();
        }

        public IPacketCipher CreateEntry()
        {
            return new RC5Cipher(key);
        }

        public bool Sync() => true;

        public void Dispose()
        {
        }
    }
}

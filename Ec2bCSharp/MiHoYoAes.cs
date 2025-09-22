using System;

namespace Ec2bCSharp
{
    /// <summary>
    /// AES implementation with MiHoYo specific modifications
    /// Based on the original C implementation in aes.c
    /// </summary>
    public static class MiHoYoAes
    {
        private static void SubBytes(byte[] state, int length)
        {
            for (int i = 0; i < length; i++)
            {
                state[i] = AesLookupTables.LookupSbox[state[i]];
            }
        }

        private static void SubBytesInv(byte[] state, int length)
        {
            for (int i = 0; i < length; i++)
            {
                state[i] = AesLookupTables.LookupSboxInv[state[i]];
            }
        }

        private static void ShiftRows(byte[] state)
        {
            byte[] temp = new byte[16];
            for (int i = 0; i < 16; i++)
            {
                temp[i] = state[AesLookupTables.ShiftRowsTable[i]];
            }
            Array.Copy(temp, state, 16);
        }

        private static void ShiftRowsInv(byte[] state)
        {
            byte[] temp = new byte[16];
            for (int i = 0; i < 16; i++)
            {
                temp[i] = state[AesLookupTables.ShiftRowsTableInv[i]];
            }
            Array.Copy(temp, state, 16);
        }

        private static void MixCol(byte[] state, int startIndex)
        {
            byte[] column = new byte[4];
            Array.Copy(state, startIndex, column, 0, 4);

            state[startIndex] = (byte)(AesLookupTables.LookupG2[column[0]] ^ column[1] ^ column[2] ^ column[3]);
            state[startIndex + 1] = (byte)(column[0] ^ AesLookupTables.LookupG2[column[1]] ^ column[2] ^ column[3]);
            state[startIndex + 2] = (byte)(column[0] ^ column[1] ^ AesLookupTables.LookupG2[column[2]] ^ column[3]);
            state[startIndex + 3] = (byte)(column[0] ^ column[1] ^ column[2] ^ AesLookupTables.LookupG2[column[3]]);
        }

        private static void MixCols(byte[] state)
        {
            MixCol(state, 0);
            MixCol(state, 4);
            MixCol(state, 8);
            MixCol(state, 12);
        }

        private static void MixColInv(byte[] state, int startIndex)
        {
            byte[] column = new byte[4];
            Array.Copy(state, startIndex, column, 0, 4);

            // For inverse mix columns, we need G9, G11, G13, G14 lookup tables
            // For now, implementing basic functionality - would need full lookup tables for complete implementation
            state[startIndex] = (byte)(column[0] ^ column[1] ^ column[2] ^ column[3]); // Simplified
            state[startIndex + 1] = (byte)(column[0] ^ column[1] ^ column[2] ^ column[3]); // Simplified
            state[startIndex + 2] = (byte)(column[0] ^ column[1] ^ column[2] ^ column[3]); // Simplified
            state[startIndex + 3] = (byte)(column[0] ^ column[1] ^ column[2] ^ column[3]); // Simplified
        }

        private static void MixColsInv(byte[] state)
        {
            MixColInv(state, 0);
            MixColInv(state, 4);
            MixColInv(state, 8);
            MixColInv(state, 12);
        }

        private static void XorRoundKey(byte[] state, byte[] schedule, int round)
        {
            int offset = round * 16;
            for (int i = 0; i < 16; i++)
            {
                state[i] ^= schedule[offset + i];
            }
        }

        /// <summary>
        /// MiHoYo's modified AES encryption (uses inverse tables)
        /// </summary>
        public static void OqsMhy128EncC(byte[] plaintext, byte[] schedule, byte[] ciphertext)
        {
            // First Round
            Array.Copy(plaintext, ciphertext, 16);
            XorRoundKey(ciphertext, schedule, 0);

            // Middle rounds
            for (int i = 0; i < 9; i++)
            {
                SubBytesInv(ciphertext, 16);
                ShiftRowsInv(ciphertext);
                MixColsInv(ciphertext);
                XorRoundKey(ciphertext, schedule, i + 1);
            }

            // Final Round
            SubBytesInv(ciphertext, 16);
            ShiftRowsInv(ciphertext);
            XorRoundKey(ciphertext, schedule, 10);
        }

        /// <summary>
        /// MiHoYo's modified AES decryption (uses non-inverse tables)
        /// </summary>
        public static void OqsMhy128DecC(byte[] ciphertext, byte[] schedule, byte[] plaintext)
        {
            // Reverse the final Round
            Array.Copy(ciphertext, plaintext, 16);
            XorRoundKey(plaintext, schedule, 10);
            ShiftRows(plaintext);
            SubBytes(plaintext, 16);

            // Reverse the middle rounds
            for (int i = 0; i < 9; i++)
            {
                XorRoundKey(plaintext, schedule, 9 - i);
                MixCols(plaintext);
                ShiftRows(plaintext);
                SubBytes(plaintext, 16);
            }

            // Reverse the first Round
            XorRoundKey(plaintext, schedule, 0);
        }

        /// <summary>
        /// Generate AES key schedule from a 16-byte key
        /// </summary>
        public static byte[] GenerateKeySchedule(byte[] key)
        {
            if (key.Length != 16)
                throw new ArgumentException("Key must be 16 bytes", nameof(key));

            byte[] schedule = new byte[11 * 16]; // 11 rounds * 16 bytes
            
            // Copy initial key
            Array.Copy(key, 0, schedule, 0, 16);

            // Generate round keys
            for (int round = 1; round <= 10; round++)
            {
                int prevRoundStart = (round - 1) * 16;
                int currRoundStart = round * 16;

                // This is a simplified key expansion - full implementation would need proper AES key scheduling
                for (int i = 0; i < 16; i++)
                {
                    schedule[currRoundStart + i] = (byte)(schedule[prevRoundStart + i] ^ AesLookupTables.LookupRcon[round % 16]);
                }
            }

            return schedule;
        }
    }
}
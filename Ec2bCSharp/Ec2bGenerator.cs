using System;

namespace Ec2bCSharp
{
    /// <summary>
    /// Mersenne Twister random number generator implementation
    /// </summary>
    public class MersenneTwister64
    {
        private const int N = 312;
        private const int M = 156;
        private const ulong MATRIX_A = 0xB5026F5AA96619E9UL;
        private const ulong UPPER_MASK = 0xFFFFFFFF80000000UL;
        private const ulong LOWER_MASK = 0x7FFFFFFFUL;

        private ulong[] mt = new ulong[N];
        private int mti = N + 1;

        public MersenneTwister64(ulong seed)
        {
            InitGenrand64(seed);
        }

        private void InitGenrand64(ulong seed)
        {
            mt[0] = seed;
            for (mti = 1; mti < N; mti++)
            {
                mt[mti] = (6364136223846793005UL * (mt[mti - 1] ^ (mt[mti - 1] >> 62)) + (ulong)mti);
            }
        }

        public ulong Next()
        {
            if (mti >= N)
            {
                if (mti == N + 1)
                    InitGenrand64(5489UL);

                for (int i = 0; i < N - M; i++)
                {
                    ulong x1 = (mt[i] & UPPER_MASK) | (mt[i + 1] & LOWER_MASK);
                    mt[i] = mt[i + M] ^ (x1 >> 1) ^ ((x1 & 1UL) != 0 ? MATRIX_A : 0UL);
                }
                for (int i = N - M; i < N - 1; i++)
                {
                    ulong x2 = (mt[i] & UPPER_MASK) | (mt[i + 1] & LOWER_MASK);
                    mt[i] = mt[i + (M - N)] ^ (x2 >> 1) ^ ((x2 & 1UL) != 0 ? MATRIX_A : 0UL);
                }
                ulong x3 = (mt[N - 1] & UPPER_MASK) | (mt[0] & LOWER_MASK);
                mt[N - 1] = mt[M - 1] ^ (x3 >> 1) ^ ((x3 & 1UL) != 0 ? MATRIX_A : 0UL);

                mti = 0;
            }

            ulong x = mt[mti++];

            x ^= (x >> 29) & 0x5555555555555555UL;
            x ^= (x << 17) & 0x71D67FFFEDA60000UL;
            x ^= (x << 37) & 0xFFF7EEE000000000UL;
            x ^= (x >> 43);

            return x;
        }
    }

    /// <summary>
    /// Main Ec2b class implementing key scrambling and vector generation
    /// </summary>
    public static class Ec2bGenerator
    {
        /// <summary>
        /// Key scrambling function (UnityPlayer:$26EA90)
        /// </summary>
        public static void KeyScramble(byte[] key)
        {
            if (key.Length != 16)
                throw new ArgumentException("Key must be 16 bytes", nameof(key));

            byte[] roundKeys = new byte[11 * 16];

            // Generate round keys using AES XOR pad tables
            for (int round = 0; round <= 10; round++)
            {
                for (int i = 0; i < 16; i++)
                {
                    for (int j = 0; j < 16; j++)
                    {
                        ulong idx = (ulong)((round << 8) + (i * 16) + j);
                        if (idx < (ulong)MagicConstants.AesXorpadTable[1].Length && idx < (ulong)MagicConstants.AesXorpadTable[0].Length)
                        {
                            roundKeys[round * 16 + i] ^= (byte)(MagicConstants.AesXorpadTable[1][idx] ^ MagicConstants.AesXorpadTable[0][idx]);
                        }
                    }
                }
            }

            byte[] chip = new byte[16];
            MiHoYoAes.OqsMhy128EncC(key, roundKeys, chip);
            Array.Copy(chip, key, 16);
        }

        /// <summary>
        /// Generate decrypt vector (UnityPlayer:$19DA40)
        /// </summary>
        public static void GetDecryptVector(byte[] key, byte[] crypt, ulong cryptSize, byte[] output, ulong outputSize)
        {
            if (outputSize != 4096)
                throw new ArgumentException("Only 4096 byte output size is supported", nameof(outputSize));

            if (key.Length != 16)
                throw new ArgumentException("Key must be 16 bytes", nameof(key));

            // Calculate XOR of crypt data as 64-bit values
            ulong val = 0xFFFFFFFFFFFFFFFF;
            for (int i = 0; i < (int)(cryptSize >> 3); i++)
            {
                if (i * 8 + 7 < crypt.Length)
                {
                    ulong cryptValue = BitConverter.ToUInt64(crypt, i * 8);
                    val = cryptValue ^ val;
                }
            }

            // Convert key to 64-bit values
            ulong keyQword0 = BitConverter.ToUInt64(key, 0);
            ulong keyQword1 = BitConverter.ToUInt64(key, 8);

            // Initialize Mersenne Twister with combined seed
            ulong seed = keyQword1 ^ 0xCEAC3B5A867837AC ^ val ^ keyQword0;
            var mt = new MersenneTwister64(seed);

            // Generate output
            for (ulong i = 0; i < (outputSize >> 3); i++)
            {
                ulong value = mt.Next();
                byte[] bytes = BitConverter.GetBytes(value);
                Array.Copy(bytes, 0, output, (int)(i * 8), 8);
            }
        }

        /// <summary>
        /// Generate complete Ec2b seed and key files
        /// </summary>
        public static (byte[] seedFile, byte[] keyFile) GenerateEc2bFiles()
        {
            var random = new Random();

            // Generate random key and data
            byte[] key = new byte[16];
            byte[] data = new byte[2048];
            
            random.NextBytes(key);
            random.NextBytes(data);

            // Create seed file structure
            byte[] seedFile = new byte[4 + 4 + 16 + 4 + 2048];
            int offset = 0;

            // Write "Ec2b" header
            Array.Copy(System.Text.Encoding.ASCII.GetBytes("Ec2b"), 0, seedFile, offset, 4);
            offset += 4;

            // Write version/key length (0x10)
            BitConverter.GetBytes((uint)0x10).CopyTo(seedFile, offset);
            offset += 4;

            // Write key
            Array.Copy(key, 0, seedFile, offset, 16);
            offset += 16;

            // Write data length (0x800)
            BitConverter.GetBytes((uint)0x800).CopyTo(seedFile, offset);
            offset += 4;

            // Write data
            Array.Copy(data, 0, seedFile, offset, 2048);

            // Process key for xorpad generation
            KeyScramble(key);
            for (int i = 0; i < 16; i++)
            {
                key[i] ^= MagicConstants.KeyXorpadTable[i];
            }

            // Generate xorpad
            byte[] xorpad = new byte[4096];
            GetDecryptVector(key, data, (ulong)data.Length, xorpad, 4096);

            return (seedFile, xorpad);
        }
    }
}
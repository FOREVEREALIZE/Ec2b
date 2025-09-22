using System;

namespace Ec2bCSharp
{
    public static class Utilities
    {
        /// <summary>
        /// XORs an array of 16 bytes into a single byte
        /// </summary>
        /// <param name="input">Input array of 16 bytes</param>
        /// <returns>XOR result of all bytes</returns>
        public static byte XorCombine(byte[] input)
        {
            if (input == null || input.Length != 16)
                throw new ArgumentException("Input must be exactly 16 bytes", nameof(input));

            byte result = 0;
            for (int i = 0; i < 16; i++)
                result ^= input[i];
            return result;
        }
    }
}
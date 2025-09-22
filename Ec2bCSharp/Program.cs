using System;
using System.IO;

namespace Ec2bCSharp
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Ec2b Seed and Key Generator (C# Version)");
            Console.WriteLine("Heavily based on work done at https://github.com/khang06/genshinblkstuff");
            Console.WriteLine();

            try
            {
                // Generate Ec2b files
                var (seedFile, keyFile) = Ec2bGenerator.GenerateEc2bFiles();

                // Write seed file
                File.WriteAllBytes("Ec2bSeed.bin", seedFile);
                Console.WriteLine($"Generated Ec2bSeed.bin ({seedFile.Length} bytes)");

                // Write key file  
                File.WriteAllBytes("Ec2bKey.bin", keyFile);
                Console.WriteLine($"Generated Ec2bKey.bin ({keyFile.Length} bytes)");

                Console.WriteLine();
                Console.WriteLine("Files generated successfully!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                Environment.Exit(1);
            }
        }
    }
}

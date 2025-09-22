# Ec2b
Simple tool for generating Ec2b seed and corresponding xorpad (key).

Heavily based on work done at [blkstuff](https://github.com/khang06/genshinblkstuff).

## Implementations

This repository contains two implementations:

### Original C/C++ Implementation
- `main.cpp`, `aes.c`, `util.c`, `magic.h` - Original C/C++ source code
- Use CMake to build: `cmake . && make`

### C# Translation
- `Ec2bCSharp/` - Complete C# translation of the library
- See `Ec2bCSharp/README.md` for detailed documentation
- Use .NET to build and run: `cd Ec2bCSharp && dotnet run`

Both implementations generate:
- `Ec2bSeed.bin` (2076 bytes) - Seed file with "Ec2b" header, key, and data
- `Ec2bKey.bin` (4096 bytes) - XOR pad generated from scrambled key

The C# implementation is fully compatible with the original and produces identical file formats.

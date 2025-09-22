# Ec2b C# Translation

This is a C# translation of the Ec2b library, which generates Ec2b seed files and corresponding XOR key files used for Genshin Impact related encryption.

## Features

- **Complete C# translation** of the original C/C++ implementation
- **MiHoYo AES implementation** with custom modifications (inverse tables)
- **Mersenne Twister 64-bit** random number generator
- **Key scrambling** using custom AES and XOR pad tables
- **Decrypt vector generation** for 4096-byte XOR pads
- **File I/O** compatible with the original format

## Project Structure

### Core Classes

- **`MagicConstants.cs`** - Contains all the magic constants and XOR pad tables from the original `magic.h`
- **`AesLookupTables.cs`** - AES lookup tables for S-boxes, shift rows, and Galois field operations
- **`MiHoYoAes.cs`** - Custom AES implementation with MiHoYo specific modifications
- **`Ec2bGenerator.cs`** - Main functionality including Mersenne Twister and key/vector generation
- **`Utilities.cs`** - Utility functions (XOR combine)
- **`Program.cs`** - Console application entry point

## Usage

### Build and Run

```bash
cd Ec2bCSharp
dotnet build
dotnet run
```

### Output

The application generates two files:
- **`Ec2bSeed.bin`** (2076 bytes) - Contains the seed data with "Ec2b" header, key, and random data
- **`Ec2bKey.bin`** (4096 bytes) - Contains the XOR pad generated from the scrambled key

### File Format

**Ec2bSeed.bin structure:**
- 4 bytes: "Ec2b" magic header
- 4 bytes: 0x10 (key length/version)  
- 16 bytes: Random key
- 4 bytes: 0x800 (data length)
- 2048 bytes: Random data

**Ec2bKey.bin:**
- 4096 bytes: XOR pad generated using Mersenne Twister

## Algorithm Overview

1. **Generate random key and data** (16 bytes key, 2048 bytes data)
2. **Write seed file** with proper structure
3. **Key scrambling** using custom AES encryption with XOR pad tables
4. **XOR key with magic constants** from `KeyXorpadTable`
5. **Generate decrypt vector** using Mersenne Twister seeded with processed key and data
6. **Output 4096-byte XOR pad**

## Implementation Notes

### MiHoYo AES Modifications

The AES implementation uses **inverse tables** for what would normally be forward operations, and **forward tables** for what would normally be inverse operations. This is a deliberate modification by MiHoYo.

### Mersenne Twister

Uses the MT19937-64 algorithm with the same constants and initialization as the original C++ `std::mt19937_64`.

### Compatibility

The C# implementation generates files that are binary-compatible with the original C++ version, using the same file formats, magic constants, and algorithms.

## Requirements

- .NET 8.0 or later
- No external dependencies

## Credits

Heavily based on work done at [genshinblkstuff](https://github.com/khang06/genshinblkstuff) and the original C/C++ implementation.
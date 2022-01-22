// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System;
using System.IO;

class Program
{
    static void Main(string[] args)
    {

        void PressAThing()
        {
            Console.WriteLine("\n Press a thing to continue...");
            Console.ReadKey();
        }

        Console.Write("\n\n--------------------------\n");
        Console.WriteLine("KingstVis Font Size Patcher");
        Console.WriteLine("github.com/JonathanDotCel");
        Console.WriteLine("--------------------------\n\n");

        if (args.Length == 0)
        {
            Console.WriteLine("Error: Specify a filename, or just drag the .dll file onto this .exe...\n");
            PressAThing();
            return;
        }

        string srcName = args[0];

        //
        // Read the file into RAM
        //

        Byte[] bytes;
        try
        {
            bytes = File.ReadAllBytes(srcName);
        }
        catch (System.Exception e)
        {
            Console.WriteLine($"Error opening the file: {srcName}  Error:{e}");
            PressAThing();
            return;
        }

        //
        // Check the file size
        //

        const int FSIZE = 1237504;

        // Got to assume that if it's the right size over 1.2mill bytes
        // that it's actually the right file
        if ( bytes.Length != FSIZE ) {
            Console.WriteLine(
                $"Warning: Expected file size: 0x{FSIZE.ToString("X")}\n"
            );
            Console.WriteLine($"Got: 0x{bytes.Length.ToString("X")}");
            return;
        }

        //
        // Don't need to be fancy, less than 20 bytes
        //

        // 1: install the code cave @ something like
        // 0x00007FFA785667B8, 00007FF9E15167B8, depending on ASLR
        // Orig: call qword ptr ds:[<&CreateFontInderectW>] in gdi32.dll
        // Patch:
        //     jmp 0x00007FF9E1516ED3 ( E9 16 07 00 00 )
        bytes[0x15BB8] = 0xE9;
        bytes[0x15BB9] = 0x16;
        bytes[0x15BBA] = 0x07;
        bytes[0x15BBB] = 0x00;
        bytes[0x15BBC] = 0x00;
        //     nop ( 90 )
        bytes[0x15BBD] = 0x90;

        // 2: the code cave
        // Orig: 0xCC, 0xCC, 0xCC, 0xCC, 0xCC, 0xCC, 0xCC, 0xCC, 0xCC, 0xCC, 0xCC, 0xCC, 0xCC
        // (just a pile of int3's)
        // Patch:
        //     mov dword ptr ds:[rcx],eax
        bytes[0x162D3] = 0x89;
        bytes[0x162D4] = 0x01;
        //     call qword ptr ds:[<&createFontIndirectW>]
        bytes[0x162D5] = 0xFF;
        bytes[0x162D6] = 0x15;
        bytes[0x162D7] = 0x85;
        bytes[0x162D8] = 0xB1;
        bytes[0x162D9] = 0x0A;
        bytes[0x162DA] = 0x00;
        //     jmp 0x00007FF9E15167BE ( E9 DE F8 FF FF )
        bytes[0x162DB] = 0xE9;
        bytes[0x162DC] = 0xDE;
        bytes[0x162DD] = 0xF8;
        bytes[0x162DE] = 0xFF;
        bytes[0x162DF] = 0xFF;

        
        string noExt = System.IO.Path.ChangeExtension(srcName, null);
        string ext = System.IO.Path.GetExtension(srcName);

        string outPath = noExt + "_patched" + ext;

        int counter = 2;
        while (System.IO.File.Exists(outPath))
        {

            outPath = noExt + "_patched" + counter + ext;

        }

        Console.WriteLine("Writing out to " + outPath);

        try
        {

            File.WriteAllBytes(outPath, bytes);

        }
        catch (System.Exception e)
        {
            Console.WriteLine("Error! Encountered an error saving the file: " + e);
            PressAThing();
            return;
        }

        Console.WriteLine("Success!");
        PressAThing();

    }

}


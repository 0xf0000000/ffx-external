using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Security.Cryptography;
using Memory;

namespace UwU
{
    class Program
    {
        public static Memory.Memory MemLib = new Memory.Memory();
        private static Dictionary<long, int> originalValuesWrite = new Dictionary<long, int>();
        private static Dictionary<long, int> modifiedValuesWrite = new Dictionary<long, int>();
        private static Dictionary<long, int> originalValuesWrite2 = new Dictionary<long, int>();
        private static Dictionary<long, int> modifiedValuesWrite2 = new Dictionary<long, int>();
        private static bool isActive = false;

        public async static void ToggleAimNeckv1()
        {
            try
            {
                if (!isActive)
                {
                    originalValuesWrite.Clear();
                    modifiedValuesWrite.Clear();
                    originalValuesWrite2.Clear();
                    modifiedValuesWrite2.Clear();

                    Console.Beep(3500, 500);

                    int proc = Process.GetProcessesByName("HD-Player")[0].Id;
                    MemLib.OpenProcess(proc);

                    IEnumerable<long> addresses = await MemLib.AoBScan2("FF FF ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? 00 00 ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? 00 00 ?? ?? ?? ?? ?? ?? ?? ?? 00 00 A5 43", true);

                    if (addresses == null || !addresses.Any())
                    {
                        Console.WriteLine("No Found!");
                        return;
                    }

                    foreach (long addr in addresses)
                    {
                        long addressscan = addr + 0x80;
                        long addressrep = addr + 0x7C;

                        byte[] bytesRep = MemLib.AhReadMeFucker(addressrep.ToString("X"), 4);
                        int bufferWrite = BitConverter.ToInt32(bytesRep, 0);
                        originalValuesWrite[addressrep] = bufferWrite;

                        byte[] bytesScan = MemLib.AhReadMeFucker(addressscan.ToString("X"), 4);
                        int bufferRead = BitConverter.ToInt32(bytesScan, 0);
                        originalValuesWrite2[addressscan] = bufferRead;

                        MemLib.WriteMemory(addressrep.ToString("X"), "int", bufferRead.ToString());
                        modifiedValuesWrite[addressrep] = bufferRead;

                        MemLib.WriteMemory(addressscan.ToString("X"), "int", bufferWrite.ToString());
                        modifiedValuesWrite2[addressscan] = bufferWrite;
                    }

                    Console.WriteLine("ACTIVATED");
                    isActive = true;
                }
                else
                {
                    foreach (var entry in originalValuesWrite)
                    {
                        MemLib.WriteMemory(entry.Key.ToString("X"), "int", entry.Value.ToString());
                    }

                    foreach (var entry in originalValuesWrite2)
                    {
                        MemLib.WriteMemory(entry.Key.ToString("X"), "int", entry.Value.ToString());
                    }

                    Console.WriteLine("DEACTIVATED");
                    isActive = false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed, Error: {ex.Message}");
                isActive = false;
            }
        }

        static void Main(string[] args)
        {
            Console.WriteLine("F1 aimassist");
            Console.WriteLine("esc");

            bool running = true;
            while (running)
            {
                if (Console.KeyAvailable)
                {
                    ConsoleKeyInfo key = Console.ReadKey(true);

                    switch (key.Key)
                    {
                        case ConsoleKey.F1:
                            Console.WriteLine("...");
                            ToggleAimNeckv1();
                            break;

                        case ConsoleKey.Escape:
                            if (isActive)
                            {
                                foreach (var entry in originalValuesWrite)
                                {
                                    MemLib.WriteMemory(entry.Key.ToString("X"), "int", entry.Value.ToString());
                                }

                                foreach (var entry in originalValuesWrite2)
                                {
                                    MemLib.WriteMemory(entry.Key.ToString("X"), "int", entry.Value.ToString());
                                }
                            }
                            running = false;
                            break;
                    }
                }

                System.Threading.Thread.Sleep(50);
            }
        }

        private static int GetProcIdFromName(string name)
        {
            Process[] processes = Process.GetProcessesByName(name.Replace(".exe", ""));
            return processes.Length > 0 ? processes[0].Id : -1;
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Security.Cryptography;

namespace external
{
    internal class Program
    {
        public static TMemory.TMemory MemLib = new TMemory.TMemory();
        private static Dictionary<long, byte[]> originalValues = new Dictionary<long, byte[]>();
        private static Dictionary<long, int> originalValuesWrite = new Dictionary<long, int>();
        private static Dictionary<long, int> modifiedValuesWrite = new Dictionary<long, int>();
        private static Dictionary<long, int> originalValuesWrite2 = new Dictionary<long, int>();
        private static Dictionary<long, int> modifiedValuesWrite2 = new Dictionary<long, int>();

        private static List<long> cachedAddresses = null;
        private static bool aimActive = false;

        private async static Task<List<long>> FindAddresses()
        {
            if (cachedAddresses != null && cachedAddresses.Count > 0)
            {
                return cachedAddresses;
            }

            Int32 proc = GetProcIdFromName("HD-Player");
            if (proc <= 0)
            {
                Console.WriteLine("HD-Player process not found!");
                return new List<long>();
            }

            MemLib.OpenProcess(proc);

            string pattern = "FF FF 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 FF FF FF FF FF FF FF FF 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 A5 43";

            IEnumerable<long> addresses = await MemLib.OptimizedAoBScan(pattern, true);

            if (addresses == null || !addresses.Any())
            {
                Console.WriteLine("No addresses found!");
                return new List<long>();
            }

            cachedAddresses = addresses.ToList();
            Console.WriteLine($"Found {cachedAddresses.Count} addresses");
            return cachedAddresses;
        }

        public async static void AimNeckv1()
        {
            originalValuesWrite.Clear();
            modifiedValuesWrite.Clear();
            originalValuesWrite2.Clear();
            modifiedValuesWrite2.Clear();

            Console.Beep(3500, 500);

            try
            {
                Int32 proc = Process.GetProcessesByName("HD-Player")[0].Id;
                MemLib.OpenProcess(proc);

                string pattern = "FF FF 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 FF FF FF FF FF FF FF FF 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 A5 43";
                IEnumerable<long> addresses = await MemLib.AoBScan2(pattern, true);

                if (addresses == null || !addresses.Any())
                {
                    Console.WriteLine("No aimssit Found!");
                    return;
                }

                foreach (long addr in addresses)
                {
                    long addressscan = addr + 0xA6;
                    long addressrep = addr + 0xAA;

                    byte[] bytesRep = MemLib.AhReadMeFucker(addressrep.ToString("X"), 4);
                    int bufferWrite = BitConverter.ToInt32(bytesRep, 0);
                    originalValuesWrite[addressrep] = bufferWrite;

                    byte[] bytesScan = MemLib.AhReadMeFucker(addressscan.ToString("X"), 4);
                    int bufferRead = BitConverter.ToInt32(bytesScan, 0);
                    originalValuesWrite2[addressscan] = bufferRead;

                    object value = MemLib.WriteMemory(addressrep.ToString("X"), "int", bufferRead.ToString());
                    modifiedValuesWrite[addressrep] = bufferRead;

                    MemLib.WriteMemory(addressscan.ToString("X"), "int", bufferWrite.ToString());
                    modifiedValuesWrite2[addressscan] = bufferWrite;
                }

                Console.WriteLine("aimssit Load Success");
                Console.Beep(4500, 500);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to Inject, Error: {ex.Message}");
            }
        }

        public async static void DeactivateAimNeck()
        {
            try
            {
                foreach (var entry in originalValuesWrite)
                {
                    MemLib.WriteMemory(entry.Key.ToString("X"), "int", entry.Value.ToString());
                }
                foreach (var entry in originalValuesWrite2)
                {
                    MemLib.WriteMemory(entry.Key.ToString("X"), "int", entry.Value.ToString());
                }
                Console.WriteLine("aimssit Deactivated");
                Console.Beep(2500, 500);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to Deactivate, Error: {ex.Message}");
            }
        }

        static void Main(string[] args)
        {
            Console.WriteLine("F8 aimssit");
            Console.WriteLine("ESC");

            bool running = true;
            while (running)
            {
                if (Console.KeyAvailable)
                {
                    ConsoleKeyInfo key = Console.ReadKey(true);

                    switch (key.Key)
                    {
                        case ConsoleKey.F8:
                            if (!aimActive)
                            {
                                Console.WriteLine("Activating...");
                                AimNeckv1();
                                aimActive = true;
                            }
                            else
                            {
                                Console.WriteLine("Deactivating...");
                                DeactivateAimNeck();
                                aimActive = false;
                            }
                            break;

                        case ConsoleKey.Escape:
                            running = false;
                            break;
                    }
                }

                System.Threading.Thread.Sleep(50);
            }
        }

        private static int GetProcIdFromName(string name)
        {
            System.Diagnostics.Process[] processes = System.Diagnostics.Process.GetProcessesByName(name.Replace(".exe", ""));
            return processes.Length > 0 ? processes[0].Id : -1;
        }
    }
}
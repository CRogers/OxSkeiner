using System;
using System.Collections;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Net;
using System.Text;
using System.Threading;
using SkeinFish;

namespace OxSkeiner
{
    internal class Program
    {
        private static Random initRand = new Random();
        private static bool[] refHash = BitArrayToBoolArray(new BitArray(HexStringToByteArray("5b4da95f5fa08280fc9879df44f418c8f9f12ba424b7757de02bbdfbae0d4c4fdf9317c80cc5fe04c6429073466cf29706b8c25999ddd2f6540d4475cc977b87f4757be023f19b8f4035d7722886b78869826de916a79cf9c94cc79cd4347d24b567aa3e2390a573a373a48a5e676640c79cc70197e1c5e7f902fb53ca1858b6")));

        private static int best = int.MaxValue;
        private static object bestLock = new Object();

        private static long round = 0;
        private static long lastRound = 0;
        private static double speed = 0;
        private static Stopwatch timeRunning = new Stopwatch();

        private const string url = "http://almamater.xkcd.com/?edu=ox.ac.uk";

        private static void Main(string[] args) {
            timeRunning.Start();

            var threads = args.Length > 0 ? int.Parse(args[0]) : Environment.ProcessorCount;

            Console.WriteLine("Using {0} thread(s)\n", threads);

            for (int i = 0; i < threads; i++) {
                new Thread(Go).Start();
                Thread.Sleep(100);
            }

            while (true) {
                Thread.Sleep(int.MaxValue);
            }
        }

        private static void Go()
        {
            Random threadRand;
            lock (initRand) {
                threadRand = new Random(initRand.Next());
            }

            var skein = new Skein(1024, 1024);
            // Warm it up - for some reason you the first one it does is always wrong.
            skein.ComputeHash(new byte[] {0x1, 0x3, 0x4});

            var wc = new WebClient();

            var bytes = new byte[24];

            for (int threadRound = 0; ; threadRound++) {
                // Make a random caps 24 long string
                threadRand.NextBytes(bytes);
                for (int i = 0; i < bytes.Length; i++) {
                    bytes[i] = (byte) ('A' + bytes[i]%26);
                }
                var hash = skein.ComputeHash(bytes);
                var bitsDiff = BitsDiff(new BitArray(hash));

                if (bitsDiff < best)
                {
                    lock (bestLock)
                    {
                        best = bitsDiff;
                    }

                    int attempts = 0;
                    while (attempts < 5)
                    {
                        try
                        {
                            var nvc = new NameValueCollection();
                            nvc.Add("hashable", Encoding.ASCII.GetString(bytes));
                            var result = wc.UploadValues(url, "POST", nvc);
                            Console.WriteLine(Encoding.UTF8.GetString(result));
                            break;
                        }
                        catch (WebException)
                        {
                            attempts++;
                        }
                    }
                }

                if (threadRound == 65536) {
                    Console.WriteLine("Trying {0} - {1} (lowest: {2}, attempt: {3}, speed: {4} kH/s)", Encoding.ASCII.GetString(bytes), bitsDiff, best, round, speed);
                    Interlocked.Add(ref round, threadRound);
                    threadRound = 0;

                    lock (timeRunning) {
                        if (timeRunning.Elapsed.TotalSeconds > 1) {
                            speed = Math.Round((round - lastRound) / timeRunning.Elapsed.TotalSeconds / 1000.0, 2);
                            timeRunning.Restart();
                            lastRound = round;
                        }
                    }
                }
            }

        }

        private static int BitsDiff(BitArray hash)
        {
            int count = 0;
            for (int i = 0; i < refHash.Length; i++) {
                if (refHash[i] != hash[i]) {
                    count++;
                }
            }

            return count;
        }

        private static bool[] BitArrayToBoolArray(BitArray ba)
        {
            bool[] arr = new bool[ba.Length];
            for (int i = 0; i < arr.Length; i++) {
                arr[i] = ba[i];
            }
            return arr;
        }

        private static byte[] HexStringToByteArray(string hex)
        {
            if (hex.Length%2 == 1)
                throw new Exception("The binary key cannot have an odd number of digits");

            byte[] arr = new byte[hex.Length >> 1];

            for (int i = 0; i< hex.Length >> 1; ++i) {
                arr[i] = (byte) ((GetHexVal(hex[i << 1]) << 4) + (GetHexVal(hex[(i << 1) + 1])));
            }

            return arr;
        }

        private static int GetHexVal(char hex)
        {
            int val = (int) hex;
            return val - (val < 58 ? 48 : 87);
        }

        private static string ByteArrayToHexString(byte[] ba)
        {
            var hex = new StringBuilder(ba.Length * 2);
            foreach (byte b in ba)
                hex.AppendFormat("{0:x2}", b);
            return hex.ToString();
        }
    }
}

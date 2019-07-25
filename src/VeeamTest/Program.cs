using System;
using System.Diagnostics;
using System.IO;

namespace VeeamTest
{
    public static class Program
    {
        private const int BUFFER_SIZE = 1024 * 1024;

        public static int Main(string[] args)
        {
            try
            {
                if (args.Length != 3) throw new Exception(
                    "Incorrect cli arguments length. There are should be three arguments. Command, input file and output file");

                Console.WriteLine($"Current time: {DateTime.UtcNow}");
                Console.WriteLine($"Action: {args[0]}");
                Console.WriteLine($"Input file: {args[1]}");
                Console.WriteLine($"Output file: {args[2]}");

                var timewatch = Stopwatch.StartNew();

                using (var inputStream = File.OpenRead(args[1]))
                using (var outputStream = File.Create(args[2]))
                {
                    var action = GetAction(args[0]);
                    action(inputStream, outputStream);
                }

                Console.WriteLine($"Action completed at {DateTime.UtcNow}");
                Console.WriteLine($"Time ellapsed: {timewatch.Elapsed}");

                return 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return 1;
            }
        }

        private static Action<Stream, Stream> GetAction(string actionName)
        {
            switch (actionName)
            {
                case "compress":
                    return (input, output) => new Compressor(BUFFER_SIZE, Environment.ProcessorCount).Compress(input, output);

                case "decompress":
                    return (input, output) => new Decompressor(Environment.ProcessorCount).Decompress(input, output);

                default:
                    throw new Exception("Only commands \"compress\" and \"decompress\" are supported.");
            }
        }
    }
}

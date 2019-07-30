using System;
using System.IO;
using VeeamTest.MultithreadGZip;

namespace VeeamTest.CLI
{
    public static class Program
    {
        public static int Main(string[] args)
        {
            try
            {
                if (args.Length != 3) throw new Exception(
                    "Incorrect cli arguments length. There are should be three arguments. Command, input file and output file");

                using (var inputStream = File.OpenRead(args[1]))
                using (var outputStream = File.Create(args[2]))
                {
                    var action = GetAction(args[0]);
                    action(inputStream, outputStream);
                }

                return 0;
            }
            catch (OutOfMemoryException)
            {
                Console.WriteLine("Not enough memory, please lower buffer-size value in config.json");
                return 1;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return 1;
            }
        }

        private static Action<Stream, Stream> GetAction(string actionName)
        {
            var config = ConfigBuilder.Build();

            switch (actionName)
            {
                case "compress":
                    return (input, output) => new Compressor(config.BufferSize, Environment.ProcessorCount).Compress(input, output);

                case "decompress":
                    return (input, output) => new Decompressor(Environment.ProcessorCount).Decompress(input, output);

                default:
                    throw new Exception("Only commands \"compress\" and \"decompress\" are supported.");
            }
        }
    }
}

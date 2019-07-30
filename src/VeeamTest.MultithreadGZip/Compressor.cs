using System;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using VeeamTest.MultithreadGZip.Multithreading;

namespace VeeamTest.MultithreadGZip
{
    public class Compressor
    {
        private readonly int _bufferSize;
        private readonly int _degreeOfParallelism;

        /// <param name="bufferSize">Size of chunk to compress</param>
        /// <param name="degreeOfParallelism">Total threads for parralel compression.</param>
        /// <exception cref="ArgumentOutOfRangeException">bufferSize or degreeOfParallelism is out of range</exception>
        public Compressor(int bufferSize, int degreeOfParallelism)
        {
            if (bufferSize <= 0) throw new ArgumentOutOfRangeException(nameof(bufferSize));
            if (degreeOfParallelism <= 0) throw new ArgumentOutOfRangeException(nameof(degreeOfParallelism));
            _bufferSize = bufferSize;
            _degreeOfParallelism = degreeOfParallelism;
        }

        /// <exception cref="ArgumentNullException">Input or output stream is nullable.</exception>
        /// <exception cref="ArgumentException">Input stream is not readable.</exception>
        /// <exception cref="ArgumentException">Output stream is not writable.</exception>
        public void Compress(Stream input, Stream output)
        {
            if (input == null) throw new ArgumentNullException(nameof(input));
            if (output == null) throw new ArgumentNullException(nameof(output));
            if (!input.CanRead) throw new ArgumentException("Input stream is not readable", nameof(input));
            if (!output.CanWrite) throw new ArgumentException("Output stream is not writable", nameof(output));

            using (var writer = new Consumer<Chunk>(
                action: item => WriteSerialized(item, output)))
            using (var compressor = new Consumer<Chunk>(
                action: item => CompressAndPush(item, writer),
                degreeOfParallelism: _degreeOfParallelism))
            {
                ReadAndPush(input, compressor);
            }
        }

        private void ReadAndPush(Stream stream, Consumer<Chunk> consumer)
        {
            var position = 0;
            var bytesRead = 0;
            var buffer = new byte[_bufferSize];

            while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) > 0)
            {
                var bytes = buffer.Take(bytesRead).ToArray();
                var chunk = new Chunk(position, bytes);

                consumer.Enqueue(chunk);

                position++;
            }
        }

        private static void CompressAndPush(Chunk chunk, Consumer<Chunk> consumer)
        {
            var compressed = chunk.Bytes.Zip();
            var newChunk = new Chunk(chunk.Index, compressed);
            consumer.Enqueue(newChunk);
        }

        private static void WriteSerialized(Chunk chunk, Stream stream)
        {
            var formatter = new BinaryFormatter();
            formatter.Serialize(stream, chunk);
        }
    }
}

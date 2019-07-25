using System;
using System.Buffers;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;

namespace VeeamTest
{
    public class Compressor
    {
        private readonly int _bufferSize;
        private readonly int _parralellism;

        public Compressor(int bufferSize, int parralellism)
        {
            if (bufferSize <= 0) throw new ArgumentOutOfRangeException(nameof(bufferSize));
            if (parralellism <= 0) throw new ArgumentOutOfRangeException(nameof(parralellism));
            _bufferSize = bufferSize;
            _parralellism = parralellism;
        }

        public void Compress(Stream input, Stream output)
        {
            using (var writer = new Consumer<Chunk>(
                action: item => WriteSerialized(item, output)))
            using (var compressor = new Consumer<Chunk>(
                action: item => CompressAndPush(item, writer),
                degreeOfParallelism: _parralellism))
            {
                ReadAndPush(input, compressor);
                compressor.RequestCompletion();
                compressor.Wait();
                writer.RequestCompletion();
                writer.Wait();
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

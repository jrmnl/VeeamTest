using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using VeeamTest.MultithreadGZip.Multithreading;

namespace VeeamTest.MultithreadGZip
{
    public class Decompressor
    {
        private readonly int _degreeOfParallelism;

        /// <param name="degreeOfParallelism">Total threads for parralel decompression.</param>
        /// <exception cref="ArgumentOutOfRangeException"degreeOfParallelism is out of range</exception>
        public Decompressor(int degreeOfParallelism)
        {
            if (degreeOfParallelism <= 0) throw new ArgumentOutOfRangeException(nameof(degreeOfParallelism));
            _degreeOfParallelism = degreeOfParallelism;
        }

        /// <summary>
        /// Decompresses one stream to another
        /// </summary>
        /// <exception cref="InvalidGZipException">Input stream not compatible with decompressor.</exception>
        /// <exception cref="ArgumentNullException">Input or output stream is nullable.</exception>
        /// <exception cref="ArgumentException">Input stream is not readable.</exception>
        /// <exception cref="ArgumentException">Output stream is not writable.</exception>
        public void Decompress(Stream input, Stream output)
        {
            if (input == null) throw new ArgumentNullException(nameof(input));
            if (output == null) throw new ArgumentNullException(nameof(output));
            if (!input.CanRead) throw new ArgumentException("Input stream is not readable", nameof(input));
            if (!output.CanWrite) throw new ArgumentException("Output stream is not writable", nameof(output));

            using (var writer = new Consumer<byte[]>(
                action: item => Write(item, output)))
            {
                var sorter = new SortedIterator<byte[]>(writer.Enqueue);
                using (var orderer = new Consumer<Chunk>(
                    action: item => sorter.Add(item.Index, item.Bytes)))
                using (var decompressor = new Consumer<Chunk>(
                    action: item => DecompressAndPush(item, orderer),
                    degreeOfParallelism: _degreeOfParallelism))
                {
                    ReadAndPush(input, decompressor);
                }
            }
        }

        private static void ReadAndPush(Stream stream, Consumer<Chunk> consumer)
        {
            var formatter = new BinaryFormatter();
            while (stream.Position != stream.Length)
            {
                try
                {
                    var chunk = (Chunk)formatter.Deserialize(stream);
                    consumer.Enqueue(chunk);
                }
                catch (SerializationException)
                {
                    throw new InvalidGZipException("The input stream is not a valid format");
                }
            }
        }

        private static void DecompressAndPush(Chunk chunk, Consumer<Chunk> consumer)
        {
            var decompressed = chunk.Bytes.Unzip();
            var newChunk = new Chunk(chunk.Index, decompressed);
            consumer.Enqueue(newChunk);
        }

        private static void Write(byte[] chunk, Stream stream)
        {
            stream.Write(chunk, 0, chunk.Length);
        }

        private class SortedIterator<T>
        {
            private int _index = 0;
            private readonly Dictionary<int, T> _dict = new Dictionary<int, T>();
            private readonly Action<T> _onMatch;

            public SortedIterator(Action<T> onMatch)
            {
                _onMatch = onMatch ?? throw new ArgumentNullException(nameof(onMatch));
            }

            public void Add(int index, T item)
            {
                _dict.Add(index, item);

                while (_dict.ContainsKey(_index))
                {
                    var element = _dict[_index];
                    _onMatch(element);
                    _dict.Remove(_index);
                    _index++;
                }
            }
        }
    }
}

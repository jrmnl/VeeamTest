using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace VeeamTest
{
    public class Decompressor
    {
        private readonly int _parralellism;

        public Decompressor(int parralellism)
        {
            if (parralellism <= 0) throw new ArgumentOutOfRangeException(nameof(parralellism));
            _parralellism = parralellism;
        }

        public void Decompress(string inputFile, string outputFile)
        {
            using (var filestream = File.Create(outputFile))
            using (var writer = new Consumer<byte[]>(
                action: item => Write(item, filestream)))
            {
                var sorter = new SortedIterator<byte[]>(writer.Enqueue);
                using (var orderer = new Consumer<Chunk>(
                    action: item => sorter.Add(item.Index, item.Bytes)))
                using (var decompressor = new Consumer<Chunk>(
                    action: item => DecompressAndPush(item, orderer),
                    degreeOfParallelism: _parralellism))
                {
                    ReadAndPush(inputFile, decompressor);

                    decompressor.RequestCompletion();
                    decompressor.Wait();

                    orderer.RequestCompletion();
                    orderer.Wait();
                }

                writer.RequestCompletion();
                writer.Wait();
            }
        }

        private static void ReadAndPush(string filename, Consumer<Chunk> consumer)
        {
            using (var reader = File.OpenRead(filename))
            {
                var formatter = new BinaryFormatter();
                while (reader.Position != reader.Length)
                {
                    var chunk = (Chunk)formatter.Deserialize(reader);
                    consumer.Enqueue(chunk);
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

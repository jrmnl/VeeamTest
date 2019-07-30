using System;

namespace VeeamTest.MultithreadGZip
{
    [Serializable]
    internal class Chunk
    {
        public Chunk(int index, byte[] bytes)
        {
            Index = index;
            Bytes = bytes ?? throw new ArgumentNullException(nameof(bytes));
        }

        public int Index { get; }
        public byte[] Bytes { get; }
    }
}

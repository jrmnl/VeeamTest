using System;

namespace VeeamTest.CLI
{
    internal class Config
    {
        public Config(int bufferSize)
        {
            if (bufferSize <= 0) throw new ArgumentOutOfRangeException(nameof(bufferSize));
            BufferSize = bufferSize;
        }

        public int BufferSize { get; }
    }
}

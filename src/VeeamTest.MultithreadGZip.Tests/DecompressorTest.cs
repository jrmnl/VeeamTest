using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace VeeamTest.MultithreadGZip.Tests
{
    [TestClass]
    public class DecompressorTest
    {
        [TestMethod]
        public void CheckInvalidGZipException()
        {
            var decompressor = new Decompressor(1);

            var randomBytes = Helpers.GenerateRandomBytes(1000);
            using (var readStream = new MemoryStream(randomBytes))
            using (var writeStream = new MemoryStream())
            {
                Action action = () => decompressor.Decompress(readStream, writeStream);
                Assert.ThrowsException<InvalidGZipException>(action);
            }
        }

        [TestMethod]
        public void CheckCantReadInput()
        {
            var decompressor = new Decompressor(1);

            var randomBytes = Helpers.GenerateRandomBytes(1000);
            using (var readStream = new NotReadableStream(randomBytes))
            using (var writeStream = new MemoryStream())
            {
                Action action = () => decompressor.Decompress(readStream, writeStream);
                Assert.ThrowsException<ArgumentException>(action);
            }
        }

        private class NotReadableStream : MemoryStream
        {
            public NotReadableStream(byte[] buffer) : base(buffer)
            {
            }

            public override bool CanRead { get; } = false;
        }
    }
}

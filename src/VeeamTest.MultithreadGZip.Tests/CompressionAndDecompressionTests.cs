using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace VeeamTest.MultithreadGZip.Tests
{
    [TestClass]
    public class CompressionAndDecompressionTests
    {
        [TestMethod]
        public void WorksCorrectlyInOneThread()
        {
            var expectedBytes = Helpers.GenerateRandomBytes(1000000);

            using (var original = new MemoryStream(expectedBytes))
            using (var compressed = new MemoryStream())
            using (var decompressed = new MemoryStream())
            {
                var compressor = new Compressor(10000, 1);
                compressor.Compress(original, compressed);

                compressed.Seek(0, SeekOrigin.Begin);

                var decompressor = new Decompressor(1);
                decompressor.Decompress(compressed, decompressed);

                CollectionAssert.AreEqual(expectedBytes, decompressed.ToArray());
            }
        }
    }
}
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace VeeamTest.Tests
{
    [TestClass]
    public class CompressionAndDecompressionTests
    {
        [TestMethod]
        public void WorksCorrectlyInOneThread()
        {
            var expectedBytes = Helpers.GenerateRandomBytes(1000000);

            var compressor = new Compressor(10000, 1);
            var decompressor = new Decompressor(1);

            using (var original = new MemoryStream(expectedBytes))
            using (var compressed = new MemoryStream())
            using (var decompressed = new MemoryStream())
            {
                compressor.Compress(original, compressed);

                compressed.Seek(0, SeekOrigin.Begin);

                decompressor.Decompress(compressed, decompressed);

                var result = decompressed.ToArray();
                CollectionAssert.AreEqual(expectedBytes, result);
            }
        }
    }
}
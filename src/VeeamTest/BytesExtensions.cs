using System.IO;
using System.IO.Compression;

namespace VeeamTest
{
    public static class BytesExtensions
    {
        public static byte[] Unzip(this byte[] bytes)
        {
            using (var readStream = new MemoryStream(bytes))
            using (var zipper = new GZipStream(readStream, CompressionMode.Decompress))
            using (var writeStream = new MemoryStream())
            {
                zipper.CopyTo(writeStream);
                return writeStream.ToArray();
            }
        }


        public static byte[] Zip(this byte[] bytes)
        {
            using (var writeStream = new MemoryStream())
            using (var zipper = new GZipStream(writeStream, CompressionMode.Compress))
            using (var readStream = new MemoryStream(bytes))
            {
                readStream.CopyTo(zipper);
                return writeStream.ToArray();
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace VeeamTest.Tests
{
    internal static class Helpers
    {
        public static byte[] GenerateRandomBytes(int count)
        {
            var buffer = new byte[count];
            new Random().NextBytes(buffer);
            return buffer;
        }
    }
}

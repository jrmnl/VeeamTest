using System;

namespace VeeamTest.MultithreadGZip
{
    public class InvalidGZipException : Exception
    {
        public InvalidGZipException(string message) : base(message) { }
    }
}

using System;
using System.IO;

namespace DllEncrypter
{
    public sealed class InvalidDllNameException : ArgumentException
    {
        public InvalidDllNameException(string message) : base(message) { }
        public InvalidDllNameException(string message, Exception inner) : base(message, inner) { }
    }

    public sealed class OutputExistsException : IOException
    {
        public OutputExistsException(string path) : base("Output already exists: " + path) { }
    }
}

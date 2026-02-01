using System;

namespace DllEncrypter
{
    public sealed class XorMdlCipherOptions
    {
        public bool Overwrite { get; set; }
        public bool Recursive { get; set; } = true;
        public bool PreserveRelativePaths { get; set; } = true;
        public string DllExtensionForKey { get; set; } = ".dll";
        public Func<string, bool> FileFilter { get; set; }
    }
}

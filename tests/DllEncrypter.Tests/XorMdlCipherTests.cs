using System;
using System.IO;
using Xunit;
using DllEncrypter;

namespace DllEncrypter.Tests
{
    public class XorMdlCipherTests
    {
        [Fact]
        public void TransformIsReversible()
        {
            var name = "Assembly-CSharp.dll";
            var data = new byte[64];
            for (int i = 0; i < data.Length; i++)
            {
                data[i] = (byte)i;
            }

            var encoded = XorMdlCipher.Transform(data, name);
            var decoded = XorMdlCipher.Transform(encoded, name);

            Assert.Equal(data, decoded);
        }

        [Fact]
        public void TransformOnlyTouchesPrefix()
        {
            var name = "TestAsm.dll";
            int key = name.Length & 0xff;

            var data = new byte[32];
            for (int i = 0; i < data.Length; i++)
            {
                data[i] = (byte)(200 + i);
            }

            var transformed = XorMdlCipher.Transform(data, name);

            for (int i = 0; i < data.Length; i++)
            {
                byte expected = data[i];
                if (i < name.Length)
                {
                    expected = (byte)(expected ^ key);
                }

                Assert.Equal(expected, transformed[i]);
            }
        }

        [Fact]
        public void TransformFolderWritesExpectedOutput()
        {
            string root = Path.Combine(Path.GetTempPath(), "DllEncrypterTests", Guid.NewGuid().ToString("N"));
            string inputDir = Path.Combine(root, "in");
            string outputDir = Path.Combine(root, "out");
            Directory.CreateDirectory(inputDir);

            string fileBase = "Sample";
            string inPath = Path.Combine(inputDir, fileBase + ".mdl");
            string dllName = fileBase + ".dll";

            byte[] data = new byte[48];
            for (int i = 0; i < data.Length; i++)
            {
                data[i] = (byte)(i * 3);
            }
            File.WriteAllBytes(inPath, data);

            try
            {
                var options = new XorMdlCipherOptions
                {
                    Overwrite = true,
                    Recursive = false,
                    PreserveRelativePaths = true,
                    DllExtensionForKey = ".dll"
                };

                var result = XorMdlCipher.TransformFolder(inputDir, outputDir, ".mdl", ".dll", options);
                Assert.Equal(1, result.FilesProcessed);
                Assert.Empty(result.Errors);

                string outPath = Path.Combine(outputDir, fileBase + ".dll");
                Assert.True(File.Exists(outPath));

                var expected = XorMdlCipher.Transform(data, dllName);
                var actual = File.ReadAllBytes(outPath);
                Assert.Equal(expected, actual);
            }
            finally
            {
                if (Directory.Exists(root))
                {
                    Directory.Delete(root, true);
                }
            }
        }
    }
}

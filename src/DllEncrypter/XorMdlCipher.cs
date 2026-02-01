using System;
using System.Collections.Generic;
using System.IO;

namespace DllEncrypter
{
    public static class XorMdlCipher
    {
        public static byte[] Transform(byte[] data, string dllFileName)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            var output = new byte[data.Length];
            Buffer.BlockCopy(data, 0, output, 0, data.Length);
            TransformInPlace(output, dllFileName);
            return output;
        }

        public static void TransformInPlace(byte[] data, string dllFileName)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            var name = GetDllFileNameOrThrow(dllFileName);
            int key = name.Length & 0xff;
            int n = Math.Min(data.Length, name.Length);

            for (int i = 0; i < n; i++)
            {
                data[i] = (byte)(data[i] ^ key);
            }
        }

        public static void TransformFile(string inputPath, string outputPath, string dllFileName)
        {
            if (string.IsNullOrEmpty(inputPath))
            {
                throw new ArgumentException("Input path is required.", nameof(inputPath));
            }
            if (string.IsNullOrEmpty(outputPath))
            {
                throw new ArgumentException("Output path is required.", nameof(outputPath));
            }
            if (!File.Exists(inputPath))
            {
                throw new FileNotFoundException("Input file not found.", inputPath);
            }

            var data = File.ReadAllBytes(inputPath);
            TransformInPlace(data, dllFileName);

            var outDir = Path.GetDirectoryName(outputPath);
            if (!string.IsNullOrEmpty(outDir) && !Directory.Exists(outDir))
            {
                Directory.CreateDirectory(outDir);
            }

            File.WriteAllBytes(outputPath, data);
        }

        public static XorMdlCipherResult TransformFolder(
            string inputDir,
            string outputDir,
            string extensionIn,
            string extensionOut,
            XorMdlCipherOptions options)
        {
            if (string.IsNullOrEmpty(inputDir))
            {
                throw new ArgumentException("Input directory is required.", nameof(inputDir));
            }
            if (string.IsNullOrEmpty(outputDir))
            {
                throw new ArgumentException("Output directory is required.", nameof(outputDir));
            }
            if (!Directory.Exists(inputDir))
            {
                throw new DirectoryNotFoundException("Input directory not found: " + inputDir);
            }

            var opts = options ?? new XorMdlCipherOptions();
            string extIn = NormalizeExtension(extensionIn, nameof(extensionIn));
            string extOut = NormalizeExtension(extensionOut, nameof(extensionOut));
            string dllExt = NormalizeExtension(opts.DllExtensionForKey ?? ".dll", nameof(opts.DllExtensionForKey));

            var errors = new List<string>();
            int processed = 0;
            int skipped = 0;

            var searchOption = opts.Recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
            foreach (var file in Directory.EnumerateFiles(inputDir, "*" + extIn, searchOption))
            {
                if (!file.EndsWith(extIn, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (opts.FileFilter != null && !opts.FileFilter(file))
                {
                    skipped++;
                    continue;
                }

                string relPath = opts.PreserveRelativePaths ? GetRelativePath(inputDir, file) : Path.GetFileName(file);
                string outRelPath = Path.ChangeExtension(relPath, extOut);
                string outPath = Path.Combine(outputDir, outRelPath);

                if (File.Exists(outPath) && !opts.Overwrite)
                {
                    skipped++;
                    continue;
                }

                string baseName = Path.GetFileNameWithoutExtension(file);
                string dllNameForKey = baseName + dllExt;

                try
                {
                    TransformFile(file, outPath, dllNameForKey);
                    processed++;
                }
                catch (Exception ex)
                {
                    errors.Add(file + ": " + ex.Message);
                }
            }

            return new XorMdlCipherResult(processed, skipped, errors);
        }

        private static string GetDllFileNameOrThrow(string dllFileName)
        {
            if (string.IsNullOrEmpty(dllFileName))
            {
                throw new InvalidDllNameException("DLL file name is required (e.g., Assembly-CSharp.dll)." );
            }

            string name = Path.GetFileName(dllFileName);
            if (string.IsNullOrEmpty(name))
            {
                throw new InvalidDllNameException("DLL file name is invalid: " + dllFileName);
            }

            return name;
        }

        private static string NormalizeExtension(string ext, string paramName)
        {
            if (string.IsNullOrEmpty(ext))
            {
                throw new ArgumentException("Extension is required.", paramName);
            }

            return ext[0] == '.' ? ext : "." + ext;
        }

        private static string GetRelativePath(string baseDir, string fullPath)
        {
            string baseFull = Path.GetFullPath(baseDir);
            string targetFull = Path.GetFullPath(fullPath);
            if (!baseFull.EndsWith(Path.DirectorySeparatorChar.ToString()))
            {
                baseFull += Path.DirectorySeparatorChar;
            }

            var baseUri = new Uri(baseFull);
            var targetUri = new Uri(targetFull);
            var relUri = baseUri.MakeRelativeUri(targetUri);
            var relPath = Uri.UnescapeDataString(relUri.ToString());
            return relPath.Replace('/', Path.DirectorySeparatorChar);
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using DllEncrypter;

namespace DllEncrypter.Cli
{
    internal static class Program
    {
        private sealed class Args
        {
            public string InPath;
            public string OutPath;
            public string DllName;
            public string ExtIn = ".mdl";
            public string ExtOut = ".dll";
            public string DllExt = ".dll";
            public bool Recursive = true;
            public bool Overwrite;
            public bool ShowHelp;
            public bool ParseOk = true;
            public string Error;
        }

        public static int Main(string[] args)
        {
            if (args.Length == 0)
            {
                PrintHelp();
                return 1;
            }

            var command = args[0].ToLowerInvariant();
            var parsed = Parse(args);
            if (parsed.ShowHelp)
            {
                PrintHelp();
                return 0;
            }
            if (!parsed.ParseOk)
            {
                Console.Error.WriteLine(parsed.Error ?? "Invalid arguments.");
                PrintHelp();
                return 1;
            }

            try
            {
                if (command == "file")
                {
                    return RunFile(parsed);
                }
                if (command == "folder")
                {
                    return RunFolder(parsed);
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.Message);
                return 1;
            }

            Console.Error.WriteLine("Unknown command: " + command);
            PrintHelp();
            return 1;
        }

        private static Args Parse(string[] args)
        {
            var result = new Args();

            for (int i = 1; i < args.Length; i++)
            {
                var token = args[i];
                if (token == "-h" || token == "--help")
                {
                    result.ShowHelp = true;
                    return result;
                }

                if (token.StartsWith("--", StringComparison.Ordinal))
                {
                    var key = token.Substring(2);
                    if (key == "overwrite")
                    {
                        result.Overwrite = true;
                        continue;
                    }
                    if (key == "no-recursive")
                    {
                        result.Recursive = false;
                        continue;
                    }

                    if (i + 1 >= args.Length)
                    {
                        result.ParseOk = false;
                        result.Error = "Missing value for option: " + token;
                        return result;
                    }

                    var value = args[++i];
                    switch (key)
                    {
                        case "in":
                            result.InPath = value;
                            break;
                        case "out":
                            result.OutPath = value;
                            break;
                        case "dll-name":
                            result.DllName = value;
                            break;
                        case "ext-in":
                            result.ExtIn = value;
                            break;
                        case "ext-out":
                            result.ExtOut = value;
                            break;
                        case "dll-ext":
                            result.DllExt = value;
                            break;
                        default:
                            result.ParseOk = false;
                            result.Error = "Unknown option: " + token;
                            return result;
                    }
                }
                else
                {
                    result.ParseOk = false;
                    result.Error = "Unexpected argument: " + token;
                    return result;
                }
            }

            return result;
        }

        private static int RunFile(Args args)
        {
            if (string.IsNullOrEmpty(args.InPath) || string.IsNullOrEmpty(args.OutPath))
            {
                Console.Error.WriteLine("--in and --out are required for file mode.");
                return 1;
            }

            string dllName = args.DllName;
            if (string.IsNullOrEmpty(dllName))
            {
                string baseName = Path.GetFileNameWithoutExtension(args.InPath);
                dllName = baseName + ".dll";
            }

            XorMdlCipher.TransformFile(args.InPath, args.OutPath, dllName);
            Console.WriteLine("Wrote: " + args.OutPath);
            return 0;
        }

        private static int RunFolder(Args args)
        {
            if (string.IsNullOrEmpty(args.InPath) || string.IsNullOrEmpty(args.OutPath))
            {
                Console.Error.WriteLine("--in and --out are required for folder mode.");
                return 1;
            }

            var options = new XorMdlCipherOptions
            {
                Overwrite = args.Overwrite,
                Recursive = args.Recursive,
                PreserveRelativePaths = true,
                DllExtensionForKey = args.DllExt
            };

            var result = XorMdlCipher.TransformFolder(args.InPath, args.OutPath, args.ExtIn, args.ExtOut, options);

            Console.WriteLine("Processed: " + result.FilesProcessed);
            Console.WriteLine("Skipped: " + result.FilesSkipped);
            if (result.Errors.Count > 0)
            {
                Console.WriteLine("Errors: " + result.Errors.Count);
                foreach (var error in result.Errors)
                {
                    Console.WriteLine(error);
                }
                return 1;
            }

            return 0;
        }

        private static void PrintHelp()
        {
            Console.WriteLine("dll-encrypter - XOR encode/decode for Unity-style .mdl/.dll files");
            Console.WriteLine();
            Console.WriteLine("Usage:");
            Console.WriteLine("  dll-encrypter file --in <path> --out <path> [--dll-name <Assembly.dll>]");
            Console.WriteLine("  dll-encrypter folder --in <dir> --out <dir> [--ext-in .mdl] [--ext-out .dll]");
            Console.WriteLine("                  [--dll-ext .dll] [--no-recursive] [--overwrite]");
            Console.WriteLine();
            Console.WriteLine("Notes:");
            Console.WriteLine("  - The XOR key length is based on the DLL file name string.");
            Console.WriteLine("  - If --dll-name is omitted in file mode, it is derived from the input name.");
        }
    }
}

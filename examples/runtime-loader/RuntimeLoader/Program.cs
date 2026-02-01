using System;
using System.IO;
using System.Reflection;
using DllEncrypter;

namespace RuntimeLoader
{
    internal static class Program
    {
        private sealed class Options
        {
            public string MdlPath;
            public string DllName = "EncryptedLib.dll";
            public string TypeName = "EncryptedLib.DemoApi";
            public string MethodName = "Process";
            public string Input = "hello";
            public string GenerateFrom;
            public bool ShowHelp;
            public string Error;
        }

        public static int Main(string[] args)
        {
            var options = Parse(args);
            if (options.ShowHelp)
            {
                PrintHelp();
                return 0;
            }
            if (options.Error != null)
            {
                Console.Error.WriteLine(options.Error);
                PrintHelp();
                return 1;
            }

            string mdlPath = options.MdlPath ?? Path.Combine(Directory.GetCurrentDirectory(), "EncryptedLib.mdl");

            try
            {
                if (!string.IsNullOrEmpty(options.GenerateFrom))
                {
                    XorMdlCipher.TransformFile(options.GenerateFrom, mdlPath, options.DllName);
                    Console.WriteLine("Generated: " + mdlPath);
                }

                if (!File.Exists(mdlPath))
                {
                    Console.Error.WriteLine(".mdl not found: " + mdlPath);
                    return 1;
                }

                byte[] mdlBytes = File.ReadAllBytes(mdlPath);
                byte[] dllBytes = XorMdlCipher.Transform(mdlBytes, options.DllName);

                var assembly = Assembly.Load(dllBytes);
                var type = assembly.GetType(options.TypeName, true);
                var method = type.GetMethod(options.MethodName, BindingFlags.Public | BindingFlags.Static);
                if (method == null)
                {
                    Console.Error.WriteLine("Method not found: " + options.TypeName + "." + options.MethodName);
                    return 1;
                }

                object result;
                var parameters = method.GetParameters();
                if (parameters.Length == 0)
                {
                    result = method.Invoke(null, null);
                }
                else if (parameters.Length == 1 && parameters[0].ParameterType == typeof(string))
                {
                    result = method.Invoke(null, new object[] { options.Input });
                }
                else
                {
                    Console.Error.WriteLine("Method signature not supported. Expected no args or a single string arg.");
                    return 1;
                }

                Console.WriteLine(result == null ? "<null>" : result.ToString());
                return 0;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.Message);
                return 1;
            }
        }

        private static Options Parse(string[] args)
        {
            var options = new Options();
            for (int i = 0; i < args.Length; i++)
            {
                var token = args[i];
                if (token == "-h" || token == "--help")
                {
                    options.ShowHelp = true;
                    return options;
                }

                if (!token.StartsWith("--", StringComparison.Ordinal))
                {
                    options.Error = "Unexpected argument: " + token;
                    return options;
                }

                string key = token.Substring(2);
                if (i + 1 >= args.Length)
                {
                    options.Error = "Missing value for option: " + token;
                    return options;
                }

                string value = args[++i];
                switch (key)
                {
                    case "mdl":
                        options.MdlPath = value;
                        break;
                    case "dll-name":
                        options.DllName = value;
                        break;
                    case "type":
                        options.TypeName = value;
                        break;
                    case "method":
                        options.MethodName = value;
                        break;
                    case "input":
                        options.Input = value;
                        break;
                    case "generate-from":
                        options.GenerateFrom = value;
                        break;
                    default:
                        options.Error = "Unknown option: " + token;
                        return options;
                }
            }

            return options;
        }

        private static void PrintHelp()
        {
            Console.WriteLine("RuntimeLoader - load encrypted .mdl at runtime and call a method");
            Console.WriteLine();
            Console.WriteLine("Usage:");
            Console.WriteLine("  dotnet run --project RuntimeLoader.csproj -- ");
            Console.WriteLine("    [--mdl <path>] [--generate-from <dll path>] [--dll-name <name>] ");
            Console.WriteLine("    [--type <Full.Type.Name>] [--method <Method>] [--input <string>] ");
            Console.WriteLine();
            Console.WriteLine("Defaults:");
            Console.WriteLine("  --mdl ./EncryptedLib.mdl");
            Console.WriteLine("  --dll-name EncryptedLib.dll");
            Console.WriteLine("  --type EncryptedLib.DemoApi");
            Console.WriteLine("  --method Process");
            Console.WriteLine("  --input hello");
        }
    }
}

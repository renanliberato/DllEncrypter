# DllEncrypter

Standalone library + CLI example demonstrating the XOR scheme used to obfuscate Unity `.dll` assemblies into `.mdl` files.

## Structure
- `src/DllEncrypter` - library (netstandard2.0 + net8.0)
- `examples/dll-encrypter` - CLI example using the library
- `examples/runtime-loader` - runtime load demo (encrypts `.mdl`, loads assembly, invokes method)
- `tests/DllEncrypter.Tests` - xUnit tests for the library

## Build

```sh
dotnet build DllEncrypter/DllEncrypter.sln
```

## Test

```sh
dotnet test DllEncrypter/DllEncrypter.sln
```

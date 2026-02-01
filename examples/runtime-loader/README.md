# Runtime loader example

This example shows how to load an encrypted `.mdl` file at runtime, decrypt it in memory, load the assembly, and invoke a method.

## Projects
- `EncryptedLib` (class library): exposes `EncryptedLib.DemoApi.Process(string)`
- `RuntimeLoader` (console app): reads `.mdl`, decrypts, loads assembly, calls method

## Build the library

```sh
dotnet build DllEncrypter/examples/runtime-loader/EncryptedLib/EncryptedLib.csproj
```

## Generate an encrypted `.mdl`

```sh
dotnet run --project DllEncrypter/examples/runtime-loader/RuntimeLoader/RuntimeLoader.csproj -- \
  --generate-from DllEncrypter/examples/runtime-loader/EncryptedLib/bin/Debug/netstandard2.0/EncryptedLib.dll
```

This writes `EncryptedLib.mdl` in the current working directory (where you ran the command).

## Load and call the method

```sh
dotnet run --project DllEncrypter/examples/runtime-loader/RuntimeLoader/RuntimeLoader.csproj -- \
  --mdl ./EncryptedLib.mdl --dll-name EncryptedLib.dll --type EncryptedLib.DemoApi --method Process --input "hello world"
```

Expected output:

```
Processed: HELLO WORLD
```

## Notes
- The XOR key uses the DLL filename length. The `--dll-name` value must match the original DLL name.
- If you omit `--mdl`, it defaults to `./EncryptedLib.mdl`.

# dll-encrypter CLI

Demo CLI for the `DllEncrypter` library. It applies the same XOR rule used by the native loader.

## Build

```sh
dotnet build DllEncrypter/examples/dll-encrypter/DllEncrypter.Cli.csproj
```

## Usage

### Single file
Decode `.mdl` -> `.dll`:

```sh
dotnet run --project DllEncrypter/examples/dll-encrypter/DllEncrypter.Cli.csproj -- \
  file --in Assembly-CSharp.mdl --out Assembly-CSharp.dll --dll-name Assembly-CSharp.dll
```

Encode `.dll` -> `.mdl` (same operation):

```sh
dotnet run --project DllEncrypter/examples/dll-encrypter/DllEncrypter.Cli.csproj -- \
  file --in Assembly-CSharp.dll --out Assembly-CSharp.mdl --dll-name Assembly-CSharp.dll
```

### Folder

```sh
dotnet run --project DllEncrypter/examples/dll-encrypter/DllEncrypter.Cli.csproj -- \
  folder --in ./Assemblies --out ./out --ext-in .mdl --ext-out .dll --overwrite
```

## Notes
- If `--dll-name` is omitted in file mode, it is derived from the input filename.
- `--dll-ext` controls the extension used for the XOR key in folder mode (default `.dll`).

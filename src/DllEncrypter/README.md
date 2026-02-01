# DllEncrypter

This library demonstrates the XOR scheme observed in the target APK, used to obfuscate `.dll` files into `.mdl` files and vice-versa.

## XOR rule
Given the DLL file name (e.g., `Assembly-CSharp.dll`):

```
key = len(dllFileName) & 0xff
for i in [0, min(data.Length, dllFileName.Length)):
    data[i] ^= key
```

Only the first `N` bytes are XORed, where `N` is the length of the DLL file name string.

## Usage

### In-memory
```csharp
byte[] decoded = XorMdlCipher.Transform(mdlBytes, "Assembly-CSharp.dll");
```

### File
```csharp
XorMdlCipher.TransformFile(
    "Assembly-CSharp.mdl",
    "Assembly-CSharp.dll",
    "Assembly-CSharp.dll");
```

### Folder
```csharp
var options = new XorMdlCipherOptions
{
    Overwrite = true,
    Recursive = true,
    PreserveRelativePaths = true,
    DllExtensionForKey = ".dll"
};

var result = XorMdlCipher.TransformFolder(
    "./Assemblies",
    "./out",
    ".mdl",
    ".dll",
    options);
```

## Notes
- This is obfuscation, not cryptographic security.
- The DLL file name controls the key length. If you pass the wrong name, output will be invalid.

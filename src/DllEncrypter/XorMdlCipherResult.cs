using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace DllEncrypter
{
    public sealed class XorMdlCipherResult
    {
        public int FilesProcessed { get; private set; }
        public int FilesSkipped { get; private set; }
        public IReadOnlyList<string> Errors { get; private set; }

        public XorMdlCipherResult(int processed, int skipped, IList<string> errors)
        {
            FilesProcessed = processed;
            FilesSkipped = skipped;
            Errors = new ReadOnlyCollection<string>(errors ?? new List<string>());
        }
    }
}

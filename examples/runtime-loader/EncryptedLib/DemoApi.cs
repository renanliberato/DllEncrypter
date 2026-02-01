namespace EncryptedLib
{
    public static class DemoApi
    {
        public static string Process(string input)
        {
            if (input == null)
            {
                return "<null>";
            }

            return "Processed: " + input.ToUpperInvariant();
        }
    }
}

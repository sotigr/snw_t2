namespace System
{
    public static class Ex_ByteArray
    {
        public static string GetString(this byte[] bytes)
        {
            return System.Text.Encoding.UTF8.GetString(bytes);
        }
    }
}

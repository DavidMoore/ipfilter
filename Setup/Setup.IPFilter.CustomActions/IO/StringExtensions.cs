namespace IPFilter.Setup.CustomActions.IO
{
    public static class StringExtensions
    {
        public static bool EndsWith(this string value, char character)
        {
            if (string.IsNullOrEmpty(value)) return false;
            return value[value.Length - 1] == character;
        }
    }
}
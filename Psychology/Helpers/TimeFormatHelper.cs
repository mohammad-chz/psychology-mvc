namespace Psychology.Helpers
{
    public static class TimeFormatHelper
    {
        /// <summary>
        /// Converts a 4-digit string like "0812" into "08:12".
        /// </summary>
        public static string InsertColonAfterTwoDigits(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return string.Empty;

            // Remove any non-digit characters
            value = new string(value.Where(char.IsDigit).ToArray());

            // Ensure at least 4 digits
            if (value.Length < 3)
                return value;

            // Limit to 4 digits max
            if (value.Length > 4)
                value = value.Substring(0, 4);

            return value.Insert(2, ":");
        }
    }
}

namespace SchoolManagement.Common
{
    public static class StringExtensions
    {
        public static string EnumToString<T>(T enumValue) where T : Enum
        {
            return enumValue.ToString();
        }
    }
}

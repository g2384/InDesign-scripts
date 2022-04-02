using Newtonsoft.Json;

namespace Generator
{
    public static class StringExtensions
    {
        public static string ToLiteral(this string input)
        {
            return JsonConvert.SerializeObject(input);
        }

    }
}
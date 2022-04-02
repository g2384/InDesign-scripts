using Newtonsoft.Json;

namespace Generator
{
    public static class ObjectExtensions
    {
        public static string ToLiteral(this object input)
        {
            return JsonConvert.SerializeObject(input, Formatting.Indented);
        }
    }
}
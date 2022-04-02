using System.Text.RegularExpressions;

namespace Generator
{
    public class TemplateExtractor
    {
        private static Regex settingsStart = new Regex(@"\/\/\s*settings\s*start", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static Regex settingsEnd = new Regex(@"\/\/\s*settings\s*end", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public TemplateExtractor(string path)
        {
            var file = File.ReadAllText(path);
            var s1 = settingsStart.Split(file);
            BeforeSettings = s1[0];
            var s2 = settingsEnd.Split(s1[1]);
            AfterSettings = s2[1];
            Settings = s2[0].Trim();
        }

        public string BeforeSettings { get; set; }
        public string Settings { get; set; }
        public string AfterSettings { get; set; }
    }
}
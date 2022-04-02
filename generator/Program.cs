using Serilog;
using Serilog.Sinks.SystemConsole.Themes;
using System.Text.RegularExpressions;

namespace Generator
{
    public static class Program
    {
        public static void Main(params string[] paths)
        {
            Log.Logger = new LoggerConfiguration()
               .MinimumLevel.Debug()
               .WriteTo.Console(outputTemplate: "[{Timestamp:yyy-MM-dd HH:mm:ss} {Level:w4}] {Message:lj}{NewLine}{Exception}", theme: AnsiConsoleTheme.Code)
               .CreateLogger();

            if(paths.Length == 0)
            {
                Log.Error("Provide a jsx template.");
                return;
            }

            var path = paths[0];

            if (string.IsNullOrEmpty(path))
            {
                Log.Error("Provide a jsx template.");
                return;
            }

            if (!File.Exists(path))
            {
                Log.Error($"Cannot find file: {path}");
                return;
            }

            var extracted = new TemplateExtractor(path);
            var parser = new TemplateParser();
            var settings = parser.Parse(path, extracted.Settings);

            var generator = new ScriptGenerator(settings, path, extracted.BeforeSettings + extracted.AfterSettings);
        }
    }

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
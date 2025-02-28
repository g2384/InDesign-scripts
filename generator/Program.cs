﻿using Serilog;
using Serilog.Sinks.SystemConsole.Themes;

namespace Generator
{
    public static class Program
    {
        public static string GetRootDirectory()
        {
            return AppContext.BaseDirectory;
        }

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
}
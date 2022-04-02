using Serilog;

namespace Generator
{
    internal class ScriptGenerator
    {
        public ScriptGenerator(Settings settings, string path, string originalText)
        {
            var fi = new FileInfo(path);
            var newPath = Path.Combine(Path.GetDirectoryName(fi.FullName), Path.GetFileNameWithoutExtension(path)) + "_output" + fi.Extension;
            Log.Information("Output path: \"" + newPath+"\"");
            var pages = settings.Pages;
            var functions = settings.Functions.ToDictionary(e => $"${e.Key}$", e => e.Value);
            var js = new List<string>();
            foreach (var page in pages)
            {
                js.Add("var activePage = $insertPage$");
                if (page.Title != null)
                {
                    js.Add("");
                }
                for (var i = 0; i < js.Count; i++)
                {
                    foreach (var f in functions)
                    {
                        js[i] = js[i].Replace(f.Key, f.Value);
                    }
                }
            }
            var text = string.Join("\n", js);
            var newText = originalText.Replace("$SCRIPTS$", text);
            File.WriteAllText(newPath, newText);
        }
    }
}
using Serilog;
using System.Text.RegularExpressions;

namespace Generator
{
    internal class ScriptGenerator
    {
        private IDictionary<string, Regex> removeBeforeCompare = new Dictionary<string, Regex>();
        public ScriptGenerator(Settings settings, string path, string originalText)
        {
            var fi = new FileInfo(path);
            var newPath = Path.Combine(Path.GetDirectoryName(fi.FullName), Path.GetFileNameWithoutExtension(path)) + "_output" + fi.Extension;
            Log.Information("Output path: \"" + newPath + "\"");
            var pages = settings.Pages;
            var functions = settings.Functions.ToDictionary(e => $"${e.Key}$", e => e.Value);
            var js = new List<string>();
            js.Add("var activePage;");
            foreach (var page in pages)
            {
                if (!page.Folder.EndsWith("\\"))
                {
                    page.Folder = page.Folder + "\\";
                }
                var files = FileHelper.GetAllFiles(page.Folder, "*.*").ToArray();
                files = files.Where(e => !e.Contains("~$")).ToArray();
                js.Add("activePage = $insertPage$;");

                AddTitleToJs(functions, js, page.Title);

                if (page.File != null)
                {
                    var matched = GetMatched(files, page.File, page.Folder);
                    AddFileToJs(functions["$insertFile$"], js, matched);
                    if (page.LoopBy == "order")
                    {
                        var isFirst = true;
                        foreach (var order in page.Order)
                        {
                            foreach (var nextPage in page.NextPages)
                            {
                                if (!isFirst && nextPage.Once)
                                {
                                    continue;
                                }
                                if (nextPage.Once)
                                {
                                    js.Add("activePage = $insertPage$;");
                                    AddTitleToJs(functions, js, nextPage.Title);
                                    var matched2 = GetMatched(files, nextPage.File, page.Folder);
                                    AddFileToJs(functions["$insertFile$"], js, matched2);
                                }
                                else
                                {
                                    js.Add("activePage = $insertPage$;");
                                    if (nextPage.TitleFormat == "useOrder")
                                    {
                                        nextPage.Title = order;
                                    }
                                    AddTitleToJs(functions, js, nextPage.Title);
                                    var matched2 = GetMatched(files, nextPage.File, page.Folder);
                                    var regex = new List<Regex>();
                                    var rbc = nextPage.RemoveBeforeCompare;
                                    if (rbc != null)
                                    {
                                        foreach (var r in rbc)
                                        {
                                            if (!string.IsNullOrEmpty(r))
                                            {
                                                if (removeBeforeCompare.ContainsKey(r))
                                                {
                                                    regex.Add(removeBeforeCompare[r]);
                                                }
                                                else
                                                {
                                                    removeBeforeCompare[r] = new Regex(r);
                                                }
                                            }
                                        }
                                    }
                                    var filtered = FindBestMatched(matched2, order, page.Folder, regex);
                                    AddFileToJs(functions["$insertFile$"], js, filtered);
                                }
                            }
                            isFirst = false;
                        }
                    }
                }

            }
            for (var i = 0; i < js.Count; i++)
            {
                foreach (var f in functions)
                {
                    js[i] = js[i].Replace(f.Key, f.Value);
                }
            }
            var text = string.Join("\n", js);
            var newText = originalText.Replace("$SCRIPTS$", text);
            File.WriteAllText(newPath, newText);
        }

        private static string[] FindBestMatched(string[] allFiles, string expectedString, string rootFolder, IReadOnlyCollection<Regex> removeBeforeCompare = null)
        {
            var scores = allFiles.Select(e =>
            {
                var f = e.Replace(rootFolder, "");
                if(removeBeforeCompare != null)
                {
                    foreach (var regex in removeBeforeCompare)
                    {
                        f = regex.Replace(f, "");
                    }
                }
                return new
                {
                    Path = e,
                    ProcessedString = f,
                    Score = StringHelper.Compare(f, expectedString)
                };
            }).ToArray();
            var min = scores.Min(e => e.Score);
            var texts = scores.Where(e => e.Score == min).Select(e => e.Path).ToArray();
            return texts;
        }

        private static void AddTitleToJs(Dictionary<string, string> functions, List<string> js, string title)
        {
            if (!string.IsNullOrEmpty(title))
            {
                var func = functions["$addTitle$"].Replace("$title$", title.ToLiteral());
                js.Add(func + ";");
            }
        }

        private static void AddFileToJs(string function,
            List<string> js,
            string[] matched)
        {
            for (var i = 0; i < matched.Length; i++)
            {
                var func = function
                    .Replace("$file$", matched[i].ToLiteral())
                    .Replace("$index$", i.ToString());
                js.Add(func + ";");
            }
        }

        private string[] GetMatched(string[] files, string[] keywords, string rootPath)
        {
            var matched = new List<string>();
            foreach (var f in files)
            {
                var f1 = f.Replace(rootPath, "");
                foreach (var k in keywords)
                {
                    if (f1.Contains(k))
                    {
                        matched.Add(f);
                        break;
                    }
                }
            }
            return matched.ToArray();
        }
    }
}
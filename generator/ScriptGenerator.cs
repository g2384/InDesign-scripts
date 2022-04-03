using Serilog;
using System.Text;
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

                var wordOrder = page.OrderFromDoc;
                if (wordOrder != null)
                {
                    if(page.Order != null)
                    {
                        Log.Warning("\"Order\" is defined more than once. Order will be read from doc again.");
                    }
                    var matched = GetMatched(files, wordOrder.File, page.Folder);
                    if(matched.Length > 1)
                    {
                        Log.Warning($"Found multiple \"{wordOrder.File}\"");
                    }
                    var htmlFile = WordHelper.ConvertWordToHtml(matched[0]);
                    var html = new HtmlAgilityPack.HtmlDocument();
                    html.Load(htmlFile, Encoding.UTF8);
                    var node = html.DocumentNode.SelectNodes(wordOrder.XPath);
                    var strs = node.Select(e => e.InnerText.Trim()).ToArray();
                    if (wordOrder.IgnoredOrders?.Any() == true)
                    {
                        strs = strs.Except(wordOrder.IgnoredOrders, StringComparer.InvariantCultureIgnoreCase).ToArray();
                    }
                    Log.Information("Found orders in \"{wordOrder.FileName}\":" + Environment.NewLine + string.Join(Environment.NewLine, strs));
                    page.Order = strs;
                }


                AddPageToJs(js);

                AddTitleToJs(functions, js, page.Title);

                if (page.File != null)
                {
                    var matched = GetMatched(files, page.File, page.Folder);
                    AddFileToJs(functions, js, matched);
                    if (page.LoopBy == "order")
                    {
                        var isFirst = true;
                        foreach (var order in page.Order)
                        {
                            var addedFileShortNames = new List<string>();
                            foreach (var nextPage in page.NextPages)
                            {
                                if (!isFirst && nextPage.Once)
                                {
                                    continue;
                                }
                                AddPageToJs(js);
                                if (nextPage.Once)
                                {
                                    AddTitleToJs(functions, js, nextPage.Title);
                                    var matched2 = GetMatched(files, nextPage.File, page.Folder);
                                    AddFileToJs(functions, js, matched2);
                                }
                                else
                                {
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
                                                    var nr = new Regex(r);
                                                    removeBeforeCompare[r] = nr;
                                                    regex.Add(nr);
                                                }
                                            }
                                        }
                                    }

                                    if (!string.IsNullOrEmpty(nextPage.FileHint))
                                    {
                                        var regex2 = new Regex(nextPage.FileHint);
                                        var matched3 = addedFileShortNames.Select(e => regex2.Match(e).Groups[1].Value).Distinct().ToArray();
                                        matched2 = GetMatched(matched2, matched3, page.Folder);
                                    }

                                    if (matched2.Any())
                                    {
                                        var filtered = FindBestMatched(matched2, order, page.Folder, regex);
                                        AddFileToJs(functions, js, filtered);
                                        var shortNames = filtered.Select(e => e.Replace(page.Folder, ""));
                                        addedFileShortNames.AddRange(shortNames);
                                    }
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

        private static void AddPageToJs(List<string> js)
        {
            js.Add("activePage = $insertPage$;");
            js.Add("$incrementAfterInsertPage$;");
        }

        private static string[] FindBestMatched(string[] allFiles, string expectedString, string rootFolder, IReadOnlyCollection<Regex> removeBeforeCompare = null)
        {
            var dict = new Dictionary<int, string[]>();
            var scores = allFiles.Select(e =>
            {
                var f = e.Replace(rootFolder, "");
                if (removeBeforeCompare != null)
                {
                    foreach (var regex in removeBeforeCompare)
                    {
                        f = regex.Replace(f, "");
                    }
                }
                var per = new string[0];
                if (dict.ContainsKey(f.Length))
                {
                    per = dict[f.Length];
                }
                else
                {
                    if (Math.Abs(expectedString.Length - f.Length) > 7 && Math.Max(expectedString.Length, f.Length) > 20)
                    {
                        Log.Warning($"Two strings are too large to compare exactly: {expectedString} (len={expectedString.Length})");
                        Log.Warning($"    {f}(len={f.Length})");
                    }
                    else
                    {
                        if (expectedString.Length > f.Length)
                        {
                            var ncr = StringHelper.EstimateComb(expectedString.Length, f.Length);
                            if (ncr < 5000)
                            {
                                per = StringHelper.GetCombinations(expectedString.ToCharArray(), f.Length).ToArray();
                            }
                            else
                            {
                                Log.Warning($"Two strings are too large to compare exactly: {expectedString} (len={expectedString.Length})");
                                Log.Warning($"    {f}(len={f.Length})");
                                Log.Warning($"    possoible combinations: {ncr}");
                            }
                        }
                    }
                    dict[f.Length] = per;
                }
                if (!per.Any())
                {
                    per = new[] { expectedString };
                }
                var score = per.Min(e => StringHelper.Compare(e, f));
                if (expectedString.ToCharArray().Count(e => f.Contains(e)) == 0)
                {
                    score += 100; // penalty if no common char
                }
                return new
                {
                    Path = e,
                    ProcessedString = f,
                    Score = score
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

        private static void AddFileToJs(Dictionary<string, string> functions,
            List<string> js,
            string[] matched)
        {
            if (functions.ContainsKey("$insertFiles$"))
            {
                var func = functions["$insertFiles$"]
                        .Replace("$files$", matched.ToLiteral());
                js.Add(func + ";");
            }
            else
            {
                for (var i = 0; i < matched.Length; i++)
                {
                    var func = functions["$insertFile$"]
                        .Replace("$file$", matched[i].ToLiteral())
                        .Replace("$index$", i.ToString());
                    js.Add(func + ";");
                }
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
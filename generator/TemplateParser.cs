using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Text.RegularExpressions;

namespace Generator
{
    internal class TemplateParser
    {
        public TemplateParser()
        { }

        internal Settings Parse(string path, string settings)
        {
            var lines = settings.Split("\n");
            var inOrder = false;
            var inFolder = false;
            var orders = new List<string>();
            var folder = "";
            var settingLines = new List<string>();
            var customSettings = new List<CustomSettings>();
            var currentCustomSetting = new CustomSettings();
            foreach (var line in lines)
            {
                var l = line.Trim();
                if (l == "order:")
                {
                    inOrder = true;
                    continue;
                }
                if (l == "folder:")
                {
                    inFolder = true;
                    continue;
                }
                if (inFolder)
                {
                    folder = l;
                    inFolder = false;
                    continue;
                }

                if (inOrder)
                {
                    if (string.IsNullOrEmpty(l))
                    {
                        inOrder = false;
                    }
                    if (inOrder)
                    {
                        orders.Add(l);
                        continue;
                    }
                }

                if (folder != "" || orders.Count > 0)
                {
                    currentCustomSetting.Folder = folder;
                    currentCustomSetting.Order = orders.ToArray();
                    customSettings.Add(currentCustomSetting);
                    currentCustomSetting = new CustomSettings();
                    folder = "";
                    orders = new List<string>();
                }

                if (string.IsNullOrEmpty(l))
                {
                    continue;
                }
                if (l.StartsWith("const settings"))
                {
                    continue;
                }

                settingLines.Add(l);
            }

            var settingLineLast = settingLines.Last();
            if (settingLineLast.EndsWith(";"))
            {
                settingLines[settingLines.Count - 1] = settingLineLast.Substring(0, settingLineLast.Length - 1);
            }

            var settingText = string.Join("\n", settingLines);

            var contractResolver = new DefaultContractResolver
            {
                NamingStrategy = new CamelCaseNamingStrategy()
            };

            var settingsObj = JsonConvert.DeserializeObject<Settings>(settingText, new JsonSerializerSettings()
            {
                ContractResolver = contractResolver
            });

            var pages = settingsObj.Pages;
            for (var i = 0; i < pages.Length; i++)
            {
                pages[i].Folder = customSettings[i].Folder;
                pages[i].Order = customSettings[i].Order;
            }

            return settingsObj;
        }
    }
}
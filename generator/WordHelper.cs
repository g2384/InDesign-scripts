using Serilog;
using Word = Microsoft.Office.Interop.Word;

namespace Generator
{
    public static class WordHelper
    {
        public static string ConvertWordToHtml(string file)
        {
            var newFileName = Path.Combine(Program.GetRootDirectory(), Path.GetFileNameWithoutExtension(file) + ".html");
            if (File.Exists(newFileName))
            {
                Log.Warning($"File already exists, skip creating \"{newFileName}\"");
                return newFileName;
            }
            var word = new Word.Application();
            var doc = word.Documents.Open(file, ReadOnly: true);
            var oldFormat = doc.WebOptions.Encoding;
            if (oldFormat != Microsoft.Office.Core.MsoEncoding.msoEncodingUTF8)
            {
                Log.Warning($"Current encoding: ${oldFormat}, changing to UTF8");
                var format = Word.WdSaveFormat.wdFormatFilteredHTML;
                doc.WebOptions.Encoding = Microsoft.Office.Core.MsoEncoding.msoEncodingUTF8;
                doc.SaveAs2(newFileName, format);
                doc.WebOptions.Encoding = oldFormat;
                Log.Warning($"Set back to encoding: ${doc.WebOptions.Encoding}");
            }
            doc.Close(SaveChanges: false);
            word.Quit(SaveChanges: false);
            return newFileName;
        }
    }
}
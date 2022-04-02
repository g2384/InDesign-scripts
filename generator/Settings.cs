namespace Generator
{
    public class Settings
    {
        public Page[] Pages { get; set; }
        public IDictionary<string, string> Functions { get; set; }
    }
    public class Page
    {
        public string Title { get; set; }
        public string Folder { get; set; }
        public string[] File { get; set; }
        public string PageRange { get; set; }
        public string LoopBy { get; set; }
        public string[] Order { get; set; }
        public bool UseOrder { get; set; }
        public NextPage[] NextPages { get; set; }
    }
}
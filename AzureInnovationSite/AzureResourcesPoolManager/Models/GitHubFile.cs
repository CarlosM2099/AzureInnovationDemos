namespace AzureResourcesPoolManager.Models
{
    public class GitHubFile
    {
        public string Name { get; set; }
        public string Path { get; set; }
        public string Sha { get; set; }
        public int Size { get; set; }
        public string Url { get; set; }
        public string Html_url { get; set; }
        public string Git_url { get; set; }
        public string Download_url { get; set; }
        public string Type { get; set; }
        public _Links _links { get; set; }
    }

    public class _Links
    {
        public string Self { get; set; }
        public string Git { get; set; }
        public string Html { get; set; }
    }

}

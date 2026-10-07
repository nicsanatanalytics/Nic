namespace PdfTextFinder
{
    public class SearchResult
    {
        public string FileName { get; set; } = string.Empty;
        public string PageNumbers { get; set; } = string.Empty;
        public string Context { get; set; } = string.Empty;
        public string FullPath { get; set; } = string.Empty;
    }
}
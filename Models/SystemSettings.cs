namespace ogloszenia.Models
{
    public class SystemSettings
    {
        public int Id { get; set; }

        // Słowa zakazane
        public List<string> ForbiddenWords { get; set; } = new();

        // Dozwolone tagi HTML
        public List<string> AllowedHtmlTags { get; set; } = new()
        {
            "p", "br", "strong", "em", "u", "a", "ul", "ol", "li", "h1", "h2", "h3"
        };

        // Ograniczenia plików
        public long MaxFileSize { get; set; } = 10 * 1024 * 1024; // 10 MB
        public int MaxFilesPerAdvertisement { get; set; } = 5;
        public int MaxMediaPerAdvertisement { get; set; } = 10;

        // Wiadomości admina na stronie głównej
        public string AdminMessage { get; set; } = string.Empty;

        public DateTime LastUpdated { get; set; } = DateTime.Now;
    }

    public class RssConfig
    {
        public int Id { get; set; }
        public DateTime LastUpdated { get; set; } = DateTime.Now;
        public List<RssItem> Items { get; set; } = new();
    }

    public class RssItem
    {
        public int Id { get; set; }
        public int AdvertisementId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Link { get; set; } = string.Empty;
        public DateTime PubDate { get; set; } = DateTime.Now;
    }
}

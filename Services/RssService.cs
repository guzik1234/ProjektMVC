using System.Xml.Linq;
using ogloszenia.Models;
using ogloszenia.Data;
using Microsoft.EntityFrameworkCore;

namespace ogloszenia.Services
{
    public interface IRssService
    {
        Task<string> GenerateRssFeedAsync();
        Task UpdateRssFeedAsync();
    }

    public class RssService : IRssService
    {
        private readonly ApplicationDbContext _context;
        private readonly string _baseUrl = "http://localhost:5166";

        public RssService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<string> GenerateRssFeedAsync()
        {
            var recentAds = await _context.Advertisements
                .Include(a => a.Categories)
                .Include(a => a.User)
                .Where(a => a.Status == AdvertisementStatus.Active)
                .OrderByDescending(a => a.CreatedAt)
                .Take(50)
                .ToListAsync();

            var rss = new XDocument(
                new XDeclaration("1.0", "utf-8", null),
                new XElement("rss",
                    new XAttribute("version", "2.0"),
                    new XAttribute(XNamespace.Xmlns + "atom", "http://www.w3.org/2005/Atom"),
                    new XElement("channel",
                        new XElement("title", "Ogłoszenia - Najnowsze"),
                        new XElement("link", _baseUrl),
                        new XElement("description", "Najnowsze ogłoszenia z naszego serwisu"),
                        new XElement("language", "pl-PL"),
                        new XElement("lastBuildDate", DateTime.Now.ToString("r")),
                        new XElement(XNamespace.Get("http://www.w3.org/2005/Atom") + "link",
                            new XAttribute("href", $"{_baseUrl}/Home/Rss"),
                            new XAttribute("rel", "self"),
                            new XAttribute("type", "application/rss+xml")),
                        recentAds.Select(ad => new XElement("item",
                            new XElement("title", ad.Title),
                            new XElement("link", $"{_baseUrl}/Advertisement/Details/{ad.Id}"),
                            new XElement("description", ad.Description),
                            new XElement("pubDate", ad.CreatedAt.ToString("r")),
                            new XElement("guid", 
                                new XAttribute("isPermaLink", "true"),
                                $"{_baseUrl}/Advertisement/Details/{ad.Id}"),
                            ad.Categories.Any() ? new XElement("category", string.Join(", ", ad.Categories.Select(c => c.Name))) : null,
                            new XElement("author", $"{ad.User?.Email ?? "noreply@ogloszenia.pl"} ({ad.User?.FirstName} {ad.User?.LastName})")
                        ))
                    )
                )
            );

            return rss.Declaration + Environment.NewLine + rss.ToString();
        }

        public async Task UpdateRssFeedAsync()
        {
            var rssContent = await GenerateRssFeedAsync();
            var rssPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "rss.xml");
            
            // Save with UTF-8 encoding to preserve Polish characters
            await File.WriteAllTextAsync(rssPath, rssContent, System.Text.Encoding.UTF8);
        }
    }
}

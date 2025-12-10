using ogloszenia.Models;
using ogloszenia.Data;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace ogloszenia.Services
{
    public interface ISearchService
    {
        Task<List<Advertisement>> SearchAsync(string query, SearchOperator searchOperator = SearchOperator.And);
        Task<List<Advertisement>> AdvancedSearchAsync(Dictionary<string, string> criteria, int? categoryId = null);
    }

    public enum SearchOperator
    {
        And,
        Or,
        Not
    }

    public class SearchService : ISearchService
    {
        private readonly ApplicationDbContext _context;

        public SearchService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Advertisement>> SearchAsync(string query, SearchOperator searchOperator = SearchOperator.And)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return await _context.Advertisements
                    .Include(a => a.User)
                    .Include(a => a.Categories)
                    .Include(a => a.Media)
                    .Where(a => a.Status == AdvertisementStatus.Active)
                    .OrderByDescending(a => a.CreatedAt)
                    .ToListAsync();
            }

            // Parse query for operators
            var keywords = ParseQuery(query);
            
            var advertisements = await _context.Advertisements
                .Include(a => a.User)
                .Include(a => a.Categories)
                .Include(a => a.Media)
                .Where(a => a.Status == AdvertisementStatus.Active)
                .ToListAsync();

            switch (searchOperator)
            {
                case SearchOperator.And:
                    return advertisements.Where(a => 
                        keywords.All(k => 
                            a.Title.Contains(k, StringComparison.OrdinalIgnoreCase) || 
                            a.Description.Contains(k, StringComparison.OrdinalIgnoreCase) ||
                            a.DetailedDescription.Contains(k, StringComparison.OrdinalIgnoreCase)
                        )
                    ).ToList();

                case SearchOperator.Or:
                    return advertisements.Where(a => 
                        keywords.Any(k => 
                            a.Title.Contains(k, StringComparison.OrdinalIgnoreCase) || 
                            a.Description.Contains(k, StringComparison.OrdinalIgnoreCase) ||
                            a.DetailedDescription.Contains(k, StringComparison.OrdinalIgnoreCase)
                        )
                    ).ToList();

                case SearchOperator.Not:
                    return advertisements.Where(a => 
                        !keywords.Any(k => 
                            a.Title.Contains(k, StringComparison.OrdinalIgnoreCase) || 
                            a.Description.Contains(k, StringComparison.OrdinalIgnoreCase) ||
                            a.DetailedDescription.Contains(k, StringComparison.OrdinalIgnoreCase)
                        )
                    ).ToList();

                default:
                    return advertisements;
            }
        }

        public async Task<List<Advertisement>> AdvancedSearchAsync(Dictionary<string, string> criteria, int? categoryId = null)
        {
            var query = _context.Advertisements
                .Include(a => a.User)
                .Include(a => a.Categories)
                .Include(a => a.Media)
                .Include(a => a.AttributeValues)
                    .ThenInclude(av => av.Attribute)
                .Where(a => a.Status == AdvertisementStatus.Active)
                .AsQueryable();

            // Filter by category
            if (categoryId.HasValue)
            {
                query = query.Where(a => a.Categories.Any(c => c.Id == categoryId.Value));
            }

            var advertisements = await query.ToListAsync();

            // Filter by attribute criteria
            if (criteria != null && criteria.Any())
            {
                advertisements = advertisements.Where(ad =>
                {
                    foreach (var criterion in criteria)
                    {
                        // Skip empty values
                        if (string.IsNullOrWhiteSpace(criterion.Value))
                            continue;

                        // Extract attribute ID from key (format: "attr_123")
                        if (!criterion.Key.StartsWith("attr_"))
                            continue;

                        var attrIdStr = criterion.Key.Replace("attr_", "");
                        if (!int.TryParse(attrIdStr, out int attrId))
                            continue;

                        var attrValue = ad.AttributeValues.FirstOrDefault(av => av.AttributeId == attrId);
                        if (attrValue == null || !attrValue.Value.Contains(criterion.Value, StringComparison.OrdinalIgnoreCase))
                        {
                            return false;
                        }
                    }
                    return true;
                }).ToList();
            }

            return advertisements;
        }

        private List<string> ParseQuery(string query)
        {
            // Split by spaces and filter out empty strings
            return query.Split(new[] { ' ', ',', ';' }, StringSplitOptions.RemoveEmptyEntries)
                        .Select(k => k.Trim())
                        .Where(k => !string.IsNullOrWhiteSpace(k))
                        .ToList();
        }
    }
}

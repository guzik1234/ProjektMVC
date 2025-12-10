using ogloszenia.Models;
using ogloszenia.Data;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace ogloszenia.Services
{
    public interface IModerationService
    {
        Task<(bool isValid, List<string> forbiddenWords)> ValidateContentAsync(string content);
        string SanitizeHtml(string html);
        Task<List<string>> GetForbiddenWordsAsync();
        Task<List<string>> GetAllowedHtmlTagsAsync();
    }

    public class ModerationService : IModerationService
    {
        private readonly ApplicationDbContext _context;

        public ModerationService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<(bool isValid, List<string> forbiddenWords)> ValidateContentAsync(string content)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                return (true, new List<string>());
            }

            var settings = await GetSystemSettingsAsync();
            var forbiddenWords = settings?.ForbiddenWords ?? new List<string>();

            var foundWords = new List<string>();
            var contentLower = content.ToLower();

            foreach (var word in forbiddenWords)
            {
                if (contentLower.Contains(word.ToLower()))
                {
                    foundWords.Add(word);
                }
            }

            return (foundWords.Count == 0, foundWords);
        }

        public string SanitizeHtml(string html)
        {
            if (string.IsNullOrWhiteSpace(html))
            {
                return string.Empty;
            }

            var settings = GetSystemSettingsAsync().Result;
            var allowedTags = settings?.AllowedHtmlTags ?? new List<string> 
            { 
                "p", "br", "strong", "em", "u", "a", "ul", "ol", "li", "h1", "h2", "h3" 
            };

            // Remove all tags except allowed ones
            var allowedTagsPattern = string.Join("|", allowedTags.Select(t => Regex.Escape(t)));
            var pattern = $@"<(?!\/?\s*({allowedTagsPattern})\b)[^>]+>";

            var sanitized = Regex.Replace(html, pattern, string.Empty, RegexOptions.IgnoreCase);

            // Remove potentially dangerous attributes from allowed tags
            sanitized = Regex.Replace(sanitized, @"(on\w+|javascript:)[^""'>\s]*", string.Empty, RegexOptions.IgnoreCase);

            return sanitized;
        }

        public async Task<List<string>> GetForbiddenWordsAsync()
        {
            var settings = await GetSystemSettingsAsync();
            return settings?.ForbiddenWords ?? new List<string>();
        }

        public async Task<List<string>> GetAllowedHtmlTagsAsync()
        {
            var settings = await GetSystemSettingsAsync();
            return settings?.AllowedHtmlTags ?? new List<string> 
            { 
                "p", "br", "strong", "em", "u", "a", "ul", "ol", "li", "h1", "h2", "h3" 
            };
        }

        private async Task<SystemSettings?> GetSystemSettingsAsync()
        {
            var settings = await _context.SystemSettings.FirstOrDefaultAsync();
            if (settings == null)
            {
                settings = new SystemSettings
                {
                    ForbiddenWords = new List<string>(),
                    AllowedHtmlTags = new List<string> 
                    { 
                        "p", "br", "strong", "em", "u", "a", "ul", "ol", "li", "h1", "h2", "h3" 
                    },
                    MaxFileSize = 10 * 1024 * 1024,
                    MaxFilesPerAdvertisement = 5,
                    MaxMediaPerAdvertisement = 10
                };
                _context.SystemSettings.Add(settings);
                await _context.SaveChangesAsync();
            }
            return settings;
        }
    }
}

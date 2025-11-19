using System.ComponentModel.DataAnnotations;

namespace ogloszenia.Models
{
    public class User
    {
        public int Id { get; set; }

        [Required]
        public string Username { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        public string FirstName { get; set; } = string.Empty;

        public string LastName { get; set; } = string.Empty;

        public bool IsAdmin { get; set; } = false;

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public int PageSize { get; set; } = 10; // Ustawienie liczby ogłoszeń na stronie

        public string Theme { get; set; } = "default"; // Wybór skórki

        public string Language { get; set; } = "pl"; // Język interfejsu

        // Newsletter categories
        public List<int> NewsletterCategoryIds { get; set; } = new();

        // Newsletter search criteria (zaawansowany newsletter)
        public List<NewsletterCriteria> NewsletterCriteria { get; set; } = new();

        // Relacja z ogłoszeniami
        public virtual List<Advertisement> Advertisements { get; set; } = new();
    }

    public class NewsletterCriteria
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public Dictionary<int, string> AttributeFilters { get; set; } = new(); // AttributeId -> wartość
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}

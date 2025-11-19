using System.ComponentModel.DataAnnotations;

namespace ogloszenia.Models
{
    public class Category
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        // Hierarchia kategorii - parent category
        public int? ParentCategoryId { get; set; }
        public virtual Category? ParentCategory { get; set; }

        // Dzieci (subcategories)
        public virtual List<Category> ChildCategories { get; set; } = new();

        // Atrybuty definiowane dla tej kategorii
        public virtual List<AdvertisementAttribute> Attributes { get; set; } = new();

        // Ogłoszenia w tej kategorii
        public virtual List<Advertisement> Advertisements { get; set; } = new();

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}

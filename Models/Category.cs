namespace Projekt_MVC.Models
{
    public class Category
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int? ParentCategoryId { get; set; } // Klucz obcy do rodzica (może być null dla korzenia)
        public virtual Category ParentCategory { get; set; }
        public virtual ICollection<Category> ChildCategories { get; set; } = new List<Category>();
        public virtual ICollection<Advertisement> Advertisements { get; set; } = new List<Advertisement>();
    }
}
namespace Projekt_MVC.Models
{
    public class Category
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int? ParentCategoryId { get; set; }
        public virtual Category? ParentCategory { get; set; }
        public virtual ICollection<Category> ChildCategories { get; set; } = null!;
        public virtual ICollection<Advertisement> Advertisements { get; set; } = null!;
    }
}

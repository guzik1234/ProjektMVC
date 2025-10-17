using System.ComponentModel.DataAnnotations;

namespace Projekt_MVC.Models
{
    public class Advertisement
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Tytuł jest wymagany")]
        [StringLength(100, ErrorMessage = "Tytuł może mieć max 100 znaków")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Opis jest wymagany")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Cena jest wymagana")]
        [Range(0, 1000000, ErrorMessage = "Cena musi być między 0 a 1 000 000")]
        public decimal Price { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public int UserId { get; set; }
        public virtual User User { get; set; }
        public virtual ICollection<Category> Categories { get; set; } = new List<Category>();
    }
}
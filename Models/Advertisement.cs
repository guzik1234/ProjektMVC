using System.ComponentModel.DataAnnotations;

namespace Projekt_MVC.Models
{
    public class Advertisement
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Tytu³ jest wymagany")]
        [StringLength(100, ErrorMessage = "Tytu³ mo¿e mieæ max 100 znaków")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Opis jest wymagany")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Cena jest wymagana")]
        [Range(0, 1000000, ErrorMessage = "Cena musi byæ miêdzy 0 a 1 000 000")]
        public decimal Price { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public int UserId { get; set; }
        public virtual User? User { get; set; }
        public virtual ICollection<Category> Categories { get; set; } = null!;
    }
}

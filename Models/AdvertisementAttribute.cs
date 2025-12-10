using System.ComponentModel.DataAnnotations;

namespace ogloszenia.Models
{
    public class AdvertisementAttribute
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        // Typ atrybutu: shorttext, longtext, int, real, bool, dictionary
        [Required]
        public string AttributeType { get; set; } = "shorttext";

        // Czy atrybut jest wymagany
        public bool IsRequired { get; set; } = false;

        // Dla typu dictionary - referencja do słownika
        public int? DictionaryId { get; set; }
        public virtual Dictionary? Dictionary { get; set; }

        // Kategoria, do której należy ten atrybut
        public int CategoryId { get; set; }
        public virtual Category? Category { get; set; }

        // Wartości atrybutów w ogłoszeniach
        public virtual List<AdvertisementAttributeValue> Values { get; set; } = new();

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }

    public class AdvertisementAttributeValue
    {
        public int Id { get; set; }

        public int AdvertisementId { get; set; }
        public virtual Advertisement? Advertisement { get; set; }

        public int AttributeId { get; set; }
        public virtual AdvertisementAttribute? Attribute { get; set; }

        // Wartość atrybutu (przechowywana jako string)
        public string Value { get; set; } = string.Empty;

        // Dla dictionary type - ID wartości słownika
        public int? DictionaryValueId { get; set; }
    }
}

namespace ogloszenia.Models
{
    public class Dictionary
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        // Wartości w słowniku
        public virtual List<DictionaryValue> Values { get; set; } = new();

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }

    public class DictionaryValue
    {
        public int Id { get; set; }

        public string Value { get; set; } = string.Empty;

        public int DictionaryId { get; set; }
        public virtual Dictionary? Dictionary { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}

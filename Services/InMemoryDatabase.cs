using ogloszenia.Models;

namespace ogloszenia.Services
{
    /// <summary>
    /// In-memory database service - przechowuje wszystkie dane w listach w pamięci
    /// </summary>
    public static class InMemoryDatabase
    {
        // Users
        public static List<User> Users { get; set; } = new();

        // Categories (hierarchical)
        public static List<Category> Categories { get; set; } = new();

        // Advertisement Attributes and Dictionaries
        public static List<AdvertisementAttribute> AdvertisementAttributes { get; set; } = new();
        public static List<Dictionary> Dictionaries { get; set; } = new();
        public static List<DictionaryValue> DictionaryValues { get; set; } = new();

        // Advertisements and related
        public static List<Advertisement> Advertisements { get; set; } = new();
        public static List<AdvertisementAttributeValue> AttributeValues { get; set; } = new();
        public static List<AdvertisementMedia> Media { get; set; } = new();
        public static List<AdvertisementFile> Files { get; set; } = new();
        public static List<ModerationReport> ModerationReports { get; set; } = new();

        // System
        public static SystemSettings SystemSettings { get; set; } = new();
        public static List<NewsletterCriteria> NewsletterCriteria { get; set; } = new();
        public static RssConfig RssConfig { get; set; } = new();

        // Counter for IDs
        private static int _userId = 1;
        private static int _categoryId = 1;
        private static int _attributeId = 1;
        private static int _dictionaryId = 1;
        private static int _dictionaryValueId = 1;
        private static int _advertisementId = 1;
        private static int _attributeValueId = 1;
        private static int _mediaId = 1;
        private static int _fileId = 1;
        private static int _reportId = 1;

        // Initialize sample data
        public static void Initialize()
        {
            if (Users.Count > 0) return; // Already initialized

            // Admin user
            var admin = new User
            {
                Id = _userId++,
                Username = "admin",
                Email = "admin@example.com",
                PasswordHash = HashPassword("admin123"),
                FirstName = "Admin",
                LastName = "User",
                IsAdmin = true,
                IsActive = true,
                CreatedAt = DateTime.Now.AddDays(-30)
            };
            Users.Add(admin);

            // Sample user
            var user1 = new User
            {
                Id = _userId++,
                Username = "user1",
                Email = "user1@example.com",
                PasswordHash = HashPassword("user123"),
                FirstName = "Jan",
                LastName = "Kowalski",
                IsAdmin = false,
                IsActive = true,
                CreatedAt = DateTime.Now.AddDays(-10)
            };
            Users.Add(user1);

            // Sample categories (hierarchical)
            var catVehicles = new Category
            {
                Id = _categoryId++,
                Name = "Pojazdy",
                Description = "Samochody, motocykle i pojazdy",
                ParentCategoryId = null
            };
            Categories.Add(catVehicles);

            var catCars = new Category
            {
                Id = _categoryId++,
                Name = "Samochody",
                Description = "Osobowe samochody",
                ParentCategoryId = catVehicles.Id,
                ParentCategory = catVehicles
            };
            Categories.Add(catCars);
            catVehicles.ChildCategories.Add(catCars);

            var catElectronics = new Category
            {
                Id = _categoryId++,
                Name = "Elektronika",
                Description = "Urządzenia elektroniczne",
                ParentCategoryId = null
            };
            Categories.Add(catElectronics);

            // Sample attributes for cars category
            var attrBrand = new AdvertisementAttribute
            {
                Id = _attributeId++,
                Name = "Marka",
                Description = "Marka pojazdu",
                AttributeType = "shorttext",
                IsRequired = true,
                CategoryId = catCars.Id,
                Category = catCars
            };
            AdvertisementAttributes.Add(attrBrand);
            catCars.Attributes.Add(attrBrand);

            var attrYear = new AdvertisementAttribute
            {
                Id = _attributeId++,
                Name = "Rok produkcji",
                Description = "Rok produkcji pojazdu",
                AttributeType = "int",
                IsRequired = true,
                CategoryId = catCars.Id,
                Category = catCars
            };
            AdvertisementAttributes.Add(attrYear);
            catCars.Attributes.Add(attrYear);

            var attrPrice = new AdvertisementAttribute
            {
                Id = _attributeId++,
                Name = "Cena",
                Description = "Cena pojazdu",
                AttributeType = "real",
                IsRequired = true,
                CategoryId = catCars.Id,
                Category = catCars
            };
            AdvertisementAttributes.Add(attrPrice);
            catCars.Attributes.Add(attrPrice);

            // Sample advertisement
            var ad1 = new Advertisement
            {
                Id = _advertisementId++,
                Title = "Sprzedam Opla Astrę",
                Description = "Piękny, zadbany Opel Astra z 2015 roku. Przebieg 120 tys km.",
                DetailedDescription = "<p>Opel Astra w doskonałym stanie.</p><p>Stan techniczny: bardzo dobry</p>",
                UserId = user1.Id,
                User = user1,
                Status = AdvertisementStatus.Active,
                ViewCount = 45,
                CreatedAt = DateTime.Now.AddDays(-5)
            };
            Advertisements.Add(ad1);
            ad1.Categories.Add(catCars);
            catCars.Advertisements.Add(ad1);
            user1.Advertisements.Add(ad1);

            // Sample attribute values
            var attrVal1 = new AdvertisementAttributeValue
            {
                Id = _attributeValueId++,
                AdvertisementId = ad1.Id,
                Advertisement = ad1,
                AttributeId = attrBrand.Id,
                Attribute = attrBrand,
                Value = "Opel"
            };
            AttributeValues.Add(attrVal1);
            ad1.AttributeValues.Add(attrVal1);

            var attrVal2 = new AdvertisementAttributeValue
            {
                Id = _attributeValueId++,
                AdvertisementId = ad1.Id,
                Advertisement = ad1,
                AttributeId = attrYear.Id,
                Attribute = attrYear,
                Value = "2015"
            };
            AttributeValues.Add(attrVal2);
            ad1.AttributeValues.Add(attrVal2);

            var attrVal3 = new AdvertisementAttributeValue
            {
                Id = _attributeValueId++,
                AdvertisementId = ad1.Id,
                Advertisement = ad1,
                AttributeId = attrPrice.Id,
                Attribute = attrPrice,
                Value = "25000"
            };
            AttributeValues.Add(attrVal3);
            ad1.AttributeValues.Add(attrVal3);

            // System settings
            SystemSettings = new SystemSettings
            {
                Id = 1,
                ForbiddenWords = new List<string> { "spam", "scam", "fake" },
                AdminMessage = "Witamy w serwisie ogłoszeń drobnych!"
            };

            // RSS
            RssConfig = new RssConfig
            {
                Id = 1,
                Items = new List<RssItem>
                {
                    new RssItem
                    {
                        Id = 1,
                        AdvertisementId = ad1.Id,
                        Title = ad1.Title,
                        Description = ad1.Description,
                        Link = $"/Advertisement/Details/{ad1.Id}",
                        PubDate = ad1.CreatedAt
                    }
                }
            };
        }

        public static string HashPassword(string password)
        {
            // Simple hash for demo - in production use proper hashing
            using (var sha256 = System.Security.Cryptography.SHA256.Create())
            {
                var hashBytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashBytes);
            }
        }

        public static bool VerifyPassword(string password, string hash)
        {
            var hashOfInput = HashPassword(password);
            return hashOfInput == hash;
        }

        public static int GetNextUserId() => _userId++;
        public static int GetNextCategoryId() => _categoryId++;
        public static int GetNextAttributeId() => _attributeId++;
        public static int GetNextDictionaryId() => _dictionaryId++;
        public static int GetNextDictionaryValueId() => _dictionaryValueId++;
        public static int GetNextAdvertisementId() => _advertisementId++;
        public static int GetNextAttributeValueId() => _attributeValueId++;
        public static int GetNextMediaId() => _mediaId++;
        public static int GetNextFileId() => _fileId++;
        public static int GetNextReportId() => _reportId++;
    }
}

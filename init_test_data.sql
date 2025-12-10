-- Test data for ogloszenia application

-- Add test categories (if not exists)
INSERT OR IGNORE INTO Categories (Id, Name, ParentCategoryId, CreatedAt) VALUES
(1, 'Elektronika', NULL, datetime('now')),
(2, 'Motoryzacja', NULL, datetime('now')),
(3, 'Dom i Ogród', NULL, datetime('now')),
(4, 'Telefony', 1, datetime('now')),
(5, 'Komputery', 1, datetime('now')),
(6, 'Samochody', 2, datetime('now'));

-- Add test advertisements
INSERT OR IGNORE INTO Advertisements (Id, UserId, Title, Description, DetailedDescription, Status, ViewCount, CreatedAt) VALUES
(10, 1, 'iPhone 15 Pro Max 256GB', 'Nowy telefon w idealnym stanie', 'Sprzedam nowy iPhone 15 Pro Max 256GB w kolorze czarnym. Telefon jest w idealnym stanie, bez żadnych uszkodzeń. W zestawie oryginalny kabel i instrukcja obsługi. Możliwość sprawdzenia przed zakupem. Kontakt: tel lub email.', 'Active', 0, datetime('now', '-5 days')),
(11, 1, 'Laptop Dell XPS 13', 'Świetny laptop do pracy', 'Laptop Dell XPS 13 w bardzo dobrym stanie. Procesor Intel i7, 16GB RAM, dysk SSD 512GB. Idealny do pracy biurowej i programowania. Bateria trzyma około 8 godzin. Sprzedam bo kupuję nowy model.', 'Active', 0, datetime('now', '-4 days')),
(12, 1, 'Samsung Galaxy S23', 'Telefon w super stanie', 'Samsung Galaxy S23 128GB, używany 6 miesięcy. Stan bardzo dobry, brak zarysowań. Sprzedaję razem z etui ochronnym i szkłem hartowanym. Działa bez zarzutu, bateria trzyma cały dzień.', 'Active', 0, datetime('now', '-3 days')),
(13, 2, 'Toyota Corolla 2018', 'Zadbany samochód rodzinny', 'Toyota Corolla 2018, 1.6 benzyna, 132 KM. Przebieg 85 000 km. Samochód kupiony w polskim salonie, regularnie serwisowany. Klimatyzacja, nawigacja, czujniki parkowania. Pierwszy właściciel w Polsce. Cena do negocjacji.', 'Active', 0, datetime('now', '-2 days')),
(14, 2, 'Rower górski Trek', 'Rower MTB w świetnym stanie', 'Rower górski Trek Marlin 7, rozmiar L. Amortyzowany widelec, 21 biegów. Używany sezonowo, regularnie serwisowany. Świetny do jazdy terenowej. W zestawie błotniki i bidon.', 'Active', 0, datetime('now', '-1 days')),
(15, 2, 'Telewizor Samsung 55 cali', 'Smart TV 4K', 'Telewizor Samsung 55 cali, 4K UHD, Smart TV. Stan bardzo dobry, kupiony 2 lata temu. Sprzedam bo przeprowadzam się i nie ma miejsca. Wszystkie funkcje działają bez zarzutu. Netflix, YouTube, itp. Pilot i kable w zestawie.', 'Active', 0, datetime('now'));

-- Link advertisements to categories
INSERT OR IGNORE INTO AdvertisementCategory (AdvertisementsId, CategoriesId) VALUES
(10, 1), (10, 4),  -- iPhone - Elektronika, Telefony
(11, 1), (11, 5),  -- Dell XPS - Elektronika, Komputery  
(12, 1), (12, 4),  -- Samsung - Elektronika, Telefony
(13, 2), (13, 6),  -- Toyota - Motoryzacja, Samochody
(14, 3),           -- Rower - Dom i Ogród
(15, 1);           -- Telewizor - Elektronika

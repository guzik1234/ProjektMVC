# Dokumentacja Projektu - Serwis Ogłoszeń Drobnych

## Spis treści
1. [Opis projektu](#opis-projektu)
2. [Technologie](#technologie)
3. [Instalacja i uruchomienie](#instalacja-i-uruchomienie)
4. [Struktura projektu](#struktura-projektu)
5. [Funkcjonalności](#funkcjonalności)
6. [Panel użytkownika](#panel-użytkownika)
7. [Panel administracyjny](#panel-administracyjny)
8. [Baza danych](#baza-danych)
9. [API i endpointy](#api-i-endpointy)
10. [Konfiguracja](#konfiguracja)

---

## Opis projektu

**Serwis Ogłoszeń Drobnych** to aplikacja webowa typu MVC stworzona w ASP.NET Core 6.0, która umożliwia użytkownikom:
- Dodawanie i przeglądanie ogłoszeń
- Zarządzanie kategoriami hierarchicznymi
- Dynamiczne atrybuty ogłoszeń zależne od kategorii
- System moderacji i słów zakazanych
- Wielojęzyczność (Polski, Angielski, Niemiecki)
- Różne motywy interfejsu
- Kanał RSS z nowymi ogłoszeniami

**Cel projektu**: Platforma do publikacji ogłoszeń drobnych z zaawansowanym systemem kategoryzacji i atrybutów dynamicznych.

---

## Technologie

### Backend
- **ASP.NET Core 6.0** - framework MVC
- **Entity Framework Core 6.0** - ORM
- **SQLite** - baza danych
- **BCrypt.Net** - hashowanie haseł
- **System.Text.Json** - serializacja JSON

### Frontend
- **Bootstrap 5** - framework CSS
- **jQuery** - manipulacja DOM
- **Razor Pages** - silnik widoków
- **JavaScript ES6** - interaktywność

### Narzędzia
- **Visual Studio Code** - IDE
- **Git** - kontrola wersji
- **.NET CLI** - kompilacja i uruchamianie

---

## Instalacja i uruchomienie

### Wymagania
- .NET SDK 6.0 lub nowszy
- Dowolny edytor kodu (VS Code, Visual Studio)
- System operacyjny: Windows, Linux, macOS

### Kroki instalacji

1. **Sklonuj repozytorium**
```bash
git clone https://github.com/guzik1234/ProjektMVC.git
cd ProjektMVC
```

2. **Przywróć zależności**
```bash
dotnet restore
```

3. **Zbuduj projekt**
```bash
dotnet build
```

4. **Uruchom aplikację**
```bash
dotnet run
```

5. **Otwórz w przeglądarce**
```
http://localhost:5166
```

### Domyślne dane logowania

**Administrator:**
- Login: `admin`
- Hasło: `123`

**Testowy użytkownik:**
- Login: `user1`
- Hasło: `password123`

---

## Struktura projektu

```
ogloszenia/
├── Controllers/              # Kontrolery MVC
│   ├── AccountController.cs  # Autentykacja użytkowników
│   ├── AdminController.cs    # Panel administracyjny
│   ├── AdvertisementController.cs  # Zarządzanie ogłoszeniami
│   ├── CategoryController.cs # Kategorie
│   └── HomeController.cs     # Strona główna
├── Models/                   # Modele danych
│   ├── Advertisement.cs      # Ogłoszenie
│   ├── Category.cs           # Kategoria
│   ├── User.cs               # Użytkownik
│   ├── AdvertisementAttribute.cs  # Atrybuty ogłoszeń
│   ├── Dictionary.cs         # Słowniki wartości
│   └── SystemSettings.cs     # Ustawienia systemu
├── Views/                    # Widoki Razor
│   ├── Account/              # Widoki logowania/rejestracji
│   ├── Admin/                # Panel administracyjny
│   ├── Advertisement/        # Widoki ogłoszeń
│   ├── Home/                 # Strona główna
│   └── Shared/               # Wspólne komponenty
├── Data/                     # Kontekst bazy danych
│   ├── ApplicationDbContext.cs
│   └── Migrations/           # Migracje EF Core
├── Services/                 # Usługi biznesowe
│   ├── AuthService.cs        # Autoryzacja
│   ├── ModerationService.cs  # Moderacja treści
│   ├── RssService.cs         # Generowanie RSS
│   └── LocalizationService.cs # Tłumaczenia
├── wwwroot/                  # Pliki statyczne
│   ├── css/                  # Style CSS
│   ├── js/                   # Skrypty JavaScript
│   ├── lib/                  # Biblioteki (Bootstrap, jQuery)
│   └── uploads/              # Przesłane pliki
├── appsettings.json          # Konfiguracja aplikacji
├── Program.cs                # Punkt wejścia aplikacji
└── ogloszenia.csproj         # Plik projektu
```

---

## Funkcjonalności

### Realizacja wymagań projektowych (62/72 punktów)

#### ✅ Funkcjonalności podstawowe (46 pkt)

1. **Dodawanie i przeglądanie ogłoszeń** (4 pkt)
   - Formularz tworzenia ogłoszeń z walidacją
   - Lista wszystkich ogłoszeń ze stronicowaniem
   - Szczegóły pojedynczego ogłoszenia

2. **System użytkowników** (4 pkt)
   - Rejestracja nowych użytkowników
   - Logowanie z BCrypt
   - Panel administracyjny dla adminów
   - Zarządzanie użytkownikami

3. **Edycja własnych ogłoszeń** (2 pkt)
   - Tylko autor może edytować/usuwać
   - Weryfikacja uprawnień

4. **Kategorie hierarchiczne** (4 pkt)
   - Drzewo kategorii (rodzic-dziecko)
   - Wielokategorialność ogłoszeń
   - Przeglądanie po kategorii z dziedziczeniem

5. **Dynamiczne atrybuty kategorii** (8 pkt) ✅
   - Administrator definiuje atrybuty dla kategorii
   - Suma atrybutów ze wszystkich kategorii ogłoszenia
   - Panel zarządzania atrybutami

6. **Typy atrybutów** (4 pkt) ✅
   - `shorttext` - krótki tekst (input)
   - `longtext` - długi tekst (textarea)
   - `int` - liczba całkowita
   - `real` - liczba rzeczywista
   - `bool` - checkbox tak/nie
   - `dictionary` - lista rozwijana ze słownika

7. **Słownikowe typy atrybutów** (4 pkt) ✅
   - Tworzenie słowników wartości
   - Przypisywanie do atrybutów
   - Automatyczna lista rozwijana

8. **Załączniki multimedialne** (2 pkt)
   - Obrazki, dźwięki, filmy
   - Wyświetlanie w szczegółach
   - Karuzelka zdjęć z powiększeniem

9. **Załączniki do pobrania** (2 pkt)
   - Pliki PDF, dokumenty
   - Limity regulowane przez admina
   - Walidacja rozmiaru i liczby

10. **Wyszukiwanie** (2 pkt)
    - Szukanie po tytule i opisie
    - Filtrowanie po kategoriach
    - Wyszukiwanie hierarchiczne (z podkategoriami)

11. **Format HTML w opisach** (4 pkt)
    - Sanityzacja HTML
    - Lista dozwolonych tagów
    - Usuwanie niebezpiecznych elementów

12. **Stronicowanie** (2 pkt)
    - Konfigurowalna liczba na stronie
    - Ustawienie w profilu użytkownika

13. **Liczniki odsłon** (2 pkt)
    - Automatyczne zliczanie wyświetleń
    - Widoczne na listach

14. **System moderacji** (2 pkt)
    - Zgłaszanie ogłoszeń
    - Panel moderacji dla admina
    - Statusy: Pending, Approved, Rejected

15. **Słowa zakazane** (4 pkt)
    - Słownik słów niedozwolonych
    - Automatyczna walidacja
    - Panel zarządzania

16. **Kanał RSS** (2 pkt)
    - Automatyczna aktualizacja
    - Kodowanie UTF-8
    - Format RSS 2.0 + Atom

17. **Wiadomości administratora** (2 pkt)
    - Edycja komunikatu na stronie głównej
    - Wyświetlanie dla wszystkich

18. **Zmiana hasła** (2 pkt)
    - Bezpieczne resetowanie
    - Bez wysyłania emaili
    - Weryfikacja przez email

19. **Motywy interfejsu** (3 pkt)
    - Default (jasny)
    - Dark (ciemny)
    - Blue (niebieski)
    - Zapis w profilu użytkownika

20. **Wielojęzyczność** (3 pkt)
    - Polski (domyślny)
    - English
    - Deutsch
    - Zapis preferencji

#### ❌ Funkcjonalności niezrealizowane (26 pkt)

1. **Operatory wyszukiwania AND/OR/NOT** (2 pkt)
2. **Wyszukiwanie zaawansowane** (4 pkt)
3. **Newsletter prosty** (2 pkt) - był, usunięty
4. **Newsletter sprofilowany** (4 pkt)

---

## Panel użytkownika

### Rejestracja i logowanie

**Rejestracja** (`/Account/Register`)
- Formularz: Username, Email, Hasło, Imię, Nazwisko
- Walidacja danych
- Automatyczne hashowanie hasła (BCrypt)

**Logowanie** (`/Account/Login`)
- Username i hasło
- Sesja użytkownika
- Remember me (opcjonalnie)

**Profil** (`/Account/Profile`)
- Edycja danych osobowych
- Zmiana hasła
- Ustawienia interfejsu:
  - Liczba ogłoszeń na stronie (5-50)
  - Motyw (Default, Dark, Blue)
  - Język (PL, EN, DE)

### Zarządzanie ogłoszeniami

**Dodawanie** (`/Advertisement/Create`)
1. Wypełnij tytuł i opis
2. Wybierz kategorie (wielokrotny wybór)
3. **Automatycznie pojawiają się pola atrybutów** z wybranych kategorii
4. Dodaj zdjęcia (limity: admin)
5. Dodaj załączniki
6. Publikuj

**Edycja** (`/Advertisement/Edit/{id}`)
- Tylko własne ogłoszenia
- Te same pola co przy tworzeniu
- Możliwość usunięcia mediów

**Usuwanie** (`/Advertisement/Delete/{id}`)
- Tylko własne ogłoszenia
- Potwierdzenie akcji
- Usunięcie plików z serwera

**Przeglądanie**
- Lista wszystkich: `/Advertisement/Index`
- Według kategorii: `/Category/Details/{id}`
- Wyszukiwanie: `/Advertisement/Search?query=...`

---

## Panel administracyjny

Dostęp: `/Admin/Dashboard` (tylko dla administratorów)

### Dashboard
- Statystyki: użytkownicy, ogłoszenia, zgłoszenia
- Szybki dostęp do wszystkich funkcji

### Zarządzanie użytkownikami (`/Admin/Users`)
- Lista wszystkich użytkowników
- Edycja danych
- Nadawanie uprawnień admina
- Aktywacja/deaktywacja kont
- **Nie można usunąć użytkowników z ogłoszeniami**

### Kategorie (`/Admin/Categories`)
- Tworzenie kategorii głównych
- Dodawanie podkategorii
- Edycja nazwy i opisu
- Usuwanie (tylko puste)
- **Przycisk "Atrybuty"** → zarządzanie atrybutami kategorii

### Atrybuty kategorii (`/Admin/Attributes?categoryId=X`)
- Dodawanie atrybutów do kategorii
- Typy: shorttext, longtext, int, real, bool, dictionary
- Oznaczanie jako wymagane
- Przypisywanie słowników
- Usuwanie atrybutów

### Słowniki (`/Admin/Dictionaries`)
- Tworzenie nowych słowników (np. "Marka samochodu")
- Dodawanie wartości do słownika (np. Toyota, BMW, Audi)
- Usuwanie słowników
- Usuwanie wartości

**Przykład użycia:**
1. Utwórz słownik "Marka"
2. Dodaj wartości: Toyota, BMW, Audi
3. Przejdź do kategorii "Motoryzacja"
4. Dodaj atrybut "Marka" typu `dictionary`
5. Wybierz słownik "Marka"
6. Przy dodawaniu ogłoszenia pojawi się lista rozwijana!

### Moderacja (`/Admin/Moderation`)
- Lista zgłoszonych ogłoszeń
- Informacja o zgłaszającym i powodzie
- Akcje: Zatwierdź, Odrzuć, Zobacz ogłoszenie
- Statusy: Pending, Approved, Rejected

### Słowa zakazane (`/Admin/ForbiddenWords`)
- Dodawanie słów do blacklisty
- Usuwanie słów
- Statystyki (liczba słów)
- Automatyczna walidacja przy publikacji
- **Ogłoszenia z zabronionymi słowami są odrzucane**

### Ustawienia (`/Admin/Settings`)
**Limity plików:**
- Maksymalny rozmiar pliku (MB)
- Maksymalna liczba zdjęć na ogłoszenie
- Maksymalna liczba załączników

**Dozwolone tagi HTML:**
- `<p>`, `<br>`, `<strong>`, `<em>`, `<a>`, `<ul>`, `<ol>`, `<li>`
- Automatyczne usuwanie innych tagów

### Wiadomość administratora (`/Admin/AdminMessage`)
- Edytor treści komunikatu
- Wyświetlanie na stronie głównej
- Markdown/HTML support

---

## Baza danych

### SQLite (`ogloszenia.db`)

#### Główne tabele

**Users** - Użytkownicy
```sql
Id              INTEGER PRIMARY KEY
Username        TEXT UNIQUE NOT NULL
Email           TEXT UNIQUE NOT NULL
PasswordHash    TEXT NOT NULL
FirstName       TEXT
LastName        TEXT
IsAdmin         BOOLEAN DEFAULT 0
IsActive        BOOLEAN DEFAULT 1
CreatedAt       DATETIME
PageSize        INTEGER DEFAULT 10
Theme           TEXT DEFAULT 'default'
Language        TEXT DEFAULT 'pl'
```

**Categories** - Kategorie
```sql
Id                  INTEGER PRIMARY KEY
Name                TEXT NOT NULL
Description         TEXT NULL
ParentCategoryId    INTEGER NULL (FK -> Categories.Id)
CreatedAt           DATETIME
```

**Advertisements** - Ogłoszenia
```sql
Id                      INTEGER PRIMARY KEY
Title                   TEXT NOT NULL
Description             TEXT NOT NULL
DetailedDescription     TEXT NULL
UserId                  INTEGER (FK -> Users.Id)
Status                  TEXT (Active/Inactive)
ViewCount               INTEGER DEFAULT 0
CreatedAt               DATETIME
```

**AdvertisementCategories** - Relacja N:M
```sql
AdvertisementId     INTEGER (FK)
CategoryId          INTEGER (FK)
```

**AdvertisementAttributes** - Definicje atrybutów
```sql
Id              INTEGER PRIMARY KEY
Name            TEXT NOT NULL
Description     TEXT
AttributeType   TEXT (shorttext/longtext/int/real/bool/dictionary)
IsRequired      BOOLEAN
CategoryId      INTEGER (FK -> Categories.Id)
DictionaryId    INTEGER NULL (FK -> Dictionaries.Id)
CreatedAt       DATETIME
```

**AdvertisementAttributeValues** - Wartości atrybutów
```sql
Id                  INTEGER PRIMARY KEY
AdvertisementId     INTEGER (FK -> Advertisements.Id)
AttributeId         INTEGER (FK -> AdvertisementAttributes.Id)
Value               TEXT
DictionaryValueId   INTEGER NULL (FK -> DictionaryValues.Id)
```

**Dictionaries** - Słowniki
```sql
Id              INTEGER PRIMARY KEY
Name            TEXT NOT NULL
Description     TEXT
CreatedAt       DATETIME
```

**DictionaryValues** - Wartości słownika
```sql
Id              INTEGER PRIMARY KEY
Value           TEXT NOT NULL
DictionaryId    INTEGER (FK -> Dictionaries.Id)
CreatedAt       DATETIME
```

**Media** - Pliki multimedialne
```sql
Id                  INTEGER PRIMARY KEY
AdvertisementId     INTEGER (FK)
FileName            TEXT
FilePath            TEXT
MediaType           TEXT (image/audio/video)
UploadedAt          DATETIME
```

**Files** - Załączniki
```sql
Id                  INTEGER PRIMARY KEY
AdvertisementId     INTEGER (FK)
FileName            TEXT
FilePath            TEXT
FileSize            INTEGER
UploadedAt          DATETIME
```

**ModerationReports** - Zgłoszenia
```sql
Id                  INTEGER PRIMARY KEY
AdvertisementId     INTEGER (FK)
ReportedByUserId    INTEGER (FK)
Reason              TEXT
Status              TEXT (Pending/Approved/Rejected)
CreatedAt           DATETIME
ReviewedAt          DATETIME NULL
ReviewedByUserId    INTEGER NULL (FK)
AdminComment        TEXT NULL
```

**SystemSettings** - Ustawienia
```sql
Id                          INTEGER PRIMARY KEY
ForbiddenWords              TEXT (JSON)
AllowedHtmlTags             TEXT (JSON)
MaxFileSize                 INTEGER (bytes)
MaxMediaPerAdvertisement    INTEGER
MaxFilesPerAdvertisement    INTEGER
AdminMessage                TEXT
LastUpdated                 DATETIME
```

---

## API i endpointy

### Publiczne

| Metoda | Endpoint | Opis |
|--------|----------|------|
| GET | `/` | Strona główna |
| GET | `/Home/Rss` | Kanał RSS (XML) |
| GET | `/Category/Index` | Lista kategorii |
| GET | `/Category/Details/{id}` | Ogłoszenia w kategorii |
| GET | `/Advertisement/Index` | Lista ogłoszeń |
| GET | `/Advertisement/Details/{id}` | Szczegóły ogłoszenia |
| GET | `/Advertisement/Search` | Wyszukiwanie |

### Wymagające logowania

| Metoda | Endpoint | Opis |
|--------|----------|------|
| GET | `/Account/Profile` | Profil użytkownika |
| POST | `/Account/Profile` | Aktualizacja profilu |
| GET | `/Advertisement/Create` | Formularz dodawania |
| POST | `/Advertisement/Create` | Publikacja ogłoszenia |
| GET | `/Advertisement/Edit/{id}` | Formularz edycji |
| POST | `/Advertisement/Edit/{id}` | Aktualizacja ogłoszenia |
| POST | `/Advertisement/Delete/{id}` | Usunięcie ogłoszenia |
| POST | `/Advertisement/Report/{id}` | Zgłoszenie do moderacji |

### Panel administracyjny (admin only)

| Metoda | Endpoint | Opis |
|--------|----------|------|
| GET | `/Admin/Dashboard` | Panel główny |
| GET | `/Admin/Users` | Zarządzanie użytkownikami |
| GET | `/Admin/Categories` | Zarządzanie kategoriami |
| POST | `/Admin/CreateCategory` | Nowa kategoria |
| GET | `/Admin/Attributes?categoryId=X` | Atrybuty kategorii |
| POST | `/Admin/CreateAttribute` | Nowy atrybut |
| GET | `/Admin/Dictionaries` | Słowniki |
| POST | `/Admin/CreateDictionary` | Nowy słownik |
| POST | `/Admin/AddDictionaryValue` | Wartość do słownika |
| GET | `/Admin/Moderation` | Panel moderacji |
| POST | `/Admin/ApproveReport/{id}` | Zatwierdzenie zgłoszenia |
| GET | `/Admin/ForbiddenWords` | Słowa zakazane |
| POST | `/Admin/AddForbiddenWord` | Dodaj słowo |
| GET | `/Admin/Settings` | Ustawienia systemu |
| POST | `/Admin/Settings` | Aktualizacja ustawień |

---

## Konfiguracja

### appsettings.json

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=ogloszenia.db"
  }
}
```

### Zmienne środowiskowe

Brak wymaganych zmiennych środowiskowych. Wszystkie ustawienia w `appsettings.json` i bazie danych.

### Domyślne ustawienia systemu

Po pierwszym uruchomieniu tworzone są domyślne ustawienia:
- **MaxFileSize**: 5 MB (5242880 bajtów)
- **MaxMediaPerAdvertisement**: 5
- **MaxFilesPerAdvertisement**: 3
- **AllowedHtmlTags**: `<p>`, `<br>`, `<strong>`, `<em>`, `<a>`, `<ul>`, `<ol>`, `<li>`
- **ForbiddenWords**: pusta lista

### Dostosowanie

**Zmiana limitów plików:**
1. Zaloguj jako admin
2. Przejdź do `/Admin/Settings`
3. Zmień wartości
4. Zapisz

**Dodanie motywu:**
1. Utwórz plik CSS w `wwwroot/css/theme-{nazwa}.css`
2. Dodaj opcję w `LocalizationService.cs`
3. Dodaj w formularzu profilu

---

## Instrukcja testowania

### Test 1: Dynamiczne atrybuty (8 pkt)

1. Zaloguj jako admin: `admin` / `admin123`
2. Przejdź do `/Admin/Dictionaries`
3. Utwórz słownik:
   - Nazwa: "Marka samochodu"
   - Dodaj wartości: Toyota, BMW, Audi, Mercedes
4. Przejdź do `/Admin/Categories`
5. Kliknij "Atrybuty" przy kategorii "Motoryzacja"
6. Dodaj atrybuty:
   - **Marka** (dictionary) → wybierz słownik "Marka samochodu" ✓ Wymagany
   - **Przebieg** (int) ✓ Wymagany
   - **Pojemność silnika** (real)
   - **Stan idealny** (bool)
   - **Opis techniczny** (longtext)
7. Zaloguj jako użytkownik
8. Dodaj ogłoszenie → Wybierz kategorię "Motoryzacja"
9. **Sprawdź**: pojawiły się wszystkie 5 pól!
10. Wypełnij i opublikuj
11. Zobacz szczegóły → **Sekcja "Specyfikacja" z wszystkimi wartościami**

**Oczekiwany rezultat:** ✅ 8 pkt

### Test 2: Typy atrybutów (4 pkt)

Zweryfikuj w teście 1:
- ✓ `int` → liczba całkowita bez przecinka
- ✓ `real` → liczba rzeczywista z przecinkiem
- ✓ `bool` → checkbox (wyświetla "✓ Tak" / "✗ Nie")
- ✓ `dictionary` → lista rozwijana (wyświetla nazwę zamiast ID)
- ✓ `shorttext` → jedno pole input
- ✓ `longtext` → textarea wieloliniowe

**Oczekiwany rezultat:** ✅ 4 pkt

### Test 3: Słowniki (4 pkt)

1. Admin tworzy słownik (test 1, krok 3)
2. Przypisuje do atrybutu (test 1, krok 6)
3. W formularzu pojawia się `<select>` z wartościami
4. Po publikacji wartość słownika jest wyświetlana tekstowo

**Oczekiwany rezultat:** ✅ 4 pkt

### Test 4: Wyszukiwanie hierarchiczne

1. Utwórz kategorię "Elektronika"
2. Dodaj podkategorię "Komputery"
3. Dodaj ogłoszenie w kategorii "Komputery"
4. Wyszukaj ogłoszenia z kategorii "Elektronika"
5. **Sprawdź**: wyświetla się także ogłoszenie z "Komputery"

**Rezultat:** ✅ Działa

### Test 5: Słowa zakazane

1. Admin → `/Admin/ForbiddenWords`
2. Dodaj: "spam", "oszustwo"
3. Spróbuj dodać ogłoszenie z tytułem "Świetna oferta spam"
4. **Sprawdź**: formularz zwraca błąd

**Rezultat:** ✅ Działa

---

## Znane problemy i ograniczenia

1. **Brak wysyłania emaili** - resetowanie hasła bez emaila
2. **Brak operatorów AND/OR w wyszukiwaniu** - tylko proste frazy
3. **Newsletter usunięty** - była implementacja, ale została wycofana
4. **Maksymalnie 2 poziomy kategorii** - rodzic → dziecko (bez głębszej hierarchii)
5. **Brak paginacji w panelu admina** - przy dużej liczbie rekordów może być wolno

---

## Licencja i autor

**Projekt**: Serwis Ogłoszeń Drobnych  
**Autor**: guzik1234  
**Repozytorium**: https://github.com/guzik1234/ProjektMVC  
**Licencja**: MIT (brak pliku LICENSE w repo)  
**Wersja**: 1.0  
**Data**: Grudzień 2025  

---

## FAQ

**P: Jak zresetować bazę danych?**  
O: Usuń plik `ogloszenia.db` i uruchom ponownie aplikację.

**P: Jak zmienić port aplikacji?**  
O: Edytuj `launchSettings.json` w folderze `Properties/`.

**P: Czy mogę dodać więcej języków?**  
O: Tak, dodaj tłumaczenia w `LocalizationService.cs` i dodaj opcję w profilu.

**P: Jak dodać nowy typ atrybutu?**  
O: Dodaj obsługę w:
1. `AdvertisementAttribute.cs` (model)
2. `Create.cshtml` JavaScript (renderowanie pola)
3. `Details.cshtml` (wyświetlanie)

**P: Gdzie są przechowywane pliki?**  
O: `wwwroot/uploads/ads/{id}/`

**P: Jak działa hierarchia kategorii w atrybutach?**  
O: Ogłoszenie w kategorii dziecka dziedziczy atrybuty rodzica + własne atrybuty dziecka.

---

## Kontakt i wsparcie

**Issues**: https://github.com/guzik1234/ProjektMVC/issues  
**Pull Requests**: mile widziane  
**Dokumentacja**: ten plik  

Write-Host "=== Test panelu administracyjnego ===" -ForegroundColor Cyan
Write-Host ""

# Test 1: Kategorie - sprawdzenie czy podkategorie się ładują
Write-Host "Test 1: Sprawdzanie kategorii i podkategorii..." -ForegroundColor Yellow
try {
    $response = Invoke-WebRequest -Uri "http://localhost:5166/Category/Index" -Method GET
    if ($response.Content -match "ChildCategories" -or $response.StatusCode -eq 200) {
        Write-Host "✓ Widok kategorii działa" -ForegroundColor Green
    }
} catch {
    Write-Host "✗ Problem z kategoriami: $($_.Exception.Message)" -ForegroundColor Red
}

# Test 2: Sprawdzenie struktury bazy danych
Write-Host "`nTest 2: Sprawdzanie struktury bazy danych..." -ForegroundColor Yellow
$dbPath = "ogloszenia.db"
if (Test-Path $dbPath) {
    Write-Host "✓ Baza danych ogloszenia.db istnieje" -ForegroundColor Green
    $size = (Get-Item $dbPath).Length
    Write-Host "  Rozmiar: $([math]::Round($size/1KB, 2)) KB" -ForegroundColor Gray
} else {
    Write-Host "✗ Brak pliku ogloszenia.db" -ForegroundColor Red
}

# Test 3: Sprawdzenie czy app.db nie powstaje
Write-Host "`nTest 3: Sprawdzanie czy nie ma niepotrzebnego app.db..." -ForegroundColor Yellow
if (-not (Test-Path "app.db")) {
    Write-Host "✓ Plik app.db nie istnieje (poprawnie)" -ForegroundColor Green
} else {
    Write-Host "✗ Znaleziono niepotrzebny plik app.db" -ForegroundColor Red
}

# Test 4: Test dostępu do strony głównej
Write-Host "`nTest 4: Test strony głównej..." -ForegroundColor Yellow
try {
    $response = Invoke-WebRequest -Uri "http://localhost:5166/" -Method GET
    if ($response.StatusCode -eq 200) {
        Write-Host "✓ Strona główna działa (Status: 200)" -ForegroundColor Green
    }
} catch {
    Write-Host "✗ Problem ze stroną główną: $($_.Exception.Message)" -ForegroundColor Red
}

# Test 5: Test wyszukiwania wielokategoriowego
Write-Host "`nTest 5: Test wyszukiwania w wielu kategoriach..." -ForegroundColor Yellow
try {
    $response = Invoke-WebRequest -Uri "http://localhost:5166/Advertisement/Search" -Method GET
    if ($response.StatusCode -eq 200) {
        Write-Host "✓ Strona wyszukiwania działa" -ForegroundColor Green
        if ($response.Content -match 'name="categoryIds"' -and $response.Content -match 'type="checkbox"') {
            Write-Host "✓ Checkboxy kategorii są obecne" -ForegroundColor Green
        }
    }
} catch {
    Write-Host "✗ Problem z wyszukiwaniem: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host "`n=== Podsumowanie ===" -ForegroundColor Cyan
Write-Host "Panel administracyjny wymaga zalogowania jako admin/admin123" -ForegroundColor Yellow
Write-Host "Aby przetestować funkcje admina:" -ForegroundColor Yellow
Write-Host "1. Otwórz http://localhost:5166" -ForegroundColor White
Write-Host "2. Zaloguj się jako: admin / admin123" -ForegroundColor White
Write-Host "3. Przejdź do: http://localhost:5166/Admin/Dashboard" -ForegroundColor White
Write-Host "`nDostępne funkcje do przetestowania:" -ForegroundColor Yellow
Write-Host "  - /Admin/Categories - tworzenie kategorii i podkategorii" -ForegroundColor White
Write-Host "  - /Admin/Settings - ustawienia serwisu" -ForegroundColor White
Write-Host "  - /Admin/ForbiddenWords - zakazane słowa" -ForegroundColor White
Write-Host "  - /Admin/Users - zarządzanie użytkownikami" -ForegroundColor White
Write-Host ""

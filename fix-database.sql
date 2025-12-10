-- Dodaj brakujące kolumny do tabeli SystemSettings
ALTER TABLE SystemSettings ADD COLUMN ForbiddenWords TEXT;
ALTER TABLE SystemSettings ADD COLUMN AllowedHtmlTags TEXT;

-- Wstaw domyślne wartości JSON
UPDATE SystemSettings SET ForbiddenWords = '[]' WHERE ForbiddenWords IS NULL;
UPDATE SystemSettings SET AllowedHtmlTags = '["p","br","strong","em","u","a","ul","ol","li","h1","h2","h3"]' WHERE AllowedHtmlTags IS NULL;

-- Dodaj wpis do historii migracji
INSERT INTO __EFMigrationsHistory (MigrationId, ProductVersion) 
VALUES ('00000000000000_CreateIdentitySchema', '6.0.0');

INSERT INTO __EFMigrationsHistory (MigrationId, ProductVersion) 
VALUES ('20251209212450_AddJsonColumnsToSystemSettings', '6.0.0');

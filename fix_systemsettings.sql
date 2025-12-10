UPDATE SystemSettings SET ForbiddenWords = '[]' WHERE ForbiddenWords IS NULL OR ForbiddenWords = '';
UPDATE SystemSettings SET AllowedHtmlTags = '["p","br","strong","em","u","a","ul","ol","li","h1","h2","h3"]' WHERE AllowedHtmlTags IS NULL OR AllowedHtmlTags = '';
-- Fix malformed JSON
UPDATE SystemSettings SET AllowedHtmlTags = '["p","br","strong","em","u","a","ul","ol","li","h1","h2","h3"]';

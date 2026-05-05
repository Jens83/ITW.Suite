# Phase 9: TempData-Reads für generische Success/Error aus Views entfernen
# Layout rendert sie jetzt zentral via _AppLayout.cshtml

$root = "C:\Users\jensc\OneDrive\Desktop\ITW.Suite"
$utf8NoBom = New-Object System.Text.UTF8Encoding $false

function UpdateView($path, $transform) {
    $content = [System.IO.File]::ReadAllText($path)
    $new = & $transform $content
    if ($new -ne $content) {
        [System.IO.File]::WriteAllText($path, $new, $utf8NoBom)
        Write-Host "  Updated: $([System.IO.Path]::GetFileName($path))"
        return $true
    }
    return $false
}

# ----- Dienstplan/Plan.cshtml -----
# Before: var successMessage + var finalError + IsSuccess fallback + success block + error block
# After:  var finalError (model only) + error block (no success block)
UpdateView "$root\src\ITW.Web\Areas\Intensivtransport\Views\Dienstplan\Plan.cshtml" {
    param($c)
    $c = [regex]::Replace($c, '[ \t]+var successMessage = TempData\[FlashKeys\.Success\]\?\.ToString\(\);?\r?\n', '')
    $c = [regex]::Replace($c, '[ \t]+var finalError = TempData\[FlashKeys\.Error\]\?\.ToString\(\);?\r?\n', '    string? finalError = null;' + "`n")
    # Keep model fallback but remove TempData part from condition
    $c = $c.Replace('if (string.IsNullOrWhiteSpace(finalError) && !Model.IsSuccess)', 'if (!Model.IsSuccess)')
    # Remove success rendering block (between @if (!string.IsNullOrWhiteSpace(successMessage)) and its closing })
    $c = [regex]::Replace($c, '(?s)[ \t]+@if \(!string\.IsNullOrWhiteSpace\(successMessage\)\)\r?\n\s*\{.*?\r?\n\s*\}\r?\n', '')
    # Update outer if: remove successMessage condition
    $c = $c.Replace('@if (!string.IsNullOrWhiteSpace(successMessage) || !string.IsNullOrWhiteSpace(finalError))',
                    '@if (!string.IsNullOrWhiteSpace(finalError))')
    return $c
}

# ----- Dienstplan/WachleiterIndex.cshtml -----
UpdateView "$root\src\ITW.Web\Areas\Intensivtransport\Views\Dienstplan\WachleiterIndex.cshtml" {
    param($c)
    $c = [regex]::Replace($c, '[ \t]+var successMessage = TempData\[FlashKeys\.Success\]\?\.ToString\(\);?\r?\n', '')
    $c = [regex]::Replace($c, '[ \t]+var finalError = TempData\[FlashKeys\.Error\]\?\.ToString\(\);?\r?\n', '    string? finalError = null;' + "`n")
    $c = $c.Replace('if (string.IsNullOrWhiteSpace(finalError) && !Model.IsSuccess)', 'if (!Model.IsSuccess)')
    $c = [regex]::Replace($c, '(?s)[ \t]+@if \(!string\.IsNullOrWhiteSpace\(successMessage\)\)\r?\n\s*\{.*?\r?\n\s*\}\r?\n', '')
    $c = $c.Replace('@if (!string.IsNullOrWhiteSpace(successMessage) || !string.IsNullOrWhiteSpace(finalError))',
                    '@if (!string.IsNullOrWhiteSpace(finalError))')
    return $c
}

# ----- Personal/Dokumente.cshtml -----
UpdateView "$root\src\ITW.Web\Areas\Intensivtransport\Views\Personal\Dokumente.cshtml" {
    param($c)
    $c = [regex]::Replace($c, '[ \t]+var successMessage = TempData\[FlashKeys\.Success\]\?\.ToString\(\);?\r?\n', '')
    $c = [regex]::Replace($c, '[ \t]+var errorMessage = TempData\[FlashKeys\.Error\]\?\.ToString\(\);?\r?\n', '')
    $c = [regex]::Replace($c, '(?s)[ \t]+@if \(!string\.IsNullOrWhiteSpace\(successMessage\)\)\r?\n\s*\{.*?\r?\n\s*\}\r?\n', '')
    $c = $c.Replace('@if (!string.IsNullOrWhiteSpace(successMessage) || !string.IsNullOrWhiteSpace(errorMessage))',
                    '@if (!string.IsNullOrWhiteSpace(errorMessage))')
    return $c
}

Write-Host "Done."

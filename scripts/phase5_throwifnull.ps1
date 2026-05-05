# Phase 5: Konstruktor-Boilerplate eliminieren
# Ersetzt ?? throw new ArgumentNullException(nameof(...)) durch ThrowIfNull + separate Zuweisung

$srcPath = Join-Path $PSScriptRoot "..\src"
$files = Get-ChildItem -Path $srcPath -Recurse -Filter "*.cs"
$modified = 0
$totalRemaining = 0

foreach ($file in $files) {
    $content = [System.IO.File]::ReadAllText($file.FullName, [System.Text.Encoding]::UTF8)

    if ($content -notlike '*throw new ArgumentNullException(nameof*') {
        continue
    }

    $original = $content

    # Pattern 1: multiline
    # _field = param\r?\n    ?? throw new ArgumentNullException(nameof(param));
    # Regex: captures indent, field, param — param appears in both nameof and assignment
    $multiPattern = '(?m)^([ \t]+)(_\w+) = (\w+)\r?\n[ \t]+\?\? throw new ArgumentNullException\(nameof\(\3\)\);'
    $content = [regex]::Replace($content, $multiPattern, {
        param($m)
        $indent = $m.Groups[1].Value
        $field  = $m.Groups[2].Value
        $param  = $m.Groups[3].Value
        "$indent`ArgumentNullException.ThrowIfNull($param);`n$indent$field = $param;"
    })

    # Pattern 2: single-line
    # _field = param ?? throw new ArgumentNullException(nameof(param));
    $singlePattern = '(?m)^([ \t]+)(_\w+) = (\w+) \?\? throw new ArgumentNullException\(nameof\(\3\)\);'
    $content = [regex]::Replace($content, $singlePattern, {
        param($m)
        $indent = $m.Groups[1].Value
        $field  = $m.Groups[2].Value
        $param  = $m.Groups[3].Value
        "$indent`ArgumentNullException.ThrowIfNull($param);`n$indent$field = $param;"
    })

    if ($content -ne $original) {
        $utf8NoBom = New-Object System.Text.UTF8Encoding $false
        [System.IO.File]::WriteAllText($file.FullName, $content, $utf8NoBom)
        Write-Host "  Modified: $($file.FullName.Replace($srcPath, 'src'))"
        $modified++
    }

    $totalRemaining += ([regex]::Matches($content, 'throw new ArgumentNullException\(nameof')).Count
}

Write-Host ""
Write-Host "Modified files : $modified"
Write-Host "Remaining hits : $totalRemaining"

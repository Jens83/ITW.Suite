# Phase 8: TempData-Magic-Strings durch FlashKeys-Konstanten ersetzen
# Controllers (.cs): TempData["SuccessMessage"] -> TempData[FlashKeys.Success]
# Views (.cshtml):   TempData["SuccessMessage"] -> TempData[FlashKeys.Success]

$srcPath = ".\src\ITW.Web"
$modified = 0

$replacements = @(
    # Generic keys
    @{ From = 'TempData["SuccessMessage"]'; To = 'TempData[FlashKeys.Success]' },
    @{ From = "TempData[`"SuccessMessage`"]"; To = 'TempData[FlashKeys.Success]' },
    @{ From = 'TempData["ErrorMessage"]';   To = 'TempData[FlashKeys.Error]'   },
    @{ From = "TempData[`"ErrorMessage`"]";   To = 'TempData[FlashKeys.Error]'   },
    @{ From = 'TempData["InfoMessage"]';    To = 'TempData[FlashKeys.Info]'    },

    # ternary patterns  result.IsSuccess ? "SuccessMessage" : "ErrorMessage"
    @{ From = '"SuccessMessage" : "ErrorMessage"'; To = 'FlashKeys.Success : FlashKeys.Error' },

    # Fahrzeugmanagement section keys
    @{ From = 'TempData["FahrzeugDokumente.Erfolg"]'; To = 'TempData[FlashKeys.FahrzeugDokumenteErfolg]' },
    @{ From = 'TempData["FahrzeugDokumente.Fehler"]'; To = 'TempData[FlashKeys.FahrzeugDokumenteFehler]' },
    @{ From = 'TempData["FahrzeugPruefstatus.Erfolg"]'; To = 'TempData[FlashKeys.FahrzeugPruefstatusErfolg]' },
    @{ From = 'TempData["FahrzeugPruefstatus.Fehler"]'; To = 'TempData[FlashKeys.FahrzeugPruefstatusFehler]' },
    @{ From = 'TempData["Fahrtenbuch.Erfolg"]'; To = 'TempData[FlashKeys.FahrtenbuchErfolg]' },
    @{ From = 'TempData["Fahrtenbuch.Fehler"]'; To = 'TempData[FlashKeys.FahrtenbuchFehler]' },
    @{ From = 'TempData["Fahrzeuge.Hinweis"]'; To = 'TempData[FlashKeys.FahrzeugeHinweis]' },
    @{ From = 'TempData["TrackingGeraete.Erfolg"]'; To = 'TempData[FlashKeys.TrackingGeraeteErfolg]' },
    @{ From = 'TempData["TrackingGeraete.Fehler"]'; To = 'TempData[FlashKeys.TrackingGeraeteFehler]' }
)

$extensions = @('*.cs', '*.cshtml')
$utf8NoBom = New-Object System.Text.UTF8Encoding $false

foreach ($ext in $extensions) {
    $files = Get-ChildItem -Path $srcPath -Recurse -Filter $ext

    foreach ($file in $files) {
        $content = [System.IO.File]::ReadAllText($file.FullName)
        $original = $content

        foreach ($r in $replacements) {
            $content = $content.Replace($r.From, $r.To)
        }

        if ($content -ne $original) {
            [System.IO.File]::WriteAllText($file.FullName, $content, $utf8NoBom)
            Write-Host "  Modified: $($file.FullName.Replace((Resolve-Path $srcPath).Path, 'ITW.Web'))"
            $modified++
        }
    }
}

Write-Host ""
Write-Host "Modified files: $modified"

# Verify no magic strings remain
$remaining = 0
foreach ($ext in $extensions) {
    $files = Get-ChildItem -Path $srcPath -Recurse -Filter $ext
    foreach ($file in $files) {
        $content = [System.IO.File]::ReadAllText($file.FullName)
        $hits = [regex]::Matches($content, 'TempData\["(SuccessMessage|ErrorMessage|InfoMessage)"').Count
        $remaining += $hits
    }
}
Write-Host "Remaining TempData magic strings (Success/Error/Info): $remaining"

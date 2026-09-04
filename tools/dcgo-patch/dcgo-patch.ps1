#requires -Version 5.1
<#
.SYNOPSIS
  Export and reapply DCGO custom modules (android, ranked, tournament, reconnect)
  onto a new official branch.
#>
[CmdletBinding()]
param(
    [Parameter(Position = 0)]
    [ValidateSet('export', 'apply', 'rebase', 'status', 'verify', 'help')]
    [string]$Command = 'help',

    [string]$Onto,
    [string[]]$Modules,
    [switch]$IncludeUncommitted,
    [switch]$CommittedOnly,
    [switch]$Portable,
    [string]$Target,
    [string]$CustomBranch,
    [string]$Baseline,
    [string]$BranchName,
    [string]$LocalAssets
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$ToolRoot = $PSScriptRoot
$RepoRoot = (Resolve-Path (Join-Path $ToolRoot '..\..')).Path
$ConfigPath = Join-Path $ToolRoot 'config.json'
$ModulesDir = Join-Path $ToolRoot 'modules'
$PatchesDir = Join-Path $ToolRoot 'patches'
$PatchesFilesDir = Join-Path $PatchesDir 'files'
$RegionsPath = Join-Path $ToolRoot 'regions.json'
$OverlayListPath = Join-Path $ToolRoot 'overlay-files.json'
$WorkspaceDir = Join-Path $RepoRoot 'PATCHWorkspace'
$ReportPath = Join-Path $WorkspaceDir 'apply-report.md'

function Write-Info([string]$Message) { Write-Host "[dcgo-patch] $Message" }
function Write-Warn([string]$Message) { Write-Warning "[dcgo-patch] $Message" }

function ConvertTo-UnixPath([string]$Path) {
    return ($Path -replace '\\', '/')
}

function ConvertTo-WinPath([string]$Path) {
    return ($Path -replace '/', '\')
}

function Get-Config {
    if (-not (Test-Path -LiteralPath $ConfigPath)) {
        throw "Missing config: $ConfigPath"
    }
    return (Get-Content -LiteralPath $ConfigPath -Raw -Encoding UTF8 | ConvertFrom-Json)
}

function Get-ModuleManifests {
    param([object]$Config, [string[]]$Filter)
    $wanted = @($Config.modules)
    if ($Filter -and $Filter.Count -gt 0) {
        $wanted = @($Filter | ForEach-Object { $_.Trim() } | Where-Object { $_ })
    }
    $list = @()
    foreach ($id in $wanted) {
        $path = Join-Path $ModulesDir "$id.json"
        if (-not (Test-Path -LiteralPath $path)) {
            throw "Missing module manifest: $path"
        }
        $list += (Get-Content -LiteralPath $path -Raw -Encoding UTF8 | ConvertFrom-Json)
    }
    return $list
}

function Invoke-Git {
    param(
        [Parameter(Mandatory = $true)]
        [string[]]$GitArgs,
        [switch]$AllowFail
    )
    Push-Location -LiteralPath $RepoRoot
    try {
        $output = & git @GitArgs 2>&1
        $code = $LASTEXITCODE
        if (-not $AllowFail -and $code -ne 0) {
            $text = ($output | Out-String).Trim()
            throw "git $($GitArgs -join ' ') failed ($code): $text"
        }
        return @{ Code = $code; Output = $output }
    }
    finally {
        Pop-Location
    }
}

function Get-GitText {
    param([Parameter(Mandatory = $true)][string[]]$GitArgs)
    $result = Invoke-Git -GitArgs $GitArgs
    return @($result.Output | ForEach-Object { "$_" }) -join "`n"
}

function Test-IsTextPath {
    param([string]$RelPath, [object]$Config)
    $ext = [IO.Path]::GetExtension($RelPath).ToLowerInvariant()
    if ($Config.overlayAlways -contains $ext) { return $false }
    if ($Config.textExtensions -contains $ext) { return $true }
    return $false
}

function Get-UniqueStrings([string[]]$Items) {
    $seen = New-Object 'System.Collections.Generic.HashSet[string]' ([StringComparer]::OrdinalIgnoreCase)
    $out = @()
    foreach ($item in $Items) {
        if ([string]::IsNullOrWhiteSpace($item)) { continue }
        $norm = ConvertTo-UnixPath $item.Trim()
        if ($seen.Add($norm)) { $out += $norm }
    }
    return $out
}

function Expand-RepoPaths {
    param([string[]]$Entries)
    $out = @()
    foreach ($entry in $Entries) {
        if ([string]::IsNullOrWhiteSpace($entry)) { continue }
        $rel = ConvertTo-UnixPath $entry.Trim()
        $full = Join-Path $RepoRoot (ConvertTo-WinPath $rel)
        if (Test-Path -LiteralPath $full -PathType Container) {
            Get-ChildItem -LiteralPath $full -Recurse -File | ForEach-Object {
                $out += (ConvertTo-UnixPath $_.FullName.Substring($RepoRoot.Length).TrimStart('\', '/'))
            }
        }
        elseif (Test-Path -LiteralPath $full -PathType Leaf) {
            $out += $rel
        }
        else {
            $out += $rel
        }
    }
    return Get-UniqueStrings $out
}

function Get-PatchFileSet {
    param($Manifests)
    $files = @()
    foreach ($m in $Manifests) {
        if ($m.PSObject.Properties.Name -contains 'patchFiles') {
            $files += @($m.patchFiles)
        }
    }
    return Get-UniqueStrings $files
}

function Get-OverlayEntrySet {
    param($Manifests)
    $entries = @()
    foreach ($m in $Manifests) {
        if ($m.PSObject.Properties.Name -contains 'overlay') { $entries += @($m.overlay) }
        if ($m.PSObject.Properties.Name -contains 'overlayFiles') { $entries += @($m.overlayFiles) }
    }
    return Get-UniqueStrings $entries
}

function Get-RegionPattern([string]$Module = $null) {
    if ($Module) {
        $escaped = [regex]::Escape($Module)
        return "(?ms)^(?<indent>[ \t]*)// === DCGO-CUSTOM:$escaped begin ===\r?\n(?<body>.*?)^[ \t]*// === DCGO-CUSTOM:$escaped end ==="
    }
    return '(?ms)^(?<indent>[ \t]*)// === DCGO-CUSTOM:(?<module>[\w:-]+) begin ===\r?\n(?<body>.*?)^[ \t]*// === DCGO-CUSTOM:\k<module> end ==='
}

function Read-FileText([string]$FullPath) {
    return [IO.File]::ReadAllText($FullPath)
}

function Write-FileText([string]$FullPath, [string]$Text) {
    $dir = Split-Path -Parent $FullPath
    if (-not (Test-Path -LiteralPath $dir)) {
        New-Item -ItemType Directory -Path $dir -Force | Out-Null
    }
    $utf8 = New-Object System.Text.UTF8Encoding $false
    [IO.File]::WriteAllText($FullPath, $Text, $utf8)
}

function Export-RegionsFromTree {
    param($Manifests)
    $files = Get-PatchFileSet $Manifests
    $map = [ordered]@{}
    foreach ($rel in $files) {
        if (-not $rel.EndsWith('.cs')) { continue }
        $full = Join-Path $RepoRoot (ConvertTo-WinPath $rel)
        if (-not (Test-Path -LiteralPath $full)) { continue }
        $text = Read-FileText $full
        $matches = [regex]::Matches($text, (Get-RegionPattern))
        if ($matches.Count -eq 0) { continue }
        $regions = @()
        foreach ($match in $matches) {
            $regions += [ordered]@{
                module = $match.Groups['module'].Value
                indent = $match.Groups['indent'].Value
                body   = $match.Groups['body'].Value.TrimEnd("`r", "`n")
            }
        }
        $map[$rel] = $regions
    }
    return $map
}

function Get-SafePatchName([string]$RelPath) {
    return (($RelPath -replace '[\\/]', '_') -replace '[^A-Za-z0-9._-]', '_') + '.patch'
}

function Ensure-Dir([string]$Path) {
    if (-not (Test-Path -LiteralPath $Path)) {
        New-Item -ItemType Directory -Path $Path -Force | Out-Null
    }
}

function Get-BaselineSha($Config) {
    if ($Baseline) { return $Baseline }
    return $Config.baselineCommit
}

function Get-CustomBranchName($Config) {
    if ($CustomBranch) { return $CustomBranch }
    return $Config.customBranch
}

function Get-OptionalAssetEntries($Config) {
    if (-not ($Config.PSObject.Properties.Name -contains 'optionalAssets')) { return @() }
    return Get-UniqueStrings @($Config.optionalAssets)
}

function Test-IsOptionalAssetPath {
    param([string]$RelPath, [string[]]$OptionalEntries)
    $rel = ConvertTo-UnixPath $RelPath
    foreach ($entry in $OptionalEntries) {
        $p = ConvertTo-UnixPath $entry
        if ([string]::IsNullOrWhiteSpace($p)) { continue }
        if ($rel -eq $p -or $rel.StartsWith($p.TrimEnd('/') + '/')) { return $true }
        # Folder .meta next to an optional pack root
        if ($rel -eq ($p + '.meta')) { return $true }
    }
    return $false
}

function Filter-RequiredOverlayFiles {
    param([string[]]$OverlayFiles, $Config)
    $optional = Get-OptionalAssetEntries $Config
    if ($optional.Count -eq 0) { return Get-UniqueStrings $OverlayFiles }
    $out = @()
    foreach ($rel in $OverlayFiles) {
        if (Test-IsOptionalAssetPath -RelPath $rel -OptionalEntries $optional) { continue }
        $out += $rel
    }
    return Get-UniqueStrings $out
}

function Resolve-LocalAssetsRoot {
    param($Config, [string]$DestRoot)
    if ($LocalAssets) {
        if ([IO.Path]::IsPathRooted($LocalAssets)) { return $LocalAssets }
        return (Join-Path $DestRoot $LocalAssets)
    }
    $configured = 'CustomAssets'
    if ($Config.PSObject.Properties.Name -contains 'localAssetsRoot' -and $Config.localAssetsRoot) {
        $configured = [string]$Config.localAssetsRoot
    }
    if ([IO.Path]::IsPathRooted($configured)) { return $configured }
    return (Join-Path $DestRoot $configured)
}

function Copy-OptionalAssetsOntoTarget {
    param($Config, [string]$DestRoot)
    $optional = Get-OptionalAssetEntries $Config
    $root = Resolve-LocalAssetsRoot -Config $Config -DestRoot $DestRoot
    $copied = @()
    $skipped = @()

    if (-not (Test-Path -LiteralPath $root -PathType Container)) {
        return @{
            Root            = $root
            RootExists      = $false
            Copied          = @()
            Skipped         = @($optional)
            FilesCopied     = 0
        }
    }

    $filesCopied = 0
    foreach ($entry in $optional) {
        $rel = ConvertTo-UnixPath $entry
        $src = Join-Path $root (ConvertTo-WinPath $rel)
        if (-not (Test-Path -LiteralPath $src)) {
            $skipped += $rel
            continue
        }

        $dest = Join-Path $DestRoot (ConvertTo-WinPath $rel)
        if (Test-Path -LiteralPath $src -PathType Container) {
            $destParent = Split-Path -Parent $dest
            Ensure-Dir $destParent
            if (Test-Path -LiteralPath $dest) {
                Get-ChildItem -LiteralPath $src -Force | ForEach-Object {
                    Copy-Item -LiteralPath $_.FullName -Destination $dest -Recurse -Force
                }
            }
            else {
                Copy-Item -LiteralPath $src -Destination $dest -Recurse -Force
            }
            $filesCopied += @(Get-ChildItem -LiteralPath $src -Recurse -File -ErrorAction SilentlyContinue).Count
            $srcMeta = "$src.meta"
            $destMeta = "$dest.meta"
            if (Test-Path -LiteralPath $srcMeta -PathType Leaf) {
                Copy-Item -LiteralPath $srcMeta -Destination $destMeta -Force
                $filesCopied++
            }
        }
        else {
            Ensure-Dir (Split-Path -Parent $dest)
            Copy-Item -LiteralPath $src -Destination $dest -Force
            $filesCopied++
        }
        $copied += $rel
    }

    return @{
        Root            = $root
        RootExists      = $true
        Copied          = @($copied)
        Skipped         = @($skipped)
        FilesCopied     = $filesCopied
    }
}

function Invoke-Export {
    param($Config, $Manifests)
    $base = Get-BaselineSha $Config
    $useWorkTree = $IncludeUncommitted -or (-not $CommittedOnly)
    Write-Info "Exporting vs baseline $base ($($Config.baselineLabel)) workTree=$useWorkTree"

    Ensure-Dir $PatchesDir
    Ensure-Dir $PatchesFilesDir
    Get-ChildItem -LiteralPath $PatchesFilesDir -Filter *.patch -ErrorAction SilentlyContinue | Remove-Item -Force

    $patchFiles = Get-PatchFileSet $Manifests
    $modulesByFile = @{}
    foreach ($m in $Manifests) {
        foreach ($f in @($m.patchFiles)) {
            $key = ConvertTo-UnixPath $f
            if (-not $modulesByFile.ContainsKey($key)) { $modulesByFile[$key] = @() }
            $modulesByFile[$key] += $m.id
        }
    }

    $exportedPatches = @()
    foreach ($rel in $patchFiles) {
        $gitArgs = @('diff', '--no-color', '--binary', $base)
        if (-not $useWorkTree) { $gitArgs += 'HEAD' }
        $gitArgs += @('--', $rel)
        $diff = Get-GitText -GitArgs $gitArgs
        if ([string]::IsNullOrWhiteSpace($diff)) { continue }
        $name = Get-SafePatchName $rel
        $outPath = Join-Path $PatchesFilesDir $name
        Write-FileText $outPath $diff
        $mods = @()
        if ($modulesByFile.ContainsKey($rel)) { $mods = $modulesByFile[$rel] }
        $exportedPatches += [ordered]@{
            file    = $rel
            patch   = (ConvertTo-UnixPath (Join-Path 'patches\files' $name))
            modules = $mods
        }
        Write-Info "Patch $($mods -join ',') -> $rel"
    }

    $overlayEntries = Get-OverlayEntrySet $Manifests
    $overlayFiles = Expand-RepoPaths $overlayEntries
    if ($useWorkTree) {
        $untracked = Get-GitText -GitArgs @('ls-files', '--others', '--exclude-standard')
        foreach ($line in ($untracked -split "`n")) {
            $u = ConvertTo-UnixPath $line.Trim()
            if ([string]::IsNullOrWhiteSpace($u)) { continue }
            foreach ($prefix in $overlayEntries) {
                $p = ConvertTo-UnixPath $prefix
                if ($u -eq $p -or $u.StartsWith($p.TrimEnd('/') + '/')) {
                    $overlayFiles += $u
                }
            }
        }
        $overlayFiles = Get-UniqueStrings $overlayFiles
    }

    $overlayFiles = Filter-RequiredOverlayFiles -OverlayFiles $overlayFiles -Config $Config

    $overlayDoc = [ordered]@{
        baseline      = $base
        generatedUtc  = [DateTime]::UtcNow.ToString('o')
        includeUncommitted = [bool]$useWorkTree
        files         = @($overlayFiles)
        patches       = @($exportedPatches)
        optionalAssets = @(Get-OptionalAssetEntries $Config)
    }
    $overlayJson = $overlayDoc | ConvertTo-Json -Depth 6
    Write-FileText $OverlayListPath $overlayJson

    $regionMap = Export-RegionsFromTree $Manifests
    $regionDoc = [ordered]@{
        baseline     = $base
        generatedUtc = [DateTime]::UtcNow.ToString('o')
        files        = $regionMap
    }
    Write-FileText $RegionsPath ($regionDoc | ConvertTo-Json -Depth 8)

    $index = [ordered]@{
        baseline     = $base
        label        = $Config.baselineLabel
        generatedUtc = [DateTime]::UtcNow.ToString('o')
        modules      = @($Manifests | ForEach-Object { $_.id })
        patchCount   = $exportedPatches.Count
        overlayCount = $overlayFiles.Count
        regionFiles  = @($regionMap.Keys)
    }
    Write-FileText (Join-Path $PatchesDir 'index.json') ($index | ConvertTo-Json -Depth 5)

    if ($Portable) {
        Export-PortableBundle $Config $overlayFiles
    }

    Write-Info "Export complete: $($exportedPatches.Count) patches, $($overlayFiles.Count) overlay files, regions.json updated."
}

function Export-PortableBundle {
    param($Config, [string[]]$OverlayFiles)
    Ensure-Dir $WorkspaceDir
    $overlayRoot = Join-Path $WorkspaceDir 'overlay'
    if (Test-Path -LiteralPath $overlayRoot) {
        Remove-Item -LiteralPath $overlayRoot -Recurse -Force
    }
    Ensure-Dir $overlayRoot
    $copied = 0
    foreach ($rel in $OverlayFiles) {
        $src = Join-Path $RepoRoot (ConvertTo-WinPath $rel)
        if (-not (Test-Path -LiteralPath $src -PathType Leaf)) { continue }
        $dst = Join-Path $overlayRoot (ConvertTo-WinPath $rel)
        Ensure-Dir (Split-Path -Parent $dst)
        Copy-Item -LiteralPath $src -Destination $dst -Force
        $copied++
    }
    $stamp = Get-Date -Format 'yyyyMMdd-HHmmss'
    $zip = Join-Path $WorkspaceDir "dcgo-custom-$stamp.zip"
    if (Test-Path -LiteralPath $zip) { Remove-Item -LiteralPath $zip -Force }
    Compress-Archive -Path $overlayRoot -DestinationPath $zip -Force
    Write-Info "Portable overlay: $copied files -> $zip"
}

function Copy-OverlayOntoTarget {
    param($Config, [string[]]$OverlayFiles, [string]$DestRoot)
    $custom = Get-CustomBranchName $Config
    $workspaceOverlay = Join-Path $WorkspaceDir 'overlay'
    $copied = @()
    $missing = @()
    $destResolved = (Resolve-Path -LiteralPath $DestRoot).Path
    $sameRepo = [string]::Equals($destResolved, $RepoRoot, [StringComparison]::OrdinalIgnoreCase)

    foreach ($rel in $OverlayFiles) {
        $dest = Join-Path $DestRoot (ConvertTo-WinPath $rel)
        $copiedOk = $false

        $ws = Join-Path $workspaceOverlay (ConvertTo-WinPath $rel)
        if (Test-Path -LiteralPath $ws -PathType Leaf) {
            Ensure-Dir (Split-Path -Parent $dest)
            Copy-Item -LiteralPath $ws -Destination $dest -Force
            $copiedOk = $true
        }

        if (-not $copiedOk -and $sameRepo) {
            $checkout = Invoke-Git -GitArgs @('checkout', $custom, '--', $rel) -AllowFail
            if ($checkout.Code -eq 0 -and (Test-Path -LiteralPath $dest -PathType Leaf)) {
                $copiedOk = $true
            }
        }

        if (-not $copiedOk) {
            $src = Join-Path $RepoRoot (ConvertTo-WinPath $rel)
            if (Test-Path -LiteralPath $src -PathType Leaf) {
                Ensure-Dir (Split-Path -Parent $dest)
                Copy-Item -LiteralPath $src -Destination $dest -Force
                $copiedOk = $true
            }
        }

        if ($copiedOk) { $copied += $rel } else { $missing += $rel }
    }

    return @{ Copied = $copied; Missing = $missing }
}

function Apply-GitPatch([string]$PatchPath) {
    $relPatch = $PatchPath
    if ($PatchPath.StartsWith($RepoRoot, [StringComparison]::OrdinalIgnoreCase)) {
        $relPatch = $PatchPath.Substring($RepoRoot.Length).TrimStart('\', '/')
    }
    $three = Invoke-Git -GitArgs @('apply', '--3way', '--whitespace=nowarn', $relPatch) -AllowFail
    if ($three.Code -eq 0) {
        return 'applied-3way'
    }
    $reject = Invoke-Git -GitArgs @('apply', '--reject', '--whitespace=nowarn', $relPatch) -AllowFail
    if ($reject.Code -eq 0) {
        return 'applied-reject'
    }
    return 'failed'
}

function Find-ModuleAnchor {
    param($Manifests, [string]$Module, [string]$RelPath)
    foreach ($m in $Manifests) {
        if ($m.id -ne $Module) { continue }
        if (-not ($m.PSObject.Properties.Name -contains 'anchors')) { continue }
        foreach ($a in @($m.anchors)) {
            if ((ConvertTo-UnixPath $a.file) -eq $RelPath) {
                return [string]$a.afterContains
            }
        }
    }
    return $null
}

function Apply-RegionsToFile {
    param(
        [string]$RelPath,
        [object[]]$Regions,
        [object]$Manifests,
        [string]$DestRoot
    )
    $full = Join-Path $DestRoot (ConvertTo-WinPath $RelPath)
    if (-not (Test-Path -LiteralPath $full)) {
        return 'missing-file'
    }
    $text = Read-FileText $full
    $allPattern = Get-RegionPattern
    $existing = [regex]::Matches($text, $allPattern)
    $used = New-Object 'System.Collections.Generic.HashSet[int]'
    $replacements = @()
    $inserts = @()

    foreach ($region in @($Regions)) {
        $module = [string]$region.module
        $body = [string]$region.body
        $indent = [string]$region.indent
        $foundIdx = -1
        for ($i = 0; $i -lt $existing.Count; $i++) {
            if ($used.Contains($i)) { continue }
            if ($existing[$i].Groups['module'].Value -eq $module) {
                $foundIdx = $i
                break
            }
        }
        if ($foundIdx -ge 0) {
            [void]$used.Add($foundIdx)
            $replacements += @{ Match = $existing[$foundIdx]; Module = $module; Body = $body }
            continue
        }

        $anchor = Find-ModuleAnchor -Manifests $Manifests -Module $module -RelPath $RelPath
        $block = "$indent// === DCGO-CUSTOM:$module begin ===`r`n$body`r`n$indent// === DCGO-CUSTOM:$module end ==="
        if ($anchor) {
            $inserts += @{ Anchor = $anchor; Block = $block; Module = $module }
        }
        else {
            return "missing-region:$module"
        }
    }

    $changed = $false
    foreach ($rep in ($replacements | Sort-Object { $_.Match.Index } -Descending)) {
        $m = $rep.Match
        $ind = $m.Groups['indent'].Value
        $replacement = "$ind// === DCGO-CUSTOM:$($rep.Module) begin ===`r`n$($rep.Body)`r`n$ind// === DCGO-CUSTOM:$($rep.Module) end ==="
        $text = $text.Remove($m.Index, $m.Length).Insert($m.Index, $replacement)
        $changed = $true
    }

    $inserted = 0
    foreach ($ins in $inserts) {
        $idx = $text.IndexOf($ins.Anchor)
        if ($idx -lt 0) {
            return "missing-region:$($ins.Module)"
        }
        $nl = $text.IndexOf("`n", $idx)
        if ($nl -lt 0) { $nl = $text.Length - 1 }
        $text = $text.Insert($nl + 1, $ins.Block + "`r`n")
        $inserted++
        $changed = $true
    }

    if ($changed) {
        Write-FileText $full $text
        return "regions-replaced=$($replacements.Count) inserted=$inserted"
    }
    return 'regions-unchanged'
}

function Invoke-Apply {
    param($Config, $Manifests)
    $dest = $RepoRoot
    if ($Target) { $dest = (Resolve-Path -LiteralPath $Target).Path }
    Ensure-Dir $WorkspaceDir

    $overlayDoc = $null
    if (Test-Path -LiteralPath $OverlayListPath) {
        $overlayDoc = Get-Content -LiteralPath $OverlayListPath -Raw -Encoding UTF8 | ConvertFrom-Json
    }
    $overlayFiles = @()
    if ($overlayDoc -and $overlayDoc.files) {
        $overlayFiles = @($overlayDoc.files)
    }
    else {
        $overlayFiles = Expand-RepoPaths (Get-OverlayEntrySet $Manifests)
    }
    $overlayFiles = Filter-RequiredOverlayFiles -OverlayFiles $overlayFiles -Config $Config

    Write-Info "Applying overlays to $dest ($($overlayFiles.Count) files)"
    $overlayResult = Copy-OverlayOntoTarget $Config $overlayFiles $dest

    Write-Info "Applying optional local assets (if present)..."
    $optionalResult = Copy-OptionalAssetsOntoTarget -Config $Config -DestRoot $dest
    if (-not $optionalResult.RootExists) {
        Write-Info "Optional assets folder not found: $($optionalResult.Root) (skipping)"
    }
    else {
        Write-Info "Optional assets from $($optionalResult.Root): copied $($optionalResult.Copied.Count) pack(s), skipped $($optionalResult.Skipped.Count), files=$($optionalResult.FilesCopied)"
    }

    $patchResults = @()
    $patchDir = $PatchesFilesDir
    if (-not (Test-Path -LiteralPath $patchDir)) {
        throw "No patches exported yet. Run: dcgo-patch.ps1 export"
    }

    $regionDoc = $null
    if (Test-Path -LiteralPath $RegionsPath) {
        $regionDoc = Get-Content -LiteralPath $RegionsPath -Raw -Encoding UTF8 | ConvertFrom-Json
    }

    Get-ChildItem -LiteralPath $patchDir -Filter *.patch | Sort-Object Name | ForEach-Object {
        $status = Apply-GitPatch $_.FullName
        $relGuess = $_.BaseName
        $patchResults += [ordered]@{ patch = $_.Name; status = $status }
        if ($status -eq 'failed' -and $regionDoc) {
            Write-Warn "Patch $($_.Name) failed; trying region fallback."
        }
    }

    $regionResults = @()
    if ($regionDoc -and $regionDoc.files) {
        foreach ($prop in $regionDoc.files.PSObject.Properties) {
            $rel = $prop.Name
            $regions = @($prop.Value)
            $status = Apply-RegionsToFile -RelPath $rel -Regions $regions -Manifests $Manifests -DestRoot $dest
            $regionResults += [ordered]@{ file = $rel; status = $status }
        }
    }

    $lines = @()
    $lines += "# DCGO custom apply report"
    $lines += ""
    $lines += "Generated (UTC): $([DateTime]::UtcNow.ToString('o'))"
    $lines += "Target: ``$dest``"
    $lines += "Modules: $($Manifests.id -join ', ')"
    $lines += ""
    $lines += "## Overlay"
    $lines += ""
    $lines += "- Copied: $($overlayResult.Copied.Count)"
    $lines += "- Missing: $($overlayResult.Missing.Count)"
    $lines += ""
    if ($overlayResult.Missing.Count -gt 0) {
        $lines += "Missing overlay files:"
        foreach ($m in $overlayResult.Missing) { $lines += "- ``$m``" }
        $lines += ""
    }
    $lines += "## Optional assets"
    $lines += ""
    $lines += "- Local root: ``$($optionalResult.Root)``"
    if (-not $optionalResult.RootExists) {
        $lines += "- Status: folder not found (optional packs skipped)"
    }
    else {
        $lines += "- Status: found"
        $lines += "- Packs copied: $($optionalResult.Copied.Count) ($($optionalResult.FilesCopied) files)"
        $lines += "- Packs skipped: $($optionalResult.Skipped.Count)"
        if ($optionalResult.Copied.Count -gt 0) {
            $lines += ""
            $lines += "Copied packs:"
            foreach ($p in $optionalResult.Copied) { $lines += "- ``$p``" }
        }
        if ($optionalResult.Skipped.Count -gt 0) {
            $lines += ""
            $lines += "Skipped packs (not present under local root):"
            foreach ($p in $optionalResult.Skipped) { $lines += "- ``$p``" }
        }
    }
    $lines += ""
    $lines += "## Patches"
    $lines += ""
    foreach ($p in $patchResults) {
        $lines += "- ``$($p.patch)``: $($p.status)"
    }
    $lines += ""
    $lines += "## Regions"
    $lines += ""
    foreach ($r in $regionResults) {
        $lines += "- ``$($r.file)``: $($r.status)"
    }
    $report = $lines -join "`r`n"
    Write-FileText $ReportPath $report
    Write-Info "Apply report: $ReportPath"
    if ($overlayResult.Missing.Count -gt 0) {
        Write-Warn "Some required overlay files were missing. Run export -Portable from the custom branch, or pass a branch that still has those files."
    }
}

function Invoke-Rebase {
    param($Config)
    $onto = $Onto
    if (-not $onto) { $onto = "$($Config.officialRemote)/$($Config.officialBranch)" }
    $base = Get-BaselineSha $Config
    $custom = Get-CustomBranchName $Config
    $label = ($onto -replace '[^A-Za-z0-9._]+', '-').Trim('-')
    $newBranch = $BranchName
    if (-not $newBranch) { $newBranch = "custom/$label" }

    Write-Info "Fetching $($Config.officialRemote)..."
    Invoke-Git -GitArgs @('fetch', $Config.officialRemote) | Out-Null

    $status = Get-GitText -GitArgs @('status', '--porcelain')
    $stashed = $false
    if (-not [string]::IsNullOrWhiteSpace($status)) {
        Write-Info "Stashing working tree (including untracked) before rebase."
        Invoke-Git -GitArgs @('stash', 'push', '-u', '-m', 'dcgo-patch rebase') | Out-Null
        $stashed = $true
    }

    $ontoSha = (Get-GitText -GitArgs @('rev-parse', $onto)).Trim()
    Write-Info "Creating $newBranch from $onto ($ontoSha)"
    Invoke-Git -GitArgs @('checkout', '-B', $newBranch, $onto) | Out-Null

    $commitsText = Get-GitText -GitArgs @('rev-list', '--reverse', "$base..$custom")
    $commits = @($commitsText -split "`n" | ForEach-Object { $_.Trim() } | Where-Object { $_ })
    if ($commits.Count -eq 0) {
        Write-Warn "No commits in $base..$custom"
        return
    }

    $failed = @()
    foreach ($sha in $commits) {
        Write-Info "Cherry-pick $sha"
        $pick = Invoke-Git -GitArgs @('cherry-pick', '--allow-empty', $sha) -AllowFail
        if ($pick.Code -ne 0) {
            $failed += $sha
            Write-Warn "Conflict while cherry-picking $sha. Resolve, then: git cherry-pick --continue"
            Write-Warn "Remaining commits:`n$($commits[$commits.IndexOf($sha)..($commits.Count-1)] -join "`n")"
            break
        }
    }

    if ($stashed -and $failed.Count -eq 0) {
        $pop = Invoke-Git -GitArgs @('stash', 'pop') -AllowFail
        if ($pop.Code -ne 0) {
            Write-Warn "Stash pop had conflicts. Run git stash pop after resolving the branch."
        }
    }
    elseif ($stashed) {
        Write-Warn "Your WIP is still in the stash (git stash list)."
    }

    if ($failed.Count -eq 0) {
        Write-Info "Rebase complete on $newBranch. After Unity conflict fixes, run: dcgo-patch.ps1 export"
    }
}

function Invoke-Status {
    param($Config, $Manifests)
    $base = Get-BaselineSha $Config
    $head = (Get-GitText -GitArgs @('rev-parse', '--short', 'HEAD')).Trim()
    $branch = (Get-GitText -GitArgs @('rev-parse', '--abbrev-ref', 'HEAD')).Trim()
    $onto = "$($Config.officialRemote)/$($Config.officialBranch)"
    $behind = ''
    $count = Invoke-Git -GitArgs @('rev-list', '--left-right', '--count', "${base}...HEAD") -AllowFail
    Write-Host "Repo:        $RepoRoot"
    Write-Host "Branch:      $branch ($head)"
    Write-Host "Baseline:    $base ($($Config.baselineLabel))"
    Write-Host "Official:    $onto"
    if ($count.Code -eq 0) {
        Write-Host "vs baseline: $($count.Output)"
    }
    $officialCount = Invoke-Git -GitArgs @('rev-list', '--left-right', '--count', "${onto}...HEAD") -AllowFail
    if ($officialCount.Code -eq 0) {
        Write-Host "vs official: $($officialCount.Output)  (left=official-only, right=custom-only)"
    }
    Write-Host "Modules:     $($Manifests.id -join ', ')"
    Write-Host ""
    foreach ($m in $Manifests) {
        $overlay = @(Expand-RepoPaths (Get-UniqueStrings (@($m.overlay) + @($m.overlayFiles))))
        $overlay = @(Filter-RequiredOverlayFiles -OverlayFiles $overlay -Config $Config)
        $present = @($overlay | Where-Object { Test-Path -LiteralPath (Join-Path $RepoRoot (ConvertTo-WinPath $_)) }).Count
        Write-Host ("  [{0}] overlay {1}/{2}  patchFiles {3}" -f $m.id, $present, $overlay.Count, @($m.patchFiles).Count)
    }

    $localRoot = Resolve-LocalAssetsRoot -Config $Config -DestRoot $RepoRoot
    Write-Host ""
    Write-Host "Optional assets root: $localRoot"
    if (-not (Test-Path -LiteralPath $localRoot -PathType Container)) {
        Write-Host "  (folder not found - apply will skip optional packs)"
    }
    else {
        foreach ($entry in @(Get-OptionalAssetEntries $Config)) {
            $src = Join-Path $localRoot (ConvertTo-WinPath $entry)
            $inRepo = Join-Path $RepoRoot (ConvertTo-WinPath $entry)
            $srcState = if (Test-Path -LiteralPath $src) { 'in-local' } else { 'missing-local' }
            $repoState = if (Test-Path -LiteralPath $inRepo) { 'in-Assets' } else { 'not-in-Assets' }
            Write-Host ("  [{0}] {1}, {2}" -f $entry, $srcState, $repoState)
        }
    }
}

function Invoke-Verify {
    param($Config, $Manifests)
    $errors = @()
    $base = Get-BaselineSha $Config
    $rev = Invoke-Git -GitArgs @('cat-file', '-t', $base) -AllowFail
    if ($rev.Code -ne 0) { $errors += "Baseline commit not found: $base" }

    $optional = Get-OptionalAssetEntries $Config
    foreach ($m in $Manifests) {
        foreach ($entry in @($m.overlay)) {
            if (Test-IsOptionalAssetPath -RelPath $entry -OptionalEntries $optional) { continue }
            $full = Join-Path $RepoRoot (ConvertTo-WinPath $entry)
            if (-not (Test-Path -LiteralPath $full)) {
                $errors += "[$($m.id)] overlay missing: $entry"
            }
        }
        foreach ($file in @($m.patchFiles)) {
            $full = Join-Path $RepoRoot (ConvertTo-WinPath $file)
            if (-not (Test-Path -LiteralPath $full)) {
                $errors += "[$($m.id)] patch file missing: $file"
                continue
            }
            if ($file.EndsWith('.cs')) {
                $text = Read-FileText $full
                $begin = "// === DCGO-CUSTOM:$($m.id) begin ==="
                if ($text -notlike "*$begin*") {
                    Write-Warn "[$($m.id)] no DCGO-CUSTOM:$($m.id) markers in $file (git apply will still be used)"
                }
            }
        }
    }

    $localRoot = Resolve-LocalAssetsRoot -Config $Config -DestRoot $RepoRoot
    Write-Info "Optional assets root: $localRoot"
    if (-not (Test-Path -LiteralPath $localRoot -PathType Container)) {
        Write-Info "Optional assets folder not found (OK - apply skips these packs)."
    }
    else {
        foreach ($entry in $optional) {
            $src = Join-Path $localRoot (ConvertTo-WinPath $entry)
            if (Test-Path -LiteralPath $src) {
                Write-Info "Optional pack present: $entry"
            }
            else {
                Write-Info "Optional pack skipped (not under local root): $entry"
            }
        }
    }

    if ($errors.Count -eq 0) {
        Write-Info "Verify OK."
        return
    }
    Write-Host "Verify found $($errors.Count) issue(s):"
    foreach ($e in $errors) { Write-Host " - $e" }
    exit 1
}

function Show-Help {
    @"
DCGO custom patching tool

Usage (from repo root):
  .\tools\dcgo-patch\dcgo-patch.ps1 export [-IncludeUncommitted] [-Portable]
  .\tools\dcgo-patch\dcgo-patch.ps1 apply [-Modules ranked,tournament] [-Target <dir>] [-LocalAssets <dir>]
  .\tools\dcgo-patch\dcgo-patch.ps1 rebase [-Onto origin/develop] [-BranchName custom/1.18]
  .\tools\dcgo-patch\dcgo-patch.ps1 status
  .\tools\dcgo-patch\dcgo-patch.ps1 verify

Optional licensed packs (Sound, SCI-FI UI, DigitalEnvironmentEffects) are NOT exported.
Place them under CustomAssets/ (mirroring Assets/...) or pass -LocalAssets before apply.

Typical next official version:
  git fetch origin
  .\tools\dcgo-patch\dcgo-patch.ps1 rebase -Onto origin/develop
  # resolve cherry-pick conflicts, open Unity, then:
  .\tools\dcgo-patch\dcgo-patch.ps1 export -IncludeUncommitted

See tools/dcgo-patch/README.md
"@ | Write-Host
}

$cfg = Get-Config
$manifests = Get-ModuleManifests -Config $cfg -Filter $Modules

switch ($Command) {
    'export' { Invoke-Export $cfg $manifests }
    'apply' { Invoke-Apply $cfg $manifests }
    'rebase' { Invoke-Rebase $cfg }
    'status' { Invoke-Status $cfg $manifests }
    'verify' { Invoke-Verify $cfg $manifests }
    default { Show-Help }
}

function GetPretzel {
    $pretzelFolder = Join-Path $PSScriptRoot "Pretzel"
    $pretzelExe = Join-Path $pretzelFolder "pretzel.exe"

    if (Test-Path $pretzelExe){
        Write-Debug "Pretzel found"
        return $pretzelExe;
    }

    if (!(Test-Path $pretzelFolder)) {
        New-Item $pretzelFolder -ItemType Directory | Out-Null
    }

    $pretzelArchive = Join-Path $pretzelFolder "Pretzel.0.5.0.zip"
    Invoke-WebRequest -Uri "https://github.com/Code52/pretzel/releases/download/v0.5.0/Pretzel.0.5.0.zip" -OutFile $pretzelArchive
    Expand-Archive $pretzelArchive $pretzelFolder
    Remove-Item $pretzelArchive

    $pretzelScriptArchive = Join-Path $pretzelFolder "Pretzel.ScriptCs.0.5.0.zip"
    Invoke-WebRequest -Uri "https://github.com/Code52/pretzel/releases/download/v0.5.0/Pretzel.ScriptCs.0.5.0.zip" -OutFile $pretzelScriptArchive
    Expand-Archive $pretzelScriptArchive $pretzelFolder
    Remove-Item $pretzelScriptArchive

    $pretzelExe
}

function Assert($expected, $actual) {
    $expected = $expected.Trim() -replace "`r`n","`n"
    $actual = $actual.Trim() -replace "`r`n","`n"
    if ($expected -ne $actual) {
        Write-Error "`$expected was $expected, but `$actual was $actual"
    }
}

function Assert-FileContent{
    param($Filename, $ExpectedContent)
    $content = Get-Content $Filename

    if (Compare-Object $ExpectedContent $content) {
        Compare-Object $ExpectedContent $content -IncludeEqual
        Write-Host "Files are different" -ForegroundColor Red
    } else {
        Write-Host "files are the same" -ForegroundColor Green
    }
}

function New-TestSite ($testRoot) {
    $testFolder = Join-Path $testRoot ("test-" + [Guid]::NewGuid())
    if (Test-Path $testFolder) {
        Remove-Item $testFolder -Confirm:$false -Force -Recurse
    }
    New-Item $testFolder -ItemType Directory | Out-Null

    & $pretzelExe create $testFolder | Out-Null
    # remove default post
    Remove-Item "$testFolder\_posts\$([System.DateTime]::Today.ToString("yyyy-MM-dd"))-myfirstpost.md"
    New-Item "$testFolder\_plugins" -ItemType Directory | Out-Null
    Copy-Item $testScript "$testFolder\_plugins"

    return $testFolder
}

function Bake-Pretzel {
    param (
        [Parameter(Mandatory=$true)][string] $pretzelExe,
        [Parameter(Mandatory=$true)][string] $site
    )
    & $pretzelExe bake $site | Out-Null
}
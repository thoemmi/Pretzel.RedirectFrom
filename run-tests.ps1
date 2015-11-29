Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$sourceFolder = $PSScriptRoot
$testSiteFolder = Join-Path $PSScriptRoot "testsite"
$pluginsFolder = Join-Path $testSiteFolder "_plugins"
$pretzelExe = Join-Path $PSScriptRoot "libs\Pretzel\Pretzel.exe"

function CreateEmptyFolder($path) {
    if (Test-Path $path) {
        Remove-Item $path -recurse -Force
        Start-Sleep -milliseconds 100
    }
    New-Item $path -type directory | Out-Null
}
function Assert($expected, $actual) {
    $expected = $expected.Trim() -replace "`r`n","`n"
    $actual = $actual.Trim() -replace "`r`n","`n"
    if ($expected -ne $actual) {
        Write-Error "`$expected was $expected, but `$actual was $actual"
    }
}

CreateEmptyFolder $testSiteFolder

New-Item $pluginsFolder -type directory | Out-Null

Push-Location $testSiteFolder
try {
    # create a default site and delete sitemap template
    & $pretzelExe create

    # the default _config.yml does not specify "url", so the plugin must fail with an error message
    Copy-Item (Join-Path $sourceFolder "Pretzel.RedirectFrom.csx") $pluginsFolder
    
    Set-Content (Join-Path $testSiteFolder "_posts/2015-11-29-test.md") '---
title: Test Title
redirect_from: ["old/path/file.aspx"]
---
My Content'
    
    & $pretzelExe bake

    if (!(Test-Path "_site\old\path\file.aspx\index.html")) {
        Write-Error "Redirect file was not generated."
    }

    $redirectContent = (Get-Content "_site\old\path\file.aspx\index.html") -join "`n"
    $expectedContent = '<!DOCTYPE html>
<meta charset="utf-8" />
<title>Redirecting...</title>
<link rel ="canonical" href="/2015/11/29/test.html" />
<meta http-equiv="refresh" content="0; url=/2015/11/29/test.html" />
<h1>Redirecting...</h1>
<a href ="/2015/11/29/test.html">Click here if you are not redirected.</a>
<script>
    location="/2015/11/29/test.html"
</script>'
    Assert $expectedContent $redirectContent

} finally {
    Pop-Location
}
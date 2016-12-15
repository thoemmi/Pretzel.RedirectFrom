Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

. (Join-Path $PSScriptRoot "test-helper.ps1")

$testScript = Join-Path $PSScriptRoot "Pretzel.RedirectFrom.csx"

$pretzelExe = GetPretzel

$testRoot = Join-Path $PSScriptRoot "tests"
if (!(Test-Path $testRoot)) {
    New-Item $testRoot -ItemType Directory | Out-Null
}


#----------------------------------------

Write-Host "Testing scalar redirect" -ForegroundColor Yellow

$testSite = New-TestSite $testRoot
Set-Content (Join-Path $testSite "_posts\2016-10-31-myfirstpost.md") @"
---
layout: post
title: "My First Post"
author: "Author"
comments: true
redirect_from: /pages/page1
---
test
"@

Bake-Pretzel -PretzelExe $pretzelExe -Site $testSite

$expectedContent = @"
<!DOCTYPE html>
<meta charset="utf-8" />
<title>Redirecting...</title>
<link rel="canonical" href="/2016/10/31/myfirstpost.html" />
<meta http-equiv="refresh" content="0; url=/2016/10/31/myfirstpost.html" />
<h1>Redirecting...</h1>
<a href="/2016/10/31/myfirstpost.html">Click here if you are not redirected.</a>
<script>
    location="/2016/10/31/myfirstpost.html"
</script>
"@ -split "`r`n" | ? { $_ -ne "" }
Assert-FileContent -Filename "$testSite\_site\pages\page1\index.html" -ExpectedContent $expectedContent

Remove-Item $testSite -Confirm:$false -Force -Recurse

#----------------------------------------

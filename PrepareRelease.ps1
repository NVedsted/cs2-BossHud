Push-Location $PSScriptRoot

$version = (Select-String -Pattern "<Version>(.+)</Version>" -Path .\BossHud\BossHud.csproj).Matches.Groups[1]

Write-Host "Preparing version $version"

if (Test-Path ./dist) {
    Write-Host "Removing existing dist folder"
    Remove-Item -Recurse -Force ./dist | Out-Null
}

New-Item -ItemType Directory ./dist/addons/counterstrikesharp/plugins/BossHud -Force | Out-Null

dotnet publish BossHud/BossHud.csproj -o ./dist/addons/counterstrikesharp/plugins/BossHud

Compress-Archive -Path ./dist/addons -DestinationPath ./dist/BossHud-$version.zip

Remove-Item -Recurse -Force ./dist/addons | Out-Null

Write-Host "Created dist/BossHud-$version-with-hitmarkers.zip"

#git tag "v$version"
#git-cliff --current

Pop-Location
$version = Read-Host "Please enter version: (default: 1.0.0)"
if ('' -eq $version) {
    $version = "1.0.0"
}
Write-Host "Version $version"

$configuration = Read-Host "Please enter config [release|debug]: (default: release)"
if( ('debug' -eq $configuration) ) {
    $configuration = "Debug"
}
if( ('release' -eq $configuration) ) {
    $configuration = "Release"
}
if( ('Debug' -ne $configuration) -and ('Release' -ne $configuration) ){
    $configuration = "Release"
}
Write-Host "Configuration $configuration"

dotnet build src `
        -o "build/$version/$configuration" `
        -c $configuration `
        -p:PackageVersion=$version

dotnet pack src `
    -o "pack" `
    -c $configuration `
    -p:PackageVersion=$version

$publish = Read-Host "Publish: (s|n)"
if( 's' -eq $publish ) {
    dotnet nuget push pack/VQLib.$version.nupkg --api-key oy2g3r6nhy6nbiv7n7qazd3hc2v3ebkepbizgt5yp4c57q --source https://api.nuget.org/v3/index.json
    dotnet nuget push pack/VQLib.Api.$version.nupkg --api-key oy2g3r6nhy6nbiv7n7qazd3hc2v3ebkepbizgt5yp4c57q --source https://api.nuget.org/v3/index.json
}


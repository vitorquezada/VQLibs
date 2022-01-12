$version = Read-Host "Please enter version: (default: 1.0.0.0)"
if ('' -eq $version) {
    $version = "1.0.0.0"
}
Write-Host "Version $version"

$configuration = Read-Host "Please enter config: (debug|release)"
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

$generate_build = Read-Host "Generate Build: (s|n)"
if( 's' -eq $generate_build ) {
    dotnet build `
        -o "build/$version/$configuration" `
        -c $configuration `
        -p:PackageVersion=$version
}

dotnet pack `
    -o "pack" `
    -c $configuration `
    -p:PackageVersion=$version
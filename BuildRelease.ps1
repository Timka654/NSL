$ver = $args[0]
dotnet build --configuration Release --version-suffix "$ver" "SSF.sln"
dotnet pack --configuration Release --output "package_nupkg_release" --version-suffix "$ver" SSF.sln
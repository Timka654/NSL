$ver = $args[0]
dotnet build --configuration Release "SSF.sln"
dotnet pack --configuration Release --output "nupkg" --version-suffix "$ver" SSF.sln
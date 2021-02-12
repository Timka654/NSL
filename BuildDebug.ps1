$ver = $args[0]
dotnet build
dotnet pack --configuration Debug --output "nupkg" --version-suffix "$ver" SSF.sln
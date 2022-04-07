$ver = $args[0]
dotnet build --configuration Debug --version-suffix "$ver" "SSF.sln"
dotnet pack --configuration Debug --output "package_nupkg" --version-suffix "$ver" SSF.sln
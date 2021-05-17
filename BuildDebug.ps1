$ver = $args[0]
dotnet build --configuration Debug "SSF.sln"
dotnet pack --configuration Debug --output "nupkg" --version-suffix "$ver" SSF.sln
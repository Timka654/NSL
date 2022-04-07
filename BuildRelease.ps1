$ver = $args[0]
dotnet build "NSL.sln" --configuration Release --output "package_build_release" --version-suffix "$ver" 
dotnet pack "NSL.sln" --configuration Release --output "package_nupkg_release" --version-suffix "$ver"
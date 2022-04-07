$ver = $args[0]
dotnet build --configuration Release --output "build/Release/build" --version-suffix "$ver" "NSL.sln"
dotnet pack --configuration Release --output "build/Release/package" --version-suffix "$ver" "NSL.sln"
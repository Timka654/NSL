$ver = $args[0]
dotnet build --version-suffix "$ver" --configuration Unity --output "build/Release/unity_build" "NSLUnity.sln"
dotnet pack --version-suffix "$ver" --configuration Unity --output "build/Release/unity_package" "NSLUnity.sln"
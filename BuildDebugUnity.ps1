$ver = $args[0]
dotnet build --version-suffix "$ver" --configuration UnityDebug --output "build/Debug/unity_build" "NSLUnity.sln"
dotnet pack --version-suffix "$ver" --configuration UnityDebug --output "build/Debug/unity_package" "NSLUnity.sln"
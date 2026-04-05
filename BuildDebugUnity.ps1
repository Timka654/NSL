$ver = $args[0]
dotnet build --version-suffix "$ver" --configuration UnityDebug --output "build/Debug/unity_build" "NSL.sln"
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
dotnet pack --version-suffix "$ver" --configuration UnityDebug --output "build/Debug/unity_package" "NSL.sln"
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
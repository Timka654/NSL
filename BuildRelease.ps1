$ver = $args[0]
dotnet build --configuration Release --output "build/Release/dll_$ver" --version-suffix "$ver" "NSL.sln"
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
dotnet pack --configuration Release --output "build/Release/package_$ver" --version-suffix "$ver" "NSL.sln"
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
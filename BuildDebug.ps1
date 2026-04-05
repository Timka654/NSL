$ver = $args[0]
dotnet build --configuration Debug --output "build/Debug/build" --version-suffix "$ver" "NSL.sln"
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
dotnet pack --configuration Debug --output "build/Debug/package" --version-suffix "$ver" "NSL.sln"
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
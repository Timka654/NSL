$ver = $args[0]
dotnet build --configuration Debug --output "build/Debug/dll_$ver" --version-suffix "$ver" "NSL.ShaderVM.slnx"
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
dotnet pack --configuration Debug --output "build/Debug/package_$ver" --version-suffix "$ver" "NSL.ShaderVM.slnx"
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
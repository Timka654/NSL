$LASTEXITCODE = 0
./BuildReleaseAll
# Evaluate success/failure
if($LASTEXITCODE -eq 0)
{
	NU.SimpleClient --d "build" -upload -closeOnSuccess
}
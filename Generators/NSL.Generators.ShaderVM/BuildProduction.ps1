./BuildAll

if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

NU.SimpleClient --d "build" -upload -closeOnSuccess
$upFolder = [System.IO.Path]::Combine($PSScriptRoot,"..");
$files = [System.IO.Directory]::GetFiles($PSScriptRoot, "*.example");

foreach ($currentItemName in $files) {

    $fileName = [System.IO.Path]::GetFileNameWithoutExtension($currentItemName);

    $fileDestPath = [System.IO.Path]::Combine($upFolder, $fileName);

    if ([System.IO.File]::Exists($fileDestPath)) {
        [System.IO.File]::Delete($fileDestPath);
    }

    [System.IO.File]::Copy($currentItemName, [System.IO.Path]::Combine($upFolder,$fileName));
}
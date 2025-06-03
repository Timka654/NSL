# replace-init-template.ps1

# Получаем текущую директорию, где находится скрипт
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Definition

# Рекурсивно ищем все файлы, оканчивающиеся на ".init_template"
Get-ChildItem -Path $scriptDir -Recurse -File -Filter "*.init_template" | ForEach-Object {
    $file = $_
    $newName = $file.Name -replace '\.init_template$', ''
    $newPath = Join-Path -Path $file.DirectoryName -ChildPath $newName

    try {
        # Копируем файл, перезаписываем при необходимости
        Copy-Item -Path $file.FullName -Destination $newPath -Force
        Write-Host "Copied: $($file.FullName) -> $newPath"
    }
    catch {
        Write-Warning "Failed to copy: $($file.FullName) -> $newPath. Error: $_"
    }
}
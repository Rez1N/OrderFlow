#!/usr/bin/env powershell
# Upload OrderFlow to GitHub
# Запуск: .\upload-to-github.ps1

Write-Host "╔════════════════════════════════════════════════════════════╗" -ForegroundColor Cyan
Write-Host "║       OrderFlow - Upload to GitHub                         ║" -ForegroundColor Cyan
Write-Host "╚════════════════════════════════════════════════════════════╝" -ForegroundColor Cyan
Write-Host ""

# Проверка Git
if (-not (Get-Command git -ErrorAction SilentlyContinue)) {
    Write-Host "❌ ERROR: Git не найден! Установите Git с https://git-scm.com" -ForegroundColor Red
    exit 1
}

Write-Host "✅ Git найден: $(git --version)" -ForegroundColor Green
Write-Host ""

# Запрос данных
$username = Read-Host "🔹 Введите ваш GitHub username"
$token = Read-Host "🔹 Введите Personal Access Token (или пароль)" -AsSecureString

if ([string]::IsNullOrWhiteSpace($username)) {
    Write-Host "❌ ERROR: Username не может быть пустым!" -ForegroundColor Red
    exit 1
}

# Конвертируем token из SecureString
$tokenPlain = [System.Net.NetworkCredential]::new("", $token).Password

Write-Host ""
Write-Host "📝 Конфигурация:" -ForegroundColor Yellow
Write-Host "  Repository URL: https://github.com/$username/OrderFlow.git"
Write-Host "  Branch: main"
Write-Host ""

# Переходим в директорию репозитория
$repoPath = Split-Path -Parent $MyInvocation.MyCommand.Definition
if (-not (Test-Path "$repoPath\.git")) {
    Write-Host "❌ ERROR: Это не Git репозиторий! Запустите из корня OrderFlow" -ForegroundColor Red
    exit 1
}

Set-Location $repoPath

# Проверяем текущий статус
Write-Host "🔍 Проверка статуса репозитория..." -ForegroundColor Cyan
$status = git status --porcelain
if ($status) {
    Write-Host "⚠️  WARNING: Есть изменения, которые не закоммичены!" -ForegroundColor Yellow
    Write-Host "Выполните `git commit` перед загрузкой"
    exit 1
}
Write-Host "✅ Репозиторий чист" -ForegroundColor Green
Write-Host ""

# Добавляем remote
Write-Host "🔗 Добавление remote..." -ForegroundColor Cyan
$remoteUrl = "https://${username}:${tokenPlain}@github.com/$username/OrderFlow.git"
git remote add origin $remoteUrl
if ($LASTEXITCODE -ne 0) {
    Write-Host "⚠️  Remote уже существует, обновляю URL..." -ForegroundColor Yellow
    git remote set-url origin $remoteUrl
}
Write-Host "✅ Remote добавлен" -ForegroundColor Green
Write-Host ""

# Переименовываем ветку на main если нужно
Write-Host "🌿 Проверка ветки..." -ForegroundColor Cyan
$currentBranch = git rev-parse --abbrev-ref HEAD
if ($currentBranch.Trim() -ne "main") {
    Write-Host "📝 Переименовываю ветку $currentBranch → main..." -ForegroundColor Yellow
    git branch -M main
}
Write-Host "✅ Ветка: main" -ForegroundColor Green
Write-Host ""

# Загружаем на GitHub
Write-Host "📤 Загрузка на GitHub..." -ForegroundColor Cyan
Write-Host "    (это может занять несколько секунд)" -ForegroundColor Gray
Write-Host ""

git push -u origin main

if ($LASTEXITCODE -eq 0) {
    Write-Host ""
    Write-Host "╔════════════════════════════════════════════════════════════╗" -ForegroundColor Green
    Write-Host "║           ✅ УСПЕШНО ЗАГРУЖЕНО НА GITHUB!                ║" -ForegroundColor Green
    Write-Host "╚════════════════════════════════════════════════════════════╝" -ForegroundColor Green
    Write-Host ""
    Write-Host "📍 Репозиторий: https://github.com/$username/OrderFlow" -ForegroundColor Green
    Write-Host ""
    Write-Host "Ваш код теперь доступен на GitHub!" -ForegroundColor Green
    Write-Host "Преподаватель может просмотреть его по ссылке выше." -ForegroundColor Green
} else {
    Write-Host ""
    Write-Host "❌ ОШИБКА при загрузке!" -ForegroundColor Red
    Write-Host "Проверьте:"
    Write-Host "  1. Username правильный"
    Write-Host "  2. Personal Access Token действителен"
    Write-Host "  3. Репозиторий создан на GitHub"
    Write-Host "  4. Интернет соединение активно"
    exit 1
}

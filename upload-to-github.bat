@echo off
REM Upload OrderFlow to GitHub - Windows Batch Script
REM Запуск: double-click на этот файл или выполните upload-to-github.bat

setlocal enabledelayedexpansion

echo.
echo ═══════════════════════════════════════════════════════════════
echo.
echo   OrderFlow - Upload to GitHub (Windows)
echo.
echo ═══════════════════════════════════════════════════════════════
echo.

REM Check if Git is installed
git --version >nul 2>&1
if errorlevel 1 (
    echo ERROR: Git не найден! Установите Git с https://git-scm.com
    pause
    exit /b 1
)

echo [OK] Git найден
echo.

REM Ask for GitHub username
set /p username="Введите ваш GitHub username: "
if "!username!"=="" (
    echo ERROR: Username не может быть пустым!
    pause
    exit /b 1
)

REM Ask for token/password
echo.
echo Введите Personal Access Token (или пароль):
echo (примечание: пароль не будет виден при вводе)
set /p token="Token: "

if "!token!"=="" (
    echo ERROR: Token не может быть пустым!
    pause
    exit /b 1
)

echo.
echo ═══════════════════════════════════════════════════════════════
echo.
echo Конфигурация:
echo   Repository: https://github.com/!username!/OrderFlow.git
echo   Branch: main
echo.
echo ═══════════════════════════════════════════════════════════════
echo.

REM Change to repo directory
cd /d "%~dp0"

REM Check if it's a git repo
if not exist ".git" (
    echo ERROR: Это не Git репозиторий!
    pause
    exit /b 1
)

REM Check for uncommitted changes
for /f %%i in ('git status --porcelain') do (
    echo WARNING: Есть неэтапированные изменения!
    echo Выполните git commit перед загрузкой.
    pause
    exit /b 1
)

echo [OK] Репозиторий чист

REM Add remote
echo.
echo [*] Добавление remote...
git remote add origin https://!username!:!token!@github.com/!username!/OrderFlow.git
if errorlevel 1 (
    echo [*] Remote уже существует, обновляю...
    git remote set-url origin https://!username!:!token!@github.com/!username!/OrderFlow.git
)
echo [OK] Remote добавлен

REM Rename branch to main
echo.
echo [*] Проверка ветки...
for /f %%i in ('git rev-parse --abbrev-ref HEAD') do set branch=%%i
if not "!branch!"=="main" (
    echo [*] Переименовываю ветку !branch! в main...
    git branch -M main
)
echo [OK] Ветка: main

REM Push to GitHub
echo.
echo [*] Загрузка на GitHub (это может занять несколько секунд)...
echo.

git push -u origin main

if errorlevel 1 (
    echo.
    echo ═══════════════════════════════════════════════════════════════
    echo ERROR! Ошибка при загрузке!
    echo ═══════════════════════════════════════════════════════════════
    echo.
    echo Проверьте:
    echo   1. Username правильный
    echo   2. Personal Access Token действителен
    echo   3. Репозиторий создан на GitHub
    echo   4. Интернет соединение активно
    echo.
    pause
    exit /b 1
) else (
    echo.
    echo ═══════════════════════════════════════════════════════════════
    echo SUCCESS! Успешно загружено на GitHub!
    echo ═══════════════════════════════════════════════════════════════
    echo.
    echo Repository: https://github.com/!username!/OrderFlow
    echo.
    echo Ваш код теперь доступен на GitHub!
    echo.
    pause
)

endlocal

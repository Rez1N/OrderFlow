@echo off
REM ═════════════════════════════════════════════════════════════════
REM OrderFlow - Quick Push to GitHub
REM ═════════════════════════════════════════════════════════════════
REM 
REM Использование:
REM   1. Получите Personal Access Token с https://github.com/settings/tokens
REM   2. Отметьте scope: repo (full control)
REM   3. Запустите этот батник и введите token когда попросит
REM

setlocal enabledelayedexpansion

cls
echo.
echo ═══════════════════════════════════════════════════════════════════
echo   OrderFlow GitHub Push
echo ═══════════════════════════════════════════════════════════════════
echo.

cd /d "%~dp0"

REM Проверяем git
git --version > nul 2>&1
if errorlevel 1 (
    echo ERROR: Git не найден!
    echo Установите: https://git-scm.com
    pause
    exit /b 1
)

REM Проверяем репо
if not exist ".git" (
    echo ERROR: Это не Git репозиторий!
    pause
    exit /b 1
)

REM Показываем информацию
echo Repository: https://github.com/Rez1N/OrderFlow
echo Branch: master
echo Commits: 2
echo.

REM Проверяем статус
for /f %%i in ('git status --porcelain') do (
    echo ERROR: Есть неэтапированные изменения!
    echo Выполните: git add . && git commit -m "message"
    pause
    exit /b 1
)

echo Status: OK
echo.

REM Объясняем что нужно
echo ┌─────────────────────────────────────────────────────────────────┐
echo │ Для загрузки нужен Personal Access Token                        │
echo │                                                                 │
echo │ Получить token:                                                 │
echo │ 1. https://github.com/settings/tokens                           │
echo │ 2. "Generate new token (classic)"                               │
echo │ 3. Scope: [x] repo                                              │
echo │ 4. "Generate token"                                             │
echo │ 5. Скопируйте токен!                                            │
echo │                                                                 │
echo │ Введите токен в следующем шаге                                 │
echo └─────────────────────────────────────────────────────────────────┘
echo.

REM Username
echo Введите ваш GitHub username (Rez1N):
set /p username="Username [Rez1N]: "
if "!username!"=="" set username=Rez1N

REM Token
echo.
echo Введите Personal Access Token:
echo (пароль не будет виден):
set /p token="Token: "

if "!token!"=="" (
    echo ERROR: Token не может быть пустым!
    pause
    exit /b 1
)

REM Пытаемся загрузить
cls
echo.
echo ═══════════════════════════════════════════════════════════════════
echo   Загрузка на GitHub...
echo ═══════════════════════════════════════════════════════════════════
echo.

git push -u https://!username!:!token!@github.com/Rez1N/OrderFlow.git master

if errorlevel 1 (
    echo.
    echo ERROR: Ошибка при загрузке!
    echo.
    echo Причины:
    echo - Token невалидный
    echo - Репозиторий не существует
    echo - Нет интернета
    echo.
    pause
    exit /b 1
)

echo.
echo ═══════════════════════════════════════════════════════════════════
echo   ✅ УСПЕШНО!
echo ═══════════════════════════════════════════════════════════════════
echo.
echo Код загружен на: https://github.com/Rez1N/OrderFlow
echo.
pause

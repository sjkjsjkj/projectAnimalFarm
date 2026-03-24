@echo off
chcp 65001 > nul
cls

echo [1단계] 초기화 중...
:: 배치 파일이 있는 원래 위치 저장
set "ORIGINAL_PATH=%~dp0"

:: 기존 파일 삭제
if exist "all_scripts.txt" del "all_scripts.txt"

echo [2단계] Scripts 폴더로 진입 시도...
:: 경로 변수 대신 직접 이동
cd /d "%ORIGINAL_PATH%Assets\Scripts"

if errorlevel 1 (
    echo.
    echo [치명적 오류] Assets\Scripts 폴더로 들어갈 수 없습니다!
    echo 배치 파일 위치를 확인해주세요.
    pause
    exit /b
)

echo [3단계] 스크립트 수집 시작...
echo (파일이 많으면 시간이 좀 걸릴 수 있습니다)

:: 현재 폴더(.)에서 하위 폴더(/r)까지 모두 검색
for /r %%f in (*.cs) do (
    :: 진행 상황을 보여주면 멈췄는지 알 수 있습니다
    echo 수집 중: %%~nxf
    
    echo. >> "%ORIGINAL_PATH%all_scripts.txt"
    echo // ======================================================== >> "%ORIGINAL_PATH%all_scripts.txt"
    echo // 경로: %%~f >> "%ORIGINAL_PATH%all_scripts.txt"
    echo // ======================================================== >> "%ORIGINAL_PATH%all_scripts.txt"
    echo. >> "%ORIGINAL_PATH%all_scripts.txt"
    
    type "%%f" >> "%ORIGINAL_PATH%all_scripts.txt"
)

echo.
echo [완료] 모든 작업이 끝났습니다!
echo 파일 위치: "%ORIGINAL_PATH%all_scripts.txt"
pause
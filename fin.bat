@echo off
cd /d %~dp0
tasklist | find /i "Unity.exe" >nul && (echo Cierra Unity y vuelve a intentar. & pause & exit /b)
git add -A
git commit -m "Avance %date% %time:~0,5%"
git pull --rebase
git push
pause
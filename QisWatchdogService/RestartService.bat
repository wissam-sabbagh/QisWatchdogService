	

:stop
sc stop %1

rem :kill
taskkill /F /IM %2.exe
rem cause a ~5 second sleep before checking the service state
ping 127.0.0.1 -n 10 -w 1000 > nul

sc query %1 | find /I "STATE" | find "STOPPED"
if errorlevel 1 goto :stop
goto :start

:start
net start | find /i "%1" >nul && goto :start
sc start %1


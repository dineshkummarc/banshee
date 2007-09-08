@ECHO OFF
CALL candle.exe banshee.wxs
IF ERRORLEVEL == 1 GOTO FAILED
CALL light.exe banshee.wixobj
IF ERRORLEVEL == 1 GOTO FAILED
GOTO END
:FAILED
ECHO Failed!
pause
:END
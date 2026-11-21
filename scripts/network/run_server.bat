@echo off
title Bullet Nie Heaven - Backend API
echo Uruchamianie serwera backendowego na http://localhost:5157 ...
echo Swagger UI dostepny pod: http://localhost:5157/swagger
echo Aby zatrzymac serwer, nacisnij Ctrl+C lub zamknij to okno.
echo.

dotnet run --project "server\Bullet_Not_Heaven_DB\Bullet_Not_Heaven_DB.csproj" --urls "http://localhost:5157"

pause
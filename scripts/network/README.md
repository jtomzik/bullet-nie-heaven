# Backend i komunikacja sieciowa (Bullet Nie Heaven)

## 1. Uruchomienie serwera (bez Visual Studio)
1. Wymagany jest zainstalowany **.NET 8 SDK** (taki sam jak do kompilacji C# w Godocie).
2. Kliknij dwukrotnie plik `scripts/network/run_server.bat`.
3. Serwer uruchomi się pod adresem `http://localhost:5157` i **automatycznie utworzy bazę danych SQLite** (`bulletheaven.db`) przy pierwszym starcie.
4. Dokumentacja endpointów (Swagger UI): `http://localhost:5157/swagger`.

*Uwaga: Jeśli serwer nie jest uruchomiony, gra automatycznie przełączy się w tryb offline (zapisując wyniki lokalnie w `user://local_scores.json`).*

---

## 2. Użycie `ApiClient` w Godocie (dla UI / Core)

`ApiClient` jest skonfigurowany jako **Autoload (Singleton)** dostępny z każdej sceny pod ścieżką `/root/ApiClient`.

### Pobranie referencji w skrypcie C#
```csharp
private ApiClient _api;

public override void _Ready()
{
    _api = GetNode<ApiClient>("/root/ApiClient");
}

100% vibe coded :3
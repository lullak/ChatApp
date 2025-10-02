# ChatApp

Real‑time chat built with .NET 9, MVC and SignalR.  
Features: SignalR , JWT auth via cookie, AES‑GCM client encryption, private direct chats, SQLite and Serilog logging.

## Prerequisites
- .NET 9 SDK
- Visual Studio 2022 - Readme only explains how to set it up in visual studio.

## Configuration
`appsettings.json`:
- `"ConnectionStrings": { "DefaultConnection": "Data Source=chat.db" },`
- `"Jwt": { "Key": "Addsecretkeyhere!", "Issuer": "https://localhost:7061", "Audience": "https://localhost:7061"}`

## Run locally (Visual Studio)
1. Open solution.
2. Restore packages: __Build > Restore NuGet Packages__.
3. Set `ChatApp` as startup project: right‑click project → __Set as Startup Project__.
4. EF Migrations: open __Tools > NuGet Package Manager > Package Manager Console__ (select `ChatApp` as Default project) and run:
   - `Add-Migration InitialCreate` (only first time)
   - `Update-Database`
5. Trust dev HTTPS certificate (required for secure cookie):
   - Visual Studio prompts on first run, or run:  
    - `dotnet dev-certs https --trust`
    - `dotnet dev-certs https --clean` IF NEEDED
6. Start.
7. Open: `https://localhost:7061`


## How it works
- Login issues a JWT stored as an HttpOnly cookie (`token`). SignalR uses that cookie for auth.
- On connect the server sends per‑group base64 AES keys. Client `wwwroot/js/aes.js` .
- Encrypted messages use the format: `ENC:<ivBase64>:<cipherBase64>`. Server stores encrypted payloads; clients decrypt and sanitize with DOMPurify .

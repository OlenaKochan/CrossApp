# CrossApp 
Наскрізний проєкт з крос-платформного програмування. 
Предметна область: Склад. Сутності: Product, StockBatch, Warehouse, Movement. 
Призначення: облік залишків товарів по партіях.
## Запуск 
dotnet build 
dotnet run --project src/Cli 
## Середовище 
.NET SDK 10.0, Windows 11 x64 / Ubuntu 24.04 x64

## Додаткове завдання
1. Self-contained публікація
Проєкт опубліковано у режимі self-contained для двох різних RID: win-x64 — Windows 64-bit, linux-x64 — Linux 64-bit
Команди публікації:
dotnet publish src/Cli -c Release -r win-x64 --self-contained true
dotnet publish src/Cli -c Release -r linux-x64 --self-contained true

Розмір каталогів publish:
win-x64	78 MB
linux-x64	80 MB
Розмір каталогів перевірено за допомогою команди:
du -sh src/Cli/bin/Release/net10.0/win-x64/publish
du -sh src/Cli/bin/Release/net10.0/linux-x64/publish

2. Прапорець --json
Звичайний запуск:
dotnet run --project src/Cli
Запуск із прапорцем --json:
dotnet run --project src/Cli -- --json

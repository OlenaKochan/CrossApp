# CrossApp 
Наскрізний проєкт з крос-платформного програмування. 
Предметна область: Склад. Сутності: Product, StockBatch, Warehouse, Movement. 
Призначення: облік залишків товарів по партіях.
## Запуск 
`dotnet build`
`dotnet run --project src/Cli `
## Середовище 
.NET SDK 10.0, Windows 11 x64 / Ubuntu 24.04 x64

## Додаткове завдання лабораторна 1
1. Self-contained публікація
Проєкт опубліковано у режимі self-contained для двох різних RID: win-x64 — Windows 64-bit, linux-x64 — Linux 64-bit
Команди публікації:
`dotnet publish src/Cli -c Release -r win-x64 --self-contained true`
`dotnet publish src/Cli -c Release -r linux-x64 --self-contained true`

Розмір каталогів publish:
win-x64	78 MB
linux-x64	80 MB
Розмір каталогів перевірено за допомогою команди:
`du -sh src/Cli/bin/Release/net10.0/win-x64/publish`
`du -sh src/Cli/bin/Release/net10.0/linux-x64/publish`

2. Прапорець --json
Звичайний запуск:
dotnet run --project src/Cli
Запуск із прапорцем --json:
dotnet run --project src/Cli -- --json

3. Запуск у Docker
`MSYS_NO_PATHCONV=1 docker run --rm -v "$PWD:/src" -w /src [mcr.microsoft.com/dotnet/sdk:10.0](https://mcr.microsoft.com/dotnet/sdk:10.0) dotnet run --project src/Cli`


## Лабораторна робота 2. Публікація у двох режимах

1. Self-contained.
Self-contained публікація містить код застосунку, залежності та .NET runtime. 
Команда публікації:
`dotnet publish src/Cli -c Release -r win-x64 --self-contained true`

2. Framework-dependent
Framework-dependent публікація містить код застосунку та залежності, але не містить .NET runtime.
Команда публікації:
`dotnet publish src/Cli -c Release -r win-x64 --self-contained false`

### Порівняння
| RID | Режим | Розмір | Чи потрібен встановлений runtime |
| :--- | :--- | :--- | :--- |
| win-x64 | Self-contained | 62 MB | Ні |
| win-x64 | Framework-dependent | 233 KB | Так |

Розмір каталогу перевірявся командою:
`du -sh src/Cli/bin/Release/net10.0/win-x64/publish`

Запуск із каталогу publish:
`cd src/Cli/bin/Release/net10.0/win-x64/publish`
`./Cli.exe`

using System.Runtime.InteropServices;
using System.Text.Json;

Console.OutputEncoding = System.Text.Encoding.UTF8;

var environmentInfo = new
{
    Application = "CrossApp – практикум з крос-платформного програмування",
    Student = "Кочан Олена",
    Group = "Група ФЕІ-32",
    OSDescription = RuntimeInformation.OSDescription,
    EnvironmentOSVersion = Environment.OSVersion.ToString(),
    ProcessArchitecture = RuntimeInformation.ProcessArchitecture.ToString(),
    DotNetVersion = Environment.Version.ToString(),
    Runtime = RuntimeInformation.FrameworkDescription,
    ApplicationDirectory = AppContext.BaseDirectory,
    CurrentDirectory = Environment.CurrentDirectory,
    Domain = "Склад",
    Entities = new[]
    {
        "Product",
        "StockBatch",
        "Warehouse",
        "Movement"
    }
};

if (args.Contains("--json"))
{
    var options = new JsonSerializerOptions
    {
        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    string json = JsonSerializer.Serialize(environmentInfo, options);

    Console.WriteLine(json);
}
else
{
    Console.WriteLine(environmentInfo.Application);
    Console.WriteLine($"Студент: {environmentInfo.Student}, група {environmentInfo.Group}");
    Console.WriteLine(new string('-', 60));

    Console.WriteLine($"ОС (OSDescription)   : {environmentInfo.OSDescription}");
    Console.WriteLine($"ОС (Environment)     : {environmentInfo.EnvironmentOSVersion}");
    Console.WriteLine($"Архітектура процесу  : {environmentInfo.ProcessArchitecture}");
    Console.WriteLine($"Версія .NET (CLR)    : {environmentInfo.DotNetVersion}");
    Console.WriteLine($"Runtime              : {environmentInfo.Runtime}");
    Console.WriteLine($"Каталог застосунку   : {environmentInfo.ApplicationDirectory}");
    Console.WriteLine($"Поточний каталог     : {environmentInfo.CurrentDirectory}");

    Console.WriteLine(new string('-', 60));
    Console.WriteLine($"Предметна область: {environmentInfo.Domain}");
    Console.WriteLine($"Сутності: {string.Join(", ", environmentInfo.Entities)}");
}
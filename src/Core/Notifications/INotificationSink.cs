using Core.Events;
namespace Core.Notifications;
public interface INotificationSink
{
    void Notify(CatalogChangedEventArgs e);
}

// Перша реалізація: вивід у консоль
public sealed class ConsoleSink : INotificationSink
{
    public void Notify(CatalogChangedEventArgs e) =>
        Console.WriteLine($"  [ConsoleSink] {e.At:HH:mm:ss} | {e.Kind,-8} | Товар: {e.Id} | Дельта: {e.Delta,4:+0;-0} | Залишок: {e.QuantityAfter}");
}

// Друга реалізація: запис у файл sink.log
public sealed class FileSink(string path) : INotificationSink
{
    private readonly string _path = Path.GetFullPath(path);

    public void Notify(CatalogChangedEventArgs e)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(_path)!);
        var line = $"{e.At:yyyy-MM-dd HH:mm:ss} [{e.Kind}] ID={e.Id} Delta={e.Delta:+0;-0} After={e.QuantityAfter}";
        File.AppendAllText(_path, line + Environment.NewLine);
    }
}
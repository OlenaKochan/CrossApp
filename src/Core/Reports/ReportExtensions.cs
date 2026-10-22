namespace Core.Reports;

public static class ReportExtensions
{
    //2 додаткове : Власний метод-розширення для вибірки топ-N елементів
    public static IEnumerable<T> TopBy<T, TKey>(
        this IEnumerable<T> source, 
        Func<T, TKey> keySelector, 
        int count) 
        where TKey : IComparable<TKey> =>
        source.OrderByDescending(keySelector).Take(count);
}
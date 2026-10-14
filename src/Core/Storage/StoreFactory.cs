using Core.Abstractions;
using Core.Domain;
namespace Core.Storage;
public static class StoreFactory
{
    public static ICatalogStore Create(string[] args, IEnumerable<Product>? seed = null)
    {
        bool useFile = args.Contains("--file");
        bool useCache = args.Contains("--cached");
        string dataPath = Path.Combine(AppContext.BaseDirectory, "data", "catalog.json");

        ICatalogStore store = useFile
            ? new FileCatalogStore(dataPath)
            : new InMemoryCatalogStore(seed);

        if (useCache)
        {
            store = new CachingCatalogStore(store);
        }
        return store;
    }
}
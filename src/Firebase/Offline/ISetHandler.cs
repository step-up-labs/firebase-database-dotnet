namespace Firebase.Database.Offline
{
    using Firebase.Database.Query;

    using System.Threading.Tasks;

    public interface ISetHandler<in T>
    {
        Task SetAsync(ChildQuery query, string key, OfflineEntry entry);
    }
}

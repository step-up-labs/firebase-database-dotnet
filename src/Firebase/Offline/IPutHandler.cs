namespace Firebase.Database.Offline
{
    using Firebase.Database.Query;

    using System.Threading.Tasks;

    public interface IPutHandler<in T>
    {
        Task PutAsync(ChildQuery query, string key, OfflineEntry entry);
    }
}

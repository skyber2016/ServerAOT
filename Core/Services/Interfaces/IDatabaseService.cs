
namespace Core
{
    public interface IDatabaseService
    {
        Task<string> ExecuteAsync(QueryNative query, CancellationToken cancellationToken = default);
    }
}

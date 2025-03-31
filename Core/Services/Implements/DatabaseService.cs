using System.Text;
using System.Text.Json;

namespace Core
{
    public class DatabaseService : IDatabaseService, IDisposable
    {
        private readonly HttpClient _http;
        public DatabaseService()
        {
            _http = new HttpClient
            {
                BaseAddress = new Uri(ApplicationContext.Instance.AppConfig.DatabaseConfig.Host)
            };
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            _http.Dispose();
        }

        public async Task<string> ExecuteAsync(QueryNative query, CancellationToken cancellationToken = default)
        {
            if (query == null)
            {
                throw new ArgumentNullException($"Property {nameof(query)} cannot be null");
            }
            var json = JsonSerializer.Serialize(query, QueryNativeContext.Default.QueryNative);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _http.PostAsync("/query2json", content);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadAsStringAsync(cancellationToken);
            }
            return null;
        }
    }
}

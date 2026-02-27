using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace HazelnutVeb.Services
{
    public class NotificationService
    {
        private readonly IConfiguration _configuration;
        private const string AppId = "76c8b428-a07d-4e09-9b01-1497eed30586";
        private const string ApiUrl = "https://onesignal.com/api/v1/notifications";

        public NotificationService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendLowInventoryAlert()
        {
            var apiKey = _configuration["OneSignal:ApiKey"];
            if (string.IsNullOrEmpty(apiKey))
            {
                return;
            }

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", apiKey);

                var payload = new
                {
                    app_id = AppId,
                    included_segments = new[] { "All" },
                    headings = new { en = "Low Inventory Alert" },
                    contents = new { en = "Hazelnut inventory is below 5kg. Please restock." }
                };

                var json = JsonSerializer.Serialize(payload);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                await client.PostAsync(ApiUrl, content);
            }
        }

        public async Task SendLowInventoryNotification(double quantity)
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", "76c8b428-a07d-4e09-9b01-1497eed30586");

                var payload = new
                {
                    app_id = AppId,
                    included_segments = new[] { "All" },
                    headings = new { en = "Low Inventory Alert" },
                    contents = new { en = $"Only {quantity} kg left in stock!" }
                };

                var json = JsonSerializer.Serialize(payload);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                await client.PostAsync(ApiUrl, content);
            }
        }
    }
}

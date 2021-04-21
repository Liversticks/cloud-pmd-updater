using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Threading.Tasks;

namespace cloud_pmd_updater
{    
    public static class UpdateBRT
    {
        private static HttpClient httpClient = new HttpClient();

        // Add Cosmos DB input source
        [FunctionName("UpdateBRT")]
        public static async Task Run([TimerTrigger("*/30 * * * * *")]TimerInfo myTimer, ILogger log)
        {
            // Fetch "Pokemon Mystery Dungeon: Blue Rescue Team" from internal table

            // Parse BRT record

            // Populate SRC API URL
            log.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");
            string gameId = "j1lqnj6g";
            string categoryId = "zd37wy2n";
            string url = $"https://speedrun.com/api/v1/leaderboards/{gameId}/category/{categoryId}?var-ylpvm5kl=z195k58q&top=1&var-78963368=814e7kwl&embed=players";
            var response = await httpClient.GetAsync(url);
            var resString = await response.Content.ReadAsStringAsync();
            
            // parse response JSON

            // Add Cosmos DB output

            log.LogInformation(resString);
        }
    }
}

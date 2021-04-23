using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text.Json.Serialization;

namespace cloud_pmd_updater
{
    public class BRTCollection
    {
        [JsonPropertyName("id")]
        public string ID { get; set; }
        public string GameID { get; set; }
        public string[] Categories { get; set; }
        public string PlatformID { get; set; }
        public string[] Platforms { get; set; }
        public string LanguageID { get; set; }
        public string[] Languages { get; set; }
    }

    public static class UpdateBRT
    {   
        private static HttpClient httpClient = new HttpClient();

        [FunctionName("UpdateBRT")]
        public static async Task Run([TimerTrigger("0 * * * * *")]TimerInfo myTimer, 
            [CosmosDB(
                databaseName: "Shared-Free",
                collectionName: "V1-pmdboard",
                ConnectionStringSetting = "CosmosDBConnection",
                Id = "internal-Pokémon-Blue-Rescue-Team",
                PartitionKey = "internal-Pokémon-Blue-Rescue-Team"
            )] BRTCollection runInfo,
            ILogger log)
        {
            // Fetch "Pokemon Mystery Dungeon: Blue Rescue Team" from internal table
            
            //Need to trim category, platform, and language
            //Category format: xxxxxxxx-Category Name
            //Platform format: xxxxxxxx-Platform Name
            //Language format: ENG/JPN-xxxxxxxx
            foreach (var category in runInfo.Categories)
            {
                foreach (var platform in runInfo.Platforms)
                {
                    foreach (var language in runInfo.Languages)
                    {
                        var categoryInfo = category.Split('-');
                        var platformInfo = platform.Split('-');
                        var languageInfo = language.Split('-');
                        
                        string url = $"https://speedrun.com/api/v1/leaderboards/{runInfo.GameID}/category/{categoryInfo[0]}?var-{runInfo.PlatformID}={platformInfo[0]}&var-{runInfo.LanguageID}={languageInfo[1]}&top=1&embed=players";
                        log.LogInformation(url);
                        var response = await httpClient.GetAsync(url);
                        var resString = await response.Content.ReadAsStringAsync();
                        log.LogInformation(resString);
                    }
                }
            }
            
            // Populate SRC API URL
            log.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");           
            
            
            // parse response JSON

            // Add Cosmos DB output

            
        }
    }
}

using System;
using System.Net.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using CloudPMD.Shared;
using System.Threading.Tasks;
using System.Text.Json;

namespace CloudPMD.Updater
{
    public static class UpdateRRT
    {
        public static HttpClient httpClient = new HttpClient();

        [FunctionName("UpdateRRT")]
        public static async Task Run([TimerTrigger("0 0 1 * * *")] TimerInfo myTimer,
            [CosmosDB(
                databaseName: "Shared-Free",
                collectionName: "V1-pmdboard",
                ConnectionStringSetting = "CosmosDBConnection",
                Id = "internal-Pokémon-Red-Rescue-Team",
                PartitionKey = "internal-Pokémon-Red-Rescue-Team"
            )] V1GameMetadata runInfo,
            [CosmosDB(
                databaseName: "Shared-Free",
                collectionName: "V1-pmdboard",
                ConnectionStringSetting = "CosmosDBConnection"
            )] IAsyncCollector<V1Entry> entries,
            ILogger log)
        {
            log.LogInformation($"RRT Updater function started execution at: {DateTime.Now}");

            //Category format: xxxxxxxx-Category Name
            //Platform format: xxxxxxxx-Platform Name
            //Language format: xxxxxxxx-ENG/JPN
            foreach (var category in runInfo.Categories)
            {
                foreach (var platform in runInfo.Platforms)
                {
                    foreach (var language in runInfo.Languages)
                    {
                        var categoryInfo = category.Split('-');
                        var platformInfo = platform.Split('-');
                        var languageInfo = language.Split('-');

                        string url = $"https://speedrun.com/api/v1/leaderboards/{runInfo.GameID}/category/{categoryInfo[0]}?var-{runInfo.PlatformID}={platformInfo[0]}&var-{runInfo.LanguageID}={languageInfo[0]}&top=1&embed=players";
                        log.LogInformation(url);

                        var response = await httpClient.GetAsync(url);
                        var resStream = await response.Content.ReadAsStreamAsync();
                        Response result = await JsonSerializer.DeserializeAsync<Response>(resStream);

                        if (response.IsSuccessStatusCode)
                        {
                            if (result.ResponseBody.Players.PlayerList.Count > 0 && result.ResponseBody.RunList.Count > 0)
                            {
                                // Check whether the player is a guest or not
                                string playerName;
                                if (string.Equals(result.ResponseBody.Players.PlayerList[0].Role, "guest", StringComparison.InvariantCultureIgnoreCase))
                                {
                                    var jsonString = await response.Content.ReadAsStringAsync();
                                    playerName = Utils.GetGuestUser(jsonString);
                                }
                                else
                                {
                                    playerName = !string.IsNullOrEmpty(result.ResponseBody.Players.PlayerList[0].Names.Name) ?
                                    result.ResponseBody.Players.PlayerList[0].Names.Name :
                                    result.ResponseBody.Players.PlayerList[0].Names.NameJP;
                                }

                                var runTime = result.ResponseBody.RunList[0].Run.Times.PrimaryTime;
                                var runDate = result.ResponseBody.RunList[0].Run.RunDate;
                                var srcID = result.ResponseBody.RunList[0].Run.Id;

                                var row = new V1Entry
                                {
                                    id = $"run-{runInfo.GameID}-{categoryInfo[0]}-{platformInfo[0]}-{languageInfo[0]}",
                                    Game = "Pokémon Mystery Dungeon: Red Rescue Team",
                                    Category = categoryInfo[1],
                                    Platform = platformInfo[1],
                                    Language = languageInfo[1],
                                    Version = string.Empty,
                                    Runner = playerName,
                                    RunDate = runDate,
                                    RunTime = runTime,
                                    SRCLink = srcID
                                };
                                await entries.AddAsync(row);
                            }
                        }
                        else
                        {
                            log.LogError($"Request to {url} failed. Error code: {response.StatusCode}");
                        }
                    }
                }
            }

        }
    }
}


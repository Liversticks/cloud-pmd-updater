using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Threading.Tasks;
using System.Net.Http;
using System.Collections.Generic;
using CloudPMD.Shared;

namespace CloudPMD.Updater
{
    public static class UpdateWiiWare
    {
        public static HttpClient httpClient = new HttpClient();

        [FunctionName("UpdateWiiWare")]
        public static async Task Run([TimerTrigger("0 0 4 * * *")] TimerInfo myTimer,
            [CosmosDB(
                databaseName: "Shared-Free",
                collectionName: "V1-pmdboard",
                ConnectionStringSetting = "CosmosDBConnection",
                Id = "internal-Pokémon-Mystery-Dungeon-WiiWare",
                PartitionKey = "internal-Pokémon-Mystery-Dungeon-WiiWare"
            )] V1GameMetadata runInfo,
            [CosmosDB(
                databaseName: "Shared-Free",
                collectionName: "V1-pmdboard",
                ConnectionStringSetting = "CosmosDBConnection"
            )] IAsyncCollector<V1Entry> entries,
            ILogger log)
        {
            log.LogInformation($"WiiWare Updater function started execution at: {DateTime.Now}");

            // Get version info
            var versionMap = new Dictionary<string, string>();
            foreach (var version in runInfo.Versions)
            {
                var versionInfo = version.Split('-');
                versionMap.Add(versionInfo[0], versionInfo[1]);
            }

            //Category format: xxxxxxxx-Category Name
            //Language format: xxxxxxxx-ENG/JPN
            foreach (var category in runInfo.Categories)
            {
                foreach (var language in runInfo.Languages)
                {
                    var categoryInfo = category.Split('-');
                    var languageInfo = language.Split('-');

                    string url = $"https://speedrun.com/api/v1/leaderboards/{runInfo.GameID}/category/{categoryInfo[0]}?var-{runInfo.LanguageID}={languageInfo[0]}&top=1&embed=players";
                    log.LogInformation(url);

                    var response = await httpClient.GetAsync(url);
                    var jsonString = await response.Content.ReadAsStringAsync();
                    Response result = JsonSerializer.Deserialize<Response>(jsonString);

                    if (response.IsSuccessStatusCode)
                    {
                        if (result.ResponseBody.Players.PlayerList.Count > 0 && result.ResponseBody.RunList.Count > 0)
                        {
                            // Check whether the player is a guest or not
                            string playerName;
                            if (string.Equals(result.ResponseBody.Players.PlayerList[0].Role, "guest", StringComparison.InvariantCultureIgnoreCase))
                            {
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
                            var version = versionMap[Utils.GetVersion(jsonString, runInfo.VersionID)];
                            // Currently only supports Wii (modified) and emulators
                            var platform = result.ResponseBody.RunList[0].Run.System.IsEmulator ? "Emulator" : "Wii";

                            var row = new V1Entry
                            {
                                id = $"run-{runInfo.GameID}-{categoryInfo[0]}-{runInfo.PlatformID}-{languageInfo[0]}",
                                Game = "Pokémon Mystery Dungeon: WiiWare",
                                Category = categoryInfo[1],
                                Platform = platform,
                                Language = languageInfo[1],
                                Version = version,
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

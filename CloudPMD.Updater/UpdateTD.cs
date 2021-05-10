using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System.Text.Json;
using CloudPMD.Shared;
using System.Collections.Generic;

namespace CloudPMD.Updater
{
    public static class UpdateTD
    {
        /// <summary>
        /// Checks Time/Darkness leaderboard for updates of categories on the main board.
        /// </summary>
        /// <param name="myTimer"></param>
        /// <param name="runInfo"></param>
        /// <param name="entries"></param>
        /// <param name="log"></param>
        /// <remarks>This function uses its own game metadata type because each category
        /// on SRC has its own language variable.</remarks>
        /// <returns></returns>
        [FunctionName("UpdateTD")]
        public static async Task Run([TimerTrigger("0 2 0 * * *")] TimerInfo myTimer,
            [CosmosDB(
                databaseName: "Shared-Free",
                collectionName: "V1-pmdboard",
                ConnectionStringSetting = "CosmosDBConnection",
                Id = "internal-Pokémon-Explorers-of-Time-Darkness",
                PartitionKey = "internal-Pokémon-Explorers-of-Time-Darkness"
            )] V1TDMetadata runInfo,
            [CosmosDB(
                databaseName: "Shared-Free",
                collectionName: "V1-pmdboard",
                ConnectionStringSetting = "CosmosDBConnection"
            )] IAsyncCollector<V1CombinedRuns> entries,
            ILogger log)
        {
            log.LogInformation($"T/D Updater function started execution at: {DateTime.Now}");

            var gameInfo = new V1CombinedRuns
            {
                id = "gameinfo-TD",
                Title = "Pokémon Mystery Dungeon: Explorers of Time/Darkness",
                Categories = new List<Category>()
            };

            // Get version info
            var versionMap = new Dictionary<string, string>();
            foreach (var version in runInfo.Versions)
            {
                var versionInfo = version.Split('-');
                versionMap.Add(versionInfo[0], versionInfo[1]);
            }

            //Category format: xxxxxxxx-Category Name
            //Platform format: xxxxxxxx-Platform Name
            //Language format: xxxxxxxx-JPN/ENG            
            foreach (var category in runInfo.Categories)
            {
                var internalCategory = new Category
                {
                    Name = category.Name,
                    Runs = new List<InternalRun>()
                };
                
                foreach (var platform in runInfo.Platforms)
                {
                    var platformInfo = platform.Split('-');

                    foreach (var language in category.Languages)
                    {
                        var languageInfo = language.Split('-');
                        

                        string url = $"https://speedrun.com/api/v1/leaderboards/{runInfo.GameID}/category/{category.CategoryID}?var-{runInfo.PlatformID}={platformInfo[0]}&var-{category.LanguageID}={languageInfo[0]}&top=1&embed=players";
                        log.LogInformation(url);

                        var response = await FunctionHttpClient.httpClient.GetAsync(url);
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

                                var internalRow = new InternalRun
                                {
                                    Platform = platformInfo[1],
                                    Language = languageInfo[1],
                                    Version = version,
                                    Runner = playerName,
                                    RunDate = runDate,
                                    RunTime = runTime,
                                    SRCLink = srcID
                                };
                                internalCategory.Runs.Add(internalRow);
                            }
                        }
                        else
                        {
                            log.LogError($"Request to {url} failed. Error code: {response.StatusCode}");
                        }
                    }
                }
                gameInfo.Categories.Add(internalCategory);
            }
            await entries.AddAsync(gameInfo);
        }
    }
}


using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using CloudPMD.Shared;
using System.Collections.Generic;

namespace CloudPMD.Updater
{
    public static class UpdateRTDX
    {
        public static HttpClient httpClient = new HttpClient();

        [FunctionName("UpdateRTDX")]
        public static async Task Run([TimerTrigger("0 0 7 * * *")] TimerInfo myTimer,
            [CosmosDB(
                databaseName: "Shared-Free",
                collectionName: "V1-pmdboard",
                ConnectionStringSetting = "CosmosDBConnection",
                Id = "internal-Pokémon-Mystery-Dungeon-RTDX",
                PartitionKey = "internal-Pokémon-Mystery-Dungeon-RTDX"
            )] V1GameMetadata runInfo,
            [CosmosDB(
                databaseName: "Shared-Free",
                collectionName: "V1-pmdboard",
                ConnectionStringSetting = "CosmosDBConnection"
            )] IAsyncCollector<V1CombinedRuns> entries,
            ILogger log)
        {
            log.LogInformation($"RTDX Updater function started execution at: {DateTime.Now}");

            var gameInfo = new V1CombinedRuns
            {
                id = "gameinfo-RTDX",
                Title = "Pokémon Mystery Dungeon: Rescue Team DX",
                Categories = new List<Category>()
            };

            //Category format: xxxxxxxx-Category Name
            //Language format: xxxxxxxx-ENG/JPN
            foreach (var category in runInfo.Categories)
            {
                var categoryInfo = category.Split('-');
                var internalCategory = new Category
                {
                    Name = categoryInfo[1],
                    Runs = new List<InternalRun>()
                };

                foreach (var language in runInfo.Languages)
                {                    
                    var languageInfo = language.Split('-');

                    string url = $"https://speedrun.com/api/v1/leaderboards/{runInfo.GameID}/category/{categoryInfo[0]}?var-{runInfo.LanguageID}={languageInfo[0]}&top=1&embed=players";
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

                            var internalRow = new InternalRun
                            {
                                Platform = "Switch",
                                Language = languageInfo[1],
                                Version = string.Empty,
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
                gameInfo.Categories.Add(internalCategory);
            }
            await entries.AddAsync(gameInfo);
        }
    }
}

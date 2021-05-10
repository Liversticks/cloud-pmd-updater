using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using CloudPMD.Shared;
using System.Threading.Tasks;
using System.Text.Json;
using System.Collections.Generic;

namespace CloudPMD.Updater
{
    public static class UpdateSeries
    {        
        [FunctionName("UpdateSeries")]
        public static async Task Run([TimerTrigger("0 8 0 * * *")]TimerInfo myTimer,
            [CosmosDB(
                databaseName: "Shared-Free",
                collectionName: "V1-pmdboard",
                ConnectionStringSetting = "CosmosDBConnection",
                Id = "internal-Multiple-Mystery-Dungeon",
                PartitionKey = "internal-Multiple-Mystery-Dungeon"
            )] V1GameMetadata runInfo,
            [CosmosDB(
                databaseName: "Shared-Free",
                collectionName: "V1-pmdboard",
                ConnectionStringSetting = "CosmosDBConnection"
            )] IAsyncCollector<V1CombinedRuns> entries, ILogger log)
        {
            log.LogInformation($"PMD Series updater function started execution at: {DateTime.Now}");

            var gameInfo = new V1CombinedRuns
            {
                id = "gameinfo-PMD-Series",
                Title = "Multiple Mystery Dungeon Games",
                Categories = new List<Category>()
            };

            foreach (var category in runInfo.Categories)
            {
                var categoryInfo = category.Split('-');
                var internalCategory = new Category
                {
                    Name = categoryInfo[1],
                    Runs = new List<InternalRun>()
                };

                string url = $"https://speedrun.com/api/v1/leaderboards/{runInfo.GameID}/category/{categoryInfo[0]}?top=1&embed=players";
                var response = await FunctionHttpClient.httpClient.GetAsync(url);
                var resStream = await response.Content.ReadAsStreamAsync();
                Response result = await JsonSerializer.DeserializeAsync<Response>(resStream);

                if (response.IsSuccessStatusCode)
                {
                    if (result.ResponseBody.Players.PlayerList.Count > 0 && result.ResponseBody.RunList.Count > 0)
                    {
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
                            Language = "ENG",
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
                gameInfo.Categories.Add(internalCategory);
            }
            await entries.AddAsync(gameInfo);
        }
    }
}

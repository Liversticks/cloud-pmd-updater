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
    public static class UpdateSky
    {
        [FunctionName("UpdateSky")]
        public static async Task Run([TimerTrigger("0 3 0 * * *")] TimerInfo myTimer,
            [CosmosDB(
                databaseName: "Shared-Free",
                collectionName: "V1-pmdboard",
                ConnectionStringSetting = "CosmosDBConnection",
                Id = "internal-Pokémon-Explorers-of-Sky",
                PartitionKey = "internal-Pokémon-Explorers-of-Sky"
            )] V1SkyMetadata runInfo,
            [CosmosDB(
                databaseName: "Shared-Free",
                collectionName: "V1-pmdboard",
                ConnectionStringSetting = "CosmosDBConnection"
            )] IAsyncCollector<V1CombinedRuns> entries, ILogger log)
        {
            log.LogInformation($"Sky Updater function started execution at: {DateTime.Now}");

            var gameInfo = new V1CombinedRuns
            {
                id = "gameinfo-Sky",
                Title = "Pokémon Mystery Dungeon: Explorers of Sky",
                Categories = new List<Category>()
            };

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
                    if (category.Name.StartsWith("All Icons"))
                    {
                        // All Icons treated differently for now - only ENG and has its own WM/No WM SRC variable
                        string url = $"https://speedrun.com/api/v1/leaderboards/{runInfo.GameID}/category/{category.CategoryID}?var-{runInfo.PlatformID}={platformInfo[0]}&var-{category.WMKey}={category.WMValue}&top=1&embed=players";
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


                                var internalRow = new InternalRun
                                {
                                    Platform = platformInfo[1],
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
                    }
                    else
                    {
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

                                    var internalRow = new InternalRun
                                    {
                                        Platform = platformInfo[1],
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
                    }
                }
                gameInfo.Categories.Add(internalCategory);
            }
            await entries.AddAsync(gameInfo);
        }
    }
}


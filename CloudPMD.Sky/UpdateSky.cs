using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System.Text.Json;
using System.Net.Http;
using CloudPMD.Shared;
using System.Collections.Generic;

namespace CloudPMD.Sky
{
    public static class UpdateSky
    {
        public class V1SkyMetadata
        {
            public string GameID { get; set; }
            public IList<SkyCategory> Categories { get; set; }
            public string PlatformID { get; set; }
            public string[] Platforms { get; set; }
        }
           

        public class SkyCategory
        {
            public string CategoryID { get; set; }
            public string Name { get; set; }
            public string LanguageID { get; set; }
            public string[] Languages { get; set; }
            
            // These two properties are only used for All Icons
            public string WMKey { get; set; }
            public string WMValue { get; set; }
        }

        public static HttpClient httpClient = new HttpClient();
        
        [FunctionName("UpdateSky")]
        public static async Task RunAsync([TimerTrigger("0 0 3 * * *")]TimerInfo myTimer,
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
            )] IAsyncCollector<V1Entry> entries, ILogger log)
        {
            log.LogInformation($"Sky Updater function started execution at: {DateTime.Now}");

            //Category format: xxxxxxxx-Category Name
            //Platform format: xxxxxxxx-Platform Name
            //Language format: xxxxxxxx-JPN/ENG 
            foreach (var category in runInfo.Categories)
            {
                foreach (var platform in runInfo.Platforms)
                {
                    var platformInfo = platform.Split('-');
                    if (category.Name.StartsWith("All Icons"))
                    {
                        // All Icons treated differently for now - only ENG and has its own WM/No WM SRC variable
                        string url = $"https://speedrun.com/api/v1/leaderboards/{runInfo.GameID}/category/{category.CategoryID}?var-{runInfo.PlatformID}={platformInfo[0]}&var-{category.WMKey}={category.WMValue}&top=1&embed=players";
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

                                var row = new V1Entry
                                {
                                    id = $"run-{runInfo.GameID}-{category.CategoryID}-{platformInfo[0]}-{category.WMKey}",
                                    Game = "Pokémon Mystery Dungeon: Explorers of Sky",
                                    Category = category.Name,
                                    Platform = platformInfo[1],
                                    Language = "ENG",
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
                    else
                    {
                        foreach (var language in category.Languages)
                        {
                            var languageInfo = language.Split('-');                            

                            string url = $"https://speedrun.com/api/v1/leaderboards/{runInfo.GameID}/category/{category.CategoryID}?var-{runInfo.PlatformID}={platformInfo[0]}&var-{category.LanguageID}={languageInfo[0]}&top=1&embed=players";
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

                                    var row = new V1Entry
                                    {
                                        id = $"run-{runInfo.GameID}-{category.CategoryID}-{platformInfo[0]}-{languageInfo[0]}",
                                        Game = "Pokémon Mystery Dungeon: Explorers of Sky",
                                        Category = category.Name,
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
}

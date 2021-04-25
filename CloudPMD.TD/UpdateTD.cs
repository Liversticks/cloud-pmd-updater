using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System.Text.Json;
using CloudPMD.Shared;
using System.Net.Http;
using System.Collections.Generic;

namespace CloudPMD.TD
{
    public static class UpdateTD
    {
        public class V1TDMetadata
        {
            public string GameID { get; set; }
            public IList<TDCategory> Categories { get; set; }
            public string PlatformID { get; set; }
            public string[] Platforms { get; set; }
            public string VersionID { get; set; }
            public string[] Versions { get; set; }
        }

        public class TDCategory
        {
            public string CategoryID { get; set; }
            public string Name { get; set; }
            public string LanguageID { get; set; }
            public string[] Languages { get; set; }
        }
        
        public static HttpClient httpClient = new HttpClient();
        
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
        public static async Task RunAsync([TimerTrigger("0 * * * * *")]TimerInfo myTimer,
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
            )] IAsyncCollector<V1Entry> entries, 
            ILogger log)
        {
            log.LogInformation($"T/D Updater function started execution at: {DateTime.Now}");

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
                foreach (var platform in runInfo.Platforms)
                {
                    foreach (var language in category.Languages)
                    {
                        var languageInfo = language.Split('-');
                        var platformInfo = platform.Split('-');

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
                                var version = versionMap[Utils.GetVersion(jsonString, runInfo.VersionID)];

                                var row = new V1Entry
                                {
                                    id = $"run-{runInfo.GameID}-{category.CategoryID}-{platformInfo[0]}-{languageInfo[0]}",
                                    Game = "Pokémon Mystery Dungeon: Explorers of Time/Darkness",
                                    Category = category.Name,
                                    Platform = platformInfo[1],
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
}

using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using CloudPMD.Shared;
using System.Text.Json;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents;

namespace CloudPMD.Utilities
{
    public static class SeedDatabase
    {
        private static List<V1BaseMetadata> Seed = new List<V1BaseMetadata>
        {
            new V1GameMetadata {
                id = "internal-Pokémon-Blue-Rescue-Team",
                GameID = "k6qwpm6g",
                Categories = new string[]
                {
                    "mkerqxd6-Any% No QS, No WM",
                    "jdz7646k-Any% No WM",
                    "w203goj2-Any% Unrestricted",
                    "vdo413o2-All Icons No QS, No WM",
                    "vdo18l62-All Icons No WM",
                    "wdm6xjek-All Icons Unrestricted",
                    "n2y39ezd-Recruit 'em All No QS, No WM",
                    "wkp31xj2-Recruit 'em All No WM",
                    "7dg6njgk-Recruit 'em All Unrestricted"
                },
                PlatformID = "6nj40pn4",
                Platforms = new string[]
                {
                    "z19gw4l4-Wii U Virtual Console",
                    "21ddpv51-GBA/DS/3DS",
                    "814enkld-Emulator"
                },
                LanguageID = "38dzek0n",
                Languages = new string[]
                {
                    "jqz7xw2l-ENG",
                    "klrwydo1-JPN"
                }
            },
            new V1GameMetadata
            {
                id = "internal-Pokémon-Red-Rescue-Team",
                GameID = "k6qwpm6g",
                Categories = new string[]
                {
                    "mke496j2-Any% No QS, No WM",
                    "5dwg4w5k-Any% No WM",
                    "wk68nlo2-Any% Unrestricted"
                },
                PlatformID = "6nj40pn4",
                Platforms = new string[]
                {
                    "z19gw4l4-Wii U Virtual Console",
                    "21ddpv51-GBA/DS/3DS",
                    "814enkld-Emulator"
                },
                LanguageID = "38dzek0n",
                Languages = new string[]
                {
                    "jqz7xw2l-ENG",
                    "klrwydo1-JPN"
                }
            },
            new V1TDMetadata
            {
                id = "internal-Pokémon-Explorers-of-Time-Darkness",
                GameID = "6r34v68",
                Categories = new TDCategory[]
                {
                    new TDCategory 
                    { 
                        CategoryID = "zd38omrk",
                        Name = "Any% No WM",
                        LanguageID = "789j199n",
                        Languages = new string[]
                        {
                            "21g484n1-ENG",
                            "jqz5p5gq-JPN"
                        }
                    },
                    new TDCategory
                    {
                        CategoryID = "xd17grd8",
                        Name = "Any% WM",
                        LanguageID = "onv6xr08",
                        Languages = new string[]
                        {
                            "9qjzrpo1-ENG",
                            "jq64m8o1-JPN"
                        }
                    },
                    new TDCategory
                    {
                        CategoryID = "wk68v4p2",
                        Name = "Beat Darkrai No WM",
                        LanguageID = "2lgr1m6n",
                        Languages = new string[]
                        {
                            "5leg8k61-ENG",
                            "0q5e8ovq-JPN"
                        }
                    },
                    new TDCategory
                    {
                        CategoryID = "7kj13oxk",
                        Name = "Beat Darkrai WM",
                        LanguageID = "38djw5e8",
                        Languages = new string[]
                        {
                            "5lm4w98l-ENG",
                            "81wdrkmq-JPN"
                        }
                    },
                    new TDCategory
                    {
                        CategoryID = "zd3xwqrd",
                        Name = "Recruit 'Em All Minimum WM",
                        LanguageID = "j84evvjn",
                        Languages = new string[]
                        {
                            "5legmkp1-ENG",
                            "0q5egorq-JPN"
                        }
                    },
                    new TDCategory
                    {
                        CategoryID = "z27lrwgd",
                        Name = "Recruit 'Em All Unlimited WM",
                        LanguageID = "r8roqwr8",
                        Languages = new string[]
                        {
                            "zqo3275q-ENG",
                            "013m4yrl-JPN"
                        }
                    }
                },
                PlatformID = "rn10ggk8",
                Platforms = new string[]
                {
                    "4lxwexj1-DS/3DS",
                    "814e7xwl-Emulator"
                },
                VersionID = "kn0qmo83",
                Versions = new string[]
                {
                    "gq75dpn1-Time",
                    "21gnd5xl-Darkness"
                }
            },
            new V1SkyMetadata
            {
                id = "internal-Pokémon-Explorers-of-Sky",
                GameID = "1lqnj6g",
                Categories = new SkyCategory[]
                {
                    new SkyCategory
                    {
                        CategoryID = "zd37wy2n",
                        Name = "Any% WM",
                        LanguageID = "ylpvm5kl",
                        Languages = new string[]
                        {
                            "p12opo4l-ENG",
                            "z195k58q-JPN"
                        }
                    },
                    new SkyCategory
                    {
                        CategoryID = "9kv76g3k",
                        Name = "Any% No WM",
                        LanguageID = "2lgg5d7l",
                        Languages = new string[]
                        {
                            "klr0p0jl-ENG",
                            "21dyzy41-JPN"
                        }
                    },
                    new SkyCategory
                    {
                        CategoryID = "xk9jw94d",
                        Name = "Beat Darkrai WM",
                        LanguageID = "9l73mqpn",
                        Languages = new string[]
                        {
                            "013vj73l-ENG",
                            "zqoygo21-JPN"
                        }
                    },
                    new SkyCategory
                    {
                        CategoryID = "xd1g35zd",
                        Name = "Beat Darkrai No WM",
                        LanguageID = "wlexmo48",
                        Languages = new string[]
                        {
                            "81090oj1-ENG",
                            "9qjew601-JPN"
                        }
                    },
                    new SkyCategory
                    {
                        CategoryID = "zdn1ennk",
                        Name = "Recruit 'Em All WM",
                        LanguageID = "0nwk11kn",
                        Languages = new string[]
                        {
                            "z19rezy1-ENG",
                            "p12zj2d1-JPN"
                        }
                    },
                    new SkyCategory
                    {
                        CategoryID = "8240g8nd",
                        Name = "Recruit 'Em All No WM",
                        LanguageID = "0nwoggkl",
                        Languages = new string[]
                        {
                            "0q5egxrq-ENG",
                            "4lxwe0j1-JPN"
                        }
                    },
                    new SkyCategory
                    {
                        CategoryID = "9d8p36qk",
                        Name = "All Special Episodes",
                        LanguageID = "p854pp0l",
                        Languages = new string[]
                        {
                            "z19g8zjl-ENG",
                            "p12ex2v1-JPN"
                        }
                    },
                    new SkyCategory
                    {
                        CategoryID = "5dw8y35d",
                        Name = "All Icons WM",
                        WMKey = "jlzkyg0l",
                        WMValue = "5lmdnvml"
                    },
                    new SkyCategory
                    {
                        CategoryID = "5dw8y35d",
                        Name = "All Icons No WM",
                        WMKey = "jlzkyg0l",
                        WMValue = "81wn8gv1"
                    }
                },
                PlatformID = "78963368",
                Platforms = new string[]
                {
                    "814e7kwl-Wii U Virtual Console",
                    "z19g84jl-DS/3DS",
                    "p12ex5v1-Emulator"
                }
            },
            new V1GameMetadata
            {
                id = "internal-Pokémon-Mystery-Dungeon-WiiWare",
                GameID = "9d37e06l",
                Categories = new string[]
                {
                    "z27w45k0-Any%",
                    "9kv9yy82-Any% (Password)"
                },
                LanguageID = "6nj4kren",
                Languages = new string[]
                {
                    "810e072q-ENG",
                    "jqz7vwgl-JPN"
                },
                Platforms = new string[]
                {
                    "v06dk3e4-Wii"
                },
                VersionID = "6njvren4",
                Versions = new string[]
                {
                    "mlnemnlp-Forward! Adventurers of Flame",
                    "5q8486ld-Let's Go! Adventurers of Storm",
                    "4qy7odq7-Aspire! Adventurers of Light",
                    "8106jp1v-All Versions"
                }
            },
            new V1GameMetadata
            {
                id = "internal-Pokémon-Mystery-Dungeon-Gates",
                GameID = "v1pxlz68",
                Categories = new string[]
                {
                    "jdzv7gkv-Any% WM",
                    "rklv388d-Any% No WM",
                    "jdre870k-Recruit 'em All WM",
                    "q25mg1gd-Recruit 'em All No WM"
                },
                LanguageID = "jlz590yl",
                Languages = new string[]
                {
                    "4qy8zr71-ENG",
                    "mlnk8ndl-JPN"
                },
                PlatformID = "onvj99rn",
                Platforms = new string[]
                {
                    "zqowrmgl-3DS/New 3DS",
                    "013e5wkq-NTR/Emulator"
                }
            },
            new V1GameMetadata
            {
                id = "internal-Pokémon-Super-Mystery-Dungeon",
                GameID = "76ro4468",
                Categories = new string[]
                {
                    "zd30omvd-Any% WM",
                    "ndx75wv2-Any% No WM",
                    "xd17jgrd-100% WM",
                    "zdn80yxd-100% No WM"
                },
                LanguageID = "0nw2rp5n",
                Languages = new string[]
                {
                    "klr5gw21-ENG",
                    "21d20gjl-JPN"
                },
                PlatformID = "ql67wrx8",
                Platforms = new string[]
                {
                    "9qjyxw0q-New 3DS/NTR",
                    "jq6kzynl-3DS",
                    "5lmjg8yl-Emulator"
                }
            },
            new V1GameMetadata
            {
                id = "internal-Pokémon-Mystery-Dungeon-RTDX",
                GameID = "nd2e0rrd",
                Categories = new string[]
                {
                    "ndx9ll5d-Any% WM",
                    "jdz90432-Any% No WM",
                    "xk9r14yk-All Icons WM",
                    "xk9vrxgd-All Icons No WM"
                },
                LanguageID = "9l716y9l",
                Languages = new string[]
                {
                    "zqod85x1-ENG",
                    "0133o2x1-JPN"
                },
                PlatformID = "7m6ylw9p"
            }
        };

        private static Lazy<DocumentClient> lazyClient = new Lazy<DocumentClient>(InitializeDocumentClient);
        private static DocumentClient documentClient = lazyClient.Value;

        private static DocumentClient InitializeDocumentClient()
        {
            var uri = new Uri(Environment.GetEnvironmentVariable("CosmosDBURI"));
            var authKey = Environment.GetEnvironmentVariable("CosmosDBKey");
            return new DocumentClient(uri, authKey);
        }
        
        [FunctionName("SeedDatabase")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Admin, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("Creating Database and Container if non-existent.");

            await documentClient.CreateDatabaseIfNotExistsAsync(new Database { Id = "Shared-Free" },
                new RequestOptions
                {
                    OfferThroughput = 400
                });
            Uri dbUri = UriFactory.CreateDatabaseUri("Shared-Free");
            await documentClient.CreateDocumentCollectionIfNotExistsAsync(dbUri, new DocumentCollection
            {
                Id = "V1-pmdboard",
                PartitionKey = new PartitionKeyDefinition
                {
                    Paths = new System.Collections.ObjectModel.Collection<string> { "/id" }
                }
            });

            log.LogInformation("Creating base game metadata objects.");
            Uri collectionUri = UriFactory.CreateDocumentCollectionUri("Shared-Free", "V1-pmdboard");
            foreach (var item in Seed)
            {
                try
                {
                    await documentClient.CreateDocumentAsync(collectionUri, item);
                }
                catch
                {
                    continue;
                }
            }           
            return new OkObjectResult("Created and seeded base game metadata objects.");
        }
    }
}

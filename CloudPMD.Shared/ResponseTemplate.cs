using System.Text.Json.Serialization;
using System.Collections.Generic;

namespace CloudPMD.Shared
{
    /// <summary>
    /// src API response JSON.
    /// </summary>
    public class Response
    {
        [JsonPropertyName("data")]
        public ResponseBody ResponseBody { get; set; }
    }

    public class ResponseBody
    {
        [JsonPropertyName("runs")]
        public IList<RunBox> RunList { get; set; }

        [JsonPropertyName("players")]
        public PlayerBox Players { get; set; }
    }

    public class RunBox
    {
        [JsonPropertyName("run")]
        public Run Run { get; set; }
    }

    public class Run
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("date")]
        public string RunDate { get; set; }

        [JsonPropertyName("times")]
        public TimeBox Times { get; set; }

        [JsonPropertyName("system")]
        public PlatformBox System { get; set; }

    }

    public class TimeBox
    {
        [JsonPropertyName("primary_t")]
        public int PrimaryTime { get; set; }
    }

    public class PlatformBox
    {
        [JsonPropertyName("platform")]
        public string Platform { get; set; }
        
        [JsonPropertyName("emulated")]
        public bool IsEmulator { get; set; }
    }

    public class PlayerBox
    {
        [JsonPropertyName("data")]
        public IList<PlayerInfo> PlayerList { get; set; }
    }

    public class PlayerInfo
    {
        [JsonPropertyName("rel")]
        public string Role { get; set; }

        [JsonPropertyName("id")]
        public string PlayerID { get; set; }

        [JsonPropertyName("names")]
        public NameBox Names { get; set; }
    }

    public class NameBox
    {
        [JsonPropertyName("international")]
        public string Name { get; set; }

        [JsonPropertyName("japanese")]
        public string NameJP { get; set; }
    }
}

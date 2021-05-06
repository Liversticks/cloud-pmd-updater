namespace CloudPMD.Shared
{
    public class V1GameMetadata
    {
        public string id { get; set; }
        public string GameID { get; set; }
        public string[] Categories { get; set; }
        public string PlatformID { get; set; }
        public string[] Platforms { get; set; }
        public string LanguageID { get; set; }
        public string[] Languages { get; set; }
        public string VersionID { get; set; }
        public string[] Versions { get; set; }
    }
}

using System.Collections.Generic;

namespace CloudPMD.Shared
{
    public class V1BaseMetadata
    {
        public string id { get; set; }
        public string GameID { get; set; }
        public string PlatformID { get; set; }
        public string[] Platforms { get; set; }
        public string VersionID { get; set; }
        public string[] Versions { get; set; }
    }
    
    public class V1GameMetadata : V1BaseMetadata
    {        
        public string[] Categories { get; set; }        
        public string LanguageID { get; set; }
        public string[] Languages { get; set; }
        
    }

    public class V1SkyMetadata : V1BaseMetadata
    {
        public IList<SkyCategory> Categories { get; set; }
    }

    public class SkyCategory : TDCategory
    {
        // These two properties are only used for All Icons
        public string WMKey { get; set; }
        public string WMValue { get; set; }
    }

    public class V1TDMetadata : V1BaseMetadata
    {
        public IList<TDCategory> Categories { get; set; }
    }

    public class TDCategory
    {
        public string CategoryID { get; set; }
        public string Name { get; set; }
        public string LanguageID { get; set; }
        public string[] Languages { get; set; }
    }
}

namespace CloudPMD.Shared
{
    /// <summary>
    /// Contains all base information needed for individual runs.
    /// </summary>
    /// <remarks>Deprecated. Use V1CombinedRuns instead.</remarks>
    public class V1Entry
    {
        public string id { get; set; }

        public string Game { get; set; }
    
        public string Category { get; set; }

        public string Platform { get; set; }

        public string Language { get; set; }

        public string Version { get; set; }

        public string Runner { get; set; }

        public string RunDate { get; set; }
        
        public int RunTime { get; set; }

        public string SRCLink { get; set; }
    }

}

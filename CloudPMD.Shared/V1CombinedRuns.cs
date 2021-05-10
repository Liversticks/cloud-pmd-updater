using System.Collections.Generic;

namespace CloudPMD.Shared
{
    public class V1CombinedRuns
    {
        public string id { get; set; }
        public string Title { get; set; }
        public IList<Category> Categories { get; set; }        
    }

    public class Category
    {
        public string Name { get; set; }
        public IList<InternalRun> Runs { get; set; }
    }

    public class InternalRun
    {
        public string Platform { get; set; }
        public string Language { get; set; }
        public string Version { get; set; }
        public string Runner { get; set; }
        public string RunDate { get; set; }
        public int RunTime { get; set; }
        public string SRCLink { get; set; }
    }

}

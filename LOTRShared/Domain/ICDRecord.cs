using Marten.Schema;

namespace LOTRShared.Domain
{
    public class ICDRecord
    {

        public string Id { get; set; }
        
        [FullTextIndex]
        public string Code { get; set; }
        
        [FullTextIndex]
        public string Description { get; set; }
    }

}

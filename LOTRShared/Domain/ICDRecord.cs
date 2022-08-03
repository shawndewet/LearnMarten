namespace LOTRShared.Domain
{
    public class ICDRecord
    {

        public string Id { get; set; }
        
        public string Code { get; set; }
        
        //[FullTextIndex]
        public string Description { get; set; }

        public override string ToString()
        {
            return $"{Code} {Description}";
        }
    }

}

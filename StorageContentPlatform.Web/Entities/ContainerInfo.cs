namespace StorageContentPlatform.Web.Entities
{
    public class ContainerInfo
    {
        public string Name { get; set; }
        public DateTimeOffset LastModified { get; set; }
        public string Metadata { get; set; }
    }
}

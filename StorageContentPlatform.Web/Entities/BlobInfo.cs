namespace StorageContentPlatform.Web.Entities
{
    public class BlobInfo
    {
        public string Name { get; internal set; }
        public DateTimeOffset? LastModified { get; internal set; }
        public string? ReplicationPolicyId { get; internal set; }
        public string? ReplicationRuleId { get; internal set; }
        public string? ReplicationStatus { get; internal set; }
        public long? Size { get; internal set; }
        public string Tier { get; internal set; }
    }
}

﻿namespace StorageContentPlatform.Web.Entities
{
    public class BlobContent
    {
        public string? Name { get; set; }
        public string? Content { get; set; }
        public IDictionary<string,string>? Metadata { get; set; }
    }
}

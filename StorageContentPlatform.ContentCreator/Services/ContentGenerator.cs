using Microsoft.Extensions.Configuration;
using StorageContentPlatform.ContentCreator.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Unicode;
using System.Threading.Tasks;

namespace StorageContentPlatform.ContentCreator.Services
{
    public class ContentGenerator : IContentGenerator
    {

        private class Configuration
        {
            public int BlobMinimumSizeInKb { get; set; }
            public int ContentsSizeForGenerationInKb { get; set; }
        }

        private readonly IPersistanceProvider persistanceProvider;
        private readonly IConfiguration configuration;
        private readonly Configuration configurationValues;
        
        public ContentGenerator(IPersistanceProvider persistanceProvider,
            IConfiguration configuration)
        {
            this.persistanceProvider = persistanceProvider;
            this.configuration = configuration;
            configurationValues = new Configuration();
        }

        public async Task<bool> GenerateContentsAsync()
        {
            var result = true;
            LoadConfig();
            
            var strBuild = new StringBuilder();
            var cumulativeSize = 0;
            while (cumulativeSize/1024 < this.configurationValues.ContentsSizeForGenerationInKb )
            {
                var size = 0;
                while (size/1024 < this.configurationValues.BlobMinimumSizeInKb)
                {
                    var sentence = Faker.Lorem.Sentence(Faker.RandomNumber.Next(1, 20));
                    size += System.Text.ASCIIEncoding.UTF8.GetByteCount(sentence);
                    strBuild.AppendLine(sentence);
                }

                var contentName = @$"{DateTimeOffset.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid()}.txt";
                
                await persistanceProvider.SaveContentAsync(contentName,strBuild.ToString());
                
                strBuild.Clear();
                cumulativeSize += size;
            }
            
            return result;
        }

        private void LoadConfig()
        {
            this.configurationValues.BlobMinimumSizeInKb=this.configuration.GetValue<int>("BlobMinimumSizeInKb");
            this.configurationValues.ContentsSizeForGenerationInKb = this.configuration.GetValue<int>("ContentsSizeForGenerationInKb");
        }
    }
}

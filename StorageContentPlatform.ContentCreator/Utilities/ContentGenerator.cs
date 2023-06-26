using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Unicode;
using System.Threading.Tasks;

namespace StorageContentPlatform.ContentCreator.Utilities
{
    public static class ContentGenerator
    {
        public static string GenerateRandomContent(int minimumSizeInKb, int maximumSizeInKb, out double actualSize)
        {
            var strBuild = new StringBuilder();
            var size = 0;
            var randomSize = Faker.RandomNumber.Next(minimumSizeInKb, maximumSizeInKb);

            while (size / 1024.0 < randomSize)
            {
                var sentence = Faker.Lorem.Sentence(Faker.RandomNumber.Next(1, 1000));
                size += System.Text.ASCIIEncoding.UTF8.GetByteCount(sentence);
                strBuild.AppendLine(sentence);
            }

            actualSize = size / 1024.0;

            return strBuild.ToString();
        }
    }
}

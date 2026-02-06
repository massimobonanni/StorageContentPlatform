using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Unicode;
using System.Threading.Tasks;

namespace StorageContentPlatform.ContentCreator.Utilities
{
    /// <summary>
    /// Provides utility methods for generating random text content.
    /// </summary>
    public static class ContentGenerator
    {
        /// <summary>
        /// Generates random Lorem Ipsum text content within a specified size range.
        /// </summary>
        /// <param name="minimumSizeInKb">The minimum size of the generated content in kilobytes.</param>
        /// <param name="maximumSizeInKb">The maximum size of the generated content in kilobytes.</param>
        /// <param name="actualSize">When this method returns, contains the actual size of the generated content in kilobytes.</param>
        /// <returns>A string containing randomly generated Lorem Ipsum sentences that meet the size requirements.</returns>
        /// <remarks>
        /// This method generates random sentences using the Faker library until the accumulated content
        /// reaches a size between the specified minimum and maximum values. Each sentence contains
        /// between 1 and 1000 words. The size is calculated based on UTF8 byte encoding.
        /// </remarks>
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

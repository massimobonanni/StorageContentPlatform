using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Collections.Generic;

/// <summary>
/// Provides extension methods for <see cref="IDictionary{TKey, TValue}"/> collections.
/// </summary>
internal static class DictionaryExtensions
{
    /// <summary>
    /// Converts a dictionary of string key-value pairs into a concatenated string representation.
    /// Each key-value pair is formatted as "key=value" and pairs are separated by semicolons.
    /// </summary>
    /// <param name="dictionary">The dictionary to convert. Can be null or empty.</param>
    /// <returns>
    /// A string containing all key-value pairs in the format "key1=value1;key2=value2".
    /// Returns <see cref="string.Empty"/> if the dictionary is null or contains no elements.
    /// </returns>
    /// <example>
    /// <code>
    /// var dict = new Dictionary&lt;string, string&gt; { { "name", "John" }, { "age", "30" } };
    /// string result = dict.ToConcatenatedString();
    /// // result: "name=John;age=30"
    /// </code>
    /// </example>
    public static string ToConcatenatedString(this IDictionary<string, string> dictionary)
    {
        if (dictionary == null || !dictionary.Any())
            return string.Empty;
        return string.Join(";", dictionary.Select(kv => $"{kv.Key}={kv.Value}"));
    }
}

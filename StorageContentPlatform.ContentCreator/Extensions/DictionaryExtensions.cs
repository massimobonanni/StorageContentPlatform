using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Collections.Generic;

internal static class DictionaryExtensions
{
    // write an extension method for IDictionary<string,string> the write a string concatenating the keys and the values in the format key=value;key=value
    public static string ToConcatenatedString(this IDictionary<string, string> dictionary)
    {
        if (dictionary == null || !dictionary.Any())
            return string.Empty;
        return string.Join(";", dictionary.Select(kv => $"{kv.Key}={kv.Value}"));
    }
}

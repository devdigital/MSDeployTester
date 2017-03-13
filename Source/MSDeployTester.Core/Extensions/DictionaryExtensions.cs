namespace MSDeployTester.Core.Extensions
{
    using System.Collections.Generic;
    using System.Linq;

    public static class DictionaryExtensions
    {
        // Adapted from http://stackoverflow.com/a/2679857/248164
        public static Dictionary<TK, TV> MergeLeft<TK, TV>(
            this Dictionary<TK, TV> value, 
            params IDictionary<TK, TV>[] others)
        {
            var result = new Dictionary<TK, TV>(value, value.Comparer);
            foreach (var dictionary in new List<IDictionary<TK, TV>> { value }.Concat(others))
            {
                foreach (var kvp in dictionary)
                {
                    result[kvp.Key] = kvp.Value;
                }
            }

            return result;
        }
    }
}
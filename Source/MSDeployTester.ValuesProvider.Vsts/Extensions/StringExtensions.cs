namespace MSDeployTester.ValuesProvider.Vsts.Extensions
{
    using Newtonsoft.Json;

    internal static class StringExtensions
    {
        public static TData ToObject<TData>(this string value)
        {
            return JsonConvert.DeserializeObject<TData>(value);
        }
    }
}
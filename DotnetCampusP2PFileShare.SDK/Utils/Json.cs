using Newtonsoft.Json;

namespace DotnetCampusP2PFileShare.SDK.Utils
{
    internal static class Json
    {
        public static T Parse<T>(string str)
        {
            return JsonConvert.DeserializeObject<T>(str);
        }
    }
}
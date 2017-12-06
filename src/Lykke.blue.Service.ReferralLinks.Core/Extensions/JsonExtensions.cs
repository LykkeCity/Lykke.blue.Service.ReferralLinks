using Newtonsoft.Json;

namespace Lykke.blue.Service.ReferralLinks.Core.Extensions
{
    public static class JsonExtensions
    {
        public static string ToJson(this object src, bool ignoreNulls = false)
        {
            return JsonConvert.SerializeObject(src, new JsonSerializerSettings { NullValueHandling = ignoreNulls ? NullValueHandling.Ignore : NullValueHandling.Include });
        }
    }
}

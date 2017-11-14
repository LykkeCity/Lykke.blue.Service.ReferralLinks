using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Lykke.Blue.Service.ReferralLinks.Core.Extensions
{
    public static class JsonExtensions
    {
        public static T DeserializeJson<T>(this string json, Func<T> createDefault)
        {
            try
            {
                return JsonConvert.DeserializeObject<T>(json);
            }
            catch (Exception)
            {
                return createDefault();
            }
        }

        public static object DeserializeJson(this string json, Type type)
        {
            return JsonConvert.DeserializeObject(json, type);
        }

        public static T DeserializeJson<T>(this string json)
        {
            return JsonConvert.DeserializeObject<T>(json);
        }

        public static string ToJson(this object src, bool ignoreNulls = false)
        {
            return JsonConvert.SerializeObject(src, new JsonSerializerSettings { NullValueHandling = ignoreNulls ? NullValueHandling.Ignore : NullValueHandling.Include });
        }
    }
}

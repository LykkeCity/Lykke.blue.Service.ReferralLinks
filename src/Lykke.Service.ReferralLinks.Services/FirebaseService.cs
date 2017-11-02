using System.Threading.Tasks;
using Lykke.Service.ReferralLinks.Core.Services;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Dynamic;
using Newtonsoft.Json;
using System.Text;
using Newtonsoft.Json.Linq;
using System;
using Lykke.Service.ReferralLinks.Core.Settings.ServiceSettings;

namespace Lykke.Service.ReferralLinks.Services
{
    public class FirebaseService : IFirebaseService
    {
        private readonly ReferralLinksSettings _settings;

        public FirebaseService(ReferralLinksSettings settings)
        {
            _settings = settings;
        }

        public async Task<string> GenerateUrl(string id)
        {
            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Accept.Clear();
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                dynamic expando = new ExpandoObject();

                expando.dynamicLinkInfo = new ExpandoObject();
                expando.dynamicLinkInfo.dynamicLinkDomain = _settings.DynamicLinks.DynamicLinkDomain;
                expando.dynamicLinkInfo.link = $"{_settings.DynamicLinks.Link}{id}";

                expando.dynamicLinkInfo.androidInfo = new ExpandoObject();
                expando.dynamicLinkInfo.androidInfo.androidPackageName = _settings.DynamicLinks.AndroidInfo.AndroidPackageName;

                expando.dynamicLinkInfo.iosInfo = new ExpandoObject();
                expando.dynamicLinkInfo.iosInfo.iosBundleId = _settings.DynamicLinks.IosInfo.IosBundleId;

                expando.suffix = new ExpandoObject();
                expando.suffix.option = "UNGUESSABLE";

                var jsonContent = JsonConvert.SerializeObject(expando);

                using (var stringContent = new StringContent(jsonContent, Encoding.UTF8, "application/json"))
                {
                    var response = await httpClient.PostAsync(
                        _settings.DynamicLinks.ApiUrl,
                        stringContent);

                    var result = response.Content.ReadAsStringAsync().Result;

                    dynamic dyn = JObject.Parse(result);

                    if (dyn.error != null)
                    {
                        throw new Exception($"Failed to create short link. Details: {dyn.error.message}");
                    }

                    return dyn.shortLink;
                }
            }
        }
    }
}

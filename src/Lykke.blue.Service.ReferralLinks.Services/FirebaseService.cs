// ReSharper disable ClassNeverInstantiated.Global
using Lykke.blue.Service.ReferralLinks.Core.Services;
using Lykke.blue.Service.ReferralLinks.Core.Settings;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Dynamic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Lykke.blue.Service.ReferralLinks.Services
{
    public class FirebaseService : IFirebaseService
    {
        private readonly AppSettings _settings;

        public FirebaseService(AppSettings settings)
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
                expando.dynamicLinkInfo.dynamicLinkDomain = _settings.ReferralLinksService.Firebase.DynamicLinkDomain;
                expando.dynamicLinkInfo.link = $"{_settings.ReferralLinksService.Firebase.Link}{id}";

                expando.dynamicLinkInfo.androidInfo = new ExpandoObject();
                expando.dynamicLinkInfo.androidInfo.androidPackageName = _settings.ReferralLinksService.Firebase.AndroidInfo.AndroidPackageName;

                expando.dynamicLinkInfo.iosInfo = new ExpandoObject();
                expando.dynamicLinkInfo.iosInfo.iosBundleId = _settings.ReferralLinksService.Firebase.IosInfo.IosBundleId;

                expando.suffix = new ExpandoObject();
                expando.suffix.option = "UNGUESSABLE";

                var jsonContent = JsonConvert.SerializeObject(expando);

                using (var stringContent = new StringContent(jsonContent, Encoding.UTF8, "application/json"))
                {
                    var response = await httpClient.PostAsync(
                        _settings.ReferralLinksService.Firebase.ApiUrl,
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

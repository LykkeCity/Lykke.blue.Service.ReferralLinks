// Code generated by Microsoft (R) AutoRest Code Generator 1.2.2.0
// Changes may cause incorrect behavior and will be lost if the code is
// regenerated.

namespace Lykke.blue.Service.ReferralLinks.Client.AutorestClient.Models
{
    using Lykke.blue;
    using Lykke.blue.Service;
    using Lykke.blue.Service.ReferralLinks;
    using Lykke.blue.Service.ReferralLinks.Client;
    using Lykke.blue.Service.ReferralLinks.Client.AutorestClient;
    using Newtonsoft.Json;
    using System.Linq;

    public partial class OffchainEncryptedKeyRespModel
    {
        /// <summary>
        /// Initializes a new instance of the OffchainEncryptedKeyRespModel
        /// class.
        /// </summary>
        public OffchainEncryptedKeyRespModel()
        {
          CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the OffchainEncryptedKeyRespModel
        /// class.
        /// </summary>
        public OffchainEncryptedKeyRespModel(string key = default(string))
        {
            Key = key;
            CustomInit();
        }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults
        /// </summary>
        partial void CustomInit();

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "Key")]
        public string Key { get; set; }

    }
}

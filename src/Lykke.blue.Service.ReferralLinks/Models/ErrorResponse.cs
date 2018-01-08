using System.Collections.Generic;

namespace Lykke.blue.Service.ReferralLinks.Models
{
    public class ErrorResponseModel
    {
        // ReSharper disable once MemberCanBePrivate.Global - must be public for json serialization
        public string ErrorMessage { get; }

        private Dictionary<string, List<string>> ModelErrors { get; }

        private ErrorResponseModel(string errorMessage)
        {
            ErrorMessage = errorMessage;
            ModelErrors = new Dictionary<string, List<string>>();
        }
        
        public static ErrorResponseModel Create(string message)
        {
            return new ErrorResponseModel(message);
        }
    }
}

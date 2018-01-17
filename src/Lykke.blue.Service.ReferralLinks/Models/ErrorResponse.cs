namespace Lykke.blue.Service.ReferralLinks.Models
{
    public class ErrorResponseModel
    {
        // ReSharper disable once MemberCanBePrivate.Global - must be public for json serialization
        public string ErrorMessage { get; }

        private ErrorResponseModel(string errorMessage)
        {
            ErrorMessage = errorMessage;
        }
        
        public static ErrorResponseModel Create(string message)
        {
            return new ErrorResponseModel(message);
        }

        public override string ToString()
        {
            return $"{ErrorMessage}";
        }
    }
}

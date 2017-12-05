using Newtonsoft.Json;

namespace Lykke.blue.Service.ReferralLinks.Core.BitCoinApi.Models
{
    public class ErrorResponse
    {
        [JsonIgnore]
        public string Code { get; set; }
        public string Message { get; set; }

        public ErrorResponse(string message, string code)
        {
            Message = message;
            Code = code;
        }

        public ErrorCode ErrorCode
        {
            get
            {
                ErrorCode code;
                int value;
                if (!int.TryParse(Code, out value))
                    code = ErrorCode.Exception;
                else
                    code = (ErrorCode)value;
                return code;
            }
        }

        public string ErrorCodeString
        {
            get
            {
                return ErrorCode.ToString();
            }
        }

    }
}

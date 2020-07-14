using Newtonsoft.Json;

namespace Mautom.Portunus.Entities.ErrorModel
{
    public sealed class ErrorDetails
    {
        public ErrorDetails(int statusCode, string message)
        {
            Message = message;
            StatusCode = statusCode;
        }

        public int StatusCode { get; set; }
        public string Message { get; set; }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
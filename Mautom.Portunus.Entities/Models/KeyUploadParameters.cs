namespace Mautom.Portunus.Entities.Models
{
    public class KeyUploadParameters
    {
        public KeyUploadParameters(string keyText)
        {
            KeyText = keyText;
        }

        public KeyUploadParameters() : this("")
        {
        }

        public string KeyText { get; set; }
    }
}
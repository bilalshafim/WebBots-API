using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace Araka_WebBots.Models.Firebase
{
    public class FirebaseData
    {
        [JsonPropertyName("sessions")]
        public Dictionary<string, List<GPTMessageBody>> Sessions { get; set; }
    }

    public class GPTMessageBody
    {
        [JsonPropertyName("message")]
        [JsonProperty(Required = Required.Always)]
        public string Message { get; set; }

        [JsonPropertyName("role")]
        [JsonProperty(Required = Required.Always)]
        public string Role { get; set; }

        [JsonPropertyName("status")]
        [JsonProperty(Required = Required.Always)]
        public string Status { get; set; }
    }

}
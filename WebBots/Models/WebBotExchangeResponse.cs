using Newtonsoft.Json;
using System.ComponentModel;
using System.Text.Json.Serialization;

namespace Araka_WebBots.Models
{
    public class WebBotExchangeResponse
    {
        [JsonPropertyName("messages")]
        public List<string> Messages { get; set; }

        [JsonPropertyName("user_id")]
        public string UserId { get; set; }

        [JsonPropertyName("payload")]
        public object CustomPayload { get; set; }

        [JsonPropertyName("session")]
        public object Session { get; set; }

        [JsonPropertyName("error")]
        public string Error { get; set; }

        [JsonPropertyName("end_conversation")]
        [DefaultValue(false)]
        public bool EndConversation { get; set; }
    }
}

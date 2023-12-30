using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace Araka_WebBots.Models
{
    public class WebBotExchangRequest
    {
        [JsonPropertyName("user_input")]
        [JsonProperty(Required = Required.Always)]
        public string UserInput { get; set; }

        [JsonPropertyName("user_id")]
        [JsonProperty(Required = Required.Always)]
        public string UserId { get; set; }

        [JsonPropertyName("bot_config")]
        public object BotConfig { get; set; }

        [JsonPropertyName("session")]
        public object Session { get; set; }

        [JsonPropertyName("paylaod")]
        public object CustomPayload { get; set; }
    }
}

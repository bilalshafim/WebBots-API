using Google.Api;
using Nuance.Tts.V1;
using System.Text.Json.Serialization;

namespace Araka_WebBots.Models.GPT
{
    public class OpenaiResponse
    {
        public string id { get; set; }
        [JsonPropertyName("object")]
        public string Object { get; set; }
        public long created { get; set; }
        public string model { get; set; }
        public Usage usage { get; set; }
        public List<Choice> choices { get; set; }
    }
    public class Usage
    {
        public int prompt_tokens { get; set; }
        public int completion_tokens { get; set; }
        public int total_tokens { get; set; }
    }

    public class Choice
    {
        public ResponseMessage message { get; set; }
        public string finish_reason { get; set; }
        public int index { get; set; }
    }

    public class ResponseMessage
    {
        public string role { get; set; }
        public string content { get; set; }
    }
}

using Araka_WebBots.Models;
using System.ComponentModel;

namespace Araka_WebBots.Bots.Watson.Models
{
    public class WatsonBotConfig
    {
        [DefaultValue("default")]
        public string ApiKey { get; set; }

        [DefaultValue("default")]
        public string AssistantId { get; set; }
        [DefaultValue("default")]
        public string Version { get; set; }

        [DefaultValue("default")]
        public string Url { get; set; }
    }
}

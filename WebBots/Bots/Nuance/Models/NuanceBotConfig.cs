using System.ComponentModel;

namespace Araka_WebBots.Bots.Nuance.Models
{
    public class NuanceBotConfig
    {
        [DefaultValue("default")]
        public string ClientID { get; set; }

        [DefaultValue("default")]
        public string ClientSecret { get; set; }

        [DefaultValue("default")]
        public string DialogUri { get; set; }
        
        [DefaultValue("default")]
        public string Channel { get; set; }

        [DefaultValue("default")]
        public string Library { get; set; }

        [DefaultValue(3600)]
        public uint BotSessionTimeout { get; set; }

        [DefaultValue("en-US")]
        public string Language { get; set; }
    }
}

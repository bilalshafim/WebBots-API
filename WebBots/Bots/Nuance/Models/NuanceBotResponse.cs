namespace Araka_WebBots.Bots.Nuance.Models
{
    public class NuanceBotResponse
    {
        public NuanceBotResponse()
        {
            transcripts = new List<string>();
        }

        public List<string> transcripts { get; set; }
        public bool endConversation { get; set; }
    }
}

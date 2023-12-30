namespace Araka_WebBots.Models.GPT
{
    public class OpenaiRequest
    {
        public string model { get; set; }
        public List<RequestMessage> messages { get; set; }
        public float temperature { get; set; }
        /*public int max_tokens { get; set; }*/
        public List<string> stop { get; set; }
    }
    public class RequestMessage
    {
        public string role { get; set; }
        public string content { get; set; }
    }
}

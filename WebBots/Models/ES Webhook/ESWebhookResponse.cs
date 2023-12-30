using Araka_WebBots.Models.GPT;

namespace Araka_WebBots.Models.ES_Webhook
{
    public class ESWebhookResponse
    {
        public List<FulfillmentMessage> fulfillmentMessages { get; set; }
        public object payload { get; set; }
        public EventInput followupEventInput { get; set; }
    }
    public class EventInput
    {
        public string name { get; set; }
        public object parameters { get; set; }
        public string languageCode { get; set; }

    }

    public class FulfillmentMessage
    {
        public TextMessage text { get; set; }
    }

    public class TextMessage
    {
        public List<string> text { get; set; }
    }
    public class Payload
    {
        public List<RequestMessage> payloadObject { get; set;}
    }
    


}

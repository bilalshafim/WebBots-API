using System.Text.Json.Serialization;

namespace Araka_WebBots.Models.ES_Webhook
{
    public class Parameters
    {
        public string paramName { get; set; }
    }
    public class ParameterizedContext
    {
        public string name { get; set; }
        public int lifespanCount { get; set; }
        public Parameters parameters { get; set; }
    }

    public class Intent
    {
        public string name { get; set; }
        public string displayName { get; set; }
    }

    public class QueryResult
    {
        public string queryText { get; set; }
        public object parameters { get; set; }
        public string action { get; set; }
        public bool allRequiredParamsPresent { get; set; }
        public string fulfillmentText { get; set; }
        public List<FulfillmentMessage> fulfillmentMessages { get; set; }
        public object webhookPayload { get; set; }
        public List<ParameterizedContext> outputContexts { get; set; }
        public Intent intent { get; set; }
        public double intentDetectionConfidence { get; set; }
        public Dictionary<string, object> diagnosticInfo { get; set; }
        public string languageCode { get; set; }
    }

    public class ESWebhookRequest
    {
        public string responseId { get; set; }
        public string session { get; set; }
        public QueryResult queryResult { get; set; }
        public OriginalDetectIntentRequest originalDetectIntentRequest { get; set; }
    }
    public class OriginalDetectIntentRequest
    {
        public string source { get; set; }
        public string version { get; set; }
        public dynamic payload { get; set; }

    }
    public class WebhookPayloadRequest
    {
        public object payloadObject { get; set; }
    }

    public class GPTExtentWebhookFollowupParams
    {
        public string byegpt { get; set; }
    }

    public class RentCar
    {
        [JsonPropertyName("geocity")]
        public string geoCity { get; set; }

        [JsonPropertyName("date")]
        public string date { get; set; }

        [JsonPropertyName("duration")]
        public Dictionary<string, object> duration { get; set; }

        [JsonPropertyName("car")]
        public string car { get; set; }

        [JsonPropertyName("confirmation")]
        public string confirmation { get; set; }

        [JsonPropertyName("name")]
        public string name { get; set; }

        [JsonPropertyName("number")]
        public string number { get; set; }

    }

    public class RentHotel
    {
        [JsonPropertyName("hotelcity")]
        public string hotelCity { get; set; }

        [JsonPropertyName("checkin")]
        public string checkIn { get; set; }

        [JsonPropertyName("checkout")]
        public string checkOut { get; set; }

        [JsonPropertyName("personcount")]
        public float person { get; set; }

        [JsonPropertyName("budget")]
        public string budget { get; set; }

        [JsonPropertyName("confirmation")]
        public string confirmation { get; set; }

        [JsonPropertyName("name")]
        public string name { get; set; }

        // email collection
        [JsonPropertyName("number")]
        public string number { get; set; }

    }
}

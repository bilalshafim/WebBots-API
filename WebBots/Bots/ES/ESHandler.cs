using Araka_WebBots.Bots.ES.Models;
using Araka_WebBots.Bots.Watson.Models;
using Araka_WebBots.Interafaces;
using Araka_WebBots.Models;
using Araka_WebBots.Models.ES_Webhook;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Dialogflow.V2;
using Google.Protobuf.Collections;
using Google.Protobuf.WellKnownTypes;
using Grpc.Auth;
using Grpc.Core;
using Newtonsoft.Json;

namespace Araka_WebBots.Bots.ES
{
    public class ESHandler : IBotProvider
    {
        private string _error { get; set; }
        public ESHandler() { }
        public async Task<WebBotExchangeResponse> ExchangeAsync(WebBotExchangRequest exchangeRequest)
        {
            WebBotExchangeResponse exchangeResponse;
            DetectIntentResponse detectIntentResponse = new();

            string projectID;
            string region;
            string environment;
            Sessions.SessionsClient sessionsClient = null;

            // Set bot config to default Araka bot if bot config is null in request
            ESBotConfig botConfig = new();
            try
            {
                botConfig = GetESConfig(exchangeRequest.BotConfig);
            } catch (Exception ex)
            {
                _error = $"Error in deserialzing ES config: {ex.Message}";
            }

            // Deserialize Session obj. Contains sessionId for consequent exchanges
            var botSession = new ESSession();
            if (exchangeRequest.Session != null && !string.IsNullOrWhiteSpace(exchangeRequest.Session.ToString()))
            {
                try
                {
                    botSession = JsonConvert.DeserializeObject<ESSession>(exchangeRequest.Session.ToString());
                } catch (Exception ex)
                {
                    _error = $"Error in deserialzing session: {ex.Message}";
                }
            }
            else
            {
                // Generate new session Id (needs new way to generate latter part of sessionId)
                Random rnd = new Random();
                botSession.SessionId = $"{exchangeRequest.UserId}-{rnd.Next(99999999)}";
            }

            // Create ES client
            CreateESClient(botConfig, ref sessionsClient, out projectID, out region, out environment);

            // Set and send request
            try
            {
                detectIntentResponse = await CommunicateWithES(projectID, region, environment, botSession.SessionId, exchangeRequest, sessionsClient);
            } catch (Exception ex)
            {
                _error = $"Error in communicating with ES: {ex.Message}";
            }

            // Set response data
            exchangeResponse = SetResponseData(detectIntentResponse);
            exchangeResponse.UserId = exchangeRequest.UserId;
            exchangeResponse.Session = botSession;
            exchangeResponse.Error = _error;

            return exchangeResponse;
        }
        public ESBotConfig GetESConfig(object botConfig)
        {
            ESBotConfig esBotConfig = new();
            if (botConfig != null)
            {
                string botConfigJson = JsonConvert.SerializeObject(botConfig);
                esBotConfig = JsonConvert.DeserializeObject<ESBotConfig>(botConfigJson);
            }
            return esBotConfig;
        }
        public void CreateESClient(ESBotConfig botConfig, ref Sessions.SessionsClient sessionsClient, out string projectId, out string region, out string environment)
        {
            projectId = botConfig.GCPProject;

            region = String.IsNullOrEmpty(botConfig.Region) ? "us" : botConfig.Region;
            environment = String.IsNullOrEmpty(botConfig.Environment) ? "draft" : botConfig.Environment;

            region = region.ToLower();
            environment = environment.ToLower();

            GoogleCredential googleCred = GoogleCredential.FromJson(botConfig.JSONServiceAccount.ToString()).CreateScoped("https://www.googleapis.com/auth/cloud-platform");
            ChannelCredentials channelCreds = googleCred.ToChannelCredentials();

            string host = region + "-dialogflow.googleapis.com";
            Channel channel = new Channel(host, 443, channelCreds);
            
            // May need to use SessionsClientBuilder. Testing not done as of yet
            sessionsClient = new Sessions.SessionsClient(channel);
        }
        public async Task<DetectIntentResponse> CommunicateWithES(string projectID, string region, string environment, string sessionId, WebBotExchangRequest exchangeRequest, Sessions.SessionsClient sessionsClient)
        {
            DetectIntentRequest detectIntentRequest = new DetectIntentRequest();
            detectIntentRequest.Session = "projects/" + projectID + "/locations/" + region + "/agent/environments/" + environment + "/users/" + exchangeRequest.UserId + "/sessions/" + sessionId;
            detectIntentRequest.QueryInput = new QueryInput()
            {
                Text = new TextInput()
                {
                    LanguageCode = "en_US",
                    Text = exchangeRequest.UserInput
                }
            };
            DetectIntentResponse detectIntentResponse = await sessionsClient.DetectIntentAsync(detectIntentRequest);
            return detectIntentResponse;
        }
        public WebBotExchangeResponse SetResponseData(DetectIntentResponse detectIntentResponse)
        {
            WebBotExchangeResponse exchangeResponse = new WebBotExchangeResponse();
            exchangeResponse.Messages = new List<string>();
            if (detectIntentResponse != null)
            {
                foreach (Google.Cloud.Dialogflow.V2.Intent.Types.Message message in detectIntentResponse.QueryResult.FulfillmentMessages)
                {
                    if (message.Text != null)
                    {
                        foreach (var ele in message.Text.Text_)
                        {
                            if (!string.IsNullOrWhiteSpace(ele))
                            {
                                exchangeResponse.Messages.Add(ele);
                            }
                            
                        }
                        
                    }
                    /*if (message.Payload != null)
                    {
                        // Alternate custom payload work. Will delete if new one does work.
                        //for (int i = 0; i < message.Payload.Fields.Count; i++)
                        //{
                        //    exchangeResponse.CustomPayload.Add(message.Payload.Fields.Keys.ElementAt(i), message.Payload.Fields.Values.ElementAt(i));
                        //}
                        exchangeResponse.CustomPayload = JsonConvert.DeserializeObject<Dictionary<string, object>>(message.Payload.ToString());
                    }*/
                }

                if (detectIntentResponse.QueryResult != null && detectIntentResponse.QueryResult.DiagnosticInfo != null && detectIntentResponse.QueryResult.DiagnosticInfo.Fields != null)
                {
                    if (detectIntentResponse.QueryResult.DiagnosticInfo.Fields.ContainsKey("end_conversation"))
                    {
                        exchangeResponse.EndConversation = detectIntentResponse.QueryResult.DiagnosticInfo.Fields["end_conversation"].BoolValue;
                    }
                }
                else if (detectIntentResponse.QueryResult.Intent.EndInteraction)
                {
                    // Alternate End Conversation work. Needs to be tested. Using diagnostic info for now.
                    exchangeResponse.EndConversation = true;
                }
            }
            return exchangeResponse;
        }
    }
}

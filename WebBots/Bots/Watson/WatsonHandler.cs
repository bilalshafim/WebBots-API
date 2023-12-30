using Araka_WebBots.Bots.Watson.Models;
using Araka_WebBots.Interafaces;
using Araka_WebBots.Models;
using IBM.Cloud.SDK.Core.Authentication;
using IBM.Cloud.SDK.Core.Authentication.Iam;
using IBM.Cloud.SDK.Core.Http;
using IBM.Watson.Assistant.v2;
using IBM.Watson.Assistant.v2.Model;
using Newtonsoft.Json;

namespace Araka_WebBots.Bots.Watson
{
    public class WatsonHandler : IBotProvider
    {
        private IamAuthenticator _authenticator;
        private AssistantService _assistantService;

        private string _error { get; set; }
        public WatsonHandler() { }
        public async Task<WebBotExchangeResponse> ExchangeAsync(WebBotExchangRequest exchangeRequest)
        {
            WebBotExchangeResponse exchangeResponse;
            MessageResponse botResponse = new MessageResponse();

            // Set bot config to default Araka bot if bot config is null in request
            WatsonBotConfig botConfig = new();
            try
            {
                botConfig = GetWatsonConfig(exchangeRequest.BotConfig);
            } catch (Exception ex)
            {
                _error = $"Error in deserialzing Watson config: {ex.Message}";
            }

            // Deserialize Session obj. Contains sessionId for consequent messages
            var botSession = new WatsonSession();
            try
            {
                if (exchangeRequest.Session != null && !string.IsNullOrWhiteSpace(exchangeRequest.Session.ToString()))
                {
                    string sessionString = JsonConvert.SerializeObject(exchangeRequest.Session);
                    botSession = JsonConvert.DeserializeObject<WatsonSession>(sessionString);
                } else
                {
                    botSession.SessionId = CreateSession(botConfig.AssistantId);
                }
            }
            catch (Exception ex) 
            {
                _error = $"Error in deserialzing or creating session: {ex.Message}";
            }

            // Authenticate client
            try
            {
                AuthenticateWatsonClient(botConfig);
            } catch (Exception ex) 
            {
                _error = $"Error in authenticating Watson client: {ex.Message}";
            }

            // Set and send request
            try
            {
                botResponse = CommunicateWithWatson(exchangeRequest.UserInput, botConfig, botSession.SessionId);
            } catch (Exception ex) 
            {
                _error = $"Error in communicating with Watson: {ex.Message}";
            }

            // Set response data
            exchangeResponse = SetResponseData(botResponse);
            exchangeResponse.UserId = exchangeRequest.UserId;
            exchangeResponse.Session = botSession;
            exchangeResponse.Error = _error;

            // Delete session if end conversation flag received from bot
            if (exchangeResponse.EndConversation)
            {
                try
                {
                    var result = _assistantService.DeleteSession(
                    assistantId: botConfig.AssistantId,
                    sessionId: botSession.SessionId
                    );
                } catch (Exception ex)
                {
                    _error = $"Error in deleting Watson session at end conversation: {ex.Message}";
                }

            }

            return exchangeResponse;
        }
        public WatsonBotConfig GetWatsonConfig(object botConfig)
        {
            WatsonBotConfig watsonBotConfig = new();
            if (botConfig != null)
            {
                string botConfigJson = JsonConvert.SerializeObject(botConfig);
                watsonBotConfig = JsonConvert.DeserializeObject<WatsonBotConfig>(botConfigJson);
            }
            return watsonBotConfig;
        }
        public void AuthenticateWatsonClient(WatsonBotConfig botConfig)
        {
            _authenticator = new IamAuthenticator(botConfig.ApiKey);
            _assistantService = new AssistantService(botConfig.Version, _authenticator);
            _assistantService.SetServiceUrl(botConfig.Url);
        }
        public string CreateSession(string assistantId)
        {
            var result = _assistantService.CreateSession(
                   assistantId: assistantId);

            return result.Result.SessionId;
        }
        public MessageResponse CommunicateWithWatson(string text, WatsonBotConfig botConfig, string sessionId)
        {
            MessageResponse botResponse = new MessageResponse();
            MessageInput messageInput = new MessageInput()
            {
                Text = text
            };
            DetailedResponse<MessageResponse> result = _assistantService.Message(assistantId: botConfig.AssistantId, sessionId: sessionId, input: messageInput);
            botResponse = result.Result;
            return botResponse;
        }
        public WebBotExchangeResponse SetResponseData(MessageResponse botResponse)
        {
            WebBotExchangeResponse exchangeResponse = new();
            if (botResponse.Output != null)
            {
                foreach (RuntimeResponseGeneric genericResponse in botResponse.Output.Generic)
                {
                    if (genericResponse.ResponseType == "text")
                    {
                        if (!string.IsNullOrEmpty(genericResponse.Text))
                        {
                            exchangeResponse.Messages.Add(genericResponse.Text);
                        }
                    }
                    if (genericResponse.ResponseType == "user_defined")
                    {
                        exchangeResponse.CustomPayload = genericResponse.UserDefined;
                    }
                    if (genericResponse.ResponseType == "end_session")
                    {
                        exchangeResponse.EndConversation = true;
                    }
                }
            }
            return exchangeResponse;
        }
    }
}

using Araka_WebBots.Bots.Nuance.Models;
using Araka_WebBots.Bots.Watson.Models;
using Araka_WebBots.Interafaces;
using Araka_WebBots.Models;
using Grpc.Core;
using Microsoft.AspNetCore.Mvc.Formatters.Xml;
using Microsoft.Extensions.Configuration.UserSecrets;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Nuance.Dlg.V1;
using Nuance.Dlg.V1.Common;
using static Google.Rpc.Context.AttributeContext.Types;
using static Nuance.Dlg.V1.DialogService;

namespace Araka_WebBots.Bots.Nuance
{
    public class NuanceHandler : IBotProvider
    {
        private DialogServiceClient _clientStub { get; set; }
        private ResourceReference _resourceReference { get; set; }
        private Selector _resourceSelector { get; set; }

        private string _error { get; set; }

        public NuanceHandler() { }
        public async Task<WebBotExchangeResponse> ExchangeAsync(WebBotExchangRequest exchangeRequest)
        {
            WebBotExchangeResponse exchangeResponse = new();

            // Set bot config to default Araka bot if bot config is null in request
            NuanceBotConfig botConfig = new();
            try
            {
                botConfig = GetNuanceConfig(exchangeRequest.BotConfig);
            }
            catch (Exception ex)
            {
                _error = $"Error in deserialzing Nuance config: {ex.Message}";
            }

            // Create gRPC client
            try
            {
                NuanceHttpAuthenticator nuanceHttpAuthenticator = new NuanceHttpAuthenticator(botConfig.ClientID.Replace(":", "%3A"), botConfig.ClientSecret);
                Channel authenticatedChannel = nuanceHttpAuthenticator.GetAuthenticatedChannel();
                _clientStub = new DialogServiceClient(authenticatedChannel);
                // Nuance DLGaaS Uri
                _resourceReference = new ResourceReference()
                {
                    Uri = botConfig.DialogUri,
                    Type = ResourceReference.Types.EnumResourceType.ApplicationModel
                };
                // Nuance container selector[multiple channels, libraries, languages can be provisioned by bot account]
                _resourceSelector = new Selector
                {
                    Channel = botConfig.Channel,
                    Language = botConfig.Language,
                    Library = botConfig.Library
                };
            } catch (Exception ex)
            {
                _error = $"Error in creating gRPC client for Nuance: {ex.Message}";
            }


            // Deserialize Session obj. Contains sessionId for consequent messages. CreateSession if null
            NuanceSession botSession = null;
            if (exchangeRequest.Session != null)
            {
                try
                {
                    string sessionString = JsonConvert.SerializeObject(exchangeRequest.Session);
                    botSession = JsonConvert.DeserializeObject<NuanceSession>(sessionString);
                    // Check session for expiry
                    StatusRequest statusRequest = new StatusRequest()
                    {
                        SessionId = botSession.SessionId
                    };
                    StatusResponse statusResponse = _clientStub.Status(statusRequest);
                    if (statusResponse.SessionRemainingSec < 20) // threshold was originally 5.
                    {
                        // Create new session because already existing session is nearing expiry
                        botSession = CreateNewSession(botConfig, exchangeRequest.UserId);
                    }
                }
                catch (Exception ex) 
                {
                    _error = $"Error in deserialzing or creating new session: {ex.Message}";
                }
            }
            else
            {
                // Create new session
                try
                {
                    botSession = CreateNewSession(botConfig, exchangeRequest.UserId);
                } catch (Exception ex)
                {
                    _error = $"Error in creating new session: {ex.Message}";
                }
            }

            // Set and send request
            NuanceBotResponse botResponse = new();
            try
            {
                botResponse = CommunicateWithNuance(botSession.SessionId, exchangeRequest.UserInput, exchangeRequest.UserId);
            } catch (Exception ex)
            {
                _error = $"Error in communicating with Nuance: {ex.Message}";
            }

            // Set response data
            exchangeResponse = SetResponseData(botResponse);
            exchangeResponse.UserId = exchangeRequest.UserId;
            exchangeResponse.Session = botSession;
            exchangeResponse.Error = _error;
            // Nuance does not support custom payload

            return exchangeResponse;
        }
        private NuanceSession CreateNewSession(NuanceBotConfig botConfig, string userId)
        {
            NuanceSession botSession = new();
            try
            {
                StartRequest request = new StartRequest()
                {
                    Selector = _resourceSelector,
                    Payload = new StartRequestPayload()
                    {
                        ModelRef = _resourceReference
                    },
                    UserId = userId,
                    SessionTimeoutSec = botConfig.BotSessionTimeout
                };
                StartResponse startResponse = _clientStub.Start(request);
                if (startResponse.Payload.SessionId != null && !string.IsNullOrEmpty(startResponse.Payload.SessionId))
                {
                    botSession.SessionId = startResponse.Payload.SessionId;
                }
                return botSession;
            } catch (Grpc.Core.RpcException httpReqEx)
            {
                if (httpReqEx.StatusCode == StatusCode.Internal)
                {
                    _error = httpReqEx.StatusCode + ": Please provide valid credentials";
                }
                else
                {
                    _error = httpReqEx.StatusCode + ": Error while creating a new session";
                }
            }
            return botSession;
            
        }
        private NuanceBotConfig GetNuanceConfig(object botConfig)
        {
            NuanceBotConfig nuanceBotConfig = new();
            if (botConfig != null)
            {
                string botConfigJson = JsonConvert.SerializeObject(botConfig);
                nuanceBotConfig = JsonConvert.DeserializeObject<NuanceBotConfig>(botConfigJson);
            }
            return nuanceBotConfig;
        }
        private NuanceBotResponse CommunicateWithNuance(string sessionId, string userInput, string userId)
        {
            ExecuteResponse executeResponse;
            try
            {
                ExecuteRequest executeRequest = new ExecuteRequest()
                {
                    SessionId = sessionId,
                    Selector = _resourceSelector,
                    Payload = new ExecuteRequestPayload()
                    {
                        UserInput = new UserInput()
                        {
                            UserText = userInput
                        }
                    },
                    UserId = userId
                };
                executeResponse = _clientStub.Execute(executeRequest);
            } catch (Exception ex)
            {
                _error = ex.Message;
                return null;
            }
            NuanceBotResponse botResponse = new NuanceBotResponse();

            SetBotResponse(executeResponse.Payload, ref botResponse);
            

            return botResponse;
        }
        private void SetBotResponse(ExecuteResponsePayload payload, ref NuanceBotResponse botResponse)
        {
            List<string> transcripts = new List<string>();

            botResponse.endConversation = false;

            if (payload is null)
            {
                return;
            }
            if (payload.Messages != null)
            {
                foreach (var message in payload.Messages)
                {
                    parseMessage(message, ref transcripts);
                }
            }
            if (payload.QaAction != null && payload.QaAction.Message != null)
            {
                parseMessage(payload.QaAction.Message, ref transcripts);
            }
            if (payload.DaAction != null && payload.DaAction.Message != null)
            {
                parseMessage(payload.DaAction.Message, ref transcripts);
            }
            botResponse.endConversation = payload.EndAction != null;
            botResponse.transcripts.AddRange(transcripts);
        }
        private void parseMessage(Message message, ref List<string> transcripts)
        {
            var numVisualMessages = message.Visual == null ? 0 : message.Visual.Count;
            for (int i = 0; i <= numVisualMessages; i++)
            {
                string transcript = message.Visual[i].Text;
                if (transcript != null)
                {
                    transcripts.Add(transcript);
                }
            }
        }
        private WebBotExchangeResponse SetResponseData(NuanceBotResponse botResponse)
        {
            WebBotExchangeResponse exchangeResponse = new();
            if (botResponse != null)
            {
                if (botResponse.transcripts == null || botResponse.transcripts.Count == 0)
                {
                    exchangeResponse.Messages = botResponse.transcripts;
                }
                exchangeResponse.EndConversation = botResponse.endConversation;
            }
            return exchangeResponse;
        }
    }
}

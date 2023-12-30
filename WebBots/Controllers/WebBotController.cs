using Araka_WebBots.Bots.ES;
using Araka_WebBots.Bots.Nuance;
using Araka_WebBots.Bots.Watson;
using Araka_WebBots.Interafaces;
using Araka_WebBots.Models;
using Araka_WebBots.Models.ES_Webhook;
using Araka_WebBots.Models.Firebase;
using Araka_WebBots.Utils;
using Araka_WebBots.Webhooks;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Dialogflow.V2;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Collections.Concurrent;
using static Nuance.Dlg.V1.Common.Selectable.Types.SelectableItem.Types;

namespace Araka_WebBots.Controllers
{
    [ApiController]
    [Route("api/v1")]
    public class WebBotController : ControllerBase
    {
        static string[] Scopes = { "https://www.googleapis.com/auth/userinfo.email", "https://www.googleapis.com/auth/firebase.database" }; // Example scope, you can change it to fit your needs

        private ILogger<WebBotController> _logger;
        private IBotProvider _provider;
        private ConcurrentDictionary<string, IBotProvider> _botProviders;
        public WebBotController(ILogger<WebBotController> logger)
        {
            _logger = logger;
            _botProviders = new ConcurrentDictionary<string, IBotProvider>();
            _botProviders["es"] = new ESHandler();
            _botProviders["watson"] = new WatsonHandler();
            _botProviders["nuance"] = new NuanceHandler();
        }

        [HttpPost]
        [Route("communicate/{bot}")]
        public async Task<WebBotExchangeResponse> WebBotExchange([FromBody] WebBotExchangRequest exchangeRequest, string bot)
        {
            // Need to add check here to verify if bot parameter is correct. Maybe not seeing this is an interanal API currently.
            _provider = _botProviders[$"{bot}"];
            WebBotExchangeResponse exchangeResponse = await _provider.ExchangeAsync(exchangeRequest);
            return exchangeResponse;
        }
        [HttpPost]
        [Route("es/webhook")]
        public async Task<ESWebhookResponse> WebBotExchange([FromBody] ESWebhookRequest ESrequest)
        {
            Console.WriteLine("Request Body " + JsonConvert.SerializeObject(ESrequest));

            var response = new ESWebhookResponse();

            try
            {
                var webhook = new ESWebhook();
                // car intent
                if (ESrequest.queryResult.intent.displayName == "Book a Car")
                {
                    webhook.BookCar(ESrequest, ref response);
                }
                // hotel intent
                if (ESrequest.queryResult.intent.displayName == "Book a Hotel")
                {
                    webhook.BookHotel(ESrequest, ref response);
                }
                // gpt intent
                if (ESrequest.queryResult.intent.displayName == "Chat gpt" ||
                    ESrequest.queryResult.intent.displayName == "extent_webhook_followup" ||
                    ESrequest.queryResult.intent.displayName == "extent_webhook_followup_2")
                {
                    webhook.ChatGPT(ESrequest, ref response);
                }

            }
            catch (Exception ex) 
            {
                Console.WriteLine($"Error: {ex.Message}");
            }

            return response;
        }

        [HttpPost]
        [Route("test")]
        public async Task test([FromBody] object data)
        {
            var credential = GoogleCredential.GetApplicationDefault()
                .CreateScoped(Scopes)
                .CreateWithUser("owner-395@chromatic-craft-384800.iam.gserviceaccount.com"); // Replace with the user's email for authentication

            // If you have a refresh token saved, you can set it here
            // credential = credential.CreateWithRefreshToken("your_refresh_token");

            // Access token
            string accessToken = credential.UnderlyingCredential.GetAccessTokenForRequestAsync().Result;

            var restLib = new ASRestLib();
            var gptURL = "https://chromatic-craft-384800-default-rtdb.firebaseio.com/.json";
            var authDict = new Dictionary<string, string>
            {
                { "Authorization", $"Bearer {accessToken}" }
            };
            var request = "";
            restLib.MakeRestApiCallAsync(gptURL, HttpMethod.Get, authDict, out string reponseBody, request);

            Console.WriteLine(reponseBody);
        }
        [HttpGet]
        [Route("check")]
        public void Firebase()
        {
            var credential = GoogleCredential.GetApplicationDefault()
                .CreateScoped(Scopes)
                .CreateWithUser("owner-395@chromatic-craft-384800.iam.gserviceaccount.com"); // Replace with the user's email for authentication

            // If you have a refresh token saved, you can set it here
            // credential = credential.CreateWithRefreshToken("your_refresh_token");

            // Access token
            string accessToken = credential.UnderlyingCredential.GetAccessTokenForRequestAsync().Result;

            var restLib = new ASRestLib();
            var gptURL = "https://chromatic-craft-384800-default-rtdb.firebaseio.com/arakaESBot/.json";
            var authDict = new Dictionary<string, string>
            {
                { "Authorization", $"Bearer {accessToken}" }
            };
            restLib.MakeRestApiCallAsync(gptURL, HttpMethod.Get, authDict, out string reponseBody);

            //Console.WriteLine(reponseBody);

            // TODO: Return all the data saved inside firebase in the `resp` var
            var firebaseResponse = new FirebaseData();
            firebaseResponse = JsonConvert.DeserializeObject<FirebaseData>(reponseBody);


            /*firebaseResponse.Sessions.TryGetValue("83e5f0bf-9f31-4195-b38a-94b28598131c-75064", out List<GPTMessageBody> body);
            Console.WriteLine(body[2].Message);*/


        }
    }
}


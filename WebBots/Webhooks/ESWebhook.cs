using Araka_WebBots.Models.ES_Webhook;
using Araka_WebBots.Models.Firebase;
using Araka_WebBots.Models.GPT;
using Araka_WebBots.Utils;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Util.Store;
using Newtonsoft.Json;
using Nuance.Nlu.V1;
using System.Collections.Generic;
using System.Diagnostics;

namespace Araka_WebBots.Webhooks
{
    public class ESWebhook
    {
        static string[] Scopes = { "https://www.googleapis.com/auth/userinfo.email", "https://www.googleapis.com/auth/firebase.database" }; // Example scope, you can change it to fit your needs

        public void BookCar(ESWebhookRequest ESrequest, ref ESWebhookResponse response)
        {
            var param = JsonConvert.DeserializeObject<RentCar>(ESrequest.queryResult.parameters.ToString());

            var botResponse1 = string.Empty;
            var botResponse2 = string.Empty;
            if (param.confirmation == "No")
            {
                botResponse1 = "Alright, looks like you changed your mind.";
                botResponse2 = "Is there anything else I can help you with?";
            }
            else if (param.confirmation == "Yes" &&
                param.number == "")
            {
                botResponse1 = "Could you please provide me with your phone number to make the reservation?";
            }
            else if (!string.IsNullOrWhiteSpace(param.date) && param.duration != null && !string.IsNullOrWhiteSpace(param.car) && !string.IsNullOrWhiteSpace(param.confirmation)
                 && !string.IsNullOrWhiteSpace(param.number))
            {
                botResponse1 = "Thank you. I'll send the reservation confirmation to your phone number.";
                botResponse2 = "Is there anything else I can help you with?";
            }
            else
            {
                botResponse1 = ESrequest.queryResult.fulfillmentText;
            }
            
            List<string> messageList = new List<string>()
            {
                botResponse1,
                botResponse2
            };
            response = new ESWebhookResponse()
            {
                fulfillmentMessages = setESResponseMessage(messageList),
            };
        }
        public void BookHotel(ESWebhookRequest ESrequest, ref ESWebhookResponse response)
        { 
            var param = JsonConvert.DeserializeObject<RentHotel>(ESrequest.queryResult.parameters.ToString());

            var botResponse1 = string.Empty;
            var botResponse2 = string.Empty;
            if (param.confirmation == "No")
            {
                botResponse1 = "Alright, looks like you changed your mind.";
                botResponse2 = "Is there anything else I can help you with?";
            }
            else if (param.confirmation == "Yes" &&
                string.IsNullOrWhiteSpace(param.number))
            {
                botResponse1 = "Could you please provide me with your phone number to make the reservation?";
            }
            else if (!string.IsNullOrWhiteSpace(param.hotelCity) && !string.IsNullOrWhiteSpace(param.checkIn) && !string.IsNullOrWhiteSpace(param.checkOut) 
                && !string.IsNullOrWhiteSpace(param.budget) && !string.IsNullOrWhiteSpace(param.number))
            {
                botResponse1 = "Thank you. I'll send the reservation confirmation to your phone number.";
                botResponse2 = "Is there anything else I can help you with?";
            }
            else
            {
                botResponse1 = ESrequest.queryResult.fulfillmentText;
            }

            List<string> messageList = new List<string>()
            {
                botResponse1,
                botResponse2
            };

            response = new ESWebhookResponse()
            {
                fulfillmentMessages = setESResponseMessage(messageList),
            };
        }
        public void ChatGPT(ESWebhookRequest ESRequest, ref ESWebhookResponse response)
        {
            // get all previous messages from payload
            GPTExtentWebhookFollowupParams webhookFollowupParams = null;
            try
            {
                webhookFollowupParams = JsonConvert.DeserializeObject<GPTExtentWebhookFollowupParams>(ESRequest.queryResult.parameters.ToString());
            } catch (Exception ex) { }

            // getting the sessionId from a long string of detailed sessionId
            var sessionArray = ESRequest.session.Split("/");
            string sessionId = sessionArray[sessionArray.Count() - 1];

            if (ESRequest.queryResult.action == "FirstRequestCustomGPT" ||
                (ESRequest.queryResult.queryText != "extent_webhook_deadline_2" &&
                webhookFollowupParams.byegpt == ""))
            {
                // send request to gpt and save response from gpt in firebase realtime db
                //TODO: CREATE CONDITION FOR NEW CONVO fixed 
                if (ESRequest.queryResult.action == "FirstRequestCustomGPT" && FetchFirebaseRealtime(sessionId) == null)
                {
                    Task.Run(() => SendGPTRequest(ESRequest.queryResult.queryText, sessionId, true));
                    response = new ESWebhookResponse()
                    {
                        fulfillmentMessages = new List<FulfillmentMessage>()
                        {
                            new FulfillmentMessage()
                            {
                                text = new TextMessage()
                                {
                                    text = new List<string>()
                                    {
                                        "Greetings! As an AI travel assistant powered by OpenAI, I'm here to provide you with any assistance you may need regarding your travel inquiries. Please feel free to let me know how I can be of service to you."
                                    }
                                }
                            }
                        }
                    };
                }
                else if((ESRequest.queryResult.action == "FirstRequestCustomGPT" && FetchFirebaseRealtime(sessionId) != null) ||
                    (ESRequest.queryResult.queryText != "extent_webhook_deadline_2" && webhookFollowupParams.byegpt == ""))
                {
                    if (string.IsNullOrWhiteSpace(webhookFollowupParams.byegpt))
                    {
                        Task.Run(() => SendGPTRequest(ESRequest.queryResult.queryText, sessionId, false));
                        // set delay here for 3.5 seconds
                        Thread.Sleep(2000);

                        // invoke first event on dialogflow
                        response = new ESWebhookResponse()
                        {
                            followupEventInput = new EventInput()
                            {
                                name = "extent_webhook_deadline",
                                languageCode = ESRequest.queryResult.languageCode,
                            }
                        };
                    }
                    else
                    {
                        response = new ESWebhookResponse()
                        {
                            followupEventInput = new EventInput()
                            {
                                name = "Welcome",
                                languageCode = ESRequest.queryResult.languageCode,
                            }
                        };
                    }

                }

                
            } 
            else if (ESRequest.queryResult.action == "SecondRequestCustomGPT")
            {
                // set delay here for 3.5 seconds
                Thread.Sleep(3500);

                // invoke second event on dialogflow
                response = new ESWebhookResponse()
                {
                    followupEventInput = new EventInput()
                    {
                        name = "extent_webhook_deadline_2",
                        languageCode = ESRequest.queryResult.languageCode,
                    }
                };

            } 
            else if (ESRequest.queryResult.action == "ThirdRequestCustomGPT")
            {
                // fetch firebase gpt message from the session id
                Thread.Sleep(3000);
                var fetchedFirebaseMessage = FetchFirebaseRealtime(sessionId);

                

                var dialogFlowText = new List<string>();

                foreach (var indexMessage in fetchedFirebaseMessage)
                {
                    if (indexMessage.Status == "unread")
                    {
                        dialogFlowText.Add(indexMessage.Message);
                    }
                }

                if (dialogFlowText.Count == 0)
                {
                    dialogFlowText.Add("Looks like GPT response is taking longer than usual. Can you try asking him again?");
                }

                if (string.IsNullOrWhiteSpace(webhookFollowupParams.byegpt))
                {
                    response = new ESWebhookResponse()
                    {
                        fulfillmentMessages = new List<FulfillmentMessage>()
                        {
                            new FulfillmentMessage()
                            {
                                text = new TextMessage()
                                {
                                    text = dialogFlowText
                                }
                            }
                        }
                    };
                }
                else
                {
                    response = new ESWebhookResponse()
                    {
                        followupEventInput = new EventInput()
                        {
                            name = "Welcome",
                            languageCode = ESRequest.queryResult.languageCode,
                        }
                    };
                }
                
                // updating the message to read from unread
                var copiedFirebaseMessage = new List<GPTMessageBody>();
                foreach (var message in fetchedFirebaseMessage)
                {
                    var messageBody = new GPTMessageBody();
                    messageBody.Message = message.Message;
                    messageBody.Role = message.Role;
                    if (message.Role == "assistant" && message.Status == "unread")
                    {
                        messageBody.Status = "read";
                    } else
                    {
                        messageBody.Status = message.Status;
                    }
                    copiedFirebaseMessage.Add(messageBody);
                }
                Task.Run(() => PushFirebaseRealtime(sessionId, copiedFirebaseMessage));
            } 
            else
            {
                response = new ESWebhookResponse()
                {
                    fulfillmentMessages = new List<FulfillmentMessage>()
                        {
                            new FulfillmentMessage()
                            {
                                text = new TextMessage()
                                {
                                    text = new List<string>(){ ESRequest.queryResult.fulfillmentText }
                                }
                            }
                        }
                };
                
            }
        }
        private List<FulfillmentMessage> setESResponseMessage(List<string> messageList)
        {

            var fulfillmentMessage = new FulfillmentMessage();
            fulfillmentMessage.text = new TextMessage();
            fulfillmentMessage.text.text = new List<string>();
            var responseMessage = new List<FulfillmentMessage>();
            foreach (var item in messageList)
            {
                fulfillmentMessage.text.text.Add(item);
            }

            responseMessage.Add(fulfillmentMessage);
            return responseMessage;
        }

        private static async Task SendGPTRequest(string userMessage, string sessionId, bool newConvo)
        {
            var firebaseMessages = FetchFirebaseRealtime(sessionId);

            if (firebaseMessages == null)
            {
                firebaseMessages = new List<GPTMessageBody>();
            }

            var convoBody = new List<RequestMessage>();
            if (newConvo)
            {
                convoBody.Add(new RequestMessage()
                {
                    role = "system",
                    content = "You are a travel assistant bot which is running on a google dialogflow bot which helps in escalation of cases where dialogflow bot is not able to assist in user queries on traveling questions",
                });
                firebaseMessages.Add(new GPTMessageBody()
                {
                    Message = "You are a travel assistant bot which is running on a google dialogflow bot which helps in escalation of cases where dialogflow bot is not able to assist in user queries on traveling questions",
                    Role = "system",
                    Status = "read"
                });
            }
            else
            {
                //TODO: FETCH ALL PREVIOUS CONVO FROM FIREBASE AND SAVE INSIDE `convoBody` variable Completed
                foreach (var item in firebaseMessages)
                {
                    convoBody.Add(new RequestMessage()
                    {
                        role = item.Role,
                        content = item.Message,
                    });
                }
                // THIS MESSAGE IS FOR THE NEW MESSAGE FROM USER
                convoBody.Add(new RequestMessage()
                {
                    role = "user",
                    content = userMessage,
                });
                // adding new message to the firebase list of message to be sent to patch with the previous data
                firebaseMessages.Add(new GPTMessageBody()
                {
                    Message = userMessage,
                    Role = "user",
                    Status = "read"
                });
            }

            if (!newConvo) 
            {
                var restLib = new ASRestLib();
                var gptURL = "https://api.openai.com/v1/chat/completions";
                var authDict = new Dictionary<string, string>
                {
                    { "Authorization", $"Bearer {Environment.GetEnvironmentVariable("GPT_KEY")}" }
                };

                var openaiRequest = new OpenaiRequest()
                {
                    model = "gpt-3.5-turbo",
                    stop = new List<string>() {
                        "Is there anything else I can help you with?",
                        "Let me know if you have any more questions.",
                        "Do you have any other travel-related questions?",
                        "I hope that answers your question."
                    },
                    temperature = 0.0f,
                    messages = convoBody
                };

                // request gpt
                var request = JsonConvert.SerializeObject(openaiRequest);
                // Create a Stopwatch instance
                Stopwatch stopwatch = new Stopwatch();

                // Start the stopwatch
                stopwatch.Start();

                restLib.MakeRestApiCallAsync(gptURL, HttpMethod.Post, authDict, out string reponseBody, request);
                stopwatch.Stop();

                // Get the elapsed time in milliseconds
                TimeSpan elapsedTime = stopwatch.Elapsed;
                double milliseconds = elapsedTime.TotalMilliseconds;

                // Print the elapsed time
                Console.WriteLine("Elapsed Time: " + milliseconds + " ms");
                var openaiRes = new OpenaiResponse();
                try
                {
                    openaiRes = JsonConvert.DeserializeObject<OpenaiResponse>(reponseBody);
                }
                catch (Exception ex) { }

                try
                {

                    // Print the elapsed time
                    Console.WriteLine("OPEN.AI Request Body: " + JsonConvert.SerializeObject(openaiRes));
                    // adding open ai new response data inside firebase list of message. This message will then be fetched once the new request arrives in the code.
                    foreach (var item in openaiRes.choices)
                    {
                        firebaseMessages.Add(new GPTMessageBody()
                        {
                            Message = item.message.content,
                            Role = item.message.role,
                            Status = "unread"
                        });
                    }
                }
                catch (Exception ex) { }
            }    
            
            // TODO: save or update all the data in firebase completed
            await PushFirebaseRealtime(sessionId, firebaseMessages);

        }
        private static async Task PushFirebaseRealtime(string sessionId, List<GPTMessageBody> request)
        {
            // adding the session ID variable inside the JSON body.
            var firebaseNewData = new Dictionary<string, List<GPTMessageBody>>();
            firebaseNewData.Add(sessionId, request);

            var requestJsonInString = JsonConvert.SerializeObject(firebaseNewData);


            var credential = GoogleCredential.GetApplicationDefault()
                .CreateScoped(Scopes); // Replace with the user's email for authentication

            // If you have a refresh token saved, you can set it here
            // credential = credential.CreateWithRefreshToken("your_refresh_token");

            // Access token
            string accessToken = credential.UnderlyingCredential.GetAccessTokenForRequestAsync().Result;

            var restLib = new ASRestLib();
            var gptURL = "https://chromatic-craft-384800-default-rtdb.firebaseio.com/arakaESBot/sessions.json";
            var authDict = new Dictionary<string, string>
            {
                { "Authorization", $"Bearer {accessToken}" }
            };
            // TODO: Should be patch method completed
            restLib.MakeRestApiCallAsync(gptURL, HttpMethod.Patch, authDict, out string reponseBody, requestJsonInString);

            Console.WriteLine(reponseBody);
        }

        // TODO: Implement this function to fetch firebase response completed
        private static List<GPTMessageBody> FetchFirebaseRealtime(string sessionId)
        {
            var credential = GoogleCredential.GetApplicationDefault()
                .CreateScoped(Scopes); // Replace with the user's email for authentication

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

            var firebaseResponse = new FirebaseData();
            firebaseResponse = JsonConvert.DeserializeObject<FirebaseData>(reponseBody);
            
            var messsages = new List<GPTMessageBody>();
            firebaseResponse.Sessions.TryGetValue(sessionId, out messsages);

            return messsages;
        }

    }
}

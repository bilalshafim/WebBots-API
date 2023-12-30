using Araka_WebBots.Bots.Nuance.Models;
using Grpc.Core;
using Grpc.Core.Utils;
using Microsoft.AspNetCore.DataProtection;
using Newtonsoft.Json;
using System.Net.Http.Headers;

namespace Araka_WebBots.Bots.Nuance
{
    public class NuanceHttpAuthenticator
    {
        // Ripped class
        public string client_id { get; set; }
        public string secret { get; set; }
        public NuanceHttpAuthenticator(string client_id, string secret)
        {
            this.client_id = client_id;
            this.secret = secret;
        }
        public Channel GetAuthenticatedChannel()
        {
            CallCredentials accessTokenCredentials = CallCredentials.FromInterceptor(new AsyncAuthInterceptor((context, metadata) =>
            {
                metadata.Add("authorization", "Bearer " + GetNuanceAccessToken().access_token);
                return TaskUtils.CompletedTask;
            }));
            SslCredentials channelCredentials = new SslCredentials();
            Dictionary<string, string> getAppsettingJson = GetDictionaryFromConfigFile("appsettings.json", "");
            ChannelCredentials authenticatedCredentials = ChannelCredentials.Create(channelCredentials, accessTokenCredentials);
            Channel authenticatedChannel = new Channel(getAppsettingJson["ChannelTarget"], authenticatedCredentials);
            return authenticatedChannel;
        }
        public NuanceToken GetNuanceAccessToken()
        {
            Dictionary<string, string> getAppsettingJson = GetDictionaryFromConfigFile("appsettings.json", "");
            //setup reusable http client
            HttpClient client = new HttpClient();
            Uri baseUri = new Uri(getAppsettingJson["Authuri"]);
            client.BaseAddress = baseUri;
            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.ConnectionClose = true;

            //Post body content
            List<KeyValuePair<string, string>> values = new List<KeyValuePair<string, string>>();
            values.Add(new KeyValuePair<string, string>("grant_type", getAppsettingJson["grant_type"]));
            values.Add(new KeyValuePair<string, string>("scope", getAppsettingJson["scope"]));
            FormUrlEncodedContent content = new FormUrlEncodedContent(values);

            var authenticationString = $"{client_id}:{secret}";
            var base64EncodedAuthenticationString = Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes(authenticationString));

            HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Post, "/oauth2/token");
            requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Basic", base64EncodedAuthenticationString);
            requestMessage.Content = content;

            //make the request
            Task<HttpResponseMessage> task = client.SendAsync(requestMessage);
            var response = task.Result;
            response.EnsureSuccessStatusCode();
            string responseBody = response.Content.ReadAsStringAsync().Result;
            NuanceToken accessToken = System.Text.Json.JsonSerializer.Deserialize<NuanceToken>(responseBody);

            return accessToken;
        }
        public static Dictionary<string, string> GetDictionaryFromConfigFile(string configFileName, string configDir)
        {
            var pathToFile = $@"{configFileName}";
            Dictionary<string, object> configObject = JsonConvert
                .DeserializeObject<Dictionary<string, object>>(
                File.ReadAllText(pathToFile));
            Dictionary<string, string> VahBotConfig = JsonConvert.DeserializeObject<Dictionary<string, string>>(configObject["VahBotConfigs"].ToString());

            return VahBotConfig;
        }
    }
}

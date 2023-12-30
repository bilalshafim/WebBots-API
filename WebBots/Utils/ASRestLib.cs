using IBM.Cloud.SDK.Core.Http;
using System.Text;

namespace Araka_WebBots.Utils
{
    public class ASRestLib
    {
        private readonly HttpClient _httpClient = new HttpClient();

        public string MakeRestApiCallAsync(string url, HttpMethod method, Dictionary<string, string> headers, out string responseBodyString, string jsonBody = null)
        {
            responseBodyString = string.Empty;
            var theResponsePayload = string.Empty;
            try
            {
                var request = new HttpRequestMessage(method, url);

                // Add headers to request
                if (headers != null)
                {
                    foreach (var header in headers)
                    {
                        request.Headers.Add(header.Key, header.Value);
                    }
                }

                // Add json body to request
                if (!string.IsNullOrEmpty(jsonBody))
                {
                    request.Content = new StringContent(jsonBody, Encoding.UTF8, "application/json");
                }

                var response = _httpClient.SendAsync(request).Result;

                theResponsePayload = response.Content.ReadAsStringAsync().Result;

                responseBodyString = theResponsePayload;
            } catch (Exception ex)
            {
                responseBodyString = ex.Message;
            }
            
            return responseBodyString;
        }
    }
}

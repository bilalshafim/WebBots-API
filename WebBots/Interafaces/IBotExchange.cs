using Araka_WebBots.Models;

namespace Araka_WebBots.Interafaces
{
    public interface IBotProvider
    {
        public Task<WebBotExchangeResponse> ExchangeAsync(WebBotExchangRequest exchangeRequest);
    }
}

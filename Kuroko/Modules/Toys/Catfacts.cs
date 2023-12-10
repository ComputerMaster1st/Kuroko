using Discord.Interactions;
using Kuroko.Core;
using Newtonsoft.Json;

namespace Kuroko.Modules.Toys
{
    public class Catfacts : KurokoModuleBase
    {
        private readonly HttpClient _httpClient = new() 
        {
            BaseAddress = new Uri("https://catfact.ninja")
        };

        [SlashCommand("catfacts", "Gets a random cat fact.")]
        public async Task ExecuteAsync()
        {
            var webRequest = await _httpClient.GetStringAsync("/fact");
            var result = JsonConvert.DeserializeObject<Dictionary<string,object>>(webRequest);

            await RespondAsync(result["fact"].ToString());
        }
    }
}

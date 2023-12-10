using Discord;
using Discord.Interactions;
using Kuroko.Core;
using Newtonsoft.Json;
using System.Net;

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
            var json = JsonConvert.DeserializeObject<Dictionary<string,object>>(webRequest);
            var result = json["fact"].ToString();

                await RespondAsync(result);
                   
               

           
        }
    }
}

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Telegram.Bot.Types;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace KURSOVA
{
    public class SearchCityClient
    {
        private HttpClient _httpClient;
        public static string _address;

        public SearchCityClient()
        {
            _address = Constants.address;
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri(_address);
        }

        public async Task<Response> SearchSomeCityAsync(string address, long UserId)
        {


            var uriBuilder = new UriBuilder($"{_address}/SearchCity");
            var query = HttpUtility.ParseQueryString(uriBuilder.Query);
            query["address"] = address;
            query["UserId"] = UserId.ToString();
            uriBuilder.Query = query.ToString();

            var responce = await _httpClient.GetAsync(uriBuilder.ToString());
            responce.EnsureSuccessStatusCode();
            var content = responce.Content.ReadAsStringAsync().Result;
            var result = JsonConvert.DeserializeObject<Response>(content);
            return result;
        }

        public async Task<List<CityMain>> GetStatistCityAsync(long UserId)
        {
            var responce = await _httpClient.GetAsync($"{_address}/StatistCity?UserId={UserId}");
            responce.EnsureSuccessStatusCode();
            var content = await responce.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<List<CityMain>>(content);
            return result;
        }



    }
}

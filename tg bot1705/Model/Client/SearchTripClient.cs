using Microsoft.VisualBasic;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualBasic;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Web;
using Telegram.Bot.Types;

namespace KURSOVA
{
    public class SearchTripClient
    {
        private HttpClient _httpClient;
        public static string _address;

        public SearchTripClient()
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

        public async Task<BlaBlaCarResponse> SearchSomeTripAsync(string coor1, string coor2, string data, long UserId)
        {
            var uriBuilder = new UriBuilder($"{_address}/SearchTrip");
            var query = HttpUtility.ParseQueryString(uriBuilder.Query);
            query["coor1"] = coor1;
            query["coor2"] = coor2;
            query["data"] = data;
            query["UserId"] = UserId.ToString();
            uriBuilder.Query = query.ToString();

            var responce = await _httpClient.GetAsync(uriBuilder.ToString());

            var content = responce.Content.ReadAsStringAsync().Result;
            var result = JsonConvert.DeserializeObject<BlaBlaCarResponse>(content);
            return result;
        }

        public async Task<List<TripMain>> GetStatistTripAsync(long UserId)
        {
            var uriBuilder = new UriBuilder($"{_address}/StatistTrip");
            var query = HttpUtility.ParseQueryString(uriBuilder.Query);
            query["UserId"] = UserId.ToString();
            uriBuilder.Query = query.ToString();

            var responce = await _httpClient.GetAsync(uriBuilder.ToString());

            responce.EnsureSuccessStatusCode();
            var content = responce.Content.ReadAsStringAsync().Result;
            var result = JsonConvert.DeserializeObject<List<TripMain>>(content);
            return result;
        }



        ///////////////////////////////////////////////////////////////////////////////////////


        public async Task SaveFavoriteTripAsync(string trip, long UserId)
        {
            HttpClient client = new HttpClient();
            await client.PostAsync($"{_address}/InsertFavoriteTrip?FavoriteTrip={Uri.EscapeDataString(trip)}&UserId={UserId}", null);

        }


        public async Task DeleteListOfFavoriteTripsAsync(long UserId)
        {
            HttpClient client = new HttpClient();

            var uriBuilder = new UriBuilder($"{_address}/DeleteListOfFavoriteTrips");
            var query = HttpUtility.ParseQueryString(uriBuilder.Query);
            query["UserId"] = UserId.ToString();
            uriBuilder.Query = query.ToString();

            await client.DeleteAsync(uriBuilder.ToString());

        }



        public async Task<List<FavoriteTripMain>> GetStatistFavoriteTripAsync(long UserId)
        {



            var uriBuilder = new UriBuilder($"{_address}/StatistFavoriteTrip");
            var query = HttpUtility.ParseQueryString(uriBuilder.Query);
            query["UserId"] = UserId.ToString();
            uriBuilder.Query = query.ToString();




            var responce = await _httpClient.GetAsync(uriBuilder.ToString());
            responce.EnsureSuccessStatusCode();
            var content = responce.Content.ReadAsStringAsync().Result;
            var result = JsonConvert.DeserializeObject<List<FavoriteTripMain>>(content);
            return result;
        }

    }
}

using Microsoft.AspNetCore.Authorization;
using System.Net.Http.Json;
using static BlazorApp.Pages.Weather;

namespace BlazorApp.Model
{
    public class SecondApiHttpClientWrapper
    {
        private readonly HttpClient _httpClient;

        public SecondApiHttpClientWrapper(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }
        public async Task<T?> GetAsync<T>(string requestUrl)
        {
            // Interfering code before sending the request
            var response = await _httpClient.GetFromJsonAsync<T>(requestUrl);
            // Interfering code after sending the request
            return response;
        }
    }
}

#if USING_AUTH0
using System;
using System.IO;
using System.Net.Http;
using System.Text;
using Auth0.ManagementApi;
using Infrastructure.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.JsonWebTokens;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Infrastructure.Factories
{
    public class Auth0ApiClientFactory : IAuth0ApiClientFactory
    {
        private readonly IConfiguration _configuration;

        public Auth0ApiClientFactory(IConfiguration configuration)
        {
            _configuration = configuration;
            _httpClient = new HttpClient() {BaseAddress = new Uri(_configuration["Auth0Settings:Authority"] ?? string.Empty)};
        }

        private HttpClient _httpClient;
        private string _accessToken;
        private JsonWebToken Token => !string.IsNullOrWhiteSpace(_accessToken) ? new JsonWebToken(_accessToken) : null;

        private string GetAccessToken()
        {
            if (Token != null && Token.ValidFrom <= DateTime.Now && Token.ValidTo >= DateTime.Now)
                return _accessToken;

            _accessToken = null;

            var input = new {client_id = _configuration["Auth0Settings:ClientId"], client_secret = _configuration["Auth0Settings:ClientSecret"], audience = $"{_configuration["Auth0Settings:Authority"]}api/v2/", grant_type = "client_credentials"};
            var data = JsonConvert.SerializeObject(input, Formatting.None, new JsonSerializerSettings {NullValueHandling = NullValueHandling.Ignore});
            var content = new StringContent(data, Encoding.UTF8, System.Net.Mime.MediaTypeNames.Application.Json);
            var response = _httpClient.Send(new HttpRequestMessage(HttpMethod.Post, "/oauth/token") {Content = content});
            if (response.IsSuccessStatusCode)
            {
                using (var streamReader = new StreamReader(response.Content.ReadAsStream()))
                {
                    var result = streamReader.ReadToEnd();
                    _accessToken = (JObject.Parse(result)["access_token"] ?? string.Empty).Value<string>();
                }
            }

            return _accessToken;
        }

        public IManagementApiClient CreateClient()
        {
            var token = GetAccessToken();
            var audience = new Uri($"{_configuration["Auth0Settings:Authority"]}api/v2/");
            var managementApiClient = new ManagementApiClient(token, audience);
            return managementApiClient;
        }
    }
}
#endif

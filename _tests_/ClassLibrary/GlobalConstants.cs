using Newtonsoft.Json.Linq;
using RestSharp;
using System.Net;

namespace ClassLibrary
{
    public class GlobalConstants
    {
        public const string BaseUrl = "http://localhost:5000/api";
        private static RestClient client;

        public static string AuthenticateUser(string username, string password)
        {
            client = new RestClient(BaseUrl);

            var AuthRequest = new RestRequest("/user/admin-login", Method.Post);
            AuthRequest.AddJsonBody(new { email = username, password });

            var AuthResponse = client.Execute(AuthRequest);

            if (AuthResponse.StatusCode != HttpStatusCode.OK)
            {
                throw new Exception($"Authentication failed with status code {AuthResponse.StatusCode}, and response content {AuthResponse.Content}");               
            }

            var content = JObject.Parse(AuthResponse.Content);
            return content["token"]?.ToString();
        }


    }
}

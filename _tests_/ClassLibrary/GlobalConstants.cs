using Newtonsoft.Json.Linq;
using RestSharp;
using System.ComponentModel.DataAnnotations;
using System.Net;

namespace ClassLibrary
{
    public static class GlobalConstants
    {
        public const string BaseUrl = "http://localhost:5000/api";
        private static RestClient client;

        public static string AuthenticateUser(string username, string password)
        {
            for (int i = 0; i < 3; i++)
            {
                try
                {
                    client = new RestClient(BaseUrl);

                    var resource = username == "admin@gmail.com" ? "/user/admin-login" : "/user/login";
                    var AuthRequest = new RestRequest(resource, Method.Post);
                    AuthRequest.AddJsonBody(new { email = username, password });

                    var AuthResponse = client.Execute(AuthRequest);

                    if (AuthResponse.StatusCode != HttpStatusCode.OK)
                    {
                        throw new Exception($"Authentication failed with status code {AuthResponse.StatusCode}, and response content {AuthResponse.Content}");
                    }

                    var content = JObject.Parse(AuthResponse.Content);
                    return content["token"].ToString();
                    
                }
                catch (Exception ex)
                {
                    client = new RestClient(BaseUrl);

                    var resource = "/user/register";
                    var AuthRequest = new RestRequest(resource, Method.Post);
                    AuthRequest.AddJsonBody(new { firstname = "John", lastname = "Doe", email = username, mobile = "+1234567890", password });

                    var AuthResponse = client.Execute(AuthRequest);
                    
                    
                }
            }

            throw new Exception("All Authentication tries failed.");
        }


        
    }
}

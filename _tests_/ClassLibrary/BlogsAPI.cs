using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ClassLibrary
{
    public class BlogsAPI : IDisposable
    {
        private RestClient client;
        private string token;

        public void Dispose()
        {
            client?.Dispose();
        }

        public dynamic GetAllBlogs()
        {
            client = new RestClient(GlobalConstants.BaseUrl);
            token = GlobalConstants.AuthenticateUser("admin@gmail.com", "admin@gmail.com");

            if (token == null)
            {
                throw new Exception("Authentication token is null or empty");
            }

            var request = new RestRequest("/blog", Method.Get);
            request.AddHeader("Authorization", $"Bearer {token}");

            var response = client.Execute(request);

            if (response.StatusCode != HttpStatusCode.OK)
            {
                Console.WriteLine($"Authentication failed with status code {response.StatusCode}, and response content {response.Content}");
            }

            return response;



        }

        public dynamic GetBlog(string id)
        {
            client = new RestClient(GlobalConstants.BaseUrl);
            token = GlobalConstants.AuthenticateUser("admin@gmail.com", "admin@gmail.com");

            if (token == null)
            {
                throw new Exception("Authentication token is null or empty");
            }

            var request = new RestRequest("/blog/" + id, Method.Get);
            request.AddHeader("Authorization", $"Bearer {token}");

            var response = client.Execute(request);

            if (response.StatusCode != HttpStatusCode.OK)
            {
                Console.WriteLine($"Authentication failed with status code {response.StatusCode}, and response content {response.Content}");
            }

            return response;



        }

        public dynamic PostBlog(string title)
        {


            client = new RestClient(GlobalConstants.BaseUrl);
            token = GlobalConstants.AuthenticateUser("admin@gmail.com", "admin@gmail.com");
            if (token == null)
            {
                throw new Exception("Authentication token is null or empty");
            }

            var request = new RestRequest("/blog", Method.Post);
            request.AddHeader("Authorization", $"Bearer {token}");
            request.AddJsonBody(new
            {
                title,
                description = "Test Description",
                category = "TestCategory"
            });
            var response = client.Execute(request);

            if (response.StatusCode != HttpStatusCode.OK)
            {

                Console.WriteLine($"Authentication failed with status code {response.StatusCode}, and response content {response.Content}");
            }

            return response;
        }

        public dynamic UpdateBlog(string id)
        {
            client = new RestClient(GlobalConstants.BaseUrl);
            token = GlobalConstants.AuthenticateUser("admin@gmail.com", "admin@gmail.com");
            if (token == null)
            {
                throw new Exception("Authentication token is null or empty");
            }
            var request = new RestRequest("/blog/" + id, Method.Put);
            request.AddHeader("Authorization", $"Bearer {token}");
            request.AddJsonBody(new
            {
                title = "Updated title",
                description = "Updated description",
                category = "Updated category"
            });
            var response = client.Execute(request);

            if (response.StatusCode != HttpStatusCode.OK)
            {
                Console.WriteLine($"Authentication failed with status code {response.StatusCode}, and response content {response.Content}");
            }
            return response;

        }

        public dynamic DeleteBlog(string id)
        {
            client = new RestClient(GlobalConstants.BaseUrl);
            token = GlobalConstants.AuthenticateUser("admin@gmail.com", "admin@gmail.com");
            if (token == null)
            {
                throw new Exception("Authentication token is null or empty");
            }
            var request = new RestRequest("/blog/" + id, Method.Delete);
            request.AddHeader("Authorization", $"Bearer {token}");

            var response = client.Execute(request);

            if (response.StatusCode != HttpStatusCode.OK)
            {
                Console.WriteLine($"Authentication failed with status code {response.StatusCode}, and response content {response.Content}");
            }
            return response;
        }




    }
}

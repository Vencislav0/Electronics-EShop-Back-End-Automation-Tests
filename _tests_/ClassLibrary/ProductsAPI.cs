﻿using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace ClassLibrary
{
    public class ProductsAPI : IDisposable
    {
        private RestClient client;
        private string token;
        
        public void Dispose()
        {
            client?.Dispose();  
        }

        public dynamic GetAllProducts()
        {
            client = new RestClient(GlobalConstants.BaseUrl);
            token = GlobalConstants.AuthenticateUser("admin@gmail.com", "admin@gmail.com");

            if (token == null) 
            {
                throw new Exception("Authentication token is null or empty");
            }

            var request = new RestRequest("/product", Method.Get);
            request.AddHeader("Authorization", $"Bearer {token}");

            var response = client.Execute(request);

            if (response.StatusCode != HttpStatusCode.OK)
            {
                Console.WriteLine($"Authentication failed with status code {response.StatusCode}, and response content {response.Content}");
            }

            return response;



        }

        public dynamic GetProduct(string id)
        {
            client = new RestClient(GlobalConstants.BaseUrl);
            token = GlobalConstants.AuthenticateUser("admin@gmail.com", "admin@gmail.com");

            if (token == null)
            {
                throw new Exception("Authentication token is null or empty");
            }

            var request = new RestRequest("/product/" + id, Method.Get);
            request.AddHeader("Authorization", $"Bearer {token}");

            var response = client.Execute(request);

            if (response.StatusCode != HttpStatusCode.OK)
            {
                Console.WriteLine($"Authentication failed with status code {response.StatusCode}, and response content {response.Content}");
            }

            return response;



        }

        public dynamic PostProduct(string title)
        {
            
            
            client = new RestClient(GlobalConstants.BaseUrl);
            token = GlobalConstants.AuthenticateUser("admin@gmail.com", "admin@gmail.com");
            if (token == null)
            {
                throw new Exception("Authentication token is null or empty");
            }

            var request = new RestRequest("/product", Method.Post);
            request.AddHeader("Authorization", $"Bearer {token}");
            request.AddJsonBody(new
            {
                title,
                slug = title,
                description = "This is a test description",
                price = 1000.99,
                category = "TestCategory",
                brand = "TestBrand",
                quantity = 25
            });                       
                var response = client.Execute(request);
                       
            if (response.StatusCode != HttpStatusCode.OK)
            {
                
                Console.WriteLine($"Authentication failed with status code {response.StatusCode}, and response content {response.Content}");
            }

            return response;
        }

        public dynamic UpdateProduct(string id) 
        {
            client = new RestClient(GlobalConstants.BaseUrl);
            token = GlobalConstants.AuthenticateUser("admin@gmail.com", "admin@gmail.com");
            if (token == null)
            {
                throw new Exception("Authentication token is null or empty");
            }

            var request = new RestRequest("/product/" + id, Method.Put);
            request.AddHeader("Authorization", $"Bearer {token}");
            request.AddJsonBody(new
            {
                title = "Edited Title",
                slug = "edited-slug",
                description = "Edited Description",
                price = 99.99,
                category = "Edited Category",
                brand = "Edited Brand",
                quantity = 20
            });

            var response = client.Execute(request);

            if (response.StatusCode != HttpStatusCode.OK)
            {

                Console.WriteLine($"Authentication failed with status code {response.StatusCode}, and response content {response.Content}");
            }

            return response;

        }

        public dynamic DeleteProduct(string id)
        {
            client = new RestClient(GlobalConstants.BaseUrl);
            token = GlobalConstants.AuthenticateUser("admin@gmail.com", "admin@gmail.com");
            if (token == null)
            {
                throw new Exception("Authentication token is null or empty");
            }

            var request = new RestRequest("/product/" + id, Method.Delete);
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

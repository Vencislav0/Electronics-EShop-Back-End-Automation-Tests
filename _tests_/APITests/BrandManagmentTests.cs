using ClassLibrary;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APITests
{
    public  class BrandManagmentTests
    {
        private RestClient client;
        private string adminToken;
        private string userToken;
        private Random random;

        [SetUp]
        public void SetUp()
        {
            client = new RestClient(GlobalConstants.BaseUrl);
            adminToken = GlobalConstants.AuthenticateUser("admin@gmail.com", "admin@gmail.com");
            userToken = GlobalConstants.AuthenticateUser("johndoe@example.com", "password123");
            random = new Random();
        }

        [TearDown]
        public void TearDown()
        {
            client.Dispose();
        }

        


    }
}

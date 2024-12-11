using ClassLibrary;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APITests
{
    public class ColorManagmentTests
    {
        private RestClient client;
        private string adminToken;
        private Random random;

        [SetUp]
        public void SetUp()
        {
            client = new RestClient(GlobalConstants.BaseUrl);
            adminToken = GlobalConstants.AuthenticateUser("admin@gmail.com", "admin@gmail.com");
            random = new Random();
        }

        [TearDown]
        public void TearDown() 
        {
            client.Dispose();
        }


        [Test]
        public void ColorLifeCycleTests()
        {
            //POST request
            var addColorRequest = new RestRequest("/color/", Method.Post);
            addColorRequest.AddHeader("Authorization", $"Bearer {adminToken}");
            addColorRequest.AddJsonBody(new { title = $"Color_{random.Next(1, 1000)}" });

            var addColorResponse = client.Execute(addColorRequest);
            var colorID = JObject.Parse(addColorResponse.Content)["_id"]?.ToString();

            //Extract the ID validate it and make a GET request to verify that its created

            Assert.That(addColorResponse.IsSuccessful, Is.True, $"POST Failed with status code {addColorResponse.StatusCode}.");
            Assert.That(colorID, Is.Not.Null.Or.Empty, $"color has unexpected value: ${colorID}");

            var getColorRequest = new RestRequest("/color/" + colorID, Method.Get);

            var getColorResponse = client.Execute(getColorRequest);

            Assert.That(getColorResponse.IsSuccessful, Is.True, $"GET Failed with status code {getColorResponse.StatusCode}.");

            //DELETE the created color

            var deleteColorRequest = new RestRequest("/color/" + colorID, Method.Delete);
            deleteColorRequest.AddHeader("Authorization", $"Bearer {adminToken}");

            var deleteResponse = client.Execute(deleteColorRequest);

            Assert.That(deleteResponse.IsSuccessful, $"DELETEFailed with status code {deleteResponse.StatusCode}.");

            //Make get request to verify the deletion

            var getColorRequest2 = new RestRequest("/color/" + colorID, Method.Get);

            var getColorResponse2 = client.Execute(getColorRequest);

            Assert.That(getColorResponse2.Content, Is.EqualTo("null"), "Color found likely failed deletion.");

        }



    }
}

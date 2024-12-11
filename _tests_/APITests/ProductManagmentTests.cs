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
    public  class ProductManagmentTests
    {
        private RestClient client;
        private string adminToken;
        private string userToken;
        private Random random;
        private ProductsAPI product;

        [SetUp]
        public void SetUp()
        {
            client = new RestClient(GlobalConstants.BaseUrl);
            adminToken = GlobalConstants.AuthenticateUser("admin@gmail.com", "admin@gmail.com");
            userToken = GlobalConstants.AuthenticateUser("johndoe@example.com", "password123");
            random = new Random();
            product = new ProductsAPI();
        }

        [TearDown]
        public void TearDown()
        {
            client.Dispose();
        }

        [Test]
        public void ProductLifecycleTest()
        {
            //Create new product
            var randomTitle = "Title_" + random.Next(1, 1000);
            var createdProduct = product.PostProduct(randomTitle);

            Assert.That(createdProduct.IsSuccessful, Is.True, $"Error creating product {createdProduct.StatusCode}");

            var productID = JObject.Parse(createdProduct.Content)["_id"]?.ToString();
            Assert.That(productID, Is.Not.Null.Or.Empty, "ID was empty for created product");

            //Get the created product
            var getProduct = product.GetProduct(productID);

            Assert.That(getProduct.IsSuccessful, Is.True, $"Error getting the new product {getProduct.StatusCode}");
            Assert.That(getProduct.Content, Is.Not.Null.Or.Empty, $"new product is empty");

            //update the created product
            var updateProduct = product.UpdateProduct(productID);

            Assert.That(updateProduct.IsSuccessful, $"Error updating the product {updateProduct.StatusCode}");

            //delete the created product and try to retrive it to verify deletion

            var deleteProduct = product.DeleteProduct(productID);

            Assert.That(deleteProduct.IsSuccessful, $"Error deleting the product {deleteProduct.StatusCode}");

            getProduct = product.GetProduct(productID);

            Assert.That(getProduct.Content, Is.EqualTo("null").Or.Null);

        }


    }
}

using ClassLibrary;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace APITests
{
    public class CouponManagmentTests
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

        [Test]
        public void CouponLiveCycleTest()
        {
            //Get all products
            var getAllProductsRequest = new RestRequest("/product/", Method.Get);
            getAllProductsRequest.AddHeader("Authorization", $"Bearer {adminToken}");

            var getAllProductsResponse = client.Execute(getAllProductsRequest);

            var products = JArray.Parse(getAllProductsResponse.Content);
            Assert.That(products.Count, Is.GreaterThan(2), "not enough coupons to proceed.");

            //We fetch 2 random product ids
            var productIDs = products.Select(p => p["_id"]?.ToString()).ToList();
            var firstID = productIDs[random.Next(productIDs.Count)];
            var secondID = productIDs[random.Next(productIDs.Count)];

            //We check if both the ids are the same if so keep changing the second one until they are different
            while(firstID == secondID)
            {
                secondID = productIDs[random.Next(productIDs.Count)];
            }

            //Create new coupon
            string couponName = $"DISCOUNT20_{random.Next(1, 1000)}";

            var createCouponRequest = new RestRequest("/coupon/", Method.Post);
            createCouponRequest.AddHeader("Authorization", $"Bearer {adminToken}");
            createCouponRequest.AddJsonBody(new {name = couponName, expiry = "2024-09-30T23:59:59Z", discount = 20 });

            var createCouponResponse = client.Execute(createCouponRequest);

            Assert.That(createCouponResponse.IsSuccessful, $"Unable to create a coupon status code:{createCouponResponse.StatusCode}");

            //extract the coupon ID
            var couponID = JObject.Parse(createCouponResponse.Content)["_id"]?.ToString();
            Assert.That(couponID, Is.Not.Null.Or.Empty, "couponID null or empty");


            //Adding the items to the user shopping cart
            var cartRequest = new RestRequest("/user/cart", Method.Post);
            cartRequest.AddHeader("Authorization", $"Bearer {userToken}");
            cartRequest.AddJsonBody(new
            {
                cart = new[]
                {
                    new { _id = firstID, count = 1, color = "red" },
                    new { _id = secondID, count = 2, color = "blue" }
                }
                                                
            });

            var cartResponse = client.Execute(cartRequest);

            Assert.That(cartResponse.IsSuccessful, Is.True, $"Unable to add items to cart {cartResponse.StatusCode}");

            //Addint the discount coupon
            var applyCouponRequest = new RestRequest("/user/cart/applycoupon", Method.Post);
                applyCouponRequest.AddHeader("Authorization", $"Bearer {userToken}");
                applyCouponRequest.AddJsonBody(new { coupon = couponName });

            var applyCouponResponse = client.Execute(applyCouponRequest);

            //extract the product prices
            var productPrice1 = products.Where(p => p["_id"]?.ToString() == firstID).Select(p => p["price"]?.ToString()).FirstOrDefault();
            var productPrice2 = products.Where(p => p["_id"]?.ToString() == secondID).Select(p => p["price"]?.ToString()).FirstOrDefault();

            Assert.That(applyCouponResponse.IsSuccessful, Is.True, $"Unable to add coupon items {applyCouponResponse.StatusCode}");


            //Delete the created coupon
            var deleteCouponRequest = new RestRequest("/coupon/" + couponID, Method.Delete);
            deleteCouponRequest.AddHeader("Authorization", $"Bearer {adminToken}");

            var deleteResponse = client.Execute(deleteCouponRequest);

            Assert.That(deleteResponse.IsSuccessful, Is.True, $"Unsuccessful coupon deletion {applyCouponResponse.StatusCode}");

            //try to get the deleted coupon
            var verifyRequest = new RestRequest("/coupon/" + couponID, Method.Get);
            verifyRequest.AddHeader("Authorization", $"Bearer {adminToken}");

            var verifyResponse = client.Execute(verifyRequest);

            Assert.That(verifyResponse.IsSuccessful, Is.True, $"Unsuccessful get response to verify coupon {applyCouponResponse.StatusCode}");
            Assert.That(verifyResponse.Content, Is.EqualTo("null").Or.Null, $"Coupon found likely unsuccessful deletion");


        }
    }
}

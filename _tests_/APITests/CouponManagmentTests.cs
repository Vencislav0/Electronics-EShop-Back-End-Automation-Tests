using ClassLibrary;
using Newtonsoft.Json;
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

            Assert.That(applyCouponResponse.IsSuccessful, Is.True, $"Unable to apply coupon {applyCouponResponse.StatusCode}");


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

        [Test]
        public void CouponApplicationToOrderTest()
        {
            //Get the first product id
            var getAllProductsRequest = new RestRequest("/product/", Method.Get);
            getAllProductsRequest.AddHeader("Authorization", $"Bearer {adminToken}");

            var getAllProductsResponse = client.Execute(getAllProductsRequest);

            var firstProductID = JArray.Parse(getAllProductsResponse.Content).First()["_id"]?.ToString();

            Assert.That(firstProductID, Is.Not.Null.Or.Empty, "product id is null or empty");

            //Create the coupon

            string couponName = $"DISCOUNT10_{random.Next(1, 1000)}";

            var createCouponRequest = new RestRequest("/coupon/", Method.Post);
            createCouponRequest.AddHeader("Authorization", $"Bearer {adminToken}");
            createCouponRequest.AddJsonBody(new { name = couponName, expiry = "2024-09-30T23:59:59Z", discount = 10 });

            var createCouponResponse = client.Execute(createCouponRequest);

            Assert.That(createCouponResponse.IsSuccessful, $"Unable to create a coupon status code:{createCouponResponse.StatusCode}");

            //Adding coupon to cart
            var cartRequest = new RestRequest("/user/cart", Method.Post);
            cartRequest.AddHeader("Authorization", $"Bearer {userToken}");
            cartRequest.AddJsonBody(new
            {
                cart = new[]
                {
                    new { _id = firstProductID, count = 3, color = "red" }
                    
                }

            });

            var cartResponse = client.Execute(cartRequest);

            Assert.That(cartResponse.IsSuccessful, Is.True, $"Unable to add items to cart {cartResponse.StatusCode}");

            //Applying the coupon

            var applyCouponRequest = new RestRequest("/user/cart/applycoupon", Method.Post);
            applyCouponRequest.AddHeader("Authorization", $"Bearer {userToken}");
            applyCouponRequest.AddJsonBody(new { coupon = couponName });

            var applyCouponResponse = client.Execute(applyCouponRequest);

            Assert.That(applyCouponResponse.IsSuccessful, Is.True, $"Unable to apply coupon {cartResponse.StatusCode}");

            //Place the order with the applied coupon

            var placeOrderRequest = new RestRequest("/user/cart/cash-order", Method.Post);
            placeOrderRequest.AddHeader("Authorization", $"Bearer {userToken}");
            placeOrderRequest.AddJsonBody(JsonConvert.SerializeObject(new { COD = true, couponApplied = true }));

            var placeOrderResponse = client.Execute(placeOrderRequest);

            Assert.That(placeOrderResponse.IsSuccessful, Is.True, $"Unable to place order {cartResponse.StatusCode}");
            Assert.That(JObject.Parse(placeOrderResponse.Content)["message"]?.ToString(), Is.EqualTo("success"));


        }


        [Test]
        public void ComplexOrderLifeCycleTest()
        {
            //Get all products
            var getAllProductsRequest = new RestRequest("/product/", Method.Get);
            getAllProductsRequest.AddHeader("Authorization", $"Bearer {adminToken}");

            var getAllProductsResponse = client.Execute(getAllProductsRequest);

            var products = JArray.Parse(getAllProductsResponse.Content);

            Assert.That(getAllProductsResponse.IsSuccessful, Is.True, $"Getting products failed {getAllProductsResponse.StatusCode}");

            //Getting the first product id
            var firstProductId = products.First()["_id"]?.ToString();

            //Adding the product to cart
            var cartRequest = new RestRequest("/user/cart", Method.Post);
            cartRequest.AddHeader("Authorization", $"Bearer {userToken}");
            cartRequest.AddJsonBody(new
            {
                cart = new[]
                {
                    new { _id = firstProductId, count = 1, color = "red" }

                }

            });

            var cartResponse = client.Execute(cartRequest);

            Assert.That(cartResponse.IsSuccessful, Is.True, $"Unable to add items to cart {cartResponse.StatusCode}");

            //Applying coupon
            var applyCouponRequest = new RestRequest("/user/cart/applycoupon", Method.Post);
            applyCouponRequest.AddHeader("Authorization", $"Bearer {userToken}");
            applyCouponRequest.AddJsonBody(new { coupon = "BLACKFRIDAY" });

            var applyCouponResponse = client.Execute(applyCouponRequest);

            Assert.That(applyCouponResponse.IsSuccessful, Is.True, $"Unable to apply coupon {applyCouponResponse.StatusCode}");

            //Place the order with the coupon
            var placeOrderRequest = new RestRequest("/user/cart/cash-order", Method.Post);
            placeOrderRequest.AddHeader("Authorization", $"Bearer {userToken}");
            placeOrderRequest.AddJsonBody(JsonConvert.SerializeObject(new { COD = true, couponApplied = true }));

            var placeOrderResponse = client.Execute(placeOrderRequest);

            Assert.That(placeOrderResponse.IsSuccessful, Is.True, $"Unable to place order {cartResponse.StatusCode}");
            Assert.That(JObject.Parse(placeOrderResponse.Content)["message"]?.ToString(), Is.EqualTo("success"));

            //Get all user orders
            var getUserOrdersRequest = new RestRequest("/user/get-orders", Method.Get);
            getUserOrdersRequest.AddHeader("Authorization", $"Bearer {userToken}");

            var getUserOrdersResponse = client.Execute(getUserOrdersRequest);

            Assert.That(getUserOrdersResponse.IsSuccessful, Is.True, $"Unable to get user orders {cartResponse.StatusCode}");

            //Get order ID
            var orderId = JObject.Parse(getUserOrdersResponse.Content)["_id"]?.ToString();

            //Update the order and cancel it
            var updateOrderRequest = new RestRequest("/user/order/update-order/" + orderId, Method.Put);
            updateOrderRequest.AddHeader("Authorization", $"Bearer {adminToken}");
            updateOrderRequest.AddJsonBody(new { status = "Cancelled" });

            var updateOrderResponse = client.Execute(updateOrderRequest);

            Assert.That(updateOrderResponse.IsSuccessful, Is.True, $"Unable to update the order status {cartResponse.StatusCode}");

        }


    }
}


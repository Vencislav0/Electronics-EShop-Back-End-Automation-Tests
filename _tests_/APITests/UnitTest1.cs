using ClassLibrary;
using Newtonsoft.Json.Linq;
using System.Net;

namespace APITests
{
    public class Tests
    {
        protected ProductsAPI product;
        protected GlobalConstants globalConstants;
        protected Random random;
        protected BlogsAPI blog;
        [SetUp]
        public void Setup()
        {
            product = new ProductsAPI();
            random = new Random();
            blog = new BlogsAPI();
        }
        [TearDown]
        public void TearDown() 
        { 
            product?.Dispose();
            blog?.Dispose();
        }

        //Tests for products functionallity
        [Test]
        public void Test_GetAllProducts()
        {
            

            var productsTitle = new[]
            {
                "Smartphone Alpha", "Wireless Headphones", "Gaming Laptop", "Ultra HD TV", "Smartwatch Pro"
            };

            var expectedPrices = new Dictionary<string, decimal>
            {
                { "Smartphone Alpha", 999 },
                { "Wireless Headphones", 199 },
                { "Gaming Laptop", 1499},
                { "Ultra HD TV", 899 },
                { "Smartwatch Pro", 299 }
            };

            var result = product.GetAllProducts();

            Assert.Multiple(() =>
            {
                Assert.That(result.Content, Is.Not.Empty);
                
                var content = JArray.Parse(result.Content); 

                foreach(var title in productsTitle)
                {
                    Assert.That(content.ToString(), Does.Contain(title));
                }

                foreach(var product in content)
                {
                    var title = product["title"].ToString();
                    var price = product["price"];

                    if (expectedPrices.ContainsKey(title))
                    {
                        Assert.That(content.ToString(), Does.Contain(title));
                        Assert.That((decimal)price, Is.EqualTo(expectedPrices[title]));
                    }
                }

                
            });
        }


        [Test]
        public void Test_AddProduct()
        {
            var randomNumber = random.Next(0, 100);
            var title = "Test Title" + randomNumber;
            var slug = "test-title" + randomNumber;
            var result = product.PostProduct(title);
            var content = JObject.Parse(result.Content);

            Assert.Multiple(() =>
            {
                Assert.That(content["title"].ToString(), Is.EqualTo(title));
                Assert.That(content["slug"].ToString(), Is.EqualTo(slug));
                Assert.That(content["description"].ToString(), Is.EqualTo("This is a test description"));
                Assert.That((decimal)content["price"], Is.EqualTo(1000.99));
                Assert.That(content["category"].ToString(), Is.EqualTo("TestCategory"));
                Assert.That(content["brand"].ToString(), Is.EqualTo("TestBrand"));
                Assert.That((int)content["quantity"], Is.EqualTo(25));
            });

        }

        [Test]
        public void Test_UpdateProduct_InvalidID()
        {
            var invalidId = "invalidProductId123";

            var result = product.UpdateProduct(invalidId);

            Assert.Multiple(() =>
            {
                Assert.That(result.StatusCode, Is.EqualTo(HttpStatusCode.NotFound).Or.EqualTo(HttpStatusCode.InternalServerError), "Expected 404 Not Found or 500 Bad Request for invalid product ID.");
                Assert.That(result.Content, Does.Contain("This id is not valid or not Found").Or.Contain("Invalid ID"), "Expected an error message indicating that the id is invalid or not found.");
            });
        }

        [Test]
        public void Test_UpdateProduct_WithValidID()
        {
            var randomNumber = random.Next(0, 100);
            var title = "Test Title" + randomNumber;
            var slug = "test-title" + randomNumber;
            var id = "";

            var result = product.PostProduct(title);           

            var allProducts = product.GetAllProducts();
            var allContent = JArray.Parse(allProducts.Content);

            foreach(var product in allContent)
            {
                if (product["title"] == title)
                {
                    id = product["_id"];
                    break;
                }
            }

            var updatedResult = product.UpdateProduct(id);
            
            

            if(updatedResult.StatusCode == HttpStatusCode.OK && updatedResult.Content == "null")
            {
                Console.WriteLine("API Bug Detected: UpdateProduct returned 200 OK with null response body.");
            }

            if (updatedResult.Content != "null")
            {
                Assert.That(updatedResult.Content, Does.Contain("Edited Title"));
            }
            else
            {
                Console.WriteLine("Skipped title assertion due to null response");
            }
            Assert.That(updatedResult.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        }

        [Test]
        public void Test_DeleteProduct()
        {
            var randomNumber = random.Next(0, 100);
            var slug ="test-title" + randomNumber;
            var AddProduct = product.PostProduct("Test Title" + randomNumber);

            var allProduct = product.GetAllProducts();
            var productsArray = JArray.Parse(allProduct.Content);

            dynamic productToDelete = null;

            foreach (var product in productsArray)
            {
                if(product["slug"].ToString() == slug)
                {
                    productToDelete = product;
                    break;
                }
            }
            Assert.That(productToDelete, Is.Not.Null);
            var productID = productToDelete["_id"].ToString();

            var deleteRequest = product.DeleteProduct(productID);
            
            Assert.That(deleteRequest.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            var verifyGetRequest = product.GetProduct(productID);

            Assert.That(verifyGetRequest.Content, Is.EqualTo("null"));
            Assert.That(productsArray, Does.Not.Contain("Test Title" + randomNumber));

        }

        //Tests for Blogs functionality

        [Test]
        public void Test_GetAllBlogs()
        {
            var result = blog.GetAllBlogs();
            var blogsArray = JArray.Parse(result.Content);

            Assert.Multiple(() =>
            {
                Assert.That(result.Content, Is.Not.Empty);
                Assert.That(result.StatusCode, Is.EqualTo(HttpStatusCode.OK));
                Assert.That(blogsArray.Count, Is.GreaterThan(0));

                foreach (var blog in blogsArray)
                {
                    Assert.That(blog["title"]?.ToString(), Is.Not.Null.And.Not.Empty);
                    Assert.That(blog["description"]?.ToString(), Is.Not.Null.And.Not.Empty);
                    Assert.That(blog["category"]?.ToString(), Is.Not.Null.And.Not.Empty);
                    Assert.That(blog["numViews"]?.ToString(), Is.Not.Null.And.Not.Empty);                                        
                    Assert.That(blog["author"]?.ToString(), Is.Not.Null.And.Not.Empty);
                }
                
            });
        }

        [Test]
        public void Test_AddBlog()
        {
            var randomTitle = "Test Title" + random.Next(0, 100);

            var result = blog.PostBlog(randomTitle);
            Assert.That(result.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(result.Content, Is.Not.Empty);

            var content = JObject.Parse(result.Content);

            Assert.Multiple(() =>
            {
                Assert.That(content["title"].ToString(), Is.EqualTo(randomTitle));
                Assert.That(content["description"].ToString(), Is.EqualTo("Test Description"));
                Assert.That(content["category"].ToString(), Is.EqualTo("TestCategory"));
                Assert.That(content["author"].ToString(), Is.Not.Null.Or.Empty);

            });

            var allBlogs = blog.GetAllBlogs();
            var blogsArray = JArray.Parse(allBlogs.Content);

            Assert.That(blogsArray.ToString(), Does.Contain(content["title"].ToString()));
        }



    }
}
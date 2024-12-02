using ClassLibrary;
using Newtonsoft.Json.Linq;

namespace APITests
{
    public class Tests
    {
        protected ProductsAPI product;
        protected GlobalConstants globalConstants;
        protected Random random;
        [SetUp]
        public void Setup()
        {
            product = new ProductsAPI();
            random = new Random();
        }
        [TearDown]
        public void TearDown() 
        { 
            product?.Dispose();
        }

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




    }
}
using ClassLibrary;
using Newtonsoft.Json.Linq;

namespace APITests
{
    public class Tests
    {
        protected ProductsAPI product;
        protected GlobalConstants globalConstants;
        [SetUp]
        public void Setup()
        {
            product = new ProductsAPI();
        }
        [TearDown]
        public void TearDown() 
        { 
            product?.Dispose();
        }

        [Test]
        public void Test1()
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
    }
}
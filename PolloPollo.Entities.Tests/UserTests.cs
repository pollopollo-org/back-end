using System.Collections.Generic;
using Xunit;

namespace PolloPollo.Entities.Tests
{
    public class UserTests
    {
        [Fact]
        public void Products_is_HashSet_of_Product()
        {
            var user = new User();

            Assert.IsType<HashSet<Product>>(user.Products);
        }

        [Fact]
        public void Applications_is_HashSet_of_Application()
        {
            var user = new User();

            Assert.IsType<HashSet<Application>>(user.Applications);
        }
    }
}

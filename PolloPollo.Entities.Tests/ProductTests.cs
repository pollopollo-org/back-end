using System.Collections.Generic;
using Xunit;

namespace PolloPollo.Entities.Tests
{
    public class ProductTests
    {
        [Fact]
        public void Applications_is_HashSet_of_Application()
        {
            var product = new Product();

            Assert.IsType<HashSet<Application>>(product.Applications);
        }
    }
}

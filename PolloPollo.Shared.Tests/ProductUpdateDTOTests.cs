using System.ComponentModel.DataAnnotations;
using System.Linq;
using Xunit;

namespace PolloPollo.Shared.Tests
{
    public class ProductUpdateDTOTests
    {
        [Fact]
        public void Price_has_RequiredAttribute()
        {
            var propertyInfo = typeof(ProductUpdateDTO).GetProperty("Available");

            var attribute = propertyInfo.GetCustomAttributes(false).Select(a => a.GetType());

            Assert.Contains(typeof(RequiredAttribute), attribute);
        }
    }
}

using PolloPollo.Shared.DTO;
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

        [Fact]
        public void Id_has_RequiredAttribute()
        {
            var propertyInfo = typeof(ProductUpdateDTO).GetProperty("Id");

            var attribute = propertyInfo.GetCustomAttributes(false).Select(a => a.GetType());

            Assert.Contains(typeof(RequiredAttribute), attribute);
        }

        [Fact]
        public void UserId_has_RequiredAttribute()
        {
            var propertyInfo = typeof(ProductUpdateDTO).GetProperty("UserId");

            var attribute = propertyInfo.GetCustomAttributes(false).Select(a => a.GetType());

            Assert.Contains(typeof(RequiredAttribute), attribute);
        }
    }
}

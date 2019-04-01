using System.ComponentModel.DataAnnotations;
using System.Linq;
using Xunit;

namespace PolloPollo.Shared.Tests
{
    public class ProductCreateUpdateDTOTests
    {
        [Fact]
        public void Title_has_RequiredAttribute()
        {
            var propertyInfo = typeof(ProductCreateUpdateDTO).GetProperty("Title");

            var attribute = propertyInfo.GetCustomAttributes(false).Select(a => a.GetType());

            Assert.Contains(typeof(RequiredAttribute), attribute);
        }

        [Fact]
        public void Title_has_MaximumLength_255()
        {
            var propertyInfo = typeof(ProductCreateUpdateDTO).GetProperty("Title");
            var maximumLength = 255;

            var attributeData = propertyInfo.GetCustomAttributesData();

            Assert.Equal(maximumLength, attributeData[0].ConstructorArguments[0].Value);
        }

        [Fact]
        public void UserId_has_RequiredAttribute()
        {
            var propertyInfo = typeof(ProductCreateUpdateDTO).GetProperty("UserId");

            var attribute = propertyInfo.GetCustomAttributes(false).Select(a => a.GetType());

            Assert.Contains(typeof(RequiredAttribute), attribute);
        }

        [Fact]
        public void Price_has_RequiredAttribute()
        {
            var propertyInfo = typeof(ProductCreateUpdateDTO).GetProperty("Price");

            var attribute = propertyInfo.GetCustomAttributes(false).Select(a => a.GetType());

            Assert.Contains(typeof(RequiredAttribute), attribute);
        }
        
        [Fact]
        public void Available_has_RequiredAttribute()
        {
            var propertyInfo = typeof(ProductCreateUpdateDTO).GetProperty("Available");

            var attribute = propertyInfo.GetCustomAttributes(false).Select(a => a.GetType());

            Assert.Contains(typeof(RequiredAttribute), attribute);
        }
    }
}

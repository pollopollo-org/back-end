using System.ComponentModel.DataAnnotations;
using System.Linq;
using Xunit;

namespace PolloPollo.Shared.Tests
{
    public class ProductCreateDTOTests
    {
        [Fact]
        public void Title_has_RequiredAttribute()
        {
            var propertyInfo = typeof(ProductCreateDTO).GetProperty("Title");

            var attribute = propertyInfo.GetCustomAttributes(false).Select(a => a.GetType());

            Assert.Contains(typeof(RequiredAttribute), attribute);
        }

        [Fact]
        public void Title_has_RegularExpression()
        {
            var propertyInfo = typeof(ProductCreateDTO).GetProperty("Title");

            var attributeData = propertyInfo.GetCustomAttributesData();

            Assert.Equal(@"\S+", attributeData[0].ConstructorArguments[0].Value);
        }

        [Fact]
        public void Title_has_MaximumLength_255()
        {
            var propertyInfo = typeof(ProductCreateDTO).GetProperty("Title");
            var maximumLength = 255;

            var attributeData = propertyInfo.GetCustomAttributesData();

            Assert.Equal(maximumLength, attributeData[1].ConstructorArguments[0].Value);
        }

        [Fact]
        public void ProducerId_has_RequiredAttribute()
        {
            var propertyInfo = typeof(ProductCreateDTO).GetProperty("ProducerId");

            var attribute = propertyInfo.GetCustomAttributes(false).Select(a => a.GetType());

            Assert.Contains(typeof(RequiredAttribute), attribute);
        }

        [Fact]
        public void Price_has_RequiredAttribute()
        {
            var propertyInfo = typeof(ProductCreateDTO).GetProperty("Price");

            var attribute = propertyInfo.GetCustomAttributes(false).Select(a => a.GetType());

            Assert.Contains(typeof(RequiredAttribute), attribute);
        }
        
        [Fact]
        public void Available_has_RequiredAttribute()
        {
            var propertyInfo = typeof(ProductCreateDTO).GetProperty("Available");

            var attribute = propertyInfo.GetCustomAttributes(false).Select(a => a.GetType());

            Assert.Contains(typeof(RequiredAttribute), attribute);
        }
    }
}

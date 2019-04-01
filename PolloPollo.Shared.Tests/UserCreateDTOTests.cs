using System.ComponentModel.DataAnnotations;
using System.Linq;
using Xunit;

namespace PolloPollo.Shared.Tests
{
    public class UserCreateDTOTests
    {
        [Fact]
        public void FirstName_has_RequiredAttribute()
        {
            var propertyInfo = typeof(UserCreateDTO).GetProperty("FirstName");

            var attribute = propertyInfo.GetCustomAttributes(false).Select(a => a.GetType());

            Assert.Contains(typeof(RequiredAttribute), attribute);
        }

        [Fact]
        public void FirstName_has_MaximumLength_255()
        {
            var propertyInfo = typeof(UserCreateDTO).GetProperty("FirstName");
            var maximumLength = 255;

            var attributeData = propertyInfo.GetCustomAttributesData();

            Assert.Equal(maximumLength, attributeData[0].ConstructorArguments[0].Value);
        }

        [Fact]
        public void Surname_has_RequiredAttribute()
        {
            var propertyInfo = typeof(UserCreateDTO).GetProperty("SurName");

            var attribute = propertyInfo.GetCustomAttributes(false).Select(a => a.GetType());

            Assert.Contains(typeof(RequiredAttribute), attribute);
        }

        [Fact]
        public void Email_has_RequiredAttribute()
        {
            var propertyInfo = typeof(UserCreateDTO).GetProperty("Email");

            var attribute = propertyInfo.GetCustomAttributes(false).Select(a => a.GetType());

            Assert.Contains(typeof(RequiredAttribute), attribute);
        }

        [Fact]
        public void Email_has_EmailAddressAttribute()
        {
            var propertyInfo = typeof(UserCreateDTO).GetProperty("Email");

            var attribute = propertyInfo.GetCustomAttributes(false).Select(a => a.GetType());

            Assert.Contains(typeof(EmailAddressAttribute), attribute);
        }

        [Fact]
        public void Email_has_MaximumLength_191()
        {
            // InnoDB keys can only be 767 bytes.
            // This limits the unique key of Email to a length of 191 to stay under 767 bytes.

            var propertyInfo = typeof(UserCreateDTO).GetProperty("Email");
            var maximumLength = 191;

            var attributeData = propertyInfo.GetCustomAttributesData();

            Assert.Equal(maximumLength, attributeData[1].ConstructorArguments[0].Value);
        }

        [Fact]
        public void Country_has_RequiredAttribute()
        {
            var propertyInfo = typeof(UserCreateDTO).GetProperty("Country");

            var attribute = propertyInfo.GetCustomAttributes(false).Select(a => a.GetType());

            Assert.Contains(typeof(RequiredAttribute), attribute);
        }

        [Fact]
        public void Country_has_RegularExpressionAttribute()
        {
            var propertyInfo = typeof(UserCreateDTO).GetProperty("Country");

            var attribute = propertyInfo.GetCustomAttributes(false).Select(a => a.GetType());

            Assert.Contains(typeof(RegularExpressionAttribute), attribute);
        }

        [Fact]
        public void Country_has_RegularExpression()
        {
            var propertyInfo = typeof(UserCreateDTO).GetProperty("Country");

            var attributeData = propertyInfo.GetCustomAttributesData();

            Assert.Equal(@"[^0-9]+", attributeData[0].ConstructorArguments[0].Value);
        }

        [Fact]
        public void Country_has_MaximumLength_255()
        {
            var propertyInfo = typeof(UserCreateDTO).GetProperty("Country");
            var maximumLength = 255;

            var attributeData = propertyInfo.GetCustomAttributesData();

            Assert.Equal(maximumLength, attributeData[1].ConstructorArguments[0].Value);
        }

        [Fact]
        public void Password_has_RequiredAttribute()
        {
            var propertyInfo = typeof(UserCreateDTO).GetProperty("Password");

            var attribute = propertyInfo.GetCustomAttributes(false).Select(a => a.GetType());

            Assert.Contains(typeof(RequiredAttribute), attribute);
        }

        [Fact]
        public void Password_has_MaximumLength_255()
        {
            var propertyInfo = typeof(UserCreateDTO).GetProperty("Password");
            var maximumLength = 255;

            var attributeData = propertyInfo.GetCustomAttributesData();

            Assert.Equal(maximumLength, attributeData[0].ConstructorArguments[0].Value);
        }

        [Fact]
        public void Password_has_MinLength_8()
        {
            var propertyInfo = typeof(UserCreateDTO).GetProperty("Password");
            var minLength = 8;

            var attributeData = propertyInfo.GetCustomAttributesData();

            Assert.Equal(minLength, attributeData[1].ConstructorArguments[0].Value);
        }

        [Fact]
        public void UserRole_has_RequiredAttribute()
        {
            var propertyInfo = typeof(UserCreateDTO).GetProperty("UserRole");

            var attribute = propertyInfo.GetCustomAttributes(false).Select(a => a.GetType());

            Assert.Contains(typeof(RequiredAttribute), attribute);
        }
    }
}

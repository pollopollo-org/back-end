using PolloPollo.Shared.DTO;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Xunit;

namespace PolloPollo.Shared.Tests
{
    public class UserUpdateDTOTests
    {
        [Fact]
        public void FirstName_has_RequiredAttribute()
        {
            var propertyInfo = typeof(UserUpdateDTO).GetProperty("FirstName");

            var attribute = propertyInfo.GetCustomAttributes(false).Select(a => a.GetType());

            Assert.Contains(typeof(RequiredAttribute), attribute);
        }

        [Fact]
        public void FirstName_has_MaximumLength_255()
        {
            var propertyInfo = typeof(UserUpdateDTO).GetProperty("FirstName");
            var maximumLength = 255;

            var attributeData = propertyInfo.GetCustomAttributesData();

            Assert.Equal(maximumLength, attributeData[0].ConstructorArguments[0].Value);
        }

        [Fact]
        public void SurName_has_RequiredAttribute()
        {
            var propertyInfo = typeof(UserUpdateDTO).GetProperty("SurName");

            var attribute = propertyInfo.GetCustomAttributes(false).Select(a => a.GetType());

            Assert.Contains(typeof(RequiredAttribute), attribute);
        }

        [Fact]
        public void SurName_has_MaximumLength_255()
        {
            var propertyInfo = typeof(UserUpdateDTO).GetProperty("SurName");
            var maximumLength = 255;

            var attributeData = propertyInfo.GetCustomAttributesData();

            Assert.Equal(maximumLength, attributeData[0].ConstructorArguments[0].Value);
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
            var propertyInfo = typeof(UserUpdateDTO).GetProperty("Email");

            var attribute = propertyInfo.GetCustomAttributes(false).Select(a => a.GetType());

            Assert.Contains(typeof(EmailAddressAttribute), attribute);
        }

        [Fact]
        public void Country_has_RequiredAttribute()
        {
            var propertyInfo = typeof(UserUpdateDTO).GetProperty("Country");

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
            var propertyInfo = typeof(UserUpdateDTO).GetProperty("Country");

            var attributeData = propertyInfo.GetCustomAttributesData();

            Assert.Equal(@"[^0-9]+", attributeData[0].ConstructorArguments[0].Value);
        }

        [Fact]
        public void Password_has_RequiredAttribute()
        {
            var propertyInfo = typeof(UserUpdateDTO).GetProperty("Password");

            var attribute = propertyInfo.GetCustomAttributes(false).Select(a => a.GetType());

            Assert.Contains(typeof(RequiredAttribute), attribute);
        }

        [Fact]
        public void Password_has_MaximumLength_255()
        {
            var propertyInfo = typeof(UserUpdateDTO).GetProperty("Password");
            var maximumLength = 255;

            var attributeData = propertyInfo.GetCustomAttributesData();

            Assert.Equal(maximumLength, attributeData[0].ConstructorArguments[0].Value);
        }

        [Fact]
        public void Password_has_MinLength_8()
        {
            var propertyInfo = typeof(UserUpdateDTO).GetProperty("Password");
            var minLength = 8;

            var attributeData = propertyInfo.GetCustomAttributesData();

            Assert.Equal(minLength, attributeData[1].ConstructorArguments[0].Value);
        }

        [Fact]
        public void NewPassword_has_MaximumLength_255()
        {
            var propertyInfo = typeof(UserUpdateDTO).GetProperty("NewPassword");
            var maximumLength = 255;

            var attributeData = propertyInfo.GetCustomAttributesData();

            Assert.Equal(maximumLength, attributeData[0].ConstructorArguments[0].Value);
        }

        [Fact]
        public void UserRole_has_RequiredAttribute()
        {
            var propertyInfo = typeof(UserUpdateDTO).GetProperty("UserRole");

            var attribute = propertyInfo.GetCustomAttributes(false).Select(a => a.GetType());

            Assert.Contains(typeof(RequiredAttribute), attribute);
        }

    }
}

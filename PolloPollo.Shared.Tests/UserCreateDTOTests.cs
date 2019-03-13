using System;
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
        public void FirstName_has_RegularExpression()
        {
            var propertyInfo = typeof(UserCreateDTO).GetProperty("FirstName");

            var attributeData = propertyInfo.GetCustomAttributesData();

            Assert.Equal(@"\S+", attributeData[0].ConstructorArguments[0].Value);
        }

        [Fact]
        public void FirstName_has_MinimumLength()
        {
            var propertyInfo = typeof(UserCreateDTO).GetProperty("FirstName");
            var minimumLength = 1;

            var attributeData = propertyInfo.GetCustomAttributesData();

            Assert.Equal(minimumLength, attributeData[1].NamedArguments[0].TypedValue.Value);
        }

        [Fact]
        public void SurName_has_RequiredAttribute()
        {
            var propertyInfo = typeof(UserCreateDTO).GetProperty("SurName");

            var attribute = propertyInfo.GetCustomAttributes(false).Select(a => a.GetType());

            Assert.Contains(typeof(RequiredAttribute), attribute);
        }

        [Fact]
        public void SurName_has_RegularExpression()
        {
            var propertyInfo = typeof(UserCreateDTO).GetProperty("SurName");

            var attributeData = propertyInfo.GetCustomAttributesData();

            Assert.Equal(@"\S+", attributeData[0].ConstructorArguments[0].Value);
        }

        [Fact]
        public void SurName_has_MinimumLength()
        {
            var propertyInfo = typeof(UserCreateDTO).GetProperty("SurName");
            var minimumLength = 1;

            var attributeData = propertyInfo.GetCustomAttributesData();

            Assert.Equal(minimumLength, attributeData[1].NamedArguments[0].TypedValue.Value);
        }

        [Fact]
        public void Email_has_RequiredAttribute()
        {
            var propertyInfo = typeof(UserCreateDTO).GetProperty("Email");

            var attribute = propertyInfo.GetCustomAttributes(false).Select(a => a.GetType());

            Assert.Contains(typeof(RequiredAttribute), attribute);
        }

        [Fact]
        public void Email_has_RegularExpressionAttribute()
        {
            var propertyInfo = typeof(UserCreateDTO).GetProperty("Email");

            var attribute = propertyInfo.GetCustomAttributes(false).Select(a => a.GetType());

            Assert.Contains(typeof(RegularExpressionAttribute), attribute);
        }

        [Fact]
        public void Email_has_RegularExpression()
        {
            var propertyInfo = typeof(UserCreateDTO).GetProperty("Email");

            var attributeData = propertyInfo.GetCustomAttributesData();

            Assert.Equal(@".+[@].+[.].+", attributeData[0].ConstructorArguments[0].Value);
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
        public void Password_has_RequiredAttribute()
        {
            var propertyInfo = typeof(UserCreateDTO).GetProperty("Password");

            var attribute = propertyInfo.GetCustomAttributes(false).Select(a => a.GetType());

            Assert.Contains(typeof(RequiredAttribute), attribute);
        }
    }
}

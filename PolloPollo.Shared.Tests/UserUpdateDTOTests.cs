﻿using System.ComponentModel.DataAnnotations;
using System.Linq;
using Xunit;

namespace PolloPollo.Shared.Tests
{
    public class UserUpdateDTOTests
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
        public void Surname_has_RequiredAttribute()
        {
            var propertyInfo = typeof(UserCreateDTO).GetProperty("SurName");

            var attribute = propertyInfo.GetCustomAttributes(false).Select(a => a.GetType());

            Assert.Contains(typeof(RequiredAttribute), attribute);
        }

        [Fact]
        public void Surname_has_RegularExpression()
        {
            var propertyInfo = typeof(UserCreateDTO).GetProperty("SurName");

            var attributeData = propertyInfo.GetCustomAttributesData();

            Assert.Equal(@"\S+", attributeData[0].ConstructorArguments[0].Value);
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
using System;
using Xunit;

namespace PolloPollo.Services.Utils.Tests
{
    public class PasswordHasherTests
    {
        [Fact]
        public void HashPassword_given_valid_email_and_password_returns_hashed_password()
        {
            var email = "test@test";
            var password = "12345678";

            var hashedPassword = PasswordHasher.HashPassword(email, password);

            Assert.NotNull(hashedPassword);
        }

        [Fact]
        public void HashPassword_given_null_password_throws_ArgumentNullException()
        {
            var email = "test@test";
            var password = default(string);
            Assert.Throws<ArgumentNullException>(() => PasswordHasher.HashPassword(email, password));
        }

        [Fact]
        public void VerifyPassword_given_valid_email_and_password_returns_True()
        {
            var email = "test@test";
            var password = PasswordHasher.HashPassword(email, "12345678");
            var plainPassword = "12345678";

            var verifiedPassword = PasswordHasher.VerifyPassword(email, password, plainPassword);

            Assert.True(verifiedPassword);
        }

        [Fact]
        public void VerifyPassword_given_invalid_password_returns_False()
        {
            var email = "test@test";
            var password = PasswordHasher.HashPassword(email, "12345678");
            var plainPassword = "87654321";

            var verifiedPassword = PasswordHasher.VerifyPassword(email, password, plainPassword);

            Assert.False(verifiedPassword);
        }

        [Fact]
        public void VerifiyPassword_given_null_password_throws_ArgumentNullException()
        {
            var email = "test@test";
            var password = PasswordHasher.HashPassword(email, "12345678");
            var plainPassword = default(string);
            Assert.Throws<ArgumentNullException>(() => PasswordHasher.VerifyPassword(email, password, plainPassword));
        }
    }
}

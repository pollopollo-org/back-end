
using System.Security.Claims;

namespace PolloPollo.Web
{
    /// <summary>
    /// Contains a colleciton of utilities that'll be used across all Controllers
    /// </summary>
    public class Utils
    {
        /// <summary>
        /// Internal helper that extracts the email of the user accessing the API
        /// </summary>
        public static string GetAssociatedUserEmail(ClaimsPrincipal User)
        {
            // Only when running tests will User be null, hence we can use this to return
            // a mail used for testing.
            if (User == null)
            {
                return "non_existing_user@itu.dk";
            }

            return User.Identity.Name;
        }
    }
}

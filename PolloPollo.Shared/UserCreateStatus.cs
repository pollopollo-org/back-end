
namespace PolloPollo.Shared
{
    public enum UserCreateStatus
    {
        SUCCESS,
        MISSING_EMAIL,
        MISSING_PASSWORD,
        PASSWORD_TOO_SHORT,
        EMAIL_TAKEN,
        UNKNOWN_FAILURE
    }
}
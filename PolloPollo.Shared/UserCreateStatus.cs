
namespace PolloPollo.Shared
{
    public enum UserCreateStatus
    {
        SUCCESS,
        MISSING_NAME,
        MISSING_EMAIL,
        MISSING_PASSWORD,
        MISSING_COUNTRY,
        PASSWORD_TOO_SHORT,
        EMAIL_TAKEN,
        INVALID_ROLE,
        NULL_INPUT,
        UNKNOWN_FAILURE
    }
}
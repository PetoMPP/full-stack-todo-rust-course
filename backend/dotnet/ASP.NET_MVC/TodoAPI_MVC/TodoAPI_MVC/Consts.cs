namespace TodoAPI_MVC
{
    public class Consts
    {
        // Formats
        public const string DateFormat = "yyyy-MM-dd hh:mm:ss.fff K";

        // Environment variables
        public const string DatabaseModeEnvName = "DB_MODE";
        public const string DatabaseUserEnvName = "DB_USER";
        public const string DatabasePasswordEnvName = "DB_PASSWORD";
        public const string JwtSecretEnvName = "JWT_SECRET";

        // Command line arguments
        public const string JwtSecretArgName = "--jwt-secret";
    }
}

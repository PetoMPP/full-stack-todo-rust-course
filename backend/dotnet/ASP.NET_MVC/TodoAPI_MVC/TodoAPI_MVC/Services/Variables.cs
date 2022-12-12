namespace TodoAPI_MVC.Services
{

    internal class Variables : IVariables
    {
        private const string DatabaseModeName = "DB_MODE";
        private const string DatabaseUserName = "DB_USER";
        private const string DatabasePasswordName = "DB_PASSWORD";
        private const string JwtSecretName = "JWT_SECRET";
        private const string ApiAdminUserName = "API_ADMIN_USER";
        private const string ApiAdminPasswordName = "API_ADMIN_PASSWORD";

        public string DatabaseMode 
        { 
            get => GetEnvironmentVariable(DatabaseModeName);
            set => SetEnvironmentVariable(DatabaseModeName, value); 
        }

        public string DatabaseUser
        {
            get => GetEnvironmentVariable(DatabaseUserName);
            set => SetEnvironmentVariable(DatabaseUserName, value);
        }

        public string DatabasePassword
        {
            get => GetEnvironmentVariable(DatabasePasswordName);
            set => SetEnvironmentVariable(DatabasePasswordName, value);
        }

        public string JwtSecret
        {
            get => GetEnvironmentVariable(JwtSecretName);
            set => SetEnvironmentVariable(JwtSecretName, value);
        }

        public string ApiAdminUser
        {
            get => GetEnvironmentVariable(ApiAdminUserName);
            set => SetEnvironmentVariable(ApiAdminUserName, value);
        }

        public string ApiAdminPassword
        {
            get => GetEnvironmentVariable(ApiAdminPasswordName);
            set => SetEnvironmentVariable(ApiAdminPasswordName, value);
        }

        private static string GetEnvironmentVariable(string variableName)
        {
            return Environment.GetEnvironmentVariable(variableName)
                ?? throw new InvalidOperationException($"{variableName} is unset!");
        }

        private static void SetEnvironmentVariable(string variableName, string value)
        {
            Environment.SetEnvironmentVariable(variableName, value);
        }
    }
}

namespace TodoAPI_MVC.Services
{
    public interface IVariables
    {
        public string DatabaseMode { get; set; }
        public string DatabaseUser { get; set; }
        public string DatabasePassword { get; set; }
        public string JwtSecret { get; set; }
        public string ApiAdminUser { get; set; }
        public string ApiAdminPassword { get; set; }
    }
}

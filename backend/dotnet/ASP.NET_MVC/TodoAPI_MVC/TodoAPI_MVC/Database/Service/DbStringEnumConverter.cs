namespace TodoAPI_MVC.Database.Service
{
    public class DbStringEnumConverter : DbValueConverter
    {
        public override Func<Type, bool> CanConvert => t => t.IsEnum;
        public override Func<object?, string> Convert => v => $"'{v}'";
    }
}

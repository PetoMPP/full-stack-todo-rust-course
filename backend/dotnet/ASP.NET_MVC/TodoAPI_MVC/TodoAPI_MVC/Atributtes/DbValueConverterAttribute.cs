using TodoAPI_MVC.Database.Service;

namespace TodoAPI_MVC.Atributtes
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Enum)]
    public class DbValueConverterAttribute : Attribute
    {
        public Type ConverterType { get; }

        public DbValueConverterAttribute(Type converterType)
        {
            if (!converterType.IsAssignableTo(typeof(DbValueConverter)))
                throw new InvalidOperationException();

            ConverterType = converterType;
        }
    }
}

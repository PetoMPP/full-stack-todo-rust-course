namespace TodoAPI_MVC.Atributtes
{
    /// <summary>
    /// Property with this attribute specify that a column represented it should be
    /// filled with default values (defined in database) on insert/update operations.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class DbDefaultAttribute : Attribute
    {
    }
}
namespace TodoAPI_MVC.Authentication
{
    [Flags]
    public enum EndpointAccess
    {
        None =       0 << 0,
        TasksOwned = 1 << 0,
        TasksAll =   1 << 1
    }
}
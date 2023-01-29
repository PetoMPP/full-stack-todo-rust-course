using TodoAPI_MVC.Models;

namespace TodoAPI_MVC.Database
{
    public static class DatabaseResults
    {
        public static IDatabaseResult<T> Ok<T>(T data)
        {
            return new DatabaseResult<T>()
            {
                Code = StatusCode.Ok,
                Data = data
            };
        }

        public static IDatabaseResult<T> Error<T>(params string[]? error)
        {
            return new DatabaseResult<T>
            {
                Code = StatusCode.Error,
                ErrorData = error
            };
        }

        public static IDatabaseResult Ok()
        {
            return new DatabaseResult
            {
                Code = StatusCode.Ok
            };
        }

        public static IDatabaseResult Error(params string[]? error)
        {
            return new DatabaseResult
            {
                Code = StatusCode.Error,
                ErrorData = error
            };
        }
    }
}

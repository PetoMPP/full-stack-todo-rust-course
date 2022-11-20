using TodoAPI_MVC.Models;

namespace TodoAPI_MVC.Database
{
    public interface IDatabaseResult<T>
    {
        StatusCode Code { get; init; }
        T? Data { get; init; }
        string[]? ErrorData { get; init; }
        bool IsOk => Code == StatusCode.Ok;
    }

    public interface IDatabaseResult
    {
        StatusCode Code { get; init; }
        string[]? ErrorData { get; init; }
        bool IsOk => Code == StatusCode.Ok;
    }

    public class DatabaseResult<T> : IDatabaseResult<T>
    {
        public StatusCode Code { get; init; }
        public T? Data { get; init; }
        public string[]? ErrorData { get; init; }

        public DatabaseResult() { }

        public DatabaseResult(StatusCode code, T? data, string[]? errorData)
        {
            Code = code;
            Data = data;
            ErrorData = errorData;
        }
    }

    public class DatabaseResult : IDatabaseResult
    {
        public StatusCode Code { get; init; }
        public string[]? ErrorData { get; init; }

        public DatabaseResult() { }

        public DatabaseResult(StatusCode code, string[]? errorData)
        {
            Code = code;
            ErrorData = errorData;
        }
    }
}

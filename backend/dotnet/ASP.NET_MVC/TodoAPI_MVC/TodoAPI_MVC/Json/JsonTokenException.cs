using System.Text.Json;

namespace TodoAPI_MVC.Json
{
    public class JsonTokenException : Exception
    {
        public JsonTokenException()
        {
        }

        public JsonTokenException(
            JsonTokenType actual,
            JsonTokenType expected)
            : base(message: GetMessage(actual, expected))
        {
        }

        public JsonTokenException(
            JsonTokenType actual,
            JsonTokenType expected,
            Exception? inner)
            : base(message: GetMessage(actual, expected),
                  innerException: inner)
        {
        }

        public JsonTokenException(
            JsonTokenType actual,
            Exception? inner = null,
            params JsonTokenType[] expected)
            : base(message: GetMessage(actual, expected),
                  innerException: inner)
        {
        }

        private static string GetMessage(JsonTokenType actual, JsonTokenType expected)
            => $"Unexpected token! Expected {expected}, got {actual}";

        private static string GetMessage(JsonTokenType actual, JsonTokenType[] expected)
            => $"Unexpected token! Expected {string.Join(" or ", expected)}, got {actual}";
    }
}

using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace TodoAPI_MVC.Middleware
{
    public class WrapResponsesMiddleware
    {
        private readonly RequestDelegate _next;

        public WrapResponsesMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            var jsonSerializerOptions = httpContext.RequestServices.GetService<JsonSerializerOptions>();
            var originBody = httpContext.Response.Body;
            httpContext.Request.EnableBuffering();
            try
            {
                var memStream = new MemoryStream();
                httpContext.Response.Body = memStream;

                await _next(httpContext).ConfigureAwait(false);

                memStream.Position = 0;
                var responseBody = new StreamReader(memStream).ReadToEnd();

                if (string.IsNullOrEmpty(responseBody))
                    return;

                var response = TryDeserialize<object>(responseBody, out var obj, jsonSerializerOptions)
                    ? obj
                    : responseBody;

                //Custom logic to modify response
                if (httpContext.Response.StatusCode > 299 || httpContext.Response.StatusCode < 200)
                    response = new { error = response };
                else
                    response = new { data = response };

                var memoryStreamModified = new MemoryStream();
                var sw = new StreamWriter(memoryStreamModified);
                sw.Write(JsonSerializer.Serialize(response, jsonSerializerOptions));
                sw.Flush();
                memoryStreamModified.Position = 0;

                await memoryStreamModified.CopyToAsync(originBody).ConfigureAwait(false);
            }
            finally
            {
                httpContext.Response.Body = originBody;
            }
        }

        private static bool TryDeserialize<T>(
            string responseBody,
            [NotNullWhen(true)] out T? obj,
            JsonSerializerOptions? jsonSerializerOptions = null)
        {
            try
            {
                obj = JsonSerializer.Deserialize<T>(responseBody, jsonSerializerOptions)!;

                return true;
            }
            catch (Exception)
            {
                obj = default;
                return false;
            }
        }
    }

    public static class WrapResponsesMiddlewareExtensions
    {
        public static IApplicationBuilder UseResponseWrapper(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<WrapResponsesMiddleware>();
        }
    }
}

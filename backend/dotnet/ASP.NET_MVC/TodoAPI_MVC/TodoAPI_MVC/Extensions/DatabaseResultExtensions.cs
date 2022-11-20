using Microsoft.AspNetCore.Mvc;
using TodoAPI_MVC.Database;

namespace TodoAPI_MVC.Extensions
{
    public static class DatabaseResultExtensions
    {
        public static IActionResult ToIActionResult<T>(this IDatabaseResult<T> dbResult, ControllerBase controller)
        {
            return dbResult.Code switch
            {
                Models.StatusCode.Ok => controller.Ok(dbResult.Data),
                Models.StatusCode.Error => controller.BadRequest(
                    string.Join(Environment.NewLine, dbResult.ErrorData ?? Array.Empty<string>())),
                _ => controller.StatusCode(500),
            };
        }

        public static IActionResult ToIActionResult(this IDatabaseResult dbResult, ControllerBase controller)
        {
            return dbResult.Code switch
            {
                Models.StatusCode.Ok => controller.Ok(),
                Models.StatusCode.Error => controller.BadRequest(
                    string.Join(Environment.NewLine, dbResult.ErrorData ?? Array.Empty<string>())),
                _ => controller.StatusCode(500),
            };
        }
    }
}

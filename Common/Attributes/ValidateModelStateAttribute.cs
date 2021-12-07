using Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Linq;

namespace Common.Attributes
{
    public class ValidateModelStateAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (!context.ModelState.IsValid)
            {
                var errors = context.ModelState.Values.Where(v => v.Errors.Any())
                        .SelectMany(v => v.Errors)
                        .Select(v => v.ErrorMessage)
                        .ToList();

                var responseObj = new Error
                {
                    Code = "BadRequest",
                    Description = string.Join(",", errors),
                    StatusCode = StatusCodes.Status400BadRequest
                };

                context.Result = new JsonResult(responseObj)
                {
                    StatusCode = StatusCodes.Status400BadRequest
                };
            }
        }
    }
}
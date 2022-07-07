using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
namespace Task1.Authorization
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public class AuthorizeAttribute : Attribute, IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            if (context.ActionDescriptor.EndpointMetadata.OfType<AllowWithoutAuthorizationAttribute>().Any())
            {
                return;
            }

            // authorization
            var user = (User?)context.HttpContext.Items["User"];
            var createdAt = (string?) context.HttpContext.Items["IAT"];
            if (user == null)
            {
                context.Result = new JsonResult(new { message = "Unauthorized" }) { StatusCode = StatusCodes.Status401Unauthorized };
            }
            if (user != null && user.Confirmed == false)
            {
                context.Result = new JsonResult(new { message = "Your account is not confirmed" }) { StatusCode = StatusCodes.Status401Unauthorized };
            }
            if (createdAt != null && user != null)
            {
                if (user.LastPasswordChange.CompareTo(createdAt) > 0)
                {
                    context.Result = new JsonResult(new { message = "The token you are using is not valid! (old token)" }) { StatusCode = StatusCodes.Status401Unauthorized };
                }
            }
            
        }
    }
}

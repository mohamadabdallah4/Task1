using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Globalization;

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
                DateTime passwordChangeDate;
                DateTime createdAtDate;
                DateTime.TryParse(user.LastPasswordChange, out passwordChangeDate);
                DateTime.TryParse(createdAt.Substring(0, createdAt.Length - 7), out createdAtDate);
                //Console.WriteLine(user.LastPasswordChange);
                //Console.WriteLine(createdAt.Substring(0,createdAt.Length-7));
                if (passwordChangeDate > createdAtDate)
                {
                    Console.WriteLine("Entered here");
                    context.Result = new JsonResult(new { message = "The token you are using is not valid! (old token)" }) { StatusCode = StatusCodes.Status401Unauthorized };
                }
            }
            
        }
    }
}

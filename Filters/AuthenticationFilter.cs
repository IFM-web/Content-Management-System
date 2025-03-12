using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Http;

namespace ContentManagementSystem.Filters
{
    public class AuthenticationFilter : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var userId = context.HttpContext.Session.GetString("UserId");
            
            if (string.IsNullOrEmpty(userId))
            {
                var controller = (Controller)context.Controller;
                context.Result = controller.RedirectToAction("Login", "Users");
            }
        }
    }
} 
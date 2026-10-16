using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace RaceDay.API.Clean.Middleware
{
    // Checks whether a user is logged in and has the required role.
    public class SessionAuthorizeAttribute : Attribute, IAsyncActionFilter
    {
        // Stores the role required to access an endpoint.
        private readonly string? _requiredRole;

        // Allows an endpoint to require a specific role.
        public SessionAuthorizeAttribute(string? requiredRole = null)
        {
            _requiredRole = requiredRole;
        }

        // Runs before the protected controller action executes.
        public async Task OnActionExecutionAsync(
            ActionExecutingContext context,
            ActionExecutionDelegate next)
        {
            // Retrieve the logged-in user's ID from the session.
            int? userId = context.HttpContext.Session.GetInt32("UserID");

            // Reject requests without an authenticated session.
            if (userId == null)
            {
                context.Result = new UnauthorizedObjectResult(new
                {
                    message = "Please log in to access this resource."
                });

                return;
            }

            // Check the user's role if the endpoint requires one.
            if (_requiredRole != null)
            {
                // Retrieve the user's role from the session.
                string? userRole =
                    context.HttpContext.Session.GetString("UserRole");

                // Reject users whose role does not match the requirement.
                if (!string.Equals(
                    userRole,
                    _requiredRole,
                    StringComparison.OrdinalIgnoreCase))
                {
                    // Return 403 because the user lacks the required permission.
                    context.Result = new ObjectResult(new
                    {
                        message = "You do not have permission to access this resource."
                    })
                    {
                        StatusCode = StatusCodes.Status403Forbidden
                    };

                    return;
                }
            }

            // Continue to the controller action if access is allowed.
            await next();
        }
    }
}
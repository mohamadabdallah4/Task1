namespace Task1.Authorization
{
    public class JwtMiddleware
    {
        private RequestDelegate next;
        public JwtMiddleware(RequestDelegate nextDelegate)
        {
            next = nextDelegate;
        }
        public async Task Invoke(HttpContext context, IUserService userService, IJwtUtils jwtUtils)
        {
            var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
            int? userId = jwtUtils.GetIdFromToken(token);
            if (userId != null)
            {
                // attach user to context on successful jwt validation
                context.Items["User"] = userService.GetById(userId.Value);
            }

            await next(context);
        }
    }
}

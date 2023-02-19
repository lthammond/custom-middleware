namespace Web.Middleware
{
    public class Authenticator
    {
        private readonly RequestDelegate _next;

        public Authenticator(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (context.Request.Query["username"] == "user1" && context.Request.Query["password"] == "password1")
            {
                context.Request.HttpContext.Items.Add("userDetails", "blabla");
                await _next(context);
            }
            else
            {
                await context.Response.WriteAsync("Authentication failed.");
            }
        }
    }
}

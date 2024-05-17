using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;

namespace BlazorAut.Services
{
    public class SetAuthTokenMiddleware
    {
        private readonly RequestDelegate _next;

        public SetAuthTokenMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (context.Items.ContainsKey("authToken"))
            {
                var authToken = context.Items["authToken"] as string;
                if (!string.IsNullOrEmpty(authToken))
                {
                    context.Response.Cookies.Append("authToken", authToken, new CookieOptions { HttpOnly = true, Expires = DateTime.Now.AddDays(1) });
                    Console.WriteLine("JWT token appended to cookies by middleware.");
                }
                else
                {
                    Console.WriteLine("JWT token in context items is null or empty.");
                }
            }

            await _next(context);
        }
    }
}

using System.IdentityModel.Tokens.Jwt;

namespace UserApiProject.Middleware
{
    public class TokenValidationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<TokenValidationMiddleware> _logger;

        public TokenValidationMiddleware(RequestDelegate next, ILogger<TokenValidationMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (!context.Request.Headers.TryGetValue("Authorization", out var tokenHeader))
            {
                _logger.LogWarning("Authorization header is missing.");
                await ReturnUnauthorizedResponse(context, "Authorization header is missing.");
                return;
            }

            var token = tokenHeader.ToString().Replace("Bearer ", string.Empty);

            if (string.IsNullOrEmpty(token) || !IsTokenValid(token))
            {
                _logger.LogWarning("Invalid or missing token.");
                await ReturnUnauthorizedResponse(context, "Invalid or missing token.");
                return;
            }

            await _next(context);
        }

        private bool IsTokenValid(string token)
        {
            try
            {
                var jwtToken = new JwtSecurityTokenHandler().ReadToken(token) as JwtSecurityToken;

                // Add your token validation logic here (e.g., check issuer, audience, expiration, etc.)
                if (jwtToken == null || jwtToken.ValidTo < System.DateTime.UtcNow)
                {
                    return false;
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        private async Task ReturnUnauthorizedResponse(HttpContext context, string message)
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            context.Response.ContentType = "application/json";

            var errorResponse = new
            {
                error = message
            };

            await context.Response.WriteAsJsonAsync(errorResponse);
        }
    }
}
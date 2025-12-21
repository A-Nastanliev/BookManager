using DataLayer.Enums;
using Microsoft.AspNetCore.Mvc;
using ServiceLayer;

namespace ServiceLayer.Controllers
{
    public abstract class BaseController : ControllerBase
    {
        protected readonly IConfiguration _configuration;

        public BaseController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        protected int? GetUserIdFromHeader(string authHeader)
        {
            if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
                return null;

            string token = authHeader.Substring("Bearer ".Length).Trim();
            return JwtTokenHelper.GetUserIdFromToken(token, _configuration["Jwt:Secret"], _configuration["Jwt:Issuer"], _configuration["Jwt:Audience"]);
        }

        protected UserRole? GetRoleFromHeader(string authHeader)
        {
            if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
                return null;

            string token = authHeader.Substring("Bearer ".Length).Trim();
            return JwtTokenHelper.GetRoleFromToken(token, _configuration["Jwt:Secret"], _configuration["Jwt:Issuer"], _configuration["Jwt:Audience"]);
        }

        protected (int? userId, UserRole? role) GetUserIdAndRoleFromHeader(string authHeader)
        {
            return (GetUserIdFromHeader(authHeader), GetRoleFromHeader(authHeader));
        }
    }
}

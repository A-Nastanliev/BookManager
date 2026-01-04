using DataLayer.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServiceLayer;
using System.Security.Claims;

namespace ServiceLayer.Controllers
{
    [ApiController]
    [Authorize]
    public abstract class BaseController : ControllerBase
    {
        protected int UserId =>
            int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        protected UserRole UserRole =>
            Enum.Parse<UserRole>(User.FindFirstValue(ClaimTypes.Role)!);

    }
}

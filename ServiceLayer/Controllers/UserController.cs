using Microsoft.AspNetCore.Mvc;
using BusinessLayer.Repositories;
namespace ServiceLayer.Controllers
{
    [ApiController]
    [Route("api/users")]
    public class UserController : BaseController
    {
        private readonly UserRepository _userRepository;

        private readonly UserRestrictionRepository _restrictionRepository;

        public UserController(IConfiguration configuration, UserRepository userRepository, UserRestrictionRepository restrictionRepository) : base(configuration)
        {
            _userRepository = userRepository;
            _restrictionRepository = restrictionRepository;
        }
    }

}

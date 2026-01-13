using BusinessLayer.Repositories;
using DataLayer.Enums;
using DataLayer.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServiceLayer.Dto;
using ServiceLayer.Dto.User;
using ServiceLayer.Mappers;
using ServiceLayer.Services;

namespace ServiceLayer.Controllers
{
	[Route("api/users")]
	public class UserController : BaseController
	{
		private readonly IConfiguration _configuration;

		private readonly IImageStorageService _imageStorageService;

		private readonly UserRepository _userRepository;

		private readonly UserRestrictionRepository _restrictionRepository;

		public UserController(UserRepository userRepository, UserRestrictionRepository restrictionRepository, 
			IConfiguration configuration, IImageStorageService imageStorageService)
		{
			_userRepository = userRepository;
			_restrictionRepository = restrictionRepository;
			_configuration = configuration;
			_imageStorageService = imageStorageService;
		}

		[AllowAnonymous]
		[HttpPost("signup")]
		public async Task<IActionResult> SignUp([FromForm] SignUpRequest req)
		{
            string profilePicturePath = null;

            try
            {
                profilePicturePath = await _imageStorageService.SaveImageAsync(req.ProfilePicture, "profile-pictures");
            }
            catch (InvalidOperationException e)
            {
                return BadRequest(e.Message);
            }

            var newUser = new User
            {
                Username = req.Username,
                EmailAddress = req.EmailAddress,
                PasswordHash = req.Password,
                ProfilePicture = profilePicturePath
            };

            var created = await _userRepository.SignUpAsync(newUser);
            if (!created)
                return BadRequest("Could not create user.");

            return StatusCode(201);
        }

		[AllowAnonymous]
		[HttpPost("email_login")]
		public async Task<IActionResult> EmailLogin([FromBody] EmailLoginRequest req)
		{
			var user = await _userRepository.EmailPasswordLoginAsync(req.Email, req.Password);
			if (user == null) return Unauthorized();

			var secret = _configuration["Jwt:Secret"];
			var issuer = _configuration["Jwt:Issuer"];
			var audience = _configuration["Jwt:Audience"];
            var baseUrl = _configuration["App:BaseUrl"];
            var token = JwtTokenHelper.GenerateToken(user.Id, user.Role, secret, issuer, audience);

			return Ok(new
			{
				Token = token,
				User = user.ToDto(baseUrl)
			});
		}


		[HttpGet("me")]
		public async Task<IActionResult> Me()
		{
            var baseUrl = _configuration["App:BaseUrl"];
            var user = await _userRepository.ReadAsync(UserId);
			return Ok(new { User = user.ToDto(baseUrl) });
		}

		[Authorize(Roles = "Admin")]
		[HttpGet("next-users")]
		public async Task<IActionResult> GetNextUsers([FromQuery] LoadNextDto load)
		{
			var users = await _userRepository.ReadNextAsync(load.Count, load.AlreadyLoaded);
			return Ok(users.Select(u => u.ToPublicDto()));
		}

		[HttpPut("me")]
		public async Task<IActionResult> UpdateProfile([FromBody] UserDto req)
		{
			var userToUpdate = new User
			{
				Id = UserId,
				Username = req.Username,
				EmailAddress = req.EmailAddress,
			};

			var success = await _userRepository.UpdateAsync(userToUpdate);
			if (!success)
				return NotFound();

			return NoContent();
		}

        [HttpPut("me/profile-picture")]
        public async Task<IActionResult> UpdateProfilePicture([FromForm] IFormFile picture)
        {
            if (picture == null || picture.Length == 0)
                return BadRequest("No image provided.");

            var user = await _userRepository.ReadAsync(UserId);
            if (user == null)
                return NotFound();

            _imageStorageService.DeleteImage(user.ProfilePicture);

            string newPath;
            try
            {
                newPath = await _imageStorageService.SaveImageAsync(picture, "profile-pictures");
            }
            catch (InvalidOperationException e)
            {
                return BadRequest(e.Message);
            }

            var success = await _userRepository.UpdateProfilePictureAsync(new User { Id = UserId, ProfilePicture = newPath });

            if (!success)
                return NotFound();

            return Ok(new { ProfilePicture = newPath });
        }


        [HttpPut("me/password")]
		public async Task<IActionResult> UpdatePassword([FromBody] UpdatePasswordRequest req)
		{
			var success = await _userRepository.UpdatePasswordAsync(UserId, req.NewPassword, req.CurrentPassword);
			if (!success)
				return BadRequest();

			return NoContent();
		}


		[HttpDelete("{id}")]
		public async Task<IActionResult> DeleteUser(int id)
		{
			if (UserRole != UserRole.Admin && UserId != id)
				return Forbid();

            var user = await _userRepository.ReadAsync(id);
            if (user == null)
                return NotFound();

            _imageStorageService.DeleteImage(user.ProfilePicture);

            var success = await _userRepository.DeleteAsync(new User { Id = id });
			if (!success) return NotFound();

			return NoContent();
		}

		[Authorize(Roles = "Admin")]
		[HttpPost("{id}/comment-restriction")]
		public async Task<IActionResult> CreateCommentRestriction(int id, [FromBody] RestrictionDto req)
		{
			var restriction = new UserRestriction
			{
				UserId = id,
				Reason = req.Reason,
				EndDate = req.EndDate
			};

			var success = await _restrictionRepository.CreateAsync(restriction);
			if (!success)
				return BadRequest("Could not create comment restriction.");

			return StatusCode(201);
		}

		[Authorize(Roles = "Admin")]
		[HttpGet("comment-restrictions/active")]
		public async Task<IActionResult> GetActiveCommentRestrictions([FromQuery] LoadNextDto load)
		{
			var restrictions = await _restrictionRepository.ReadNextAsync(load.Count, load.AlreadyLoaded);

			return Ok(restrictions.Select(r => r.ToDto()));
		}

		[Authorize(Roles = "Admin")]
		[HttpPut("comment-restriction/{restrictionId}/end")]
		public async Task<IActionResult> EndCommentRestriction(int restrictionId)
		{

			var success = await _restrictionRepository.UpdateAsync(new UserRestriction { Id = restrictionId });
			if (!success)
				return NotFound();

			return NoContent();
		}
	}

}

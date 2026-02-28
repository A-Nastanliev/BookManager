using DataLayer.Models;
using ServiceLayer.Dto.User;

namespace ServiceLayer.Mappers
{
    public static class UserMapper
    {
        public static UserDto ToDto(this User user, string baseUrl)
        {
            return new UserDto(
                user.Id,
                user.EmailAddress,
                user.Username,
                user.ProfilePicture == null ? null : $"{baseUrl}/{user.ProfilePicture}",
                user.Role,
                user.CreatedAt,
                user.CurrentRestriction?.ToDto(baseUrl, true)
            );
        }

        public static UserDto ToPublicDto(this User user, string baseUrl) 
        {
            return new UserDto(user.Username, user.ProfilePicture == null ? null: $"{baseUrl}/{user.ProfilePicture}",
                user.Id, user.CurrentRestriction?.ToDto(baseUrl, false));
        }

        public static RestrictionDto ToDto(this UserRestriction userRestriction,string baseUrl ,bool mapUser = true)
        {
            return new RestrictionDto(userRestriction.Id, userRestriction.Reason, userRestriction.StartDate,userRestriction.EndDate, userRestriction.User?.ToPublicDto(baseUrl) );
        }
    }
}

using DataLayer.Models;
using ServiceLayer.Dto.User;

namespace ServiceLayer.Mappers
{
    public static class UserMapper
    {
        public static UserDto ToDto(this User user)
        {
            return new UserDto(user.EmailAddress, user.Username, user.ProfilePicture, user.Role, user.CreatedAt, user.CurrentRestriction?.ToDto());
        }

        public static UserDto ToPublicDto(this User user) 
        {
            return new UserDto(user.Username, user.ProfilePicture, user.CurrentRestriction?.ToDto());
        }

        public static RestrictionDto ToDto(this UserRestriction userRestriction)
        {
            return new RestrictionDto(userRestriction.Id, userRestriction.Reason, userRestriction.StartDate,userRestriction.EndDate, userRestriction.User?.ToPublicDto() );
        }
    }
}

using DataLayer.Enums;
using System.Security.Cryptography;

namespace BusinessLayer.Repositories
{
	public class UserRepository: AbstractRepository<User, int>
	{
		public UserRepository(BookManagerContext context) : base(context) { }
		public override async Task<User> ReadAsync(int id)
		{
			return await _context.Users
				.Include(u => u.UserRestrictions)
				.FirstOrDefaultAsync(g => g.Id == id);
		}

        public override async Task<List<User>> ReadNextAsync(int count, int loaded)
        {
            return await _context.Users
               .OrderByDescending(u => u.CreatedAt)
               .Skip(loaded)
               .Take(count)
               .ToListAsync();
        }

        public async Task<bool> SignUpAsync(User user)
        {
            user.PasswordHash = HashPassword(user.PasswordHash);
            user.Role = UserRole.User;
            user.CreatedAt = DateTime.UtcNow;

            await _context.Users.AddAsync(user);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<User> EmailPasswordLoginAsync(string email, string password)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.EmailAddress == email);
            if (user == null || !VerifyPassword(password, user.PasswordHash))
                return null;

            return await ReadAsync(user.Id);
        }

        public async override Task<bool> UpdateAsync(User obj)
        {
            var user = await _context.Users.FindAsync(obj.Id);
            if (user == null)
                return false;

            user.Username = obj.Username;
            user.EmailAddress = obj.EmailAddress;

            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> UpdateProfilePictureAsync(User obj)
        {
            var user = await _context.Users.FindAsync(obj.Id);
            if (user == null)
                return false;

            user.ProfilePicture = obj.ProfilePicture;

            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> UpdatePasswordAsync(int userToUpdateId, string newPassword, string currentPassword)
        {
            var user = await _context.Users.FindAsync(userToUpdateId);
            if (user == null || !VerifyPassword(currentPassword, user.PasswordHash))
                return false;

            user.PasswordHash = HashPassword(newPassword);

            return await _context.SaveChangesAsync() > 0;
        }

        private string HashPassword(string password)
        {
            byte[] salt = RandomNumberGenerator.GetBytes(16);

            byte[] hash = Rfc2898DeriveBytes.Pbkdf2(
                password,
                salt,
                100_000,
                HashAlgorithmName.SHA256,
                32);

            return $"{Convert.ToHexString(salt)}:{Convert.ToHexString(hash)}";
        }

        private bool VerifyPassword(string password, string stored)
        {
            var parts = stored.Split(':');
            byte[] salt = Convert.FromHexString(parts[0]);
            byte[] storedHash = Convert.FromHexString(parts[1]);

            byte[] hash = Rfc2898DeriveBytes.Pbkdf2(
                password,
                salt,
                100_000,
                HashAlgorithmName.SHA256,
                32);

            return CryptographicOperations.FixedTimeEquals(hash, storedHash);
        }
    }
}

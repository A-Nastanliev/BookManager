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

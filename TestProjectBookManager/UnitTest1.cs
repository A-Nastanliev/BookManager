using System;
using System.Linq;
using System.Threading.Tasks;
using BusinessLayer.Repositories;
using DataLayer;
using DataLayer.Enums;
using DataLayer.Models;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace Tests
{
	[TestFixture]
	public class UserRepositoryTests
	{
		private static BookManagerContext CreateContext()
		{
			var options = new DbContextOptionsBuilder<BookManagerContext>()
				.UseInMemoryDatabase(Guid.NewGuid().ToString())
				.Options;

			return new BookManagerContext(options);
		}

		private static User CreateUser(
			string username = "testuser",
			string email = "test@test.com",
			string password = "password")
		{
			return new User
			{
				Username = username,
				EmailAddress = email,
				PasswordHash = password,
				ProfilePicture = "pic.png"
			};
		}

		[Test]
		public async Task SignUpAsync_CreatesUser_WithHashedPassword()
		{
			using var context = CreateContext();
			var repo = new UserRepository(context);

			var user = CreateUser();

			var result = await repo.SignUpAsync(user);

			Assert.That(result, Is.True);
			Assert.That(context.Users.Count(), Is.EqualTo(1));
			Assert.That(user.Role, Is.EqualTo(UserRole.User));
			Assert.That(user.PasswordHash, Is.Not.EqualTo("password"));
		}

		[Test]
		public async Task EmailPasswordLoginAsync_ReturnsUser_WhenCredentialsAreValid()
		{
			using var context = CreateContext();
			var repo = new UserRepository(context);

			var user = CreateUser();
			await repo.SignUpAsync(user);

			var loggedIn = await repo.EmailPasswordLoginAsync(
				user.EmailAddress,
				"password");

			Assert.That(loggedIn, Is.Not.Null);
			Assert.That(loggedIn!.Username, Is.EqualTo(user.Username));
		}

		[Test]
		public async Task EmailPasswordLoginAsync_ReturnsNull_WhenPasswordIsInvalid()
		{
			using var context = CreateContext();
			var repo = new UserRepository(context);

			var user = CreateUser();
			await repo.SignUpAsync(user);

			var loggedIn = await repo.EmailPasswordLoginAsync(
				user.EmailAddress,
				"wrong");

			Assert.That(loggedIn, Is.Null);
		}

		[Test]
		public async Task UpdateAsync_UpdatesUserFields()
		{
			using var context = CreateContext();
			var repo = new UserRepository(context);

			var user = CreateUser();
			await repo.SignUpAsync(user);

			user.Username = "updated";
			user.EmailAddress = "updated@test.com";
			user.ProfilePicture = "newpic.png";

			var result = await repo.UpdateAsync(user);
			var updated = context.Users.Single();

			Assert.That(result, Is.True);
			Assert.That(updated.Username, Is.EqualTo("updated"));
			Assert.That(updated.EmailAddress, Is.EqualTo("updated@test.com"));
			Assert.That(updated.ProfilePicture, Is.EqualTo("newpic.png"));
		}

		[Test]
		public async Task UpdatePasswordAsync_ChangesPassword_WhenCurrentPasswordIsCorrect()
		{
			using var context = CreateContext();
			var repo = new UserRepository(context);

			var user = CreateUser();
			await repo.SignUpAsync(user);

			var result = await repo.UpdatePasswordAsync(
				user.Id,
				"newpassword",
				"password");

			Assert.That(result, Is.True);

			var login = await repo.EmailPasswordLoginAsync(
				user.EmailAddress,
				"newpassword");

			Assert.That(login, Is.Not.Null);
		}

		[Test]
		public async Task ReadAsync_IncludesUserRestrictions()
		{
			using var context = CreateContext();
			var repo = new UserRepository(context);

			var user = CreateUser();
			await repo.SignUpAsync(user);

			context.UserRestrictions.Add(new UserRestriction
			{
				UserId = user.Id,
				StartDate = DateTime.UtcNow.AddDays(-1),
				Reason = "Test restriction"
			});

			await context.SaveChangesAsync();

			var loaded = await repo.ReadAsync(user.Id);

			Assert.That(loaded, Is.Not.Null);
			Assert.That(loaded!.UserRestrictions, Has.Count.EqualTo(1));
		}

		[Test]
		public async Task ReadNextAsync_ReturnsUsersOrderedByCreatedAtDescending()
		{
			using var context = CreateContext();
			var repo = new UserRepository(context);

			var u1 = CreateUser("user1", "u1@test.com");
			var u2 = CreateUser("user2", "u2@test.com");

			await repo.SignUpAsync(u1);
			await Task.Delay(10);
			await repo.SignUpAsync(u2);

			var users = await repo.ReadNextAsync(10, 0);

			Assert.That(users, Has.Count.EqualTo(2));
			Assert.That(users.First().Username, Is.EqualTo("user2"));
		}
	}
}

using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Domain.Entities.Identity;
using Infrastructure.Interfaces;
using Infrastructure.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Web.Services
{
	public class IdentityService : IIdentityService
	{
		private readonly UserManager<User> _userManager;
		private readonly IHttpContextAccessor _contextAccesor;
		public ClaimsPrincipal User => _contextAccesor.HttpContext?.User;

		public IdentityService(UserManager<User> userManager, IHttpContextAccessor contextAccesor)
		{
			_userManager = userManager;
			_contextAccesor = contextAccesor;
		}

		public async Task<string> GetUserNameAsync(string userId)
		{
			var user = await _userManager.Users.FirstAsync(u => u.Id == int.Parse(userId));

			return user.UserName;
		}

		public async Task<(Result Result, string UserId)> CreateUserAsync(string userName, string password)
		{
			var user = new User { UserName = userName, Email = userName, };

			var result = await _userManager.CreateAsync(user, password);

			return (result.ToApplicationResult(), user.Id.ToString());
		}

		public async Task<bool> IsInRoleAsync(string userId, string role)
		{
			var user = _userManager.Users.SingleOrDefault(u => u.Id == int.Parse(userId));

			return user != null && await _userManager.IsInRoleAsync(user, role);
		}

		public async Task<Result> DeleteUserAsync(string userId)
		{
			var user = _userManager.Users.SingleOrDefault(u => u.Id == int.Parse(userId));

			return user != null ? await DeleteUserAsync(user) : Result.Success();
		}
		
		public async Task<Result> DeleteUserAsync(User user)
		{
			var result = await _userManager.DeleteAsync(user);

			return result.ToApplicationResult();
		}
	}

	public static class IdentityResultExtensions
	{
		public static Result ToApplicationResult(this IdentityResult result)
		{
			return result.Succeeded
				? Result.Success()
				: Result.Failure(result.Errors.Select(e => e.Description));
		}
	}
}

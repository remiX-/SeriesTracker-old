using SeriesTracker.Core;
using SeriesTracker.Models;
using System.Threading.Tasks;

namespace SeriesTracker
{
	public class UserMethods : IUserRepository
	{
		public async Task<LoginResult> UserLoginAsync(string Email, string Password)
		{
			return await AppGlobal.Db.UserLoginAsync(Email, Password);
		}

		public async Task<SeriesResult<User>> UserRegisterAsync(string Username, string Email, string Name, string Password)
		{
			return await AppGlobal.Db.UserRegisterAsync(Username, Email, Name, Password);
		}
	}
}

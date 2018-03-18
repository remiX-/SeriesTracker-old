using SeriesTracker.Core;
using SeriesTracker.Models;
using System.Threading.Tasks;

namespace SeriesTracker
{
	public interface IUserRepository
	{
		Task<LoginResult> UserLoginAsync(string Email, string Password);
		Task<SeriesResult<User>> UserRegisterAsync(string Username, string Email, string Name, string Password);
	}
}
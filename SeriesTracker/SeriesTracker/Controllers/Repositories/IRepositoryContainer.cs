//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

namespace SeriesTracker
{
	public interface IRepositoryContainer
	{
		IUserRepository UserRepository { get; }
		IShowRepository ShowRepository { get; }
	}
}

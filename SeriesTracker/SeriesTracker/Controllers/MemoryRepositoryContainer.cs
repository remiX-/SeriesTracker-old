using System;

namespace SeriesTracker
{
	class MemoryRepositoryContainer : IRepositoryContainer
	{
		public IUserRepository UserRepository
		{
			get { return new UserMethods(); }
		}

		public IShowRepository ShowRepository
		{
			get { return new ShowMethods(); }
		}
	}
}
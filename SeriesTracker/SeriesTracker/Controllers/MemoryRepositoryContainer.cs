using System;

namespace SeriesTracker
{
	class MemoryRepositoryContainer : IRepositoryContainer
	{
		public IShowRepository ShowRepository
		{
			get { return new ShowMethods(); }
		}

		public IUserRepository UserRepository
		{
			get { return new UserMethods(); }
		}
	}
}
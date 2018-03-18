namespace SeriesTracker
{
	public static class RepositorySession
	{
		private static IRepositoryContainer Repository;

		public static IRepositoryContainer GetRepository()
		{
			if (Repository == null)
				Repository = CreateRepository();

			return Repository;
		}

		private static IRepositoryContainer CreateRepository()
		{
			return new MemoryRepositoryContainer();
		}
	}
}

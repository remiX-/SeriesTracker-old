namespace SeriesTracker.Models
{
	public class TvdbAPI
	{
		public string Token { get; set; }

		public object Data { get; set; }
		public string Error { get; set; }

		public TvdbLinks Links { get; set; }
	}

	public class TvdbLinks
	{
		public string First { get; set; }
		public string Last { get; set; }
		public string Next { get; set; }
		public string Prev { get; set; }
	}

	public class TvdbUpdate
	{
		public int Id { get; set; }
		public int LastUpdated { get; set; }
	}
}

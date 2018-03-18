namespace SeriesTracker.Models
{
	public class Eztv
	{
		public string ID { get; set; }
		public string Href { get; set; }
		public string Name { get; set; }

		public Eztv(string id, string href, string name)
		{
			ID = id;
			Href = href;
			Name = name;
		}
	}
}

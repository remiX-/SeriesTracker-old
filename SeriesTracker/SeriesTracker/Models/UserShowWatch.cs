using Newtonsoft.Json;
using System;

namespace SeriesTracker.Models
{
	public class UserShowWatch
	{
		public int ID { get; set; }
		public int UserShowID { get; set; }
		public int SeasonNo { get; set; }
		public int EpisodeNo { get; set; }

		public UserShowWatch()
		{

		}
	}
}

using GalaSoft.MvvmLight;
using System;
using System.Linq;

namespace SeriesTracker.Models
{
	public sealed class ShowViewModel : ViewModelBase
	{
		public int Id { get; }
		public string SeriesName { get; }
		public string DisplayName { get; }
		public string Status { get; }
		public string AirTimeDisplay { get; }
		public string LatestEpisodeDisplay { get; }
		public string LatestEpisodeDateDisplay { get; }
		public string NextEpisodeDisplay { get; }
		public string NextEpisodeDateDisplay { get; }
		public string HowLong { get; }
		public string LocalSeriesPath { get; }

		public string LocalPicturePath { get; }

		string[] SearchTerms { get { return new[] { Id.ToString(), SeriesName, DisplayName, Status }; } }

		public ShowViewModel(Show model)
		{
			Id = model.Id;
			SeriesName = model.SeriesName;
			DisplayName = model.DisplayName;
			Status = model.Status;
			AirTimeDisplay = model.AirDayDisplay;
			//LatestEpisodeDisplay = model.LatestEpisode.FullEpisodeString;
			//LatestEpisodeDateDisplay = model.LatestEpisode.FullDateString;
			NextEpisodeDisplay = model.NextEpisodeDisplay;
			NextEpisodeDateDisplay = model.NextEpisodeDateDisplay;
			HowLong = model.HowLongDisplay;
			LocalSeriesPath = model.LocalSeriesPath;

			LocalPicturePath = model.LocalPicturePath;
		}

		public bool HasText(string filter)
		{
			var upper = filter.ToLower().Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries).Distinct();
			return upper.All(term => SearchTerms.Any(search => search.ToLower().Contains(term)));
		}

		//public bool HasText(string filter)
		//{
		//	filter = filter.ToLower();
		//	return SeriesName.ToLower().Contains(filter) || DisplayName.ToLower().Contains(filter);
		//}
	}
}

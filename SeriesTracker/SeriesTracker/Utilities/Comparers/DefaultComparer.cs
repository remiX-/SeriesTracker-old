using SeriesTracker.Models;
using System;
using System.Collections;
using System.ComponentModel;

namespace SeriesTracker.Utilities.Comparers
{
	public class DefaultComparer : IComparer
	{
		public string ColumnName { get; }
		public ListSortDirection Direction { get; }

		public DefaultComparer(string column, ListSortDirection dir)
		{
			ColumnName = column;
			Direction = dir;
		}

		public int Compare(object x, object y)
		{
			Show _x = (Show)x;
			Show _y = (Show)y;

			int r = 0;

			switch(ColumnName.ToLower())
			{
				case "tvdb id": r = CompareTvdbId(_x, _y); break;
				case "imdb id": r = CompareImdbId(_x, _y); break;
				case "name": r = CompareName(_x, _y); break;
				case "status": r = CompareStatus(_x, _y); break;
				case "network": r = CompareNetwork(_x, _y); break;
				case "run time": r = CompareRuntime(_x, _y); break;
				case "genre": r = CompareGenre(_x, _y); break;
				case "air day": r = CompareAirDay(_x, _y); break;
				case "latest episode": r = CompareLatestEpisode(_x, _y); break;
				case "latest aired": r = CompareLatestAired(_x, _y); break;
				case "next episode": r = CompareNextEpisode(_x, _y); break;
				case "next aired":
				case "how long": r = CompareNextAired(_x, _y); break;
				case "local folder": r = CompareLocalFolder(_x, _y); break;
				default: throw new Exception("Invalid column name " + ColumnName);
			}

			// 0 means same value, so then sort by DisplayName
			if (r == 0)
				r = string.Compare(_x.DisplayName, _y.DisplayName);

			// Check for descending
			if (Direction == ListSortDirection.Descending)
				r *= -1;

			return r;
		}

		private int CompareTvdbId(Show x, Show y)
		{
			return x.Id.CompareTo(y.Id);
		}

		private int CompareImdbId(Show x, Show y)
		{
			return string.Compare(x.ImdbId, y.ImdbId);
		}

		private int CompareName(Show x, Show y)
		{
			return string.Compare(x.DisplayName, y.DisplayName);
		}

		private int CompareStatus(Show x, Show y)
		{
			if (x.Status == y.Status)
				return 0;
			else
				return string.Compare(x.Status, y.Status);
		}

		private int CompareNetwork(Show x, Show y)
		{
			return string.Compare(x.Network, y.Network);
		}

		private int CompareRuntime(Show x, Show y)
		{
			return string.Compare(x.Runtime, y.Runtime);
		}

		private int CompareGenre(Show x, Show y)
		{
			return string.Compare(x.GenreDisplay, y.GenreDisplay);
		}

		private int CompareAirDay(Show x, Show y)
		{
			return string.Compare(x.AirDayDisplay, y.AirDayDisplay);
		}

		private int CompareLatestEpisode(Show x, Show y)
		{
			return string.Compare(x.LatestEpisode.FullEpisodeString, y.LatestEpisode.FullEpisodeString);
		}

		private int CompareLatestAired(Show x, Show y)
		{
			return DateTime.Compare(x.LatestEpisode.AirDate.Value.Date, y.LatestEpisode.AirDate.Value.Date);
		}

		private int CompareNextEpisode(Show x, Show y)
		{
			if (x.Status == "Continuing" && y.Status == "Continuing")
				return string.Compare(x.NextEpisode.FullEpisodeString, y.NextEpisode.FullEpisodeString);
			else if (x.Status != "Continuing" && y.Status != "Continuing")
				return string.Compare(x.Status, y.Status) * -1;
			else if (x.Status == "Continuing" && y.Status != "Continuing")
				return -1;
			else if (x.Status != "Continuing" && y.Status == "Continuing")
				return 1;
			else
				return 0;
		}

		private int CompareNextAired(Show x, Show y)
		{
			if (x.Status == "Continuing" && y.Status == "Continuing")
				return DateTime.Compare(x.NextEpisode.AirDate.Value, y.NextEpisode.AirDate.Value);
			else if (x.Status != "Continuing" && y.Status != "Continuing")
				return string.Compare(x.Status, y.Status) * -1;
			else if (x.Status == "Continuing" && y.Status != "Continuing")
				return -1;
			else if (x.Status != "Continuing" && y.Status == "Continuing")
				return 1;
			else
				return 0;
		}

		private int CompareLocalFolder(Show x, Show y)
		{
			if (!string.IsNullOrWhiteSpace(x.LocalSeriesPath) && string.IsNullOrWhiteSpace(y.LocalSeriesPath))
				return -1;
			else if (string.IsNullOrWhiteSpace(x.LocalSeriesPath) && !string.IsNullOrWhiteSpace(y.LocalSeriesPath))
				return 1;
			else
				return string.Compare(x.LocalSeriesPath, y.LocalSeriesPath);
		}
	}
}
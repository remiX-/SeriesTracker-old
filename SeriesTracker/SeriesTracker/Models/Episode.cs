using Newtonsoft.Json;
using Prism.Mvvm;
using SeriesTracker.Core;
using System;
using System.IO;

namespace SeriesTracker.Models
{
	public class Episode : BindableBase
	{
		#region Variables
		#region Tvdb Variables
		private int id;
		private string overview;
		private string episodeName;
		private string firstAired;

		private int? absoluteNumber;
		private int? airedSeason;
		private int? airedEpisodeNumber;
		private int? airedSeasonID;

		private double? dvdSeason;
		private double? dvdEpisodeNumber;

		private int lastUpdated;

		public int Id
		{
			get { return id; }
			set { id = value; }
		}
		public string Overview
		{
			get { return overview; }
			set { overview = value; }
		}
		public string EpisodeName
		{
			get { return episodeName; }
			set { episodeName = value; }
		}
		public string FirstAired
		{
			get { return firstAired; }
			set { firstAired = value; }
		}

		public int? AbsoluteNumber
		{
			get { return absoluteNumber; }
			set { absoluteNumber = value; }
		}
		public int? AiredSeason
		{
			get { return airedSeason; }
			set { airedSeason = value; }
		}
		public int? AiredEpisodeNumber
		{
			get { return airedEpisodeNumber; }
			set { airedEpisodeNumber = value; }
		}
		public int? AiredSeasonID
		{
			get { return airedSeasonID; }
			set { airedSeasonID = value; }
		}

		public double? DvdSeason
		{
			get { return dvdSeason; }
			set { dvdSeason = value; }
		}
		public double? DvdEpisodeNumber
		{
			get { return dvdEpisodeNumber; }
			set { dvdEpisodeNumber = value; }
		}

		public int LastUpdated
		{
			get { return lastUpdated; }
			set { lastUpdated = value; }
		}
		#endregion

		#region Fields
		private string imageText;
		private bool watched;
		#endregion

		#region Properties
		[JsonIgnore]
		public string FullEpisodeString { get; private set; }
		//{
		//	get
		//	{
		//		return string.Format("S{0:d2}E{1:d2}", AiredSeason, AiredEpisodeNumber);
		//	}
		//}

		[JsonIgnore]
		public string FullDateString { get; private set; }
		//{
		//	get
		//	{
		//		return CommonMethods.GetDateTimeAsFormattedString(AirDate);
		//	}
		//}

		[JsonIgnore]
		public string ImageText
		{
			get { return imageText; }
			set { SetProperty(ref imageText, value); RaisePropertyChanged("LocalImagePath"); }
		}

		[JsonIgnore]
		public bool Watched
		{
			get { return watched; }
			set { watched = value; }
		}
		#endregion

		#region Generated
		[JsonIgnore] public DateTime? AirDate { get; private set; }

		[JsonIgnore] public string OnlineImageUrl { get; private set; }
		[JsonIgnore] public string LocalImagePath { get; private set; }
		#endregion
		#endregion

		public Episode()
		{
			ImageText = "Loading ...";
		}

		public void DoWork(Show show)
		{
			FullEpisodeString = string.Format("S{0:d2}E{1:d2}", AiredSeason, AiredEpisodeNumber);

			OnlineImageUrl = Path.Combine(AppGlobal.posterURL, "episodes", show.Id.ToString(), Id.ToString() + ".jpg");
			LocalImagePath = Path.Combine(show.LocalImagesEpisodesPath, Id.ToString() + ".jpg");

			if (!string.IsNullOrEmpty(FirstAired))
			{
				// This occurs when an episode is confirmed to be aired
				// but it does not have an aired date yet
				AirDate = CommonMethods.GetDateTimeAsLocal(FirstAired, show.AirsTime);

				FullDateString = CommonMethods.GetDateTimeAsFormattedString(AirDate);
			}
			else
			{
				//AirDate = new DateTime(9999, 1, 1);
			}
		}

		public string TimeToEpisode()
		{
			if (!AirDate.HasValue)
				return "-1";

			TimeSpan time = AirDate.Value - DateTime.Now;

			int days = time.Days;
			int hours = (int)Math.Ceiling(time.TotalHours - (days * 24));

			if (days > 0)
				return string.Format("{0}d {1}h", days, hours);
			else
				return string.Format("{0}h", hours);
		}

		public override string ToString()
		{
			return FullEpisodeString;
		}
	}
}

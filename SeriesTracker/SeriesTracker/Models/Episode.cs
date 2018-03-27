using Newtonsoft.Json;
using Prism.Mvvm;
using SeriesTracker.Core;
using System;
using System.IO;
using System.Windows.Media.Imaging;

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
			get => id;
			set => id = value;
		}
		public string Overview
		{
			get => overview;
			set => overview = value;
		}
		public string EpisodeName
		{
			get => episodeName;
			set => episodeName = value;
		}
		public string FirstAired
		{
			get => firstAired;
			set => firstAired = value;
		}

		public int? AbsoluteNumber
		{
			get => absoluteNumber;
			set => absoluteNumber = value;
		}
		public int? AiredSeason
		{
			get => airedSeason;
			set => airedSeason = value;
		}
		public int? AiredEpisodeNumber
		{
			get => airedEpisodeNumber;
			set => airedEpisodeNumber = value;
		}
		public int? AiredSeasonID
		{
			get => airedSeasonID;
			set => airedSeasonID = value;
		}

		public double? DvdSeason
		{
			get => dvdSeason;
			set => dvdSeason = value;
		}
		public double? DvdEpisodeNumber
		{
			get => dvdEpisodeNumber;
			set => dvdEpisodeNumber = value;
		}

		public int LastUpdated
		{
			get => lastUpdated;
			set => lastUpdated = value;
		}
		#endregion

		#region Fields
		private string imageText;
		private bool watched;
		#endregion

		#region Properties
		[JsonIgnore] public string FullEpisodeString { get; private set; }

		[JsonIgnore] public string FullDateString { get; private set; }

		[JsonIgnore] 
		public string ImageText
		{
			get => imageText;
			set { SetProperty(ref imageText, value); /*RaisePropertyChanged("LocalImagePath");*/ }
		}

		[JsonIgnore]
		public bool Watched
		{
			get => watched;
			set => SetProperty(ref watched, value);
		}
		#endregion

		#region Generated
		[JsonIgnore] public DateTime? AirDate { get; private set; }

		[JsonIgnore] public string OnlineImageUrl { get; private set; }
		[JsonIgnore] public string LocalImagePath { get; private set; }
		[JsonIgnore] public BitmapImage LocalImage { get; private set; }
		#endregion
		#endregion

		public Episode()
		{
			ImageText = "Loading ...";

			LocalImage = new BitmapImage();
			LocalImage.BeginInit();
			LocalImage.CacheOption = BitmapCacheOption.OnLoad;
			LocalImage.UriSource = new Uri("pack://application:,,,/Resources/noimage.jpg");
			LocalImage.EndInit();
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

		public void RefreshImage()
		{
			if (!File.Exists(LocalImagePath)) return;

			LocalImage = new BitmapImage();
			LocalImage.BeginInit();
			LocalImage.CacheOption = BitmapCacheOption.OnLoad;
			LocalImage.UriSource = new Uri(LocalImagePath);
			LocalImage.EndInit();

			RaisePropertyChanged("LocalImage");
		}

		//public override string ToString()
		//{
		//	return FullEpisodeString;
		//}
	}
}
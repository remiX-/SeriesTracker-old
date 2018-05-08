using MaterialDesignThemes.Wpf;
using GalaSoft.MvvmLight;
using SeriesTracker.Models;
using SeriesTracker.Views;
using System.Collections.Generic;

namespace SeriesTracker.ViewModels
{
	internal class ViewShowViewModel : ViewModelBase
	{
		public Show MyShow { get; set; }

		public HamburgerMenuItem[] MenuItems { get; }

		#region Variables
		#region Fields
		private string myTitle;
		private string bannerPath;
		private string overview;
		private string network;
		private string genre;
		private string status;
		private string firstAired;
		private string latestEpisode;
		private string airTime;
		private string runtime;
		private string imdbId;
		private string imdbUrl;

		private List<Actor> showCast;
		private int castColumnCount;

		private List<Episode> episodes;

		private int episodeColumnCount;
		private int viewingSeason = 1;
		#endregion

		#region Properties
		public string MyTitle
		{
			get => myTitle;
			set => Set(ref myTitle, value);
		}
		public string BannerPath
		{
			get => bannerPath;
			set => Set(ref bannerPath, value);
		}
		public string Overview
		{
			get => overview;
			set => Set(ref overview, value);
		}
		public string Network
		{
			get => network;
			set => Set(ref network, value);
		}
		public string Genre
		{
			get => genre;
			set => Set(ref genre, value);
		}
		public string Status
		{
			get => status;
			set => Set(ref status, value);
		}
		public string FirstAired
		{
			get => firstAired;
			set => Set(ref firstAired, value);
		}
		public string LatestEpisode
		{
			get => latestEpisode;
			set => Set(ref latestEpisode, value);
		}
		public string AirTime
		{
			get => airTime;
			set => Set(ref airTime, value);
		}
		public string Runtime
		{
			get => runtime;
			set => Set(ref runtime, value);
		}
		public string ImdbId
		{
			get => imdbId;
			set => Set(ref imdbId, value);
		}
		public string ImdbUrl
		{
			get => imdbUrl;
			set => Set(ref imdbUrl, value);
		}

		public List<Actor> ShowCast
		{
			get => showCast;
			set => Set(ref showCast, value);
		}

		public int CastColumnCount
		{
			get => castColumnCount;
			set => Set(ref castColumnCount, value);
		}

		public List<Episode> Episodes
		{
			get => episodes;
			set => Set(ref episodes, value);
		}

		public int EpisodeColumnCount
		{
			get => episodeColumnCount;
			set => Set(ref episodeColumnCount, value);
		}

		public int ViewingSeason
		{
			get => viewingSeason;
			set => Set(ref viewingSeason, value);
		}
		#endregion
		#endregion

		public ViewShowViewModel()
		{
			MenuItems = new[]
			{
				new HamburgerMenuItem("Overview", "Overview", PackIconKind.InformationVariant, new Overview()),
				new HamburgerMenuItem("Seasons", "Seasons", PackIconKind.Itunes, new Seasons()),
				new HamburgerMenuItem("Gallery", "Gallery", PackIconKind.ImageAlbum)
			};

			EpisodeColumnCount = 2;
			ViewingSeason = 1;
		}

		public void RefreshBanner()
		{
			RaisePropertyChanged("BannerPath");
		}

		public void SetShow(Show show)
		{
			MyShow = show;

			MyTitle = "Viewing " + MyShow.DisplayName;

			BannerPath = MyShow.LocalBannerPath;

			Overview = MyShow.Overview;
			Network = MyShow.Network;
			Genre = MyShow.GenreDisplay;
			Status = MyShow.Status;
			FirstAired = MyShow.FirstAired;
			LatestEpisode = MyShow.LatestEpisode.FullEpisodeString;
			AirTime = MyShow.AirDayDisplay;
			Runtime = MyShow.Runtime + " minutes";
			ImdbId = MyShow.ImdbId;
			imdbUrl = MyShow.GetIMDbLink();

			ShowCast = MyShow.Actors;
		}
	}
}

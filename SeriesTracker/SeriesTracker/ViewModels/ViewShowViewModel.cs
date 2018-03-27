using MaterialDesignThemes.Wpf;
using Prism.Mvvm;
using SeriesTracker.Models;
using SeriesTracker.Views;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace SeriesTracker.ViewModels
{
	internal class ViewShowViewModel : BindableBase
	{
		public Show MyShow { get; set; }

		public HamburgerMenuItem[] MenuItems { get; }

		private static readonly ObservableCollection<ViewShowMenuItem> AppMenu = new ObservableCollection<ViewShowMenuItem>();
		public ObservableCollection<ViewShowMenuItem> Menu => AppMenu;

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
			set => SetProperty(ref myTitle, value);
		}
		public string BannerPath
		{
			get => bannerPath;
			set => SetProperty(ref bannerPath, value);
		}
		public string Overview
		{
			get => overview;
			set => SetProperty(ref overview, value);
		}
		public string Network
		{
			get => network;
			set => SetProperty(ref network, value);
		}
		public string Genre
		{
			get => genre;
			set => SetProperty(ref genre, value);
		}
		public string Status
		{
			get => status;
			set => SetProperty(ref status, value);
		}
		public string FirstAired
		{
			get => firstAired;
			set => SetProperty(ref firstAired, value);
		}
		public string LatestEpisode
		{
			get => latestEpisode;
			set => SetProperty(ref latestEpisode, value);
		}
		public string AirTime
		{
			get => airTime;
			set => SetProperty(ref airTime, value);
		}
		public string Runtime
		{
			get => runtime;
			set => SetProperty(ref runtime, value);
		}
		public string ImdbId
		{
			get => imdbId;
			set => SetProperty(ref imdbId, value);
		}
		public string ImdbUrl
		{
			get => imdbUrl;
			set => SetProperty(ref imdbUrl, value);
		}

		public List<Actor> ShowCast
		{
			get => showCast;
			set => SetProperty(ref showCast, value);
		}

		public int CastColumnCount
		{
			get => castColumnCount;
			set => SetProperty(ref castColumnCount, value);
		}

		public List<Episode> Episodes
		{
			get => episodes;
			set => SetProperty(ref episodes, value);
		}

		public int EpisodeColumnCount
		{
			get => episodeColumnCount;
			set => SetProperty(ref episodeColumnCount, value);
		}

		public int ViewingSeason
		{
			get => viewingSeason;
			set => SetProperty(ref viewingSeason, value);
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

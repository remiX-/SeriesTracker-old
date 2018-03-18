using MahApps.Metro.IconPacks;
using Prism.Mvvm;
using SeriesTracker.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace SeriesTracker.ViewModels
{
	internal class ViewShowViewModel : BindableBase
	{
		public Show MyShow { get; set; }
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
		#endregion

		#region Properties
		public string MyTitle
		{
			get { return myTitle; }
			set { SetProperty(ref myTitle, value); }
		}
		public string BannerPath
		{
			get { return bannerPath; }
			set { SetProperty(ref bannerPath, value); }
		}
		public string Overview
		{
			get { return overview; }
			set { SetProperty(ref overview, value); }
		}
		public string Network
		{
			get { return network; }
			set { SetProperty(ref network, value); }
		}
		public string Genre
		{
			get { return genre; }
			set { SetProperty(ref genre, value); }
		}
		public string Status
		{
			get { return status; }
			set { SetProperty(ref status, value); }
		}
		public string FirstAired
		{
			get { return firstAired; }
			set { SetProperty(ref firstAired, value); }
		}
		public string LatestEpisode
		{
			get { return latestEpisode; }
			set { SetProperty(ref latestEpisode, value); }
		}
		public string AirTime
		{
			get { return airTime; }
			set { SetProperty(ref airTime, value); }
		}
		public string Runtime
		{
			get { return runtime; }
			set { SetProperty(ref runtime, value); }
		}
		public string ImdbId
		{
			get { return imdbId; }
			set { SetProperty(ref imdbId, value); }
		}
		public string ImdbUrl
		{
			get { return imdbUrl; }
			set { SetProperty(ref imdbUrl, value); }
		}

		public List<Actor> ShowCast
		{
			get { return showCast; }
			set { SetProperty(ref showCast, value); }
		}

		public int CastColumnCount
		{
			get { return castColumnCount; }
			set { SetProperty(ref castColumnCount, value); }
		}

		public List<Episode> Episodes
		{
			get { return episodes; }
			set { SetProperty(ref episodes, value); }
		}

		public int EpisodeColumnCount { get { return 2; } }
		#endregion
		#endregion

		public ViewShowViewModel()
		{
			Menu.Add(new ViewShowMenuItem()
			{
				Icon = new PackIconMaterial() { Kind = PackIconMaterialKind.OrnamentVariant },
				Text = "Overview",
				NavigationDestination = new Uri("Views/Overview.xaml", UriKind.RelativeOrAbsolute)
			});
			Menu.Add(new ViewShowMenuItem()
			{
				Icon = new PackIconMaterial() { Kind = PackIconMaterialKind.EmoticonPoop },
				Text = "Seasons",
				NavigationDestination = new Uri("Views/Seasons.xaml", UriKind.RelativeOrAbsolute)
			});
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

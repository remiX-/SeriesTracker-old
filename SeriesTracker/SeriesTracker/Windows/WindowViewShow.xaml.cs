using MahApps.Metro.Controls;
using SeriesTracker.Core;
using SeriesTracker.Models;
using SeriesTracker.ViewModels;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace SeriesTracker.Windows
{
	public partial class WindowViewShow : MetroWindow
	{
		#region Variables
		private ViewShowViewModel MyViewModel;
		private Show ViewingShow;

		private bool hasWindowInit = false;

		private int[,] actorResize = { { 1050, 2 }, { 1250, 3 }, { 1450, 4 }, { 0, 5 } };
		#endregion

		#region Window Events
		public WindowViewShow(Show show)
		{
			InitializeComponent();

			ViewingShow = show;
		}

		private async void Window_Loaded(object sender, RoutedEventArgs e)
		{
			MyViewModel = DataContext as ViewShowViewModel;
			MyViewModel.SetShow(ViewingShow);

			// Navigate to the home page.
			Navigation.Navigation.Frame = new Frame();
			Navigation.Navigation.Frame.Navigated += SplitViewFrame_OnNavigated;
			Navigation.Navigation.Navigate(MyViewModel.Menu[0].NavigationDestination);

			Width = AppGlobal.Settings.LayoutViewShow.Width;
			Height = AppGlobal.Settings.LayoutViewShow.Height;
			Left = (SystemParameters.PrimaryScreenWidth - Width) / 2;
			Top = (SystemParameters.PrimaryScreenHeight - Height) / 2;

			await Startup();
		}

		private void Window_Activated(object sender, EventArgs e)
		{
			if (!hasWindowInit)
			{
				WindowState = AppGlobal.Settings.LayoutViewShow.Maximized ? WindowState.Maximized : WindowState.Normal;

				hasWindowInit = true;
			}
		}

		private void Window_Closed(object sender, EventArgs e)
		{
			if (WindowState != WindowState.Maximized)
			{
				AppGlobal.Settings.LayoutViewShow.Width = Width;
				AppGlobal.Settings.LayoutViewShow.Height = Height;
			}

			AppGlobal.Settings.LayoutViewShow.Maximized = WindowState == WindowState.Maximized;
			AppGlobal.Settings.Save();
		}

		private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
		{
			if (e.PreviousSize.Height == 0 && e.PreviousSize.Width == 0) return;

			for (int i = 0; i < actorResize.GetLength(0); i++)
			{
				if (e.NewSize.Width <= actorResize[i, 0] || i == actorResize.GetLength(0) - 1)
				{
					if (MyViewModel.CastColumnCount != actorResize[i, 1])
					{
						MyViewModel.CastColumnCount = actorResize[i, 1];
					}

					break;
				}
			}
		}

		#region Navi
		private void SplitViewFrame_OnNavigated(object sender, NavigationEventArgs e)
		{
			HamburgerMenuControl.Content = e.Content;
		}

		private void HamburgerMenuControl_OnItemClick(object sender, ItemClickEventArgs e)
		{
			if (e.ClickedItem is ViewShowMenuItem menuItem && menuItem.IsNavigation)
			{
				Navigation.Navigation.Navigate(menuItem.NavigationDestination);
				HamburgerMenuControl.IsPaneOpen = false;
			}
		}
		#endregion
		#endregion

		#region Startup
		private async Task Startup()
		{
			SetupDirectories();

			await GetWatchedEpisodes();
		}

		private void SetupDirectories()
		{
			if (!Directory.Exists(ViewingShow.LocalImagesActorsPath)) Directory.CreateDirectory(ViewingShow.LocalImagesActorsPath);
			if (!Directory.Exists(ViewingShow.LocalImagesEpisodesPath)) Directory.CreateDirectory(ViewingShow.LocalImagesEpisodesPath);
			if (!Directory.Exists(ViewingShow.LocalImagesPostersPath)) Directory.CreateDirectory(ViewingShow.LocalImagesPostersPath);
			if (!Directory.Exists(ViewingShow.LocalImagesBannersPath)) Directory.CreateDirectory(ViewingShow.LocalImagesBannersPath);
		}

		private async Task GetWatchedEpisodes()
		{
			SeriesResult<UserShowWatch> result = await AppGlobal.Db.UserShowWatchListAsync(ViewingShow);

			ViewingShow.EpisodesWatched = result.ListData;

			foreach (Episode episode in ViewingShow.Episodes)
			{
				UserShowWatch watch = ViewingShow.EpisodesWatched.SingleOrDefault(x => x.SeasonNo == episode.AiredSeason && x.EpisodeNo == episode.AiredEpisodeNumber);

				if (watch != null)
				{
					episode.Watched = true;
				}
			}
		}
		#endregion

		#region General
		public async Task SetShow(Show show)
		{
			return;
			ViewingShow = show;

			MyViewModel.SetShow(show);

			await Startup();
		}
		#endregion
	}
}
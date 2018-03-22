using MaterialDesignThemes.Wpf;
using SeriesTracker.Core;
using SeriesTracker.Dialogs;
using SeriesTracker.Models;
using SeriesTracker.ViewModels;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace SeriesTracker.Windows
{
	public partial class WindowViewShow : Window
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

			Width = AppGlobal.Settings.LayoutViewShow.Width;
			Height = AppGlobal.Settings.LayoutViewShow.Height;
			Left = (SystemParameters.PrimaryScreenWidth - Width) / 2;
			Top = (SystemParameters.PrimaryScreenHeight - Height) / 2;

			await Startup();

			HamburgerListItems.SelectionChanged += DemoItemsListBox_SelectionChanged;
			HamburgerListItems.SelectedIndex = 0;
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
		#endregion

		#region Startup
		private async Task Startup()
		{
			SetupDirectories();

			await LoadBannerAsync();

			//await GetWatchedEpisodes();
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

		private async Task LoadBannerAsync()
		{
			try
			{
				Models.Image banner = MyViewModel.MyShow.Banners.First();
				if (!File.Exists(MyViewModel.MyShow.LocalBannerPath) && !File.Exists(banner.LocalImagePath))
				{
					using (WebClient client = new WebClient())
					{
						await client.DownloadFileTaskAsync(new Uri(banner.OnlineImageUrl), banner.LocalImagePath);
					}

					if (File.Exists(MyViewModel.MyShow.LocalBannerPath))
						File.Delete(MyViewModel.MyShow.LocalBannerPath);

					File.Copy(banner.LocalImagePath, MyViewModel.MyShow.LocalBannerPath);

					MyViewModel.RefreshBanner();
				}
			}
			catch (Exception ex)
			{
				ErrorMethods.LogError(ex.Message);
			}
		}

		private void DemoItemsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (HamburgerListItems.SelectedItem == null) return;

			var selected = HamburgerListItems.SelectedItem as HamburgerMenuItem;
			switch (selected.Id)
			{
				case "Overview": break;
				case "Seasons": break;
				case "Gallery": break;
				default:
					break;
			}

			// Close drawer
			MenuToggleButton.IsChecked = false;
		}

		private async void MenuPopupButton_OnClick(object sender, RoutedEventArgs e)
		{
			var sampleMessageDialog = new SampleMessageDialog
			{
				Message = { Text = "Hello" }
			};

			await DialogHost.Show(sampleMessageDialog, "RootDialog");
		}
	}
}

using MaterialDesignThemes.Wpf;
using SeriesTracker.Core;
using SeriesTracker.Dialogs;
using SeriesTracker.Models;
using SeriesTracker.ViewModels;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
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

			HamburgerListBox.SelectionChanged += HamburgerListBox_SelectionChanged;
			HamburgerListBox.SelectedIndex = 0;
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
			if (MyViewModel == null) return;
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
			MainSnackbar.MessageQueue.Enqueue("Welcome to Material Design In XAML Tookit");
			//Task.Factory.StartNew(() =>
			//{
			//	Thread.Sleep(2500);
			//}).ContinueWith(t =>
			//{
			//	//note you can use the message queue from any thread, but just for the demo here we 
			//	//need to get the message queue from the snackbar, so need to be on the dispatcher
			//	MainSnackbar.MessageQueue.Enqueue("Welcome to Material Design In XAML Tookit");
			//}, TaskScheduler.FromCurrentSynchronizationContext());

			SetupDirectories();

			await LoadBannerAsync();
			//await GetWatchedEpisodes();
		}

		private void SetupDirectories()
		{
			if (!Directory.Exists(MyViewModel.MyShow.LocalImagesActorsPath)) Directory.CreateDirectory(MyViewModel.MyShow.LocalImagesActorsPath);
			if (!Directory.Exists(MyViewModel.MyShow.LocalImagesEpisodesPath)) Directory.CreateDirectory(MyViewModel.MyShow.LocalImagesEpisodesPath);
			if (!Directory.Exists(MyViewModel.MyShow.LocalImagesPostersPath)) Directory.CreateDirectory(MyViewModel.MyShow.LocalImagesPostersPath);
			if (!Directory.Exists(MyViewModel.MyShow.LocalImagesBannersPath)) Directory.CreateDirectory(MyViewModel.MyShow.LocalImagesBannersPath);
		}

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

		private async Task GetWatchedEpisodes()
		{
			SeriesResult<UserShowWatch> result = await AppGlobal.Db.UserShowWatchListAsync(MyViewModel.MyShow);

			MyViewModel.MyShow.EpisodesWatched = result.ListData;

			foreach (Episode episode in MyViewModel.MyShow.Episodes)
			{
				UserShowWatch watch = MyViewModel.MyShow.EpisodesWatched.SingleOrDefault(x => x.SeasonNo == episode.AiredSeason && x.EpisodeNo == episode.AiredEpisodeNumber);

				if (watch != null)
				{
					episode.Watched = true;
				}
			}
		}
		#endregion

		#region HamburgerListBox
		private void HamburgerListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (HamburgerListBox.SelectedItem == null) return;

			var selected = HamburgerListBox.SelectedItem as HamburgerMenuItem;
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
		#endregion

		#region Triple Dots
		private async void MenuPopupButton_OnClick(object sender, RoutedEventArgs e)
		{
			var sampleMessageDialog = new SampleMessageDialog
			{
				Message = { Text = "Hello" }
			};

			await DialogHost.Show(sampleMessageDialog, "RootDialog");
		}
		#endregion
	}
}

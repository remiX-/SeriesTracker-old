﻿using MaterialDesignThemes.Wpf;
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

			// Setup events
			SizeChanged += Window_SizeChanged;
			Closed += Window_Closed;

			Width = AppGlobal.Settings.Windows["ViewShow"].Width;
			Height = AppGlobal.Settings.Windows["ViewShow"].Height;
			Left = (SystemParameters.PrimaryScreenWidth - Width) / 2;
			Top = (SystemParameters.PrimaryScreenHeight - Height) / 2;

			HamburgerListBox.SelectionChanged += HamburgerListBox_SelectionChanged;

			WindowState = AppGlobal.Settings.Windows["ViewShow"].Maximized ? WindowState.Maximized : WindowState.Normal;

			await Startup();
		}

		private void Window_Activated(object sender, EventArgs e)
		{
			//if (!hasWindowInit)
			//{
			//	WindowState = AppGlobal.Settings.Windows["ViewShow"].Maximized ? WindowState.Maximized : WindowState.Normal;

			//	hasWindowInit = true;
			//}
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

		private void Window_Closed(object sender, EventArgs e)
		{
			if (WindowState != WindowState.Maximized)
			{
				AppGlobal.Settings.Windows["ViewShow"].Width = Width;
				AppGlobal.Settings.Windows["ViewShow"].Height = Height;
			}

			AppGlobal.Settings.Windows["ViewShow"].Maximized = WindowState == WindowState.Maximized;
			AppGlobal.Settings.Save();
		}
		#endregion

		#region Startup
		private async Task Startup()
		{
			SetupDirectories();

			HamburgerListBox.SelectedIndex = 0;

			await LoadBannerAsync();
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

			MainSnackbar.MessageQueue.Enqueue(selected.Id);

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

			await DialogHost.Show(sampleMessageDialog, "ViewShowRootDialog");
		}
		#endregion
	}
}

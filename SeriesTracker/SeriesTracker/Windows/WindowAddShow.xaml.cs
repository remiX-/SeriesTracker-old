using Newtonsoft.Json;
using SeriesTracker.Core;
using SeriesTracker.Models;
using SeriesTracker.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Media;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace SeriesTracker.Windows
{
	public partial class WindowAddShow : Window
	{
		#region Variables
		// ViewModel
		private AddShowViewModel MyViewModel;

		private bool busySearching = false;

		public Show SelectedShow { get; private set; }
		#endregion

		#region Window
		public WindowAddShow()
		{
			InitializeComponent();

			Owner = Application.Current.MainWindow;
			WindowStartupLocation = WindowStartupLocation.CenterOwner;
		}

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			MyViewModel = DataContext as AddShowViewModel;

			txt_Search.Focus();
		}
		#endregion

		#region Searching
		private async void Btn_Search_Click(object sender, RoutedEventArgs e)
		{
			if (busySearching) return;

			string searchString = txt_Search.Text;
			if (string.IsNullOrWhiteSpace(searchString))
			{
				txt_Search.Focus();
				SystemSounds.Beep.Play();

				return;
			}

			try
			{
				busySearching = true;

				MyViewModel.SearchResults.Clear();
				btn_Accept.IsEnabled = false;

				MyViewModel.SetStatus("Searching...", Brushes.Yellow);

				string url = $"https://api.thetvdb.com/search/series?name={Uri.EscapeUriString(searchString)}";
				ReturnResult<TvdbAPI> jsonData = await Request.ExecuteAndDeserializeAsync<TvdbAPI>("GET", url);

				if (jsonData.Result.Error == null)
				{
					List<Show> results = JsonConvert.DeserializeObject<List<Show>>(jsonData.Result.Data.ToString())
						.Where(x => !x.SeriesName.Contains("Series Not Permitted"))
						.ToList();
					results.ForEach(s => s.SetupVariables());

					MyViewModel.SearchResults.AddRange(results.OrderByDescending(x => x.YearDisplay == "Unknown" ? "1" : x.YearDisplay).ThenBy(x => x.SeriesName));

					MyViewModel.SetStatus($"{MyViewModel.SearchResults.Count} results found", Brushes.LimeGreen);
				}
				else
				{
					txt_Search.Focus();
					txt_Search.SelectAll();

					MyViewModel.SetStatus("No results found", Brushes.Red);
				}

				SystemSounds.Beep.Play();

				busySearching = false;
			}
			catch (Exception ex)
			{
				ErrorMethods.LogError(ex.Message);
			}
		}

		private void Txt_Search_KeyUp(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Enter)
			{
				Btn_Search_Click(null, null);
			}

			e.Handled = true;
		}

		private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (!btn_Accept.IsEnabled)
				btn_Accept.IsEnabled = true;
		}
		#endregion

		#region Submit, Cancel
		private void Btn_Accept_Click(object sender, RoutedEventArgs e)
		{
			Show show = (Show)lv_SearchResults.SelectedItem;

			if (show != null)
			{
				if (AppGlobal.User.Shows.SingleOrDefault(x => x.Id == show.Id) == null)
				{
					SelectedShow = show;

					Close();
				}
				else
				{
					MessageBox.Show("This show already exists on your list.", "Notice", MessageBoxButton.OK, MessageBoxImage.Information);
				}
			}
		}

		private void Btn_Cancel_Click(object sender, RoutedEventArgs e)
		{
			Close();
		}
		#endregion
	}
}

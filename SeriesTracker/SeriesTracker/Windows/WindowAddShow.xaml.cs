using Newtonsoft.Json;
using SeriesTracker.Core;
using SeriesTracker.Models;
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
		//private ObservableCollection<Show> allShows = new ObservableCollection<Show>();
		public ObservableCollection<Show> SearchResults { get; set; } = new ObservableCollection<Show>();

		private bool busySearching = false;

		public Show selectedShow { get; private set; }
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
			DataContext = this;

			txt_Search.Focus();
		}
		#endregion

		#region Searching
		private async void btn_Search_Click(object sender, RoutedEventArgs e)
		{
			if (busySearching)
				return;

			string searchString = txt_Search.Text;
			if (string.IsNullOrEmpty(searchString))
			{
				txt_Search.Focus();
				SystemSounds.Beep.Play();

				return;
			}

			try
			{
				busySearching = true;

				SearchResults.Clear();
				btn_Accept.IsEnabled = false;

				lbl_Info.Foreground = Brushes.Yellow;
				lbl_Info.Content = "Searching ...";

				string url = string.Format("https://api.thetvdb.com/search/series?name={0}", Uri.EscapeUriString(searchString));
				TvdbAPI jsonData = await Request.ExecuteAndDeserializeAsync("GET", url);

				if (jsonData.Error == null)
				{
					List<Show> results = JsonConvert.DeserializeObject<List<Show>>(jsonData.Data.ToString())
						.Where(x => !x.SeriesName.Contains("Series Not Permitted")).ToList();
					results.ForEach(s => s.SetupVariables());

					SearchResults.AddRange(results.OrderByDescending(x => x.YearDisplay == "Unknown" ? "1" : x.YearDisplay).ThenBy(x => x.SeriesName));

					lbl_Info.Foreground = Brushes.LimeGreen;
					lbl_Info.Content = string.Format("{0} results found", SearchResults.Count);
				}
				else
				{
					txt_Search.Focus();
					txt_Search.SelectAll();

					lbl_Info.Foreground = Brushes.Red;
					lbl_Info.Content = "No results found";
				}

				SystemSounds.Beep.Play();

				busySearching = false;
			}
			catch (Exception ex)
			{
				ErrorMethods.LogError(ex.Message);
			}
		}

		private void txt_Search_KeyUp(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Enter)
			{
				btn_Search_Click(null, null);
			}

			e.Handled = true;
		}

		private void lv_SearchResults_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (!btn_Accept.IsEnabled)
				btn_Accept.IsEnabled = true;
		}
		#endregion

		#region Submit, Cancel
		private void btn_Accept_Click(object sender, RoutedEventArgs e)
		{
			Show show = (Show)lv_SearchResults.SelectedItem;

			if (show != null)
			{
				if (AppGlobal.User.Shows.SingleOrDefault(x => x.Id == show.Id) == null)
				{
					selectedShow = show;

					Close();
				}
				else
				{
					MessageBox.Show("This show already exists on your list.", "Notice", MessageBoxButton.OK, MessageBoxImage.Information);
				}
			}
		}

		private void btn_Cancel_Click(object sender, RoutedEventArgs e)
		{
			Close();
		}
		#endregion
	}
}

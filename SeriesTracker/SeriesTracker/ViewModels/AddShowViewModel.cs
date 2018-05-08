using GalaSoft.MvvmLight;
using SeriesTracker.Models;
using System.Collections.ObjectModel;
using System.Windows.Media;

namespace SeriesTracker.ViewModels
{
	internal class AddShowViewModel : ViewModelBase
	{
		#region Variables
		public ObservableCollection<Show> SearchResults { get; set; } = new ObservableCollection<Show>();

		#region Fields
		private string myTitle;

		private string status;
		private Brush statusForeground;
		#endregion

		#region Properties
		public string MyTitle
		{
			get => myTitle;
			set => Set(ref myTitle, value);
		}

		public string Status
		{
			get => status;
			set => Set(ref status, value);
		}
		public Brush StatusForeground
		{
			get => statusForeground;
			set => Set(ref statusForeground, value);
		}
		#endregion
		#endregion

		public AddShowViewModel()
		{
			MyTitle = "Add Show";

			Status = "Ready";
			StatusForeground = Brushes.Green;
		}

		public void SetStatus(string str, Brush colour = null)
		{
			Status = str;
			StatusForeground = colour;
		}
	}
}

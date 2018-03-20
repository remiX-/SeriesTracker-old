using Prism.Mvvm;
using SeriesTracker.Models;
using System.Collections.ObjectModel;

namespace SeriesTracker.ViewModels
{
	internal class AddShowViewModel : BindableBase
	{
		#region Variables
		public ObservableCollection<Show> SearchResults { get; set; } = new ObservableCollection<Show>();

		#region Fields
		private string myTitle;

		#endregion

		#region Properties
		public string MyTitle
		{
			get { return myTitle; }
			set { SetProperty(ref myTitle, value); }
		}
		#endregion
		#endregion

		public AddShowViewModel()
		{
			MyTitle = "Add Show";
		}
	}
}

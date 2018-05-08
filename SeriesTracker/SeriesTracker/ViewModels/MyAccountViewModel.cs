using GalaSoft.MvvmLight;

namespace SeriesTracker.ViewModels
{
	internal class MyAccountViewModel : ViewModelBase
	{
		#region Variables


		#region Fields
		private string myTitle;

		#endregion

		#region Properties
		public string MyTitle
		{
			get { return myTitle; }
			set { Set(ref myTitle, value); }
		}
		#endregion
		#endregion

		public MyAccountViewModel()
		{
			MyTitle = "My Account";
		}
	}
}

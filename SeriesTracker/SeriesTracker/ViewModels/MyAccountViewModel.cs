using Prism.Mvvm;

namespace SeriesTracker.ViewModels
{
	internal class MyAccountViewModel : BindableBase
	{
		#region Variables


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

		public MyAccountViewModel()
		{
			MyTitle = "My Account";
		}
	}
}

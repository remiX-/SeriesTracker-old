using Prism.Mvvm;

namespace SeriesTracker.ViewModels
{
	internal class ViewLoggingInViewModel : BindableBase
	{
		#region Variables
		#region Fields
		private string loginText;
		#endregion

		#region Properties
		public string LoginText
		{
			get { return loginText; }
			set { SetProperty(ref loginText, value); }
		}
		#endregion
		#endregion

		public ViewLoggingInViewModel()
		{
			loginText = "Logging in as ";
		}
	}
}

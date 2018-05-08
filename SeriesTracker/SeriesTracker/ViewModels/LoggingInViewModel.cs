using GalaSoft.MvvmLight;

namespace SeriesTracker.ViewModels
{
	internal class LoggingInViewModel : ViewModelBase
	{
		#region Variables
		#region Fields
		private string loginText;
		#endregion

		#region Properties
		public string LoginText
		{
			get => loginText;
			set => Set(ref loginText, value);
		}
		#endregion
		#endregion

		public LoggingInViewModel()
		{
			loginText = "Logging in as ";
		}
	}
}

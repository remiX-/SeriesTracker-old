using Prism.Mvvm;

namespace SeriesTracker.ViewModels
{
	internal class LoggingInViewModel : BindableBase
	{
		#region Variables
		#region Fields
		private string loginText;
		#endregion

		#region Properties
		public string LoginText
		{
			get => loginText;
			set => SetProperty(ref loginText, value);
		}
		#endregion
		#endregion

		public LoggingInViewModel()
		{
			loginText = "Logging in as ";
		}
	}
}

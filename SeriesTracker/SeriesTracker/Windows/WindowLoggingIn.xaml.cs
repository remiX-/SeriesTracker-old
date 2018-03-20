using SeriesTracker.ViewModels;
using System.Windows;
using System.Windows.Input;

namespace SeriesTracker.Windows
{
	public partial class WindowLoggingIn : Window
	{
		#region Variables
		// ViewModel
		private ViewLoggingInViewModel MyViewModel;
		#endregion

		public string LoginText { get; set; }

		public WindowLoggingIn()
		{
			InitializeComponent();
		}

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			MyViewModel = DataContext as ViewLoggingInViewModel;
			MyViewModel.LoginText += Properties.Settings.Default.UserEmail;
		}

		private void Window_MouseDown(object sender, MouseButtonEventArgs e)
		{
			if (e.ChangedButton == MouseButton.Left)
				DragMove();
		}
	}
}

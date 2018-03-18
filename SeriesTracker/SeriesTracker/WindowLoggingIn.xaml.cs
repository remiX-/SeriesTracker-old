using MahApps.Metro.Controls;
using SeriesTracker.ViewModels;
using System.Windows.Input;

namespace SeriesTracker
{
	public partial class WindowLoggingIn : MetroWindow
	{
		// ViewModel
		private ViewLoggingInViewModel MyViewModel;

		public string LoginText { get; set; }

		public WindowLoggingIn()
		{
			InitializeComponent();
		}

		private void Window_Loaded(object sender, System.Windows.RoutedEventArgs e)
		{
			MyViewModel = (ViewLoggingInViewModel)DataContext;
			MyViewModel.LoginText += Properties.Settings.Default.UserEmail;
		}

		private void Window_MouseDown(object sender, MouseButtonEventArgs e)
		{
			if (e.ChangedButton == MouseButton.Left)
				DragMove();
		}
	}
}

using SeriesTracker.Core;
using SeriesTracker.Models;
using System.Windows;
using System.Windows.Input;

namespace SeriesTracker.Windows
{
	public partial class WindowMyAccount : Window
	{
		public WindowMyAccount()
		{
			InitializeComponent();

			Owner = Application.Current.MainWindow;
			WindowStartupLocation = WindowStartupLocation.CenterOwner;

			txt_Username.Text = AppGlobal.User.Username;
			txt_Email.Text = AppGlobal.User.Email;
			txt_Name.Text = AppGlobal.User.FirstName;
			txt_Surname.Text = AppGlobal.User.LastName;
			if (AppGlobal.User.DateOfBirth != null)
				dp_DateOfBirth.SelectedDate = AppGlobal.User.DateOfBirth;
			txt_Password.Password = AppGlobal.User.Password;

			Title = "Your Profile - " + AppGlobal.User.Username;
		}

		private void grid_Header_MouseDown(object sender, MouseButtonEventArgs e)
		{
			if (e.ChangedButton == MouseButton.Left)
				DragMove();
		}

		#region Button events
		private async void btn_DeleteAccount_Click(object sender, RoutedEventArgs e)
		{
			if (MessageBox.Show("Are you sure?") == MessageBoxResult.OK)
			{
				SeriesResult<User> result = await AppGlobal.Db.UserDeleteAsync();

				if (result.Result == SQLResult.Success)
				{
					Properties.Settings.Default.UserRemember = false;
					Properties.Settings.Default.Save();

					MessageBox.Show("Your account has been deleted.");

					DialogResult = true;

					Close();
				}
				else
				{
					MessageBox.Show("Something went wrong.");
				}
			}
		}
		#endregion

		private async void btn_Accept_Click(object sender, RoutedEventArgs e)
		{
			User u = AppGlobal.User;
			u.FirstName = txt_Name.Text.Trim();
			u.LastName = txt_Surname.Text.Trim();
			u.DateOfBirth = dp_DateOfBirth.SelectedDate;
			u.Password = txt_Password.Password;

			SeriesResult<User> result = await AppGlobal.Db.UserUpdateAsync(u);

			if (result.Result == SQLResult.Success)
			{
				AppGlobal.User = u;

				//if (Properties.Settings.Default.UserRemember)
				//{
				//	Properties.Settings.Default.UserPassword = u.Password;
				//	Properties.Settings.Default.Save();
				//}

				MessageBox.Show("Your profile has been updated!");
			}
			else
			{
				MessageBox.Show("An error has occured.");
			}
		}

		private void btn_Cancel_Click(object sender, RoutedEventArgs e)
		{
			Close();
		}
	}
}

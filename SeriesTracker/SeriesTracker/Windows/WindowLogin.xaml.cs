using SeriesTracker.Core;
using SeriesTracker.Models;
using SeriesTracker.ViewModels;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace SeriesTracker.Windows
{
	public partial class WindowLogin : Window
	{
		#region Variables
		// ViewModel
		private LoginViewModel MyViewModel;

		private WindowLoggingIn window_LoggedIn;

		private bool hasNet;
		//private bool failedLogin;
		#endregion

		#region Window Events
		public WindowLogin()
		{
			InitializeComponent();
		}

		private async void Window_Initialized(object sender, System.EventArgs e)
		{
			//Properties.Settings.Default.UserEmail = "remix";
			//Properties.Settings.Default.UserPassword = "test";
			//Properties.Settings.Default.UserRemember = true;
			//Properties.Settings.Default.Save();

			//Properties.Settings.Default.UserEmail = string.Empty;
			//Properties.Settings.Default.UserPassword = string.Empty;
			//Properties.Settings.Default.UserRemember = false;
			//Properties.Settings.Default.Save();

			MyViewModel = DataContext as LoginViewModel;

			txt_Login_UsernameOrEmail.Focus();

			MyViewModel.StatusInfo = "";
			lbl_Register_Info.Content = "";

			hasNet = await CommonMethods.HasInternetConnectionAsync();
			if (hasNet)
			{
				if (Properties.Settings.Default.UserRemember)
				{
					if (!await CheckAutomaticLogin())
					{
						MyViewModel.SetLoginStatus(Brushes.Red, "Automatic login failed");
						btn_Login.IsEnabled = false;
					}
				}
				else
				{
					Show();
				}
			}
			else
			{
				Show();

				MyViewModel.SetLoginStatus(Brushes.Red, "No internet connection");
				btn_Login.IsEnabled = false;

				lbl_Register_Info.Foreground = Brushes.Red;
				lbl_Register_Info.Content = "No internet connection";
				btn_Register.IsEnabled = false;
			}
		}

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			//MyViewModel = DataContext as LoginViewModel;

			//txt_Login_UsernameOrEmail.Focus();

			//MyViewModel.StatusInfo = "";
			//lbl_Register_Info.Content = "";

			//if (hasNet)
			//{
			//	if (failedLogin)
			//	{
			//		MyViewModel.SetLoginStatus(Brushes.Red, "Automatic login failed");
			//		btn_Login.IsEnabled = false;
			//	}
			//}
			//else
			//{
			//	MyViewModel.SetLoginStatus(Brushes.Red, "No internet connection");
			//	btn_Login.IsEnabled = false;

			//	lbl_Register_Info.Foreground = Brushes.Red;
			//	lbl_Register_Info.Content = "No internet connection";
			//	btn_Register.IsEnabled = false;
			//}
		}

		private void Window_MouseDown(object sender, MouseButtonEventArgs e)
		{
			if (e.ChangedButton == MouseButton.Left)
				DragMove();
		}
		#endregion

		#region Login
		private async void Btn_Login_Click(object sender, RoutedEventArgs e)
		{
			MyViewModel.SetLoginStatus(Brushes.Red, string.Empty);

			// Validation checks
			if (string.IsNullOrEmpty(txt_Login_UsernameOrEmail.Text))
			{
				MyViewModel.StatusInfo = "Username/Email is required";
				txt_Login_UsernameOrEmail.Focus();

				return;
			}

			if (string.IsNullOrEmpty(txt_Login_Password.Password))
			{
				MyViewModel.StatusInfo = "Password is required";
				txt_Login_Password.Focus();

				return;
			}

			bool goodLogin = await CheckLogin(MyViewModel.Username, MyViewModel.Password);
			if (goodLogin)
			{
				await CompleteLogin();
			}
		}

		private void Login_KeyPress(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Enter)
			{
				Btn_Login_Click(this, null);
				e.Handled = true;
			}
		}

		private void CB_RememberMe_CheckChange(object sender, RoutedEventArgs e)
		{
			Properties.Settings.Default.UserRemember = (bool)cb_RememberMe.IsChecked;
			Properties.Settings.Default.Save();
		}

		private async Task<bool> CheckAutomaticLogin()
		{
			window_LoggedIn = new WindowLoggingIn();
			window_LoggedIn.Show();

			//return;
			await Task.Delay(1000);

			bool goodLogin = await CheckLogin(Properties.Settings.Default.UserEmail, Properties.Settings.Default.UserPassword);
			if (goodLogin)
			{
				await CompleteLogin();

				return true;
			}
			else
			{
				window_LoggedIn.Close();
				Show();

				Properties.Settings.Default.UserEmail = string.Empty;
				Properties.Settings.Default.UserPassword = string.Empty;
				Properties.Settings.Default.UserRemember = false;
				Properties.Settings.Default.Save();

				return false;
			}
		}

		private async Task<bool> CheckLogin(string UsernameOrEmail, string Password)
		{
			MyViewModel.SetLoginStatus(Brushes.Orange, "Logging in...");
			btn_Login.IsEnabled = false;

			LoginResult result = await MethodCollection.UserLoginAsync(UsernameOrEmail, Password);
			if (result.Result == SQLResult.LoginSuccessful)
			{
				AppGlobal.User = result.UserData;

				Properties.Settings.Default.UserEmail = result.UserData.Username;
				if ((bool)cb_RememberMe.IsChecked)
				{
					Properties.Settings.Default.UserPassword = Password;
				}
				Properties.Settings.Default.Save();

				MyViewModel.SetLoginStatus(Brushes.Green, "Logged in!");

				return true;
			}
			else
			{
				txt_Login_UsernameOrEmail.IsEnabled = true;
				txt_Login_Password.IsEnabled = true;
				btn_Login.IsEnabled = true;

				txt_Login_UsernameOrEmail.Focus();
				txt_Login_UsernameOrEmail.SelectAll();

				MyViewModel.StatusBrush = Brushes.Red;
				if (result.Result == SQLResult.BadLogin)
				{
					MyViewModel.StatusInfo = "Incorrect login details";
				}
				else if (result.Result == SQLResult.ErrorHasOccured)
				{
					MyViewModel.StatusInfo = "An unexpected error has occured";
				}

				return false;
			}
		}

		private async Task CompleteLogin()
		{
			if (window_LoggedIn == null)
			{
				Hide();

				window_LoggedIn = new WindowLoggingIn();
				window_LoggedIn.Show();
			}

			AppGlobal.User.Shows = (await AppGlobal.Db.UserShowListAsync()).ListData;
			AppGlobal.User.Categories = (await AppGlobal.Db.UserCategoryListAsync()).ListData;

			window_LoggedIn.Close();

			Window Main = new WindowMain();
			Main.Show();
			Application.Current.MainWindow = Main;

			Close();
		}

		private void Password_PasswordChanged(object sender, RoutedEventArgs e)
		{
			MyViewModel.Password = ((PasswordBox)sender).Password;
		}
		#endregion

		#region Register
		private async void Btn_Register_Click(object sender, RoutedEventArgs e)
		{
			lbl_Register_Info.Foreground = Brushes.Red;
			lbl_Register_Info.Content = "";

			string Username = txt_Register_Username.Text.Trim();
			string Email = txt_Register_Email.Text.Trim();
			string Name = txt_Register_Name.Text.Trim();
			string Password = txt_Register_Password.Password;
			string ConfirmPassword = txt_Register_ConfirmPassword.Password;

			// Validation checks
			if (string.IsNullOrEmpty(Username))
			{
				lbl_Register_Info.Content = "Username is required";
				txt_Register_Username.Focus();

				return;
			}
			else if (Username.Contains(' '))
			{
				lbl_Register_Info.Content = "Username is invalid";
				txt_Register_Username.Focus();

				return;
			}
			else if (string.IsNullOrEmpty(Email))
			{
				lbl_Register_Info.Content = "Email is required";
				txt_Register_Email.Focus();

				return;
			}
			else if (!CommonMethods.IsValidEmail(Email))
			{
				lbl_Register_Info.Content = "Email is invalid";
				txt_Register_Email.Focus();

				return;
			}
			else if (string.IsNullOrEmpty(Password))
			{
				lbl_Register_Info.Content = "Password is required";
				txt_Register_Password.Focus();

				return;
			}
			else if (string.IsNullOrEmpty(ConfirmPassword))
			{
				lbl_Register_Info.Content = "Confirm password is required";
				txt_Register_ConfirmPassword.Focus();

				return;
			}
			else if (Password != ConfirmPassword)
			{
				lbl_Register_Info.Content = "Passwords do not match";
				txt_Register_Password.Focus();

				return;
			}

			SeriesResult<User> result = await MethodCollection.UserRegisterAsync(Username, Email, Name, Password);

			if (result.Result == SQLResult.RegistrationSuccessful)
			{
				lbl_Register_Info.Foreground = Brushes.Green;
				lbl_Register_Info.Content = "Registration successful!";

				MyViewModel.SetLoginStatus(Brushes.Green, "Registration successful!");

				txt_Register_Username.Text = "";
				txt_Register_Name.Text = "";
				txt_Register_Email.Text = "";
				txt_Register_Password.Password = "";
				txt_Register_ConfirmPassword.Password = "";

				tc.SelectedIndex = 0;
				txt_Login_UsernameOrEmail.Focus();
			}
			else if (result.Result == SQLResult.UsernameAlreadyRegistered)
			{
				lbl_Register_Info.Content = "Username already in use";

				txt_Register_Username.Focus();
				txt_Register_Username.SelectAll();
			}
			else if (result.Result == SQLResult.EmailAlreadyRegistered)
			{
				lbl_Register_Info.Content = "Email already in use";

				txt_Register_Email.Focus();
				txt_Register_Email.SelectAll();
			}
			else if (result.Result == SQLResult.ErrorHasOccured)
			{
				lbl_Register_Info.Content = "An unexpected error has occured";

				txt_Register_Username.Focus();
			}
		}

		private void Register_KeyPress(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Enter)
			{
				Btn_Register_Click(this, null);
				e.Handled = true;
			}
		}
		#endregion
	}
}
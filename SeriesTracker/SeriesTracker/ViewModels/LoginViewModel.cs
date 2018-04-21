using Prism.Commands;
using Prism.Mvvm;
using System.Windows.Media;

namespace SeriesTracker.ViewModels
{
	internal class LoginViewModel : BindableBase, IViewModelBase
	{
		#region Vars
		#region Fields
		private bool isBusy;

		private string username;
		private string password;

		private Brush statusBrush;
		private string statusInfo;
		#endregion

		#region Properties
		public bool IsBusy
		{
			get => isBusy;
			set
			{
				SetProperty(ref isBusy, value);
				LoginCommand.RaiseCanExecuteChanged();
			}
		}

		public string Username
		{
			get => username;
			set => SetProperty(ref username, value);
		}
		public string Password
		{
			get => password;
			set => SetProperty(ref password, value);
		}

		public Brush StatusBrush
		{
			get => statusBrush;
			set => SetProperty(ref statusBrush, value);
		}
		public string StatusInfo
		{
			get => statusInfo;
			set => SetProperty(ref statusInfo, value);
		}
		#endregion

		// Commands
		public DelegateCommand LoginCommand { get; }
		public DelegateCommand RegisterCommand { get; }
		#endregion

		public LoginViewModel()
		{
			//LoginCommand = new DelegateCommand(Login, () => !IsBusy && Username.IsNotBlank() && Password.IsNotBlank());
		}

		public void SetLoginStatus(Brush brush, string status)
		{
			StatusBrush = brush;
			StatusInfo = status;
		}

		#region Login

		/*
		private async void Login()
		{
			bool goodLogin = await CheckLogin(Username, Password);
			if (goodLogin)
			{
				await CompleteLogin();
			}
		}

		private async Task<bool> CheckLogin(string UsernameOrEmail, string Password)
		{
			//lbl_Login_Info.Foreground = Brushes.Orange;
			//lbl_Login_Info.Content = "Logging in...";
			//btn_Login.IsEnabled = false;

			LoginResult result = await MethodCollection.UserLoginAsync(UsernameOrEmail, Password);
			if (result.Result == SQLResult.LoginSuccessful)
			{
				AppGlobal.User = result.UserData;

				Properties.Settings.Default.UserEmail = result.UserData.Username;
				//if ((bool)cb_RememberMe.IsChecked)
				//{
				//	Properties.Settings.Default.UserPassword = Password;
				//}
				Properties.Settings.Default.Save();

				//lbl_Login_Info.Foreground = Brushes.Green;
				//lbl_Login_Info.Content = "Logged in!";

				return true;
			}
			else
			{
				//txt_Login_UsernameOrEmail.IsEnabled = true;
				//txt_Login_Password.IsEnabled = true;

				//txt_Login_UsernameOrEmail.Focus();
				//txt_Login_UsernameOrEmail.SelectAll();

				//btn_Login.IsEnabled = true;
				//lbl_Login_Info.Foreground = Brushes.Red;
				if (result.Result == SQLResult.BadLogin)
				{
					//lbl_Login_Info.Content = "Incorrect login details";
				}
				else if (result.Result == SQLResult.ErrorHasOccured)
				{
					//lbl_Login_Info.Content = "An unexpected error has occured";
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

			Window Main = new WindowMainNew();
			Main.Show();
			Application.Current.MainWindow = Main;

			Close();
		}
		*/
		#endregion
	}
}
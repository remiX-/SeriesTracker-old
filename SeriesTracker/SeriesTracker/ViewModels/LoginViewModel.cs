using Prism.Commands;
using Prism.Mvvm;
using SeriesTracker.Core;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SeriesTracker.ViewModels
{
	internal class LoginViewModel : BindableBase
	{
		#region Vars
		#region Fields
		private string username;
		private string password;
		#endregion

		#region Properties
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
		#endregion

		// Commands
		public DelegateCommand LoginCommand { get; }
		public DelegateCommand RegisterCommand { get; }
		#endregion

		public LoginViewModel()
		{

		}

		#region Login


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
		#endregion
	}
}
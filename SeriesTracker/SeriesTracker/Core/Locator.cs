using CommonServiceLocator;
using GalaSoft.MvvmLight.Ioc;
using SeriesTracker.Services;
using SeriesTracker.ViewModels;

namespace SeriesTracker.Core
{
	public class Locator
	{
		public IAboutViewModel AboutViewModel => Resolve<IAboutViewModel>();
		public IAddShowViewModel AddShowViewModel => Resolve<IAddShowViewModel>();
		public ILoggingInViewModel LoggingInViewModel => Resolve<ILoggingInViewModel>();
		public ILoginViewModel LoginViewModel => Resolve<ILoginViewModel>();
		public IMainViewModel MainNewViewModel => Resolve<IMainViewModel>();
		public IMyAccountViewModel MyAccountViewModel => Resolve<IMyAccountViewModel>();
		public ISetCategoryViewModel SetCategoryViewModel => Resolve<ISetCategoryViewModel>();
		public ISettingsViewModel SettingsViewModel => Resolve<ISettingsViewModel>();
		public IViewShowViewModel ViewShowViewModel => Resolve<IViewShowViewModel>();

		private T Resolve<T>(string key = null)
		{
			return ServiceLocator.Current.GetInstance<T>(key);
		}

		public static void Init()
		{
			ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);
			SimpleIoc.Default.Reset();

			// Services
			SimpleIoc.Default.Register<ISettingsService, SettingsService>();
			SimpleIoc.Default.Register<IUpdateService, UpdateService>();

			// View models
			SimpleIoc.Default.Register<IAboutViewModel, AboutViewModel>(true);
			SimpleIoc.Default.Register<IAddShowViewModel, AddShowViewModel>(true);
			SimpleIoc.Default.Register<ILoggingInViewModel, LoggingInViewModel>(true);
			SimpleIoc.Default.Register<ILoginViewModel, LoginViewModel>(true);
			SimpleIoc.Default.Register<IMainViewModel, MainViewModel>(true);
			SimpleIoc.Default.Register<IMyAccountViewModel, MyAccountViewModel>(true);
			SimpleIoc.Default.Register<ISetCategoryViewModel, SetCategoryViewModel>(true);
			SimpleIoc.Default.Register<ISettingsViewModel, SettingsViewModel>(true);
			SimpleIoc.Default.Register<IViewShowViewModel, ViewShowViewModel>(true);
		}

		public static void Cleanup()
		{

		}
	}
}

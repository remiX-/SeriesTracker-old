using MahApps.Metro;
using SeriesTracker.Core;
using System.IO;
using System.Threading;
using System.Windows;

namespace SeriesTracker
{
	public partial class App : Application
	{
		private Mutex myMutex;

		public App()
		{
		}

		private void Application_Startup(object sender, StartupEventArgs e)
		{
			myMutex = new Mutex(true, AppGlobal.AssemblyTitle, out bool aIsNewInstance);
			if (!aIsNewInstance)
			{
				Current.Shutdown();
			}

			// Check program directories
			if (!Directory.Exists(AppGlobal.Paths.RootDirectory))
				Directory.CreateDirectory(AppGlobal.Paths.RootDirectory);

			if (!Directory.Exists(AppGlobal.Paths.FilesDirectory))
				Directory.CreateDirectory(AppGlobal.Paths.FilesDirectory);

			if (!Directory.Exists(AppGlobal.Paths.SeriesDirectory))
				Directory.CreateDirectory(AppGlobal.Paths.SeriesDirectory);

			// Load settings file
			if (File.Exists(AppGlobal.Paths.SettingsFile))
				AppGlobal.Settings = AppSettings.Read();
			else
				AppGlobal.Settings = new AppSettings(true);

			ThemeManager.ChangeAppStyle(Current, ThemeManager.GetAccent(AppGlobal.Settings.Accent), ThemeManager.GetAppTheme(AppGlobal.Settings.Theme));
		}
	}
}

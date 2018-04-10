using SeriesTracker.Core;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;

namespace SeriesTracker
{
	public partial class App : Application
	{
		private Mutex AppMutex { get; set; }

		public App()
		{

		}

		private void Application_Startup(object sender, StartupEventArgs e)
		{
			AppMutex = new Mutex(true, AppGlobal.AssemblyTitle, out bool aIsNewInstance);
			if (!aIsNewInstance)
			{
				GiveSpecifiedAppTheFocus();
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

			new SeriesTrackerPaletteHelper().SetLightDark(AppGlobal.Settings.Theme.Type, AppGlobal.Settings.Theme.IsDark);
			new SeriesTrackerPaletteHelper().ReplacePrimaryColor(AppGlobal.Settings.Theme.Primary);
			new SeriesTrackerPaletteHelper().ReplaceAccentColor(AppGlobal.Settings.Theme.Accent);
		}

		[DllImport("user32.dll")]
		private static extern bool ShowWindow(IntPtr hWnd, uint windowStyle);

		[DllImport("user32.dll")]
		private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, uint uFlags);

		[DllImport("user32.dll")]
		private static extern bool SetForegroundWindow(IntPtr hWnd);

		private static void GiveSpecifiedAppTheFocus()
		{
			try
			{
				Process p = Process.GetProcessesByName(AppGlobal.AssemblyTitle).FirstOrDefault();

				//ShowWindow(p.MainWindowHandle, 1);
				SetWindowPos(p.MainWindowHandle, new IntPtr(0), 0, 0, 0, 0, 3);

				//SetForegroundWindow(p.MainWindowHandle);
			}
			catch
			{
				throw;
			}
		}
	}
}
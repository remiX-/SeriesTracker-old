using System;
using System.IO;

namespace SeriesTracker.Core
{
	public class AppPaths
	{
		// Directories
		public string RootDirectory { get; }

		public string FilesDirectory { get; }
		public string SeriesDirectory { get; }

		public string LocalSeriesDirectory { get { return ""; } }

		// Files
		public string SettingsFile { get; }
		public string EztvIDFile { get; }

		public AppPaths()
		{
			RootDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "." + AppGlobal.AssemblyProduct);
			FilesDirectory = Path.Combine(RootDirectory, "files");
			SeriesDirectory = Path.Combine(RootDirectory, "series");

			SettingsFile = Path.Combine(FilesDirectory, "settings.json");
			EztvIDFile = Path.Combine(FilesDirectory, "eztvid");
		}
	}
}

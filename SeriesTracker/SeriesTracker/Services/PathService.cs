using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SeriesTracker.Services
{
	public class PathService : IPathService
	{
		private readonly ISettingsService _settingsService;

		// Directories
		public string RootDirectory { get; }

		public string FilesDirectory { get; }
		public string SeriesDirectory { get; }

		public string LocalSeriesDirectory => _settingsService.LocalSeriesFolder;

		// Files
		public string SettingsFile { get; }
		public string EztvIDFile { get; }

		public PathService(ISettingsService settingsService)
		{
			_settingsService = settingsService;

			RootDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), ".SeriesTracker");
			FilesDirectory = Path.Combine(RootDirectory, "files");
			SeriesDirectory = Path.Combine(RootDirectory, "series");

			SettingsFile = Path.Combine(FilesDirectory, "config");
			EztvIDFile = Path.Combine(FilesDirectory, "eztvid");
		}
	}
}
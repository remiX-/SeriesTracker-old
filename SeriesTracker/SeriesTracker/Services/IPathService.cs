using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SeriesTracker.Services
{
	public interface IPathService
	{
		// Directories
		string RootDirectory { get; }

		string FilesDirectory { get; }
		string SeriesDirectory { get; }

		string LocalSeriesDirectory { get; }

		// Files
		string SettingsFile { get; }
		string EztvIDFile { get; }
	}
}

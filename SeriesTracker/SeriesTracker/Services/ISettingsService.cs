using System.Collections.Generic;
using System.ComponentModel;

namespace SeriesTracker.Services
{
	public interface ISettingsService
	{
		Dictionary<string, LayoutSettings> WindowSettings { get; set; }

		bool IgnoreBracketsInNames { get; set; }
		bool UseListedName { get; set; }
		bool StartOnWindowsStart { get; set; }

		string DateFormat { get; set; }
		string DefaultSortColumn { get; set; }
		ListSortDirection DefaultSortDirection { get; set; }

		Theme Theme { get; set; }

		string LocalSeriesFolder { get; set; }
		List<Series> LocalSeriesPaths { get; set; }

		List<ColumnSetting> ColumnSettings { get; set; }

		bool IsAutoUpdateEnabled { get; set; }

		void Load();
		void Save();
	}
}
using System.Collections.Generic;

namespace SeriesTracker.Services
{
	public interface ISettingsService
	{
		LayoutSettings WindowSettings { get; set; }

		string DateFormat { get; set; }

		string OutputFolder { get; set; }

		bool IsAutoUpdateEnabled { get; set; }

		void Load();
		void Save();
	}
}
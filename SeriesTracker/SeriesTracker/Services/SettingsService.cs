using System.Collections.Generic;
using Tyrrrz.Settings;
using Newtonsoft.Json;
using System.ComponentModel;

namespace SeriesTracker.Services
{
	public class SettingsService : SettingsManager, ISettingsService
	{
		#region Variables
		// Layouts
		[JsonProperty("windows")]
		public Dictionary<string, LayoutSettings> WindowSettings { get; set; }

		// General Tab
		[JsonProperty("ignoreBracketsInNames")]
		public bool IgnoreBracketsInNames { get; set; }
		[JsonProperty("useListedName")]
		public bool UseListedName { get; set; }
		[JsonProperty("startOnWindowsStart")]
		public bool StartOnWindowsStart { get; set; }

		[JsonProperty("dateFormat")]
		public string DateFormat { get; set; }
		[JsonProperty("defaultSortColumn")]
		public string DefaultSortColumn { get; set; }
		[JsonProperty("defaultSortDirection")]
		public ListSortDirection DefaultSortDirection { get; set; }

		[JsonProperty("theme")]
		public Theme Theme { get; set; }

		// Extra Tab
		[JsonProperty("localSeriesFolder")]
		public string LocalSeriesFolder { get; set; }
		[JsonProperty("localSeriesPaths")]
		public List<Series> LocalSeriesPaths { get; set; }

		// Extras
		[JsonProperty("columnSettings")]
		public List<ColumnSetting> ColumnSettings { get; set; }

		[JsonProperty("enableAutoUpdate")]
		public bool IsAutoUpdateEnabled { get; set; }
		#endregion

		public SettingsService()
		{
			IsSaved = false;

			Configuration.StorageSpace = StorageSpace.SyncedUserDomain;
			Configuration.SubDirectoryPath = ".SeriesTracker";
			Configuration.FileName = "config";
			Configuration.ThrowIfCannotLoad = true;
			Configuration.ThrowIfCannotSave = true;
		}

		public override void Load()
		{
			base.Load();

			if (!IsSaved)
			{
				LoadDefaults();
				Save();
			}
		}

		private void LoadDefaults()
		{
			WindowSettings = new Dictionary<string, LayoutSettings>
			{
				["main"] = new LayoutSettings(0, 0, 1024, 576, false),
				["viewshow"] = new LayoutSettings(0, 0, 1024, 576, false)
			};

			// View Settings
			IgnoreBracketsInNames = false;
			UseListedName = false;
			DefaultSortColumn = "Name";
			DefaultSortDirection = ListSortDirection.Ascending;

			// General
			DateFormat = "dd/MM/yyyy";

			Theme = new Theme
			{
				Type = "SeriesTracker",
				IsDark = true,
				Primary = "bluegrey",
				Accent = "green"
			};

			// Columns
			ColumnSettings = new List<ColumnSetting>();

			// Local series
			LocalSeriesFolder = "";
			LocalSeriesPaths = new List<Series>();
		}
	}

	public class LayoutSettings
	{
		[JsonProperty("x")]
		public double X { get; set; }
		[JsonProperty("y")]
		public double Y { get; set; }
		[JsonProperty("width")]
		public double Width { get; set; }
		[JsonProperty("height")]
		public double Height { get; set; }

		[JsonProperty("maximized")]
		public bool Maximized { get; set; }

		public LayoutSettings(double x, double y, double width, double height, bool maximized)
		{
			X = x;
			Y = y;
			Width = width;
			Height = height;
			Maximized = maximized;
		}
	}

	public class Theme
	{
		[JsonProperty("type")]
		public string Type { get; set; }
		[JsonProperty("isDark")]
		public bool IsDark { get; set; }
		[JsonProperty("primary")]
		public string Primary { get; set; }
		[JsonProperty("accent")]
		public string Accent { get; set; }
	}

	public class Series
	{
		[JsonProperty("id")]
		public int Id { get; set; }
		[JsonProperty("path")]
		public string Path { get; set; }
	}

	public class ColumnSetting
	{
		[JsonProperty("name")]
		public string Name { get; set; }
		[JsonProperty("width")]
		public int Width { get; set; }
		[JsonProperty("visible")]
		public bool Visible { get; set; }
	}
}
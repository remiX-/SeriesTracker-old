using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Xml.Serialization;

namespace SeriesTracker.Core
{
	public class AppSettings
	{
		#region Variables
		// Layouts
		public Dictionary<string, LayoutSettings> Windows { get; set; }

		// General Tab
		public bool IgnoreBracketsInNames { get; set; }
		public bool UseListedName { get; set; }
		public bool StartOnWindowsStart { get; set; }

		public string DateFormat { get; set; }
		public string DefaultSortColumn { get; set; }
		public ListSortDirection DefaultSortDirection { get; set; }

		public bool IsDarkTheme { get; set; }
		public string Primary { get; set; }
		public string Accent { get; set; }

		// Extra Tab
		public string LocalSeriesFolder { get; set; }
		public List<Series> LocalSeriesPaths { get; set; }

		// Extras
		public List<ColumnSetting> ColumnSettings { get; set; }
		#endregion

		public AppSettings() { }

		public AppSettings(bool loadDefaults)
		{
			if (loadDefaults)
			{
				LoadDefaults();
			}
		}

		public void Save()
		{
			// Sort before saving
			LocalSeriesPaths = LocalSeriesPaths.OrderBy(x => x.Id).ToList();

			string jsonFormatted = JToken.Parse(JsonConvert.SerializeObject(this)).ToString(Formatting.Indented);
			File.WriteAllText(AppGlobal.Paths.SettingsFile, jsonFormatted);
		}

		public static AppSettings Read()
		{
			AppSettings settings;
			try
			{
				string jsonData = File.ReadAllText(AppGlobal.Paths.SettingsFile);
				settings = JsonConvert.DeserializeObject<AppSettings>(jsonData);
			}
			catch (Exception)
			{
				settings = new AppSettings(true);
			}

			return settings;
		}

		public void ClearSeriesPaths()
		{
			AppGlobal.Settings.LocalSeriesPaths = new List<Series>();
		}

		public void AddSeriesPath(int id, string path)
		{
			LocalSeriesPaths.Add(new Series { Id = id, Path = path });
		}

		public Series GetSeriesPath(int id)
		{
			return LocalSeriesPaths.SingleOrDefault(x => x.Id == id);
		}

		public void RemoveSeriesPath(int id)
		{
			LocalSeriesPaths.RemoveAll(x => x.Id == id);
		}

		public void SaveColumnSetting(List<DataGridColumn> columns, bool doSave)
		{
			ColumnSettings.Clear();

			foreach (DataGridColumn column in columns)
			{
				ColumnSettings.Add(new ColumnSetting
				{
					Name = column.Header.ToString(),
					Width = (int)column.ActualWidth,
					Visible = column.Visibility == Visibility.Visible
				});
			}

			if (doSave)
				Save();
		}

		private void LoadDefaults()
		{
			Windows = new Dictionary<string, LayoutSettings>
			{
				["Main"] = new LayoutSettings(1024, 576, false),
				["ViewShow"] = new LayoutSettings(1024, 576, false)
			};

			// View Settings
			IgnoreBracketsInNames = false;
			UseListedName = false;
			DefaultSortColumn = "Name";
			DefaultSortDirection = ListSortDirection.Ascending;

			// General
			DateFormat = "dd/MM/yyyy";

			IsDarkTheme = true;
			Primary = "bluegrey";
			Accent = "red";

			// Columns
			ColumnSettings = new List<ColumnSetting>();

			// Local series
			LocalSeriesFolder = "";
			LocalSeriesPaths = new List<Series>();

			Save();
		}
	}

	public class LayoutSettings
	{
		public double Width { get; set; }
		public double Height { get; set; }

		public bool Maximized { get; set; }

		public LayoutSettings(double width, double height, bool maximized)
		{
			Width = width;
			Height = height;
			Maximized = maximized;
		}
	}

	public class Series
	{
		public int Id { get; set; }
		public string Path { get; set; }
	}

	public class ColumnSetting
	{
		public string Name { get; set; }
		public int Width { get; set; }
		public bool Visible { get; set; }
	}
}
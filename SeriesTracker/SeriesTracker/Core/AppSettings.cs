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
		[XmlElement(ElementName="Layout1")]
		public LayoutSettings LayoutMain { get; set; }
		[XmlElement(ElementName="Layout2")]
		public LayoutSettings LayoutViewShow { get; set; }

		// List view settings
		public bool IgnoreBracketsInNames { get; set; }
		public bool UseListedName { get; set; }
		public string DefaultSortColumn { get; set; }
		public ListSortDirection DefaultSortDirection { get; set; }

		// General
		public string DateFormat { get; set; }
		public string Theme { get; set; }
		public string Accent { get; set; }

		// Columns
		[XmlArrayItem("Column")]
		public List<ColumnSetting> ColumnSettings { get; set; }
		#endregion

		// Local Series
		public string LocalSeriesFolder { get; set; }
		[XmlArrayItem("Path")]
		public List<Series> LocalSeriesPaths { get; set; }

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

			try
			{
				using (StreamWriter sw = new StreamWriter(AppGlobal.Paths.SettingsFile))
				{
					XmlSerializer xmls = new XmlSerializer(typeof(AppSettings));
					xmls.Serialize(sw, this);
				}
			}
			catch (Exception ex)
			{
				ErrorMethods.LogError(ex.Message);
			}
		}

		public static AppSettings Read()
		{
			AppSettings settings;
			try
			{
				using (StreamReader sw = new StreamReader(AppGlobal.Paths.SettingsFile))
				{
					XmlSerializer xmls = new XmlSerializer(typeof(AppSettings));
					settings = xmls.Deserialize(sw) as AppSettings;
				}
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
			// Layouts
			LayoutMain = new LayoutSettings()
			{
				For = "Main",
				Width = 1024,
				Height = 576,
				Maximized = false
			};
			LayoutViewShow = new LayoutSettings()
			{
				For = "ViewShow",
				Width = 1024,
				Height = 576,
				Maximized = false
			};

			// View Settings
			IgnoreBracketsInNames = false;
			UseListedName = false;
			DefaultSortColumn = "Name";
			DefaultSortDirection = ListSortDirection.Ascending;

			// General
			DateFormat = "dd/MM/yyyy";
			Theme = "BaseDark";
			Accent = "Blue";

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
		[XmlAttribute]
		public string For { get; set; }

		public double Width { get; set; }
		public double Height { get; set; }

		public bool Maximized { get; set; }
	}

	public class Series
	{
		[XmlAttribute]
		public int Id { get; set; }
		[XmlAttribute]
		public string Path { get; set; }
	}

	public class ColumnSetting
	{
		[XmlAttribute]
		public string Name { get; set; }
		[XmlAttribute]
		public int Width { get; set; }
		[XmlAttribute]
		public bool Visible { get; set; }
	}
}
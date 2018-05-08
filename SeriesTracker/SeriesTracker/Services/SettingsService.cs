using System.Collections.Generic;
using Tyrrrz.Settings;
using Newtonsoft.Json;

namespace SeriesTracker.Services
{
	public class SettingsService : SettingsManager, ISettingsService
	{
		#region Vars
		[JsonProperty("windowSettings")]
		public LayoutSettings WindowSettings { get; set; }

		[JsonProperty("dateFormat")]
		public string DateFormat { get; set; }

		[JsonProperty("outputFolder")]
		public string OutputFolder { get; set; }

		[JsonProperty("enableAutoUpdate")]
		public bool IsAutoUpdateEnabled { get; set; }
		#endregion

		public SettingsService()
		{
			IsSaved = false;

			Configuration.StorageSpace = StorageSpace.SyncedUserDomain;
			Configuration.SubDirectoryPath = ".YouTubeTool";
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
			WindowSettings = new LayoutSettings(0, 0, 1024, 576, false);

			DateFormat = "dd/MMMM/yyyy hh:mm tt";
			OutputFolder = string.Empty;
			IsAutoUpdateEnabled = true;
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
}
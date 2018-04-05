using MaterialDesignThemes.Wpf;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;

namespace SeriesTracker.Core
{
	public class SeriesTrackerPaletteHelper : PaletteHelper
	{
		public void SetLightDark(string themeType, bool isDark)
		{
			var existingResourceDictionary = Application.Current.Resources.MergedDictionaries
				.Where(rd => rd.Source != null)
				.SingleOrDefault(rd => Regex.Match(rd.Source.OriginalString, @"Theme.(Light|Dark)").Success);
			if (existingResourceDictionary == null)
				throw new ApplicationException("Unable to find Light/Dark base theme in Application resources.");

			var source = "";
			switch (themeType)
			{
				case "MaterialDesign":
					source = $"pack://application:,,,/MaterialDesignThemes.Wpf;component/Themes/MaterialDesignTheme.{(isDark ? "Dark" : "Light")}.xaml";
					break;
				default:
					source = $"pack://application:,,,/{typeof(App).Namespace};component/SeriesTrackerTheme.{(isDark ? "Dark" : "Light")}.xaml";
					break;
			}
			var newResourceDictionary = new ResourceDictionary() { Source = new Uri(source) };

			Application.Current.Resources.MergedDictionaries.Remove(existingResourceDictionary);
			Application.Current.Resources.MergedDictionaries.Add(newResourceDictionary);
		}
	}
}

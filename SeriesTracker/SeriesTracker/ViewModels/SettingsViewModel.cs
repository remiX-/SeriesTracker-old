using MaterialDesignColors;
using MaterialDesignThemes.Wpf;
using Prism.Mvvm;
using SeriesTracker.Controls;
using SeriesTracker.Core;
using SeriesTracker.Models;
using SeriesTracker.Windows;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;

namespace SeriesTracker.ViewModels
{
	public class SettingsViewModel : BindableBase
	{
		#region Variables
		#region Fields
		private bool ignoreBrackets;
		private bool useListedName;
		private bool startOnWindowsStart;

		private string exampleDate;
		private ListSortDirection defaultSortDirection;

		private bool isDark;

		private ObservableCollection<Category> categories;
		#endregion

		#region Properties
		public bool IgnoreBrackets
		{
			get => ignoreBrackets;
			set => ignoreBrackets = value;
		}

		public bool UseListedName
		{
			get => useListedName;
			set => useListedName = value;
		}

		public bool StartOnWindowsStart
		{
			get => startOnWindowsStart;
			set => startOnWindowsStart = value;
		}

		public string[] DateFormats { get; }
		public int DateFormatIndex => GetSelectedIndex(DateFormats, AppGlobal.Settings.DateFormat);

		public string ExampleDate
		{
			get => exampleDate;
			set => SetProperty(ref exampleDate, value);
		}

		public string[] ColumnHeadings { get; }
		public int DefaultSortingIndex => GetSelectedIndex(ColumnHeadings, AppGlobal.Settings.DefaultSortColumn);

		public ListSortDirection DefaultSortDirection
		{
			get => defaultSortDirection;
			set => defaultSortDirection = value;
		}
		public bool DefaultSortAsc
		{
			get => DefaultSortDirection == ListSortDirection.Ascending;
			set => DefaultSortDirection = value ? ListSortDirection.Ascending : ListSortDirection.Descending;
		}
		public bool DefaultSortDesc
		{
			get => DefaultSortDirection == ListSortDirection.Descending;
			set => DefaultSortDirection = value ? ListSortDirection.Descending : ListSortDirection.Ascending;
		}

		public bool IsDark
		{
			get => isDark;
			set => SetProperty(ref isDark, value);
		}
		public IEnumerable<Swatch> Swatches { get; }
		public IEnumerable<string> SwatchesString { get; }
		public IEnumerable<string> SwatchesAccent { get; }
		public int PrimaryIndex => GetSelectedIndex(SwatchesString, AppGlobal.Settings.Primary);
		public int AccentIndex => GetSelectedIndex(SwatchesAccent, AppGlobal.Settings.Accent);

		public ObservableCollection<Category> Categories
		{
			get => categories;
			set => SetProperty(ref categories, value);
		}

		public ICommand ToggleBaseCommand { get; } = new WPFCommandImplementation(o => ApplyBase((bool)o));
		#endregion

		#endregion

		public SettingsViewModel()
		{
			IgnoreBrackets = AppGlobal.Settings.IgnoreBracketsInNames;
			UseListedName = AppGlobal.Settings.UseListedName;
			StartOnWindowsStart = AppGlobal.Settings.StartOnWindowsStart;

			ColumnHeadings = WindowMainNew.ColumnHeadings.ToArray();

			DateFormats = new[]
			{
				"dd/MM/yyyy",
				"dd/M/yyyy",
				"d/MM/yyyy",
				"d/M/yyyy",
				"d MMM yyyy",
				"dd MMM yyyy",
				"d MMMM yyyy",
				"dd MMMM yyyy",
				"dd MMM, ddd",
				"d MMM, ddd"
			};
			DefaultSortDirection = AppGlobal.Settings.DefaultSortDirection;

			IsDark = AppGlobal.Settings.IsDarkTheme;
			Swatches = new SwatchesProvider().Swatches;
			SwatchesString = Swatches.Select(swatch => swatch.Name);
			SwatchesAccent = Swatches.Where(swatch => swatch.IsAccented).Select(swatch => swatch.Name);

			Categories = new ObservableCollection<Category>(AppGlobal.User.Categories.OrderBy(x => x.Name));
		}

		private int GetSelectedIndex(string[] list, string selected)
		{
			for (int i = 0; i < list.Length; i++)
			{
				if (selected == list[i])
					return i;
			}

			return 0;
		}

		private int GetSelectedIndex(IEnumerable<string> list, string selected)
		{
			return GetSelectedIndex(list.ToArray(), selected);
		}

		private static void ApplyBase(bool isDark)
		{
			new PaletteHelper().SetLightDark(isDark);
		}
	}
}

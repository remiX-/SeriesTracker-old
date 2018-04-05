using MaterialDesignColors;
using Prism.Commands;
using Prism.Mvvm;
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

		public string theme;
		private bool isDark;
		private string primary;
		private string accent;

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

		public string Theme { get => theme; set { SetProperty(ref theme, value); ApplyBase(); } }
		public bool IsDark { get => isDark; set => SetProperty(ref isDark, value); }
		public string Primary { get => primary; set => SetProperty(ref primary, value); }
		public string Accent { get => accent; set => SetProperty(ref accent, value); }

		public string[] Themes { get; }
		public IEnumerable<Swatch> Swatches { get; }
		public string[] SwatchesString { get; }
		public string[] SwatchesAccent { get; }

		public ObservableCollection<Category> Categories { get => categories; set => SetProperty(ref categories, value); }

		public ICommand ToggleBaseCommand => new DelegateCommand(ApplyBase);
		#endregion

		#endregion

		public SettingsViewModel()
		{
			ignoreBrackets = AppGlobal.Settings.IgnoreBracketsInNames;
			useListedName = AppGlobal.Settings.UseListedName;
			startOnWindowsStart = AppGlobal.Settings.StartOnWindowsStart;

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
			defaultSortDirection = AppGlobal.Settings.DefaultSortDirection;

			Themes = new[] { "SeriesTracker", "MaterialDesign" };
			Swatches = new SwatchesProvider().Swatches;
			SwatchesString = Swatches.Select(swatch => swatch.Name).ToArray();
			SwatchesAccent = Swatches.Where(swatch => swatch.IsAccented).Select(swatch => swatch.Name).ToArray();

			isDark = AppGlobal.Settings.Theme.IsDark;
			theme = GetSelectedItem(Themes, AppGlobal.Settings.Theme.Type);
			primary = GetSelectedItem(SwatchesString, AppGlobal.Settings.Theme.Primary);
			accent = GetSelectedItem(SwatchesString, AppGlobal.Settings.Theme.Accent);

			categories = new ObservableCollection<Category>(AppGlobal.User.Categories.OrderBy(x => x.Name));
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

		private string GetSelectedItem(string[] list, string selected)
		{
			return list[GetSelectedIndex(list, selected)];
		}

		private void ApplyBase()
		{
			new SeriesTrackerPaletteHelper().SetLightDark(theme, isDark);
		}
	}
}

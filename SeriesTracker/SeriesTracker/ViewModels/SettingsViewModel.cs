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
		private string exampleDate;
		private ListSortDirection defaultSortDirection;

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

		public string[] DateFormats { get; }
		public int DateFormatIndex => GetSelectedIndex(DateFormats, AppGlobal.Settings.DateFormat);

		//public string GetExampleDate() => exampleDate;

		public string ExampleDate
		{
			get => exampleDate;
			set => SetProperty(ref exampleDate, value);
		}

		public string[] ColumnHeadings { get; }
		public int DefaultSortingIndex { get { return GetSelectedIndex(ColumnHeadings, AppGlobal.Settings.DefaultSortColumn); } }

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

		public string[] Themes { get; }
		public int ThemeIndex => GetSelectedIndex(Themes, AppGlobal.Settings.Theme);

		public string[] Accents { get; }
		public int AccentIndex => GetSelectedIndex(Accents, AppGlobal.Settings.Accent);

		public ObservableCollection<Category> Categories
		{
			get { return categories; }
			set { SetProperty(ref categories, value); }
		}

		public IEnumerable<Swatch> Swatches { get; }
		#endregion

		public ICommand ToggleBaseCommand { get; } = new WPFCommandImplementation(o => ApplyBase((bool)o));

		public ICommand ApplyPrimaryCommand { get; } = new WPFCommandImplementation(o => ApplyPrimary((Swatch)o));
		public ICommand ApplyAccentCommand { get; } = new WPFCommandImplementation(o => ApplyAccent((Swatch)o));
		#endregion

		public SettingsViewModel()
		{
			IgnoreBrackets = AppGlobal.Settings.IgnoreBracketsInNames;
			UseListedName = AppGlobal.Settings.UseListedName;

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

			Themes = new[] { "BaseLight", "BaseDark" };
			Accents = new[]
			{
				"Red", "Green", "Blue",
				"Purple", "Orange", "Lime",
				"Emerald", "Teal", "Cyan",
				"Cobalt", "Indigo", "Violet",
				"Pink", "Magenta", "Crimson",
				"Amber", "Yellow", "Brown",
				"Olive", "Steel", "Mauve",
				"Taupe", "Sienna"
			};

			Swatches = new SwatchesProvider().Swatches;

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

		private static void ApplyBase(bool isDark)
		{
			new PaletteHelper().SetLightDark(isDark);
		}

		private static void ApplyPrimary(Swatch swatch)
		{
			new PaletteHelper().ReplacePrimaryColor(swatch);
		}

		private static void ApplyAccent(Swatch swatch)
		{
			new PaletteHelper().ReplaceAccentColor(swatch);
		}
	}
}

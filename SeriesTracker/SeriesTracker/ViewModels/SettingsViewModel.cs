using Prism.Mvvm;
using SeriesTracker.Core;
using SeriesTracker.Models;
using SeriesTracker.Windows;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

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
			get { return ignoreBrackets; }
			set { ignoreBrackets = value; }
		}

		public bool UseListedName
		{
			get { return useListedName; }
			set { useListedName = value; }
		}

		public List<string> DateFormats { get; protected set; }
		public int DateFormatIndex { get { return GetSelectedTheme(DateFormats, AppGlobal.Settings.DateFormat); } }

		public string ExampleDate
		{
			get { return exampleDate; }
			set { SetProperty(ref exampleDate, value); }
		}

		public List<string> ColumnHeadings { get; }
		public int DefaultSortingIndex { get { return GetSelectedTheme(ColumnHeadings, AppGlobal.Settings.DefaultSortColumn); } }
		public ListSortDirection DefaultSortDirection
		{
			get { return defaultSortDirection; }
			set { defaultSortDirection = value; }
		}
		public bool DefaultSortAsc
		{
			get { return DefaultSortDirection == ListSortDirection.Ascending; }
			set { DefaultSortDirection = value ? ListSortDirection.Ascending : ListSortDirection.Descending; }
		}
		public bool DefaultSortDesc
		{
			get { return DefaultSortDirection == ListSortDirection.Descending; }
			set { DefaultSortDirection = value ? ListSortDirection.Descending : ListSortDirection.Ascending; }
		}

		public List<string> Themes { get; protected set; }
		public int ThemeIndex { get { return GetSelectedTheme(Themes, AppGlobal.Settings.Theme); } }

		public List<string> Accents { get; protected set; }
		public int AccentIndex { get { return GetSelectedTheme(Accents, AppGlobal.Settings.Accent); } }

		public ObservableCollection<Category> Categories
		{
			get { return categories; }
			set { SetProperty(ref categories, value); }
		}
		#endregion
		#endregion

		public SettingsViewModel()
		{
			IgnoreBrackets = AppGlobal.Settings.IgnoreBracketsInNames;
			UseListedName = AppGlobal.Settings.UseListedName;

			ColumnHeadings = WindowMainNew.ColumnHeadings;

			DateFormats = new List<string> { "dd/MM/yyyy", "dd/M/yyyy", "d/MM/yyyy", "d/M/yyyy", "d MMM yyyy", "dd MMM yyyy", "d MMMM yyyy", "dd MMMM yyyy", "dd MMM, ddd", "d MMM, ddd" };
			DefaultSortDirection = AppGlobal.Settings.DefaultSortDirection;

			Themes = new List<string> { "BaseLight", "BaseDark" };
			Accents = new List<string> { "Red", "Green", "Blue", "Purple", "Orange", "Lime", "Emerald", "Teal", "Cyan", "Cobalt", "Indigo", "Violet", "Pink", "Magenta", "Crimson", "Amber", "Yellow", "Brown", "Olive", "Steel", "Mauve", "Taupe", "Sienna" };

			Categories = new ObservableCollection<Category>(AppGlobal.User.Categories.OrderBy(x => x.Name));
		}

		private int GetSelectedTheme(List<string> list, string selected)
		{
			for (int i = 0; i < list.Count; i++)
			{
				if (selected == list[i])
					return i;
			}

			return 0;
		}
	}
}

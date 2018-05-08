using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using MaterialDesignColors;
using MaterialDesignThemes.Wpf;
using SeriesTracker.Core;
using SeriesTracker.Dialogs;
using SeriesTracker.Models;
using SeriesTracker.Windows;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using WinForms = System.Windows.Forms;

namespace SeriesTracker.ViewModels
{
	public class SettingsViewModel : ViewModelBase, ISettingsViewModel
	{
		#region Variables
		#region Fields
		private bool ignoreBrackets;
		private bool useListedName;
		private bool startOnWindowsStart;

		private string dateFormat;
		private string exampleDate;

		private string defaultSort;
		private ListSortDirection defaultSortDirection;

		public string theme;
		private bool isDark;
		private string primary;
		private string accent;

		private string localSeriesFolder;

		private ObservableCollection<Category> categories;
		private Category category;
		#endregion

		#region Properties
		#region General
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

		public string DateFormat
		{
			get => dateFormat;
			set
			{
				Set(ref dateFormat, value);

				ExampleDate = CommonMethods.ConvertDateTimeToString(DateTime.Now, dateFormat);
			}
		}

		public string ExampleDate { get => exampleDate; set => Set(ref exampleDate, value); }

		public string[] ColumnHeadings { get; }

		public string DefaultSort { get => defaultSort; set => Set(ref defaultSort, value); }

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

		public string Theme { get => theme; set { Set(ref theme, value); ApplyBase(); } }
		public bool IsDark { get => isDark; set { Set(ref isDark, value); ApplyBase(); } }
		public string Primary { get => primary; set { Set(ref primary, value); ApplyPrimary(); } }
		public string Accent { get => accent; set { Set(ref accent, value); ApplyAccent(); }  }

		public string[] Themes { get; }
		public IEnumerable<Swatch> Swatches { get; }
		public string[] SwatchesString { get; }
		public string[] SwatchesAccent { get; }

		public ObservableCollection<Category> Categories { get => categories; set => Set(ref categories, value); }
		public Category Category
		{
			get => category;
			set => Set(ref category, value);
		}

		public ICommand BrowseSeriesFolderCommand => new RelayCommand(BrowseSeriesFolder);

		public ICommand UserListAddCommand => new RelayCommand(UserListAdd);
		public ICommand UserListRemoveCommand => new RelayCommand(UserListRemove);

		public string NewUserList { get; set; }
		#endregion

		#region Extra
		public string LocalSeriesFolder
		{
			get { return localSeriesFolder; }
			set
			{
				if (value.Length > 0 && value.Substring(value.Length - 1) != "/")
					value += "/";

				if (!Directory.Exists(value))
				{
					MessageBox.Show($"Path '{value}' does not exist", "Invalid Path", MessageBoxButton.OK, MessageBoxImage.Error);
					value = string.Empty;
				}

				Set(ref localSeriesFolder, value);
			}
		}
		#endregion
		#endregion
		#endregion

		public SettingsViewModel()
		{
			ignoreBrackets = AppGlobal.Settings.IgnoreBracketsInNames;
			useListedName = AppGlobal.Settings.UseListedName;
			startOnWindowsStart = AppGlobal.Settings.StartOnWindowsStart;

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
			DateFormat = GetSelectedItem(DateFormats, AppGlobal.Settings.DateFormat);

			ColumnHeadings = WindowMainNew.ColumnHeadings.ToArray();
			defaultSortDirection = AppGlobal.Settings.DefaultSortDirection;
			defaultSort = GetSelectedItem(ColumnHeadings, AppGlobal.Settings.DefaultSortColumn);

			Themes = new[] { "SeriesTracker", "MaterialDesign" };
			Swatches = new SwatchesProvider().Swatches;
			SwatchesString = Swatches.Select(swatch => swatch.Name).ToArray();
			SwatchesAccent = Swatches.Where(swatch => swatch.IsAccented).Select(swatch => swatch.Name).ToArray();

			isDark = AppGlobal.Settings.Theme.IsDark;
			theme = GetSelectedItem(Themes, AppGlobal.Settings.Theme.Type);
			primary = GetSelectedItem(SwatchesString, AppGlobal.Settings.Theme.Primary);
			accent = GetSelectedItem(SwatchesString, AppGlobal.Settings.Theme.Accent);

			localSeriesFolder = AppGlobal.Settings.LocalSeriesFolder;

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
			new SeriesTrackerPaletteHelper().SetLightDark(Theme, IsDark);
		}

		private void ApplyPrimary()
		{
			new SeriesTrackerPaletteHelper().ReplacePrimaryColor(Primary);
		}

		private void ApplyAccent()
		{
			new SeriesTrackerPaletteHelper().ReplaceAccentColor(Accent);
		}

		private async void UserListAdd()
		{
			NewUserList = string.Empty;

			var result = await DialogHost.Show(new AddUserListDialog(), "SettingsDialog");
			if (result is bool && !(bool)result) return;

			if (string.IsNullOrEmpty(NewUserList)) return;

			bool exists = Categories.Any(x => x.Name.ToLower() == NewUserList.ToLower());
			if (!exists)
			{
				Category toAdd = new Category(CommonMethods.TitleCase(NewUserList));

				Categories.Add(toAdd);
			}
		}

		private void UserListRemove()
		{
			Categories.Remove(Category);
		}

		private void BrowseSeriesFolder()
		{
			WinForms.FolderBrowserDialog fbd = new WinForms.FolderBrowserDialog
			{
				Description = "Select the folder where your series are stored",
				SelectedPath = LocalSeriesFolder
			};

			if (fbd.ShowDialog() == WinForms.DialogResult.OK)
				LocalSeriesFolder = fbd.SelectedPath;
		}
	}
}
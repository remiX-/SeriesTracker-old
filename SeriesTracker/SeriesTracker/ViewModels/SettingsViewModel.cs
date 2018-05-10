using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using MaterialDesignColors;
using MaterialDesignThemes.Wpf;
using SeriesTracker.Core;
using SeriesTracker.Dialogs;
using SeriesTracker.Models;
using SeriesTracker.Services;
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
		private readonly ISettingsService _settingsService;

		#region Properties
		#region General
		public bool IgnoreBrackets
		{
			get => _settingsService.IgnoreBracketsInNames;
			set => _settingsService.IgnoreBracketsInNames = value;
		}

		public bool UseListedName
		{
			get => _settingsService.UseListedName;
			set => _settingsService.UseListedName = value;
		}

		public bool StartOnWindowsStart
		{
			get => _settingsService.StartOnWindowsStart;
			set => _settingsService.StartOnWindowsStart = value;
		}

		public string[] DateFormats { get; }
		//public int DateFormatIndex => GetSelectedIndex(DateFormats, AppGlobal.Settings.DateFormat);

		public string DateFormat
		{
			get => _settingsService.DateFormat;
			set
			{
				_settingsService.DateFormat = value;
				ExampleDate = CommonMethods.ConvertDateTimeToString(DateTime.Now, _settingsService.DateFormat);
			}
		}

		public string ExampleDate { get; set; }

		//public string[] ColumnHeadings { get; }

		public string DefaultSort
		{
			get => _settingsService.DefaultSortColumn;
			set => _settingsService.DefaultSortColumn = value;
		}

		public ListSortDirection DefaultSortDirection
		{
			get => _settingsService.DefaultSortDirection;
			set => _settingsService.DefaultSortDirection = value;
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

		public string Theme
		{
			get => _settingsService.Theme.Type;
			set
			{
				_settingsService.Theme.Type = value;
				ApplyBase();
			}
		}
		public bool IsDark
		{
			get => _settingsService.Theme.IsDark;
			set
			{
				_settingsService.Theme.IsDark = value;
				ApplyBase();
			}
		}
		public string Primary
		{
			get => _settingsService.Theme.Primary;
			set
			{
				_settingsService.Theme.Primary = value;
				ApplyPrimary();
			}
		}
		public string Accent
		{
			get => _settingsService.Theme.Accent;
			set
			{
				_settingsService.Theme.Accent = value;
				ApplyAccent();
			}
		}

		public string[] Themes { get; }
		public IEnumerable<Swatch> Swatches { get; }
		public string[] SwatchesString { get; }
		public string[] SwatchesAccent { get; }

		//public ObservableCollection<Category> Categories { get => categories; set => Set(ref categories, value); }
		//public Category Category
		//{
		//	get => category;
		//	set => Set(ref category, value);
		//}

		public RelayCommand BrowseSeriesFolderCommand { get; }

		//public RelayCommand UserListAddCommand => new RelayCommand(UserListAdd);
		//public RelayCommand UserListRemoveCommand => new RelayCommand(UserListRemove);

		//public string NewUserList { get; set; }
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

		public SettingsViewModel(ISettingsService settingsService)
		{
			_settingsService = settingsService;

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
			//DateFormat = GetSelectedItem(DateFormats, AppGlobal.Settings.DateFormat);

			//ColumnHeadings = WindowMain.ColumnHeadings.ToArray();
			//defaultSortDirection = AppGlobal.Settings.DefaultSortDirection;
			//defaultSort = GetSelectedItem(ColumnHeadings, AppGlobal.Settings.DefaultSortColumn);

			Themes = new[] { "SeriesTracker", "MaterialDesign" };
			Swatches = new SwatchesProvider().Swatches;
			SwatchesString = Swatches.Select(swatch => swatch.Name).ToArray();
			SwatchesAccent = Swatches.Where(swatch => swatch.IsAccented).Select(swatch => swatch.Name).ToArray();

			//isDark = AppGlobal.Settings.Theme.IsDark;
			//theme = GetSelectedItem(Themes, AppGlobal.Settings.Theme.Type);
			//primary = GetSelectedItem(SwatchesString, AppGlobal.Settings.Theme.Primary);
			//accent = GetSelectedItem(SwatchesString, AppGlobal.Settings.Theme.Accent);

			//localSeriesFolder = AppGlobal.Settings.LocalSeriesFolder;

			//categories = new ObservableCollection<Category>(AppGlobal.User.Categories.OrderBy(x => x.Name));

			BrowseSeriesFolderCommand = new RelayCommand(BrowseSeriesFolder);
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

		//private async void UserListAdd()
		//{
		//	NewUserList = string.Empty;

		//	var result = await DialogHost.Show(new AddUserListDialog(), "SettingsDialog");
		//	if (result is bool && !(bool)result) return;

		//	if (string.IsNullOrEmpty(NewUserList)) return;

		//	bool exists = Categories.Any(x => x.Name.ToLower() == NewUserList.ToLower());
		//	if (!exists)
		//	{
		//		Category toAdd = new Category(CommonMethods.TitleCase(NewUserList));

		//		Categories.Add(toAdd);
		//	}
		//}

		//private void UserListRemove()
		//{
		//	Categories.Remove(Category);
		//}

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
﻿using SeriesTracker.Core;
using SeriesTracker.Models;
using SeriesTracker.ViewModels;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace SeriesTracker.Windows
{
	public partial class WindowSettings : Window
	{
		#region Variables
		private SettingsViewModel MyViewModel;

		public delegate void CloseHandlerDelegate(List<string> changes);
		public event CloseHandlerDelegate CloseHandler;

		// Categories
		private List<Category> categoriesToAdd = new List<Category>();
		private List<Category> categoriesToDelete = new List<Category>();
		#endregion

		#region Window Events
		public WindowSettings()
		{
			InitializeComponent();

			WindowStartupLocation = WindowStartupLocation.CenterOwner;
		}

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			MyViewModel = DataContext as SettingsViewModel;
		}
		#endregion

		#region Extra Group

		#region Categories
		private void Lb_Categories_ContextMenuOpening(object sender, ContextMenuEventArgs e)
		{
			if (lb_Categories.SelectedItems.Count == 0)
				lb_Categories.ContextMenu.IsOpen = false;
		}

		private void Cm_Remove_Click(object sender, RoutedEventArgs e)
		{
			if (lb_Categories.SelectedIndex == -1)
				return;

			Category toRemove = (Category)lb_Categories.SelectedItem;

			// Check to see if it has a proper ID
			// If it does, then it needs to be removed from DB
			if (toRemove.CategoryID != -1)
				categoriesToDelete.Add(toRemove);
			else
				categoriesToAdd.Remove(toRemove);

			MyViewModel.Categories.Remove(toRemove);
		}
		#endregion
		#endregion

		private async void Btn_Accept_Click(object sender, RoutedEventArgs e)
		{
			//ShowOverlay();

			List<string> changes = new List<string>();

			#region General

			if (AppGlobal.Settings.UseListedName != cb_ListedName.IsChecked || AppGlobal.Settings.IgnoreBracketsInNames != cb_IgnoreBrackets.IsChecked)
			{
				changes.Add("ReloadView");
			}

			AppGlobal.Settings.IgnoreBracketsInNames = MyViewModel.IgnoreBrackets;
			AppGlobal.Settings.UseListedName = MyViewModel.UseListedName;
			AppGlobal.Settings.StartOnWindowsStart = MyViewModel.StartOnWindowsStart;

			AppGlobal.Settings.DateFormat = MyViewModel.DateFormat;
			AppGlobal.Settings.DefaultSortColumn = MyViewModel.DefaultSort;
			AppGlobal.Settings.DefaultSortDirection = MyViewModel.DefaultSortDirection;

			// Theme
			AppGlobal.Settings.Theme.Type = MyViewModel.Theme;
			AppGlobal.Settings.Theme.IsDark = MyViewModel.IsDark;
			AppGlobal.Settings.Theme.Primary = MyViewModel.Primary;
			AppGlobal.Settings.Theme.Accent = MyViewModel.Accent;
			#endregion

			#region Extra
			if (AppGlobal.Settings.LocalSeriesFolder != MyViewModel.LocalSeriesFolder)
			{
				AppGlobal.Settings.LocalSeriesFolder = MyViewModel.LocalSeriesFolder;

				changes.Add("UpdateFolders");
			}

			if (categoriesToAdd.Count > 0 || categoriesToDelete.Count > 0)
			{
				if (categoriesToAdd.Count > 0)
				{
					SeriesResult<Category> r = await AppGlobal.Db.UserCategoryAddMultipleAsync(categoriesToAdd);
					if (r.Result == SQLResult.ErrorHasOccured)
					{
						MessageBox.Show("Failed to add new categories");
					}
				}

				if (categoriesToDelete.Count > 0)
				{
					SeriesResult<Category> r = await AppGlobal.Db.UserCategoryDeleteMutlipleAsync(categoriesToDelete);
					if (r.Result == SQLResult.ErrorHasOccured)
					{
						MessageBox.Show("Failed to delete categories");
					}
				}

				AppGlobal.User.Categories = MyViewModel.Categories.ToList();
				changes.Add("UpdateCategory");
			}
			#endregion

			AppGlobal.Settings.Save();

			CloseHandler(changes);

			Close();
		}

		private void Btn_Cancel_Click(object sender, RoutedEventArgs e)
		{
			Close();
		}
	}
}

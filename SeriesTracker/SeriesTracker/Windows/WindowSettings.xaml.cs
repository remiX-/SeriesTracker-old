﻿using SeriesTracker.Core;
using SeriesTracker.Models;
using SeriesTracker.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using WinForms = System.Windows.Forms;

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
			DataContext = MyViewModel = new SettingsViewModel();

			txt_SeriesFolder.Text = AppGlobal.Settings.LocalSeriesFolder;
		}
		#endregion

		#region General
		private void cmb_DateFormat_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			MyViewModel.ExampleDate = CommonMethods.ConvertDateTimeToString(DateTime.Now, cmb_DateFormat.SelectedItem.ToString());
		}

		private void cmb_Theme_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (cmb_Theme.SelectedItem == null || cmb_Accent.SelectedItem == null)
				return;

			string theme = cmb_Theme.SelectedItem.ToString();
			string accent = cmb_Accent.SelectedItem.ToString();

			//ThemeManager.ChangeAppStyle(this, ThemeManager.GetAccent(accent), ThemeManager.GetAppTheme(theme));
		}
		#endregion

		#region Extra Group
		#region Folders
		private void btn_SeriesBrowse_Click(object sender, RoutedEventArgs e)
		{
			WinForms.FolderBrowserDialog fbd = new WinForms.FolderBrowserDialog
			{
				Description = "Select the folder where your series are stored",
				SelectedPath = txt_SeriesFolder.Text
			};

			if (fbd.ShowDialog() == WinForms.DialogResult.OK)
				txt_SeriesFolder.Text = fbd.SelectedPath;
		}

		//private void btn_MoviesBrowse_Click(object sender, RoutedEventArgs e)
		//{
		//	System.Windows.Forms.FolderBrowserDialog fbd = new System.Windows.Forms.FolderBrowserDialog();
		//	fbd.Description = "Select the folder where your movies are stored";
		//	fbd.SelectedPath = txt_MoviesFolder.Text;

		//	if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
		//		txt_MoviesFolder.Text = fbd.SelectedPath;
		//}

		private void txt_SeriesFolder_LostFocus(object sender, RoutedEventArgs e)
		{
			if (txt_SeriesFolder.Text.Length > 0 && !Directory.Exists(txt_SeriesFolder.Text))
			{
				MessageBox.Show("Path '" + txt_SeriesFolder.Text + "' does not exist", "Invalid Path", MessageBoxButton.OK, MessageBoxImage.Error);
				txt_SeriesFolder.Text = "";
			}
		}

		//private void txt_MoviesFolder_LostFocus(object sender, RoutedEventArgs e)
		//{
		//	if (txt_MoviesFolder.Text.Length > 0 && !Directory.Exists(txt_MoviesFolder.Text))
		//	{
		//		MessageBox.Show("Path '" + txt_MoviesFolder.Text + "' does not exist", "Invalid Path", MessageBoxButton.OK, MessageBoxImage.Error);
		//		txt_MoviesFolder.Text = "";
		//	}
		//}
		#endregion

		#region Categories
		private void btn_AddCategory_Click(object sender, RoutedEventArgs e)
		{
			string newCategory = txt_Category.Text.Trim();

			if (string.IsNullOrEmpty(newCategory))
				return;

			bool exists = MyViewModel.Categories.Any(x => x.Name.ToLower() == newCategory.ToLower());
			if (!exists)
			{
				newCategory = CommonMethods.TitleCase(newCategory);

				Category toAdd = new Category(newCategory);

				categoriesToAdd.Add(toAdd);

				txt_Category.Text = "";

				MyViewModel.Categories.Add(toAdd);
			}
		}

		private void txt_Category_KeyPress(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Enter)
			{
				btn_AddCategory_Click(this, null);
				e.Handled = true;
			}
		}

		private void lb_Categories_ContextMenuOpening(object sender, ContextMenuEventArgs e)
		{
			if (lb_Categories.SelectedItems.Count == 0)
				lb_Categories.ContextMenu.IsOpen = false;
		}

		private void cm_Remove_Click(object sender, RoutedEventArgs e)
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

		private async void btn_Accept_Click(object sender, RoutedEventArgs e)
		{
			//ShowOverlay();

			List<string> changes = new List<string>();

			#region General
			// Check if some settings changed
			if (AppGlobal.Settings.LocalSeriesFolder != txt_SeriesFolder.Text)
			{
				changes.Add("UpdateFolders");
			}

			if (AppGlobal.Settings.UseListedName != cb_ListedName.IsChecked || AppGlobal.Settings.IgnoreBracketsInNames != cb_IgnoreBrackets.IsChecked)
			{
				changes.Add("ReloadView");
			}

			AppGlobal.Settings.IgnoreBracketsInNames = MyViewModel.IgnoreBrackets;
			AppGlobal.Settings.UseListedName = MyViewModel.UseListedName;
			AppGlobal.Settings.DateFormat = MyViewModel.DateFormats[cmb_DateFormat.SelectedIndex];
			AppGlobal.Settings.DefaultSortColumn = MyViewModel.ColumnHeadings[cmb_DefaultSorting.SelectedIndex];
			AppGlobal.Settings.DefaultSortDirection = MyViewModel.DefaultSortDirection;

			// Theme & Accent
			string theme = cmb_Theme.SelectedItem.ToString();
			string accent = cmb_Accent.SelectedItem.ToString();
			if (AppGlobal.Settings.Theme != theme || AppGlobal.Settings.Accent != accent)
			{
				AppGlobal.Settings.Theme = theme;
				AppGlobal.Settings.Accent = accent;

				changes.Add("UpdateTheme");
			}
			#endregion

			#region Extra
			AppGlobal.Settings.LocalSeriesFolder = txt_SeriesFolder.Text;

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

		private void btn_Cancel_Click(object sender, RoutedEventArgs e)
		{
			Close();
		}
	}
}

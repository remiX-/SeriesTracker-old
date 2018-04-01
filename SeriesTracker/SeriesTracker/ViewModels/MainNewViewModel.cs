﻿using MaterialDesignThemes.Wpf;
using Prism.Commands;
using Prism.Mvvm;
using SeriesTracker.Controls;
using SeriesTracker.Core;
using SeriesTracker.Dialogs;
using SeriesTracker.Models;
using SeriesTracker.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;

namespace SeriesTracker.ViewModels
{
	internal class MainNewViewModel : BindableBase
	{
		#region Variables
		public HamburgerMenuItem[] AppMenu { get; }

		public CollectionViewSource Collection { get; } = new CollectionViewSource();

		public bool IsBusy => status != "Ready";

		#region Fields
		private string myTitle;
		private Category filterCategory;
		private string filterText;
		private int gridViewColumnCount;

		private string status;
		#endregion

		#region Properties
		public string MyTitle
		{
			get => myTitle;
			set => SetProperty(ref myTitle, $"{AppGlobal.AssemblyTitle} - {value}");
		}

		public string UserEmail { get; }
		public string Username { get; }

		public Category FilterCategory
		{
			get => filterCategory;
			set { SetProperty(ref filterCategory, value); RefreshView(); }
		}
		public List<Category> Categories
		{
			get
			{
				List<Category> cat = AppGlobal.User.Categories.OrderBy(x => x.Name).ToList();
				cat.Insert(0, new Category(0, "All"));
				return cat;
			}
		}

		public string FilterText
		{
			get => filterText;
			set { SetProperty(ref filterText, value); RefreshView(); }
		}

		public int GridViewColumnCount
		{
			get => gridViewColumnCount;
			set => SetProperty(ref gridViewColumnCount, value);
		}

		public string Status
		{
			get => status;
			private set => SetProperty(ref status, value);
		}

		public string Product { get; }

		public ICommand HelpCommand => new WPFCommandImplementation(ShowWindowAboutDialog);
		#endregion
		#endregion

		public MainNewViewModel()
		{
			AppMenu = new[]
			{
				new HamburgerMenuItem("AddSeries", "Add Series", PackIconKind.Account),
				new HamburgerMenuItem("ForceUpdateSeries", "Force Update Series", PackIconKind.Update),
				new HamburgerMenuItem("CheckUpdates", "Check for Updates", PackIconKind.Update),
				new HamburgerMenuItem("NewEpisodes", "Check for New Episodes", PackIconKind.OpenInNew),
				new HamburgerMenuItem("DetectLocalSeries", "Detect local series paths", PackIconKind.FileFind),
				new HamburgerMenuItem("Profile", PackIconKind.Account),
				new HamburgerMenuItem("Settings", PackIconKind.Settings),
				new HamburgerMenuItem("Exit", PackIconKind.ExitToApp)
			};

			myTitle = AppGlobal.AssemblyTitle;
			UserEmail = AppGlobal.User.Email;
			Username = AppGlobal.User.Username;

			filterCategory = Categories[0];
			status = "Ready";
			Product = $"Made by {AppGlobal.AssemblyCompany} v{AppGlobal.AssemblyVersion}";

			Collection.Filter += Filter;
		}

		public void RefreshView()
		{
			if (Collection.View == null)
			{
				Collection.Source = AppGlobal.User.Shows;
			}
			else
			{
				Collection.View.Refresh();
			}
		}

		public void RefreshCategory()
		{
			RaisePropertyChanged("Categories");
		}

		private void Filter(object sender, FilterEventArgs e)
		{
			if (e.Item is Show show)
			{
				if (FilterCategory.Name != "All")
				{
					e.Accepted = show.Categories.Contains(FilterCategory);
				}

				if (e.Accepted && !string.IsNullOrWhiteSpace(FilterText))
				{
					e.Accepted = show.HasText(FilterText);
				}
			}
		}

		public void ResetStatus()
		{
			SetStatus("Ready");
		}

		public void SetStatus(string status)
		{
			Status = status;
		}

		private async void ShowWindowAboutDialog(object o)
		{
			var view = new AboutDialog();

			//show the dialog
			var result = await DialogHost.Show(view, "RootDialog");
		}
	}

	public class HamburgerMenuItem : BindableBase
	{
		private string id;
		private string description;
		private PackIconKind icon;

		private object content;

		public string Id
		{
			get => id;
			set => SetProperty(ref id, value);
		}

		public string Description
		{
			get => description;
			set => SetProperty(ref description, value);
		}

		public PackIconKind Icon
		{
			get => icon;
			set => SetProperty(ref icon, value);
		}


		public object Content
		{
			get => content;
			set => SetProperty(ref content, value);
		}

		public HamburgerMenuItem(string id, string description, PackIconKind icon)
		{
			this.id = id;
			this.description = description;
			this.icon = icon;
		}

		public HamburgerMenuItem(string id, PackIconKind icon)
		{
			this.id = id;
			this.description = id;
			this.icon = icon;
		}

		public HamburgerMenuItem(string id, string name, PackIconKind icon, object content) : this(id, name, icon)
		{
			this.content = content;
		}
	}
}
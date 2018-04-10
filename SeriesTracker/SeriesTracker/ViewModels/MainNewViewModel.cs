using MaterialDesignThemes.Wpf;
using Prism.Commands;
using Prism.Mvvm;
using SeriesTracker.Core;
using SeriesTracker.Dialogs;
using SeriesTracker.Enums;
using SeriesTracker.Models;
using SeriesTracker.Utilities.Commands;
using SeriesTracker.Windows;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
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

		private List<Category> categories;
		private Category filterCategory;
		private string filterText;

		private ViewType currentView = ViewType.None;
		private bool dataGridViewVisible;
		private bool gridViewVisible;
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

		public List<Category> Categories
		{
			get => categories;
			set => SetProperty(ref categories, value);
		}
		public Category FilterCategory
		{
			get => filterCategory;
			set
			{
				SetProperty(ref filterCategory, value);
				if (value != null)
					RefreshView();
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

		public ViewType CurrentView
		{
			get => currentView;
			set
			{
				if (currentView == value) return;

				SetProperty(ref currentView, value);

				DataGridViewVisible = currentView == ViewType.DataGrid;
				GridViewVisible = currentView == ViewType.Grid;
			}
		}
		public bool DataGridViewVisible { get => dataGridViewVisible; set => SetProperty(ref dataGridViewVisible, value); }
		public bool GridViewVisible { get => gridViewVisible; set => SetProperty(ref gridViewVisible, value); }

		public string Status
		{
			get => status;
			private set => SetProperty(ref status, value);
		}

		public string Product { get; }

		public ICommand HelpCommand => new WPFCommandImplementation(ShowWindowAboutDialog);
		public ICommand ViewProfileCommand => new DelegateCommand(ViewProfile);
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
				new HamburgerMenuItem("Settings", PackIconKind.Settings),
				new HamburgerMenuItem("Exit", PackIconKind.ExitToApp)
			};

			MyTitle = AppGlobal.User.Username ?? "test";
			UserEmail = AppGlobal.User.Email;
			Username = AppGlobal.User.Username;

			RefreshCategory(false);

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

		public void RefreshCategory(bool refreshMvvm)
		{
			List<Category> cat = new List<Category>
			{
				new Category(0, "All")
			};
			cat.AddRange(AppGlobal.User.Categories.OrderBy(category => category.Name));

			Categories = cat;
			if (filterCategory == null)
			{
				if (refreshMvvm)
					FilterCategory = categories[0];
				else
					filterCategory = categories[0];
			}
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

		#region Commands
		private void ViewProfile()
		{
			WindowMyAccount about = new WindowMyAccount();
			if ((bool)about.ShowDialog())
			{
				Logout();
			}
		}

		private void Logout()
		{
			AppGlobal.User = null;

			Properties.Settings.Default.UserEmail = string.Empty;
			Properties.Settings.Default.UserPassword = string.Empty;
			Properties.Settings.Default.UserRemember = false;
			Properties.Settings.Default.Save();

			Application.Current.MainWindow.Close();
			Application.Current.MainWindow = new WindowLogin();
			Application.Current.MainWindow.Show();

			//Close();
		}

		private async void ShowWindowAboutDialog(object o)
		{
			var view = new AboutDialog();

			//show the dialog
			var result = await DialogHost.Show(view, "RootDialog");
		}
		#endregion
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
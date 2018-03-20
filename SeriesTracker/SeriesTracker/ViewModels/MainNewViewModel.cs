using MaterialDesignThemes.Wpf;
using Prism.Mvvm;
using SeriesTracker.Core;
using SeriesTracker.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace SeriesTracker.ViewModels
{
	internal class MainNewViewModel : BindableBase
	{
		#region Variables
		public HamburgerMenuItem[] DemoItems { get; }

		public CollectionViewSource Collection { get; } = new CollectionViewSource();

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
			get { return myTitle; }
			set { SetProperty(ref myTitle, $"{AppGlobal.AssemblyTitle} - {value}"); }
		}

		public string UserEmail { get; }
		public string Username { get; }

		public Category FilterCategory
		{
			get { return filterCategory; }
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
			get { return filterText; }
			set { SetProperty(ref filterText, value); RefreshView(); }
		}

		public int GridViewColumnCount
		{
			get { return gridViewColumnCount; }
			set { SetProperty(ref gridViewColumnCount, value); }
		}

		public string Product { get; }

		public string Status
		{
			get { return status; }
			set { SetProperty(ref status, value); }
		}
		#endregion
		#endregion

		public MainNewViewModel()
		{
			DemoItems = new[]
			{
				new HamburgerMenuItem("AddSeries", "Add Series", PackIconKind.Account),
				new HamburgerMenuItem("ForceUpdate", "Force Update Series", PackIconKind.Settings),
				new HamburgerMenuItem("Updates", "Check for Updates", PackIconKind.Logout),
				new HamburgerMenuItem("NewEpisodes", "Check for New Episodes", PackIconKind.Logout),
				new HamburgerMenuItem("LocalSeries", "Detect local series paths", PackIconKind.Logout)
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
			Status = "Ready";
		}
	}

	public class HamburgerMenuItem : BindableBase
	{
		private string id;
		private string name;
		private PackIconKind icon;

		public string Id
		{
			get { return id; }
			set { SetProperty(ref id, value); }
		}

		public string Name
		{
			get { return name; }
			set { SetProperty(ref name, value); }
		}

		public PackIconKind Icon
		{
			get { return icon; }
			set { SetProperty(ref icon, value); }
		}

		public HamburgerMenuItem(string id, string name, PackIconKind icon)
		{
			this.id = id;
			this.name = name;
			this.icon = icon;
		}
	}
}
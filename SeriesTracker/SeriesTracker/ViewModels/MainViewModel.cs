using GalaSoft.MvvmLight;
using SeriesTracker.Core;
using SeriesTracker.Models;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Data;

namespace SeriesTracker.ViewModels
{
	internal class MainViewModel : ViewModelBase, IMainViewModel
	{
		#region Variables
		public CollectionViewSource Collection { get; } = new CollectionViewSource();

		#region Fields
		private string myTitle;
		private Category filterCategory;
		private string filterText;
		private int gridViewColumnCount;
		#endregion

		#region Properties
		public string MyTitle
		{
			get { return myTitle; }
			set { Set(ref myTitle, $"{AppGlobal.AssemblyTitle} - {value}"); }
		}

		public string UserEmail { get; }
		public string Username { get; }

		public Category FilterCategory
		{
			get { return filterCategory; }
			set { Set(ref filterCategory, value); RefreshView(); }
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
			set { Set(ref filterText, value); RefreshView(); }
		}

		public int GridViewColumnCount
		{
			get { return gridViewColumnCount; }
			set { Set(ref gridViewColumnCount, value); }
		}

		public string Product { get; }
		#endregion
		#endregion

		public MainViewModel()
		{
			myTitle = AppGlobal.AssemblyTitle;
			UserEmail = AppGlobal.User.Email;
			Username = AppGlobal.User.Username;

			filterCategory = Categories[0];
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
	}
}
using MahApps.Metro.Controls;
using SeriesTracker.Core;
using SeriesTracker.Models;
using SeriesTracker.ViewModels;
using System.Linq;
using System.Windows;

namespace SeriesTracker
{
	public partial class WindowSetCategory : MetroWindow
	{
		#region Variables
		private SetCategoryViewModel MyViewModel;

		private Show SelectedShow { get; }
		#endregion

		public WindowSetCategory(Show show)
		{
			InitializeComponent();

			Owner = Application.Current.MainWindow;
			WindowStartupLocation = WindowStartupLocation.CenterOwner;

			SelectedShow = show;
		}

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			DataContext = MyViewModel = new SetCategoryViewModel();

			if (AppGlobal.User.Categories != null && AppGlobal.User.Categories.Count > 0)
			{
				foreach (Category category in AppGlobal.User.Categories)
				{
					category.IsChecked = SelectedShow.Categories.SingleOrDefault(x => x.Name == category.Name) != null;
				}
			}
			else
			{
				sv_Categories.Visibility = Visibility.Hidden;
				lbl_NoCategories.Visibility = Visibility.Visible;

				btn_Accept.Visibility = Visibility.Hidden;
			}
		}

		private void btn_Accept_Click(object sender, RoutedEventArgs e)
		{
			foreach (Category userCategory in AppGlobal.User.Categories)
			{
				Category showCategory = SelectedShow.Categories.SingleOrDefault(x => x.CategoryID == userCategory.CategoryID);

				if (userCategory.IsChecked && showCategory == null)
				{
					// Add
					var result = AppGlobal.Db.UserShowCategoryAdd(SelectedShow.UserShowID, userCategory);

					SelectedShow.Categories.Add(result.Data);
				}
				else if (!userCategory.IsChecked && showCategory != null)
				{
					// Delete
					var result = AppGlobal.Db.UserShowCategoryDelete(showCategory.UserShowCategoryID);

					SelectedShow.Categories.Remove(showCategory);
				}
				else
				{

				}
			}

			Close();
		}

		private void btn_Cancel_Click(object sender, RoutedEventArgs e)
		{
			Close();
		}
	}
}

using MahApps.Metro.Controls;
using SeriesTracker.ViewModels;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace SeriesTracker.Windows
{
	public partial class WindowTest : MetroWindow
	{
		private TestViewModel MyViewModel;

		#region Window Events
		public WindowTest()
		{
			InitializeComponent();
		}

		private void MetroWindow_Initialized(object sender, System.EventArgs e)
		{
			MyViewModel = DataContext as TestViewModel;

			// Navigate to the home page.
			Navigation.Navigation.Frame = new Frame(); //SplitViewFrame;
			Navigation.Navigation.Frame.Navigated += SplitViewFrame_OnNavigated;
			//this.Loaded += (s, args) => Navigation.Navigation.Navigate(new PageOne());
		}
		#endregion

		#region Navi
		private void SplitViewFrame_OnNavigated(object sender, NavigationEventArgs e)
		{
			HamburgerMenuControl.Content = e.Content;
		}

		private void HamburgerMenuControl_OnItemClick(object sender, ItemClickEventArgs e)
		{
			if (e.ClickedItem is TestMenuItem menuItem && menuItem.IsNavigation)
			{
				Navigation.Navigation.Navigate(menuItem.NavigationDestination);
			}
		}
		#endregion

		//private void HamburgerMenuControl_OnItemClick(object sender, ItemClickEventArgs e)
		//{
		//	HamburgerMenuControl.Content = e.ClickedItem;
		//	HamburgerMenuControl.IsPaneOpen = false;
		//}

	}
}

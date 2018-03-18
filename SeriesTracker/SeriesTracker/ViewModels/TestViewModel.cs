using MahApps.Metro.IconPacks;
using Prism.Mvvm;
using System;
using System.Collections.ObjectModel;

namespace SeriesTracker.ViewModels
{
	internal class TestViewModel : BindableBase
	{
		private static readonly ObservableCollection<TestMenuItem> AppMenu = new ObservableCollection<TestMenuItem>();
		public ObservableCollection<TestMenuItem> Menu => AppMenu;

		private string myTitle;
		public string MyTitle
		{
			get { return myTitle; }
			set { SetProperty(ref myTitle, value); }
		}

		public TestViewModel()
		{
			MyTitle = "Title from VM";

			Menu.Add(new TestMenuItem()
			{
				Icon = new PackIconMaterial() { Kind = PackIconMaterialKind.Account },
				Text = "One",
				NavigationDestination = new Uri("Views/PageOne.xaml", UriKind.RelativeOrAbsolute)
			});
			Menu.Add(new TestMenuItem()
			{
				Icon = new PackIconOcticons() { Kind = PackIconOcticonsKind.Alert },
				Text = "Two",
				NavigationDestination = new Uri("Views/PageTwo.xaml", UriKind.RelativeOrAbsolute)
			});
		}
	}
}

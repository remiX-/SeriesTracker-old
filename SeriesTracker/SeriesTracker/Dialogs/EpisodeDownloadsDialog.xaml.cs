using SeriesTracker.Core;
using SeriesTracker.Models;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace SeriesTracker.Dialogs
{
	public partial class EpisodeDownloadsDialog : UserControl
	{
		public List<EztvTorrent> Items { get; set;  }

		public EpisodeDownloadsDialog() { }

		public EpisodeDownloadsDialog(List<EztvTorrent> eztvTorrents)
		{
			InitializeComponent();

			Items = eztvTorrents;

			DataContext = this;
		}

		private void Button_Click(object sender, RoutedEventArgs e)
		{
			string magnet = ((sender as Button).Parent as DockPanel).Tag.ToString();
			CommonMethods.StartProcess(magnet);
		}
	}
}

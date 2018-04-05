using SeriesTracker.Core;
using SeriesTracker.Models;
using SeriesTracker.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media.Animation;

namespace SeriesTracker.Views
{
	public partial class Seasons : UserControl
	{
		#region Variables
		private ViewShowViewModel MyViewModel;

		private bool busy = false;
		private bool cancel = false;

		private bool treeViewOpen = false;

		private bool loaded = false;
		#endregion

		#region UserControl Events
		public Seasons()
		{
			InitializeComponent();
		}

		private async void UserControl_Loaded(object sender, RoutedEventArgs e)
		{
			if (loaded) return;
			loaded = true;

			MyViewModel = DataContext as ViewShowViewModel;

			LoadEpisodeTreeView();

			await RefreshEpisodeView();
			await GetWatchedEpisodes();
		}
		#endregion

		#region Startup
		private void LoadEpisodeTreeView()
		{
			tv_Seasons.Items.Clear();

			int? lastSeason = MyViewModel.MyShow.Episodes[MyViewModel.MyShow.Episodes.Count - 1].AiredSeason;

			if (!lastSeason.HasValue)
			{
				throw new Exception("invalid season");
			}

			for (int i = 1; i <= lastSeason; i++)
			{
				List<Episode> episodes = MyViewModel.MyShow.GetEpisodesBySeason(i);

				TreeViewItem tvi_season = new TreeViewItem
				{
					Header = string.Format("Season {0} ({1})", i, episodes.Count)
				};

				foreach (Episode episode in episodes)
				{
					TreeViewItem tvi_episode = new TreeViewItem
					{
						Header = string.Format("Episode {0} - {1}", episode.AiredEpisodeNumber, episode.EpisodeName)
					};

					tvi_season.Items.Add(tvi_episode);
				}

				tv_Seasons.Items.Add(tvi_season);
			}
		}

		private async Task GetWatchedEpisodes()
		{
			SeriesResult<UserShowWatch> result = await AppGlobal.Db.UserShowWatchListAsync(MyViewModel.MyShow);

			MyViewModel.MyShow.EpisodesWatched = result.ListData;

			foreach (Episode episode in MyViewModel.MyShow.Episodes)
			{
				UserShowWatch watch = MyViewModel.MyShow.EpisodesWatched.SingleOrDefault(x => x.SeasonNo == episode.AiredSeason && x.EpisodeNo == episode.AiredEpisodeNumber);

				if (watch != null)
				{
					episode.Watched = true;
				}
			}
		}
		#endregion

		#region Season View
		private void Btn_TreeViewToggle_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				treeViewOpen = !treeViewOpen;

				btn_TreeViewToggle.Content = treeViewOpen ? "Collapse" : "Expand";
				SeasonTreeView.Width = new GridLength(treeViewOpen ? 300 : 140);
			}
			catch (Exception ex)
			{
				ErrorMethods.LogError(ex.Message);
			}
		}

		private async void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
		{
			if (busy)
			{
				cancel = true;

				while (busy)
				{
					await Task.Delay(250);
				}
			}

			busy = true;
			TreeViewItem item = (TreeViewItem)tv_Seasons.SelectedItem;
			if (item == null) return;

			string[] split = item.Header.ToString().Split(' ');

			if (split[0] != "Season") return;

			int newSeason = int.Parse(split[1]);
			if (MyViewModel.ViewingSeason == newSeason) return;

			MyViewModel.ViewingSeason = newSeason;
			await RefreshEpisodeView();

			busy = false;
			cancel = false;
		}

		private async void CM_MarkEpisodeWatched_Click(object sender, RoutedEventArgs e)
		{
			TreeViewItem item = (TreeViewItem)tv_Seasons.SelectedItem;
			if (item == null)
				return;

			string[] split = item.Header.ToString().Split(' ');
			string type = split[0];
			int number = int.Parse(split[1]);

			if (type == "Season")
			{

			}
			else
			{
				await EpisodeWatchedToggle(number);
			}
		}

		private async void Btn_EpisodeEyeToggle_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				if (busy)
					return;

				busy = true;

				ToggleButton m = (ToggleButton)sender;
				Grid p = m.Parent as Grid;

				m.IsHitTestVisible = false;

				int episode = int.Parse(p.Tag.ToString());
				await EpisodeWatchedToggle(episode);

				m.IsHitTestVisible = true;
				busy = false;
			}
			catch (Exception ex)
			{
				ErrorMethods.LogError(ex.Message);
			}
		}

		private void Btn_WatchEpisode_Click(object sender, RoutedEventArgs e)
		{
			bool found = false;

			try
			{
				if (string.IsNullOrEmpty(MyViewModel.MyShow.LocalSeriesPath)) return;
				if (!Directory.Exists(MyViewModel.MyShow.LocalSeriesPath)) return;

				Button m = (Button)sender;
				Panel p = (m.Parent as Panel).Parent as Panel;

				int episodeNumber = int.Parse(p.Tag.ToString());

				string seasonPath = Path.Combine(MyViewModel.MyShow.LocalSeriesPath, "Season " + MyViewModel.ViewingSeason);
				var dir = new DirectoryInfo(seasonPath);

				if (!dir.Exists)
					return;

				var files = dir.GetFiles();

				Episode episode = MyViewModel.MyShow.GetEpisode(MyViewModel.ViewingSeason, episodeNumber);
				foreach (FileInfo file in files)
				{
					if (file.Name.Contains(episode.FullEpisodeString))
					{
						found = true;
						var q = CommonMethods.StartProcess(file.FullName);
						// Testing
						//q.EnableRaisingEvents = true;
						//q.Exited += delegate
						//{
						//	TimeSpan watchTime = DateTime.Now - q.StartTime;

						//	MessageBox.Show("You watched for " + watchTime.TotalSeconds + " seconds");
						//};

						break;
					}
				}
			}
			catch (Exception ex)
			{
				ErrorMethods.LogError(ex.Message);
			}

			if (!found)
			{
				MessageBox.Show("Failed to locate episode on local drive.", "Episode not found");
			}
		}

		private async void Btn_Download_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				Button m = (Button)sender;
				Panel p = (m.Parent as Panel).Parent as Panel;

				int episodeNumber = int.Parse(p.Tag.ToString());
				Episode episode = MyViewModel.MyShow.GetEpisode(MyViewModel.ViewingSeason, episodeNumber);

				await MethodCollection.DownloadEpisode(MyViewModel.MyShow, episode);
			}
			catch (Exception ex)
			{
				ErrorMethods.LogError(ex.Message);
			}
		}

		private async Task RefreshEpisodeView()
		{
			Console.WriteLine(DateTime.Now.TimeOfDay);
			MyViewModel.Episodes = MyViewModel.MyShow.GetEpisodesBySeason(MyViewModel.ViewingSeason);

			foreach (Episode episode in MyViewModel.Episodes)
			{
				if (cancel) break;

				FileInfo fi = new FileInfo(episode.LocalImagePath);

				if (!fi.Exists || fi.Length == 0)
				{
					try
					{
						using (WebClient client = new WebClient())
						{
							await client.DownloadFileTaskAsync(new Uri(episode.OnlineImageUrl), episode.LocalImagePath);

							episode.ImageText = "Done";
						}
					}
					catch (WebException ex)
					{
						if (((HttpWebResponse)ex.Response).StatusCode == HttpStatusCode.NotFound)
						{
							File.Delete(episode.LocalImagePath);
						}

						episode.ImageText = "Not Available";
					}
					catch (Exception ex)
					{
						ErrorMethods.LogError(ex.Message);
					}
				}

				episode.RefreshImage();
			}

			Console.WriteLine(DateTime.Now.TimeOfDay);
		}

		private async Task<bool> EpisodeWatchedToggle(int episodeNumber)
		{
			try
			{
				UserShowWatch record = MyViewModel.MyShow
					.EpisodesWatched
					.SingleOrDefault(x => x.SeasonNo == MyViewModel.ViewingSeason && x.EpisodeNo == episodeNumber);

				if (record == null)
				{
					UserShowWatch newRecord = new UserShowWatch
					{
						UserShowID = MyViewModel.MyShow.UserShowID,
						SeasonNo = MyViewModel.ViewingSeason,
						EpisodeNo = episodeNumber
					};
					SeriesResult<UserShowWatch> result = await AppGlobal.Db.UserShowWatchAddAsync(newRecord);

					MyViewModel.MyShow.EpisodesWatched.Add(result.Data);
				}
				else
				{
					SeriesResult<UserShowWatch> result = await AppGlobal.Db.UserShowWatchDeleteAsync(record);

					MyViewModel.MyShow.EpisodesWatched.Remove(record);
				}

				return true;
			}
			catch (Exception ex)
			{
				ErrorMethods.LogError(ex.Message);
			}

			return false;
		}
		#endregion
	}
}
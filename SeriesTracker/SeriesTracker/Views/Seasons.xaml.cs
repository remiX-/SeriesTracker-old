﻿using MahApps.Metro.Controls;
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
	public partial class Seasons : Page
	{
		private ViewShowViewModel MyViewModel;

		private bool busy = false;

		private bool treeViewOpen = false;
		private int viewingSeason = 1;

		#region Page Events
		public Seasons()
		{
			InitializeComponent();
		}

		private async void Page_Loaded(object sender, RoutedEventArgs e)
		{
			MyViewModel = DataContext as ViewShowViewModel;

			LoadEpisodeTreeView();

			await ReloadEpisodeViewAsync();
		}
		#endregion

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

		private async Task ReloadEpisodeViewAsync()
		{
			try
			{
				MyViewModel.Episodes = MyViewModel.MyShow.GetEpisodesBySeason(viewingSeason);

				foreach (Episode episode in MyViewModel.Episodes)
				{
					if (File.Exists(episode.LocalImagePath))
						continue;

					try
					{
						using (WebClient client = new WebClient())
						{
							await client.DownloadFileTaskAsync(new Uri(episode.OnlineImageUrl), episode.LocalImagePath);

							episode.ImageText = "Loaded";
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
				}
			}
			catch (Exception ex)
			{
				ErrorMethods.LogError(ex.Message);
			}
		}

		private async void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
		{
			TreeViewItem item = (TreeViewItem)tv_Seasons.SelectedItem;
			if (item == null)
				return;

			string[] split = item.Header.ToString().Split(' ');

			if (split[0] != "Season")
				return;

			int newSeason = int.Parse(split[1]);
			if (viewingSeason == newSeason)
				return;

			viewingSeason = newSeason;
			await ReloadEpisodeViewAsync();
		}

		private void TreeViewToggle_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				treeViewOpen = !treeViewOpen;

				btn_TreeViewToggle.Content = treeViewOpen ? "Collapse" : "Expand";

				Storyboard sb = Core.FindResource(treeViewOpen ? "OpenMenu" : "CloseMenu") as Storyboard;
				sb.Begin();
			}
			catch (Exception ex)
			{
				ErrorMethods.LogError(ex.Message);
			}
		}

		private async void CM_MarkEpisodeWatched_Click(object sender, RoutedEventArgs e)
		{
			//TreeViewItem item = (TreeViewItem)tv_Seasons.SelectedItem;
			//if (item == null)
			//	return;

			//string[] split = item.Header.ToString().Split(' ');
			//string type = split[0];
			//int number = int.Parse(split[1]);

			//if (type == "Season")
			//{

			//}
			//else
			//{
			//	await ToggleEpisodeWatched(number);
			//}
		}

		private async void EpisodeEyeToggle_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				if (busy)
					return;

				busy = true;

				ToggleButton m = (ToggleButton)sender;
				Grid p = m.TryFindParent<Grid>();

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

		private async Task<bool> EpisodeWatchedToggle(int episodeNumber)
		{
			try
			{
				UserShowWatch record = MyViewModel.MyShow
					.EpisodesWatched
					.SingleOrDefault(x => x.SeasonNo == viewingSeason && x.EpisodeNo == episodeNumber);

				if (record == null)
				{
					UserShowWatch newRecord = new UserShowWatch { UserShowID = MyViewModel.MyShow.UserShowID, SeasonNo = viewingSeason, EpisodeNo = episodeNumber };
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

		private void Btn_WatchEpisode_Click(object sender, RoutedEventArgs e)
		{
			bool found = false;

			try
			{
				if (string.IsNullOrEmpty(MyViewModel.MyShow.LocalSeriesPath)) return;
				if (!Directory.Exists(MyViewModel.MyShow.LocalSeriesPath)) return;

				Button m = (Button)sender;
				Grid p = m.TryFindParent<Grid>();

				int episodeNumber = int.Parse(p.Tag.ToString());

				string seasonPath = Path.Combine(MyViewModel.MyShow.LocalSeriesPath, "Season " + viewingSeason);
				var dir = new DirectoryInfo(seasonPath);

				if (!dir.Exists)
					return;

				var files = dir.GetFiles();

				Episode episode = MyViewModel.MyShow.GetEpisode(viewingSeason, episodeNumber);
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
				Grid p = m.TryFindParent<Grid>();

				int episodeNumber = int.Parse(p.Tag.ToString());
				Episode episode = MyViewModel.MyShow.GetEpisode(viewingSeason, episodeNumber);

				await MethodCollection.DownloadEpisode(MyViewModel.MyShow, episode);
			}
			catch (Exception ex)
			{
				ErrorMethods.LogError(ex.Message);
			}
		}
	}
}
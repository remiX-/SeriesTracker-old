using CsQuery;
using MahApps.Metro.Controls;
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
using System.Windows.Navigation;

namespace SeriesTracker
{
	public partial class WindowView : MetroWindow
	{
		#region Variables
		private ViewShowViewModel MyViewModel;
		private Show ViewingShow;

		private bool hasLoaded = false;
		private bool busy = false;

		private int[,] actorResize = { { 1050, 2 }, { 1250, 3 }, { 1450, 4 }, { 0, 5 } };

		private bool treeViewOpen = true;
		private int viewingSeason = 1;
		#endregion

		#region Window Events
		public WindowView(Show show)
		{
			InitializeComponent();

			ViewingShow = show;
		}

		private async void Window_Loaded(object sender, RoutedEventArgs e)
		{
			//MyViewModel = new ViewShowViewModel(ViewingShow);
			//DataContext = MyViewModel;

			//Width = AppGlobal.Settings.LayoutViewShow.Width;
			//Height = AppGlobal.Settings.LayoutViewShow.Height;
			//Left = (SystemParameters.PrimaryScreenWidth - Width) / 2;
			//Top = (SystemParameters.PrimaryScreenHeight - Height) / 2;

			//btn_TreeViewToggle_Click(null, null);

			//await Startup();
		}

		private void Window_Activated(object sender, EventArgs e)
		{
			if (!hasLoaded)
			{
				WindowState = AppGlobal.Settings.LayoutViewShow.Maximized ? WindowState.Maximized : WindowState.Normal;

				hasLoaded = true;
			}
		}

		private void Window_Closed(object sender, EventArgs e)
		{
			if (WindowState != WindowState.Maximized)
			{
				AppGlobal.Settings.LayoutViewShow.Width = Width;
				AppGlobal.Settings.LayoutViewShow.Height = Height;
			}

			AppGlobal.Settings.LayoutViewShow.Maximized = WindowState == WindowState.Maximized;
			AppGlobal.Settings.Save();
		}

		private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
		{
			if (e.PreviousSize.Height == 0 && e.PreviousSize.Width == 0) return;

			for (int i = 0; i < actorResize.GetLength(0); i++)
			{
				if (e.NewSize.Width <= actorResize[i, 0] || i == actorResize.GetLength(0) - 1)
				{
					if (MyViewModel.CastColumnCount != actorResize[i, 1])
					{
						MyViewModel.CastColumnCount = actorResize[i, 1];
					}

					break;
				}
			}
		}
		#endregion

		#region Startup
		private async Task Startup()
		{
			SetupDirectories();
			LoadEpisodeTreeView();

			await GetWatchedEpisodes();
			await Task.WhenAll(LoadBannerAsync(), LoadCastAsync(), LoadImdbAsync(), ReloadEpisodeViewAsync());
		}

		private void SetupDirectories()
		{
			if (!Directory.Exists(ViewingShow.LocalImagesActorsPath)) Directory.CreateDirectory(ViewingShow.LocalImagesActorsPath);
			if (!Directory.Exists(ViewingShow.LocalImagesEpisodesPath)) Directory.CreateDirectory(ViewingShow.LocalImagesEpisodesPath);
			if (!Directory.Exists(ViewingShow.LocalImagesPostersPath)) Directory.CreateDirectory(ViewingShow.LocalImagesPostersPath);
			if (!Directory.Exists(ViewingShow.LocalImagesBannersPath)) Directory.CreateDirectory(ViewingShow.LocalImagesBannersPath);
		}

		private void LoadEpisodeTreeView()
		{
			tv_Seasons.Items.Clear();

			int? lastSeason = ViewingShow.Episodes[ViewingShow.Episodes.Count - 1].AiredSeason;

			if (!lastSeason.HasValue)
			{
				throw new Exception("invalid season");
			}

			for (int i = 1; i <= lastSeason; i++)
			{
				List<Episode> episodes = ViewingShow.GetEpisodesBySeason(i);

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

		private async Task LoadBannerAsync()
		{
			try
			{
				Models.Image banner = ViewingShow.Banners.First();
				if (!File.Exists(ViewingShow.LocalBannerPath) && !File.Exists(banner.LocalImagePath))
				{
					using (WebClient client = new WebClient())
					{
						await client.DownloadFileTaskAsync(new Uri(banner.OnlineImageUrl), banner.LocalImagePath);
					}

					if (File.Exists(ViewingShow.LocalBannerPath))
						File.Delete(ViewingShow.LocalBannerPath);

					File.Copy(banner.LocalImagePath, ViewingShow.LocalBannerPath);

					MyViewModel.RefreshBanner();
				}
			}
			catch (Exception ex)
			{
				ErrorMethods.LogError(ex.Message);
			}
		}

		private async Task LoadCastAsync()
		{
			try
			{
				foreach (Actor actor in ViewingShow.Actors)
				{
					if (!File.Exists(actor.LocalImagePath))
					{
						using (WebClient client = new WebClient())
						{
							await client.DownloadFileTaskAsync(new Uri(actor.OnlineImageUrl), actor.LocalImagePath);
						}
					}

					actor.RefreshImage();
				}
			}
			catch (Exception ex)
			{
				ErrorMethods.LogError(ex.Message);
			}
		}

		private async Task LoadImdbAsync()
		{
			using (WebClient client = new WebClient())
			{
				CQ htmlText = await client.DownloadStringTaskAsync(ViewingShow.GetIMDbLink());

				if (htmlText != null)
				{
					var rating = htmlText.Select("span[itemprop='ratingValue']");
					lbl_IMDBUserRating.Content = rating.Html() + "/10";
				}
			}
		}

		private async Task ReloadEpisodeViewAsync()
		{
			try
			{
				lbl_SeasonHeader.Content = "Season " + viewingSeason;

				MyViewModel.Episodes = ViewingShow.GetEpisodesBySeason(viewingSeason);

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

		private async Task GetWatchedEpisodes()
		{
			SeriesResult<UserShowWatch> result = await AppGlobal.Db.UserShowWatchListAsync(ViewingShow);

			ViewingShow.EpisodesWatched = result.ListData;

			foreach (Episode episode in ViewingShow.Episodes)
			{
				UserShowWatch watch = ViewingShow.EpisodesWatched.SingleOrDefault(x => x.SeasonNo == episode.AiredSeason && x.EpisodeNo == episode.AiredEpisodeNumber);

				if (watch != null)
				{
					episode.Watched = true;
				}
			}
		}
		#endregion

		#region General
		public async Task SetShow(Show show)
		{
			ViewingShow = show;

			MyViewModel.SetShow(show);

			await Startup();
		}

		private void Imdb_RequestNavigate(object sender, RequestNavigateEventArgs e)
		{
			e.Handled = true;

			CommonMethods.StartProcess(e.Uri.AbsoluteUri);
		}

		private void btn_SwitchView_Click(object sender, RoutedEventArgs e)
		{
			grid_Overview.Visibility = grid_Overview.Visibility == Visibility.Visible ? Visibility.Hidden : Visibility.Visible;
			grid_Episodes.Visibility = grid_Episodes.Visibility == Visibility.Visible ? Visibility.Hidden : Visibility.Visible;
		}
		#endregion

		#region Episode View
		private async void tv_Seasons_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
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

		private void btn_TreeViewToggle_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				treeViewOpen = !treeViewOpen;

				btn_TreeViewToggle.Content = treeViewOpen ? "Collapse" : "Expand";

				Storyboard sb = grid_Episodes.FindResource(treeViewOpen ? "OpenMenu" : "CloseMenu") as Storyboard;
				sb.Begin();
			}
			catch (Exception ex)
			{
				ErrorMethods.LogError(ex.Message);
			}
		}

		private async void cm_MarkEpisodeWatched_Click(object sender, RoutedEventArgs e)
		{
			return;

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
				await ToggleEpisodeWatched(number);
			}
		}

		private async void EpisodeWatchToggle(object sender, RoutedEventArgs e)
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
				await ToggleEpisodeWatched(episode);

				m.IsHitTestVisible = true;
				busy = false;
			}
			catch (Exception ex)
			{
				ErrorMethods.LogError(ex.Message);
			}
		}

		private async Task<bool> ToggleEpisodeWatched(int episodeNumber)
		{
			try
			{
				UserShowWatch record = ViewingShow.EpisodesWatched.SingleOrDefault(x => x.SeasonNo == viewingSeason && x.EpisodeNo == episodeNumber);

				if (record == null)
				{
					UserShowWatch newRecord = new UserShowWatch { UserShowID = ViewingShow.UserShowID, SeasonNo = viewingSeason, EpisodeNo = episodeNumber };
					SeriesResult<UserShowWatch> result = await AppGlobal.Db.UserShowWatchAddAsync(newRecord);

					ViewingShow.EpisodesWatched.Add(result.Data);
				}
				else
				{
					SeriesResult<UserShowWatch> result = await AppGlobal.Db.UserShowWatchDeleteAsync(record);

					ViewingShow.EpisodesWatched.Remove(record);
				}

				return true;
			}
			catch (Exception ex)
			{
				ErrorMethods.LogError(ex.Message);
			}

			return false;
		}

		private void btn_Watch_Click(object sender, RoutedEventArgs e)
		{
			bool found = false;

			try
			{
				if (string.IsNullOrEmpty(ViewingShow.LocalSeriesPath))
					return;
				if (!Directory.Exists(ViewingShow.LocalSeriesPath))
					return;

				Button m = (Button)sender;
				Grid p = m.TryFindParent<Grid>();

				int episodeNumber = int.Parse(p.Tag.ToString());

				string seasonPath = Path.Combine(ViewingShow.LocalSeriesPath, "Season " + viewingSeason);
				var dir = new DirectoryInfo(seasonPath);

				if (!dir.Exists)
					return;

				var files = dir.GetFiles();

				Episode episode = ViewingShow.GetEpisode(viewingSeason, episodeNumber);
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

		private async void btn_Download_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				Button m = (Button)sender;
				Grid p = m.TryFindParent<Grid>();

				int episodeNumber = int.Parse(p.Tag.ToString());
				Episode episode = ViewingShow.GetEpisode(viewingSeason, episodeNumber);

				await MethodCollection.DownloadEpisode(ViewingShow, episode);
			}
			catch (Exception ex)
			{
				ErrorMethods.LogError(ex.Message);
			}
		}
		#endregion
	}
}
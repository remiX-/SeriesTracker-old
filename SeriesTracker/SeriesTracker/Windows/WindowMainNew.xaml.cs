using MaterialDesignThemes.Wpf;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SeriesTracker.Comparers;
using SeriesTracker.Core;
using SeriesTracker.Dialogs;
using SeriesTracker.Models;
using SeriesTracker.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using WinForms = System.Windows.Forms;

namespace SeriesTracker.Windows
{
	public partial class WindowMainNew : Window
	{
		#region Variables
		// ViewModel
		private MainNewViewModel MyViewModel;

		// Windows
		private WindowViewShow WindowViewShow;

		private bool windowHasInit = false;

		// Column sorting
		public static List<string> ColumnHeadings { get; private set; }

		// Tray Icon
		private WinForms.NotifyIcon stTrayIcon;
		private WinForms.ContextMenu st_ContextMenu;
		private Timer ShowChecker;

		// Shortcut command keys
		//private Dictionary<Key, Task> commands = new Dictionary<Key, Task>();

		// Grid view settings
		private SeriesView currentView = SeriesView.List;
		private int[,] gridResize = { { 1300, 4 }, { 1500, 5 }, { 0, 6 } };

		public enum SeriesView
		{
			List,
			Detail,
			Grid
		}
		#endregion

		#region Window Events
		public WindowMainNew()
		{
			InitializeComponent();
		}

		private async void Window_Loaded(object sender, RoutedEventArgs e)
		{
			await Startup();

			DemoItemsListBox.SelectionChanged += DemoItemsListBox_SelectionChanged;
		}

		private void Window_Activated(object sender, EventArgs e)
		{
			if (!windowHasInit)
			{
				WindowState = AppGlobal.Settings.LayoutMain.Maximized ? WindowState.Maximized : WindowState.Normal;

				windowHasInit = true;
			}
		}

		private void Window_Closed(object sender, EventArgs e)
		{
			if (WindowState != WindowState.Maximized)
			{
				AppGlobal.Settings.LayoutMain.Width = Width;
				AppGlobal.Settings.LayoutMain.Height = Height;
			}

			AppGlobal.Settings.LayoutMain.Maximized = WindowState == WindowState.Maximized;
			AppGlobal.Settings.SaveColumnSetting(view_DataGridView.Columns.ToList(), false);
			AppGlobal.Settings.Save();

			if (WindowViewShow != null && WindowViewShow.IsLoaded)
			{
				WindowViewShow.Close();
			}

			stTrayIcon.Dispose();
			stTrayIcon = null;
		}

		private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
		{
			//if (e.PreviousSize.Height == 0 && e.PreviousSize.Width == 0) return;

			//for (int i = 0; i < gridResize.GetLength(0); i++)
			//{
			//	if (e.NewSize.Width <= gridResize[i, 0] || i == gridResize.GetLength(0) - 1)
			//	{
			//		if (MyViewModel.GridViewColumnCount != gridResize[i, 1])
			//		{
			//			MyViewModel.GridViewColumnCount = gridResize[i, 1];
			//		}

			//		break;
			//	}
			//}
		}

		private void Window_StateChanged(object sender, EventArgs e)
		{
			if (WindowState == WindowState.Minimized)
			{
				Hide();

				st_ContextMenu.MenuItems[0].Text = "&Open";
			}
			else if (WindowState == WindowState.Normal)
			{
				st_ContextMenu.MenuItems[0].Text = "&Minimize";
			}
		}
		#endregion

		#region Action Events
		private void ClearFilter_Click(object sender, RoutedEventArgs e)
		{
			txt_FilterText.Text = string.Empty;
			txt_FilterText.Focus();
		}
		#endregion

		#region Tray Icon Events
		private void NotifyIcon_MouseDoubleClick(object sender, WinForms.MouseEventArgs e)
		{
			if (e == null || e.Button == WinForms.MouseButtons.Left)
			{
				if (WindowState == WindowState.Normal)
				{
					WindowState = WindowState.Minimized;
				}
				else
				{
					Show();

					WindowState = WindowState.Normal;
				}
			}
		}
		#endregion

		#region Startup
		private async Task Startup()
		{
			try
			{
				LoadWindowSettings();
				SetupTrayIcon();
				SetupShortcuts();
				SetupNetChange();
				//SetupShowTimer();

				await SetupTvdbAPI();
				await SetupStructure();
				await CheckForMissingLocalData();

				LoadSeries();

				MyViewModel.RefreshView();
				SortDataGrid(AppGlobal.Settings.DefaultSortColumn, AppGlobal.Settings.DefaultSortDirection);

				//if (await CheckForShowUpdates())
				//	MyViewModel.RefreshView();

				MyViewModel.ResetStatus();
			}
			catch (Exception ex)
			{
				ErrorMethods.LogError(ex.Message);
			}
		}

		private void LoadWindowSettings()
		{
			MyViewModel = DataContext as MainNewViewModel;

			view_DataGridView.Visibility = currentView == SeriesView.List ? Visibility.Visible : Visibility.Hidden;
			//view_DetailView.Visibility = currentView == SeriesView.Detail ? Visibility.Visible : Visibility.Hidden;
			//view_GridView.Visibility = currentView == SeriesView.Grid ? Visibility.Visible : Visibility.Hidden;

			Width = AppGlobal.Settings.LayoutMain.Width;
			Height = AppGlobal.Settings.LayoutMain.Height;
			Left = (SystemParameters.PrimaryScreenWidth - Width) / 2;
			Top = (SystemParameters.PrimaryScreenHeight - Height) / 2;

			//view_DataGridView.Sorting += DataGrid_Sorting;

			// Column settings
			var columns = view_DataGridView.Columns.ToList();
			ColumnHeadings = columns.Select(x => x.Header.ToString()).ToList();

			if (!ColumnHeadings.Contains(AppGlobal.Settings.DefaultSortColumn))
			{
				AppGlobal.Settings.DefaultSortColumn = "Name";
			}

			if (AppGlobal.Settings.ColumnSettings.Count == 0 || AppGlobal.Settings.ColumnSettings.Count != columns.Count)
			{
				AppGlobal.Settings.SaveColumnSetting(columns, true);
			}
			else
			{
				bool tampered = false;
				foreach (DataGridColumn column in columns)
				{
					ColumnSetting setting = AppGlobal.Settings.ColumnSettings.SingleOrDefault(x => x.Name == column.Header.ToString());

					if (setting == null)
					{
						tampered = true;
						break;
					}
				}

				if (!tampered)
				{
					columns.ForEach(column =>
					{
						ColumnSetting setting = AppGlobal.Settings.ColumnSettings.SingleOrDefault(s => s.Name == column.Header.ToString());
						column.Width = setting.Width;
						column.Visibility = setting.Visible ? Visibility.Visible : Visibility.Collapsed;
					});
				}
				else
				{
					AppGlobal.Settings.SaveColumnSetting(columns, true);
				}
			}
		}

		private void SetupTrayIcon()
		{
			st_ContextMenu = new WinForms.ContextMenu();
			st_ContextMenu.MenuItems.Add(new WinForms.MenuItem("&Minimize", delegate { NotifyIcon_MouseDoubleClick(this, null); }));
			st_ContextMenu.MenuItems.Add("-");
			st_ContextMenu.MenuItems.Add(new WinForms.MenuItem("&Exit", delegate { Close(); }));

			stTrayIcon = new WinForms.NotifyIcon
			{
				BalloonTipTitle = "SeriesTracker",
				Text = "SeriesTracker",
				Icon = Properties.Resources.mainIcon,
				Visible = true,
				ContextMenu = st_ContextMenu
			};
			stTrayIcon.MouseDoubleClick += NotifyIcon_MouseDoubleClick;
		}

		private void SetupShortcuts()
		{
			//commands.Add(Key.S, Menu_Settings_Click);

			//commands.Add(Key.N, await AddSeries);
			//commands.Add(Key.O, Menu_Series_ForceUpdate_Click);
			//commands.Add(Key.U, Menu_Series_CheckForUpdates_Click);
			//commands.Add(Key.E, Menu_Series_CheckForNewEpisodes_Click);
			//commands.Add(Key.L, Menu_Series_DetectLocal_Click);

			//commands.Add(Key.C, CM_Copy_Click);

			//commands.Add(Key.F, delegate { txt_FilterText.Focus(); });

			//foreach (var kvp in commands)
			//{
			//	RoutedCommand newCmd = new RoutedCommand();
			//	newCmd.InputGestures.Add(new KeyGesture(kvp.Key, ModifierKeys.Control));
			//	CommandBindings.Add(new CommandBinding(newCmd, kvp.Value));
			//}
		}

		private void SetupNetChange()
		{
			NetworkChange.NetworkAvailabilityChanged += (sender, e) =>
			{
				if (!e.IsAvailable)
				{
					MyViewModel.MyTitle = "Offline";
					//PopupNotification("Internet connection lost. Some features have been disabled");
				}
			};
		}

		private async Task SetupTvdbAPI()
		{
			MyViewModel.SetStatus("Contacting TheTVDb API");

			try
			{
				TvdbAPI data = new TvdbAPI
				{
					Token = "eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9.eyJleHAiOjE1MjE1NjAzMTIsImlkIjoiIiwib3JpZ19pYXQiOjE1MjE0NzM5MTJ9.2EJfa2BaDK6HELVqIiBp05os-bnvqdpNcVK-GkO2YM-cAxU3RXhHTBkvZUD02Bk9Zsby6NTxc-Tgqc3y5ftuL198BIAP5iZf_bdI9P262vBhrVwUL0a2zKhUZue3pTrNMOEiXiwu8ZOrOMuNF0qVFXd8HIO-Ax2S3K1lD4TZmujg6KGo4sW0DtSvN40spNID7DRw1cvJ7ye8xfznz1jnqg_H5Rxef1U7ASavuACX-puDZ29fADwR27ZYrb67oYqCywRhiNXuJskuFBeMuXkzofQEo9tygEN3L_cYS5S8UUxngI8InXSlNRw0_d1B7QSIYXxN7YEE5zEYO5nTpprKAg"
				};

				JObject jObject = new JObject { ["apikey"] = AppGlobal.thetvAPIKey };
				data = await Request.ExecuteAndDeserializeAsync<TvdbAPI>("POST", "https://api.thetvdb.com/login", jObject.ToString());

				if (data != null && !string.IsNullOrEmpty(data.Token))
				{
					AppGlobal.thetvdbToken = data.Token;
				}
				else
				{
					GoOfflineMode();

					MessageBox.Show("Failed to contact theTVDB api!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
				}
			}
			catch (Exception ex)
			{
				ErrorMethods.LogError(ex.Message);
			}
		}

		private async Task SetupStructure()
		{
			MyViewModel.SetStatus("Setting up Structure");

			try
			{
				// Check local series folder directory
				if (!string.IsNullOrEmpty(AppGlobal.Paths.LocalSeriesDirectory) && !Directory.Exists(AppGlobal.Paths.LocalSeriesDirectory))
				{
					AppGlobal.Settings.LocalSeriesFolder = "";
					AppGlobal.Settings.Save();

					MessageBox.Show("Series path '" + AppGlobal.Paths.LocalSeriesDirectory + "' does not exist or cannot be read. Please set a new path.");
				}

				// Check for files
				if (!File.Exists(AppGlobal.Paths.EztvIDFile) || DateTime.Now > Properties.Settings.Default.EztvLastUpdate.AddDays(7))
				{
					await MethodCollection.UpdateEztvShowFileAsync();

					Properties.Settings.Default.EztvLastUpdate = DateTime.Now;
					Properties.Settings.Default.Save();
				}
			}
			catch (Exception ex)
			{
				ErrorMethods.LogError(ex.Message);
			}
		}

		private void SetupShowTimer()
		{
			// Show Timer
			ShowChecker = new Timer(x =>
			{
				MessageBox.Show("Timer triggered");
			});

			// Figure how much time until trigger time
			DateTime triggerTime = DateTime.Today.Add(new TimeSpan(16, 50, 0));

			// If it's already past 4:00, wait until 4:00 tomorrow    
			if (DateTime.Now > triggerTime)
			{
				triggerTime = triggerTime.AddDays(1);
			}

			TimeSpan triggerDiff = triggerTime - DateTime.Now;

			string popup;
			if (triggerDiff.Hours > 0)
				popup = string.Format("Checking shows in {0} hour{1} {2} minute{3}", triggerDiff.Hours, triggerDiff.Hours == 1 ? "" : "s", triggerDiff.Minutes, triggerDiff.Minutes == 1 ? "" : "s");
			else
				popup = string.Format("Checking shows in {0} minute{1}", triggerDiff.Minutes + 1, triggerDiff.Minutes + 1 == 1 ? "" : "s");

			//PopupNotification(popup);

			// Set the timer to elapse only once, at 4:00.
			ShowChecker.Change((int)(triggerDiff.TotalMilliseconds), Timeout.Infinite);
		}

		private void GoOfflineMode()
		{

		}
		#endregion

		#region Series
		private async Task UpdateShows(List<Show> shows)
		{
			try
			{
				shows.ForEach(s => s.Updating = true);

				int counter = 1;
				foreach (Show show in shows)
				{
					MyViewModel.SetStatus($"Downloading data {counter++}/{shows.Count} - {show.SeriesName}");


					await MethodCollection.RetrieveTvdbDataForSeriesAsync(show.Id);

					show.Updating = false;
				}

				MyViewModel.ResetStatus();
			}
			catch (Exception ex)
			{
				ErrorMethods.LogError(ex.Message);
			}
		}

		private void LoadSeries()
		{
			try
			{
				foreach (Show show in AppGlobal.User.Shows)
				{
					string jsonData = File.ReadAllText(show.LocalJsonPath);
					Show item = JsonConvert.DeserializeObject<Show>(jsonData);

					show.Overview = item.Overview;
					show.Genre = item.Genre;

					show.FirstAired = item.FirstAired;
					show.Runtime = item.Runtime;
					show.Network = item.Network;
					show.Status = item.Status;

					show.AirsDayOfWeek = item.AirsDayOfWeek;
					show.AirsTime = item.AirsTime;
					show.Rating = item.Rating;

					show.ImdbId = item.ImdbId;
					show.Zap2itId = item.Zap2itId;

					show.Actors = item.Actors;
					show.Episodes = item.Episodes;
					show.Posters = item.Posters;
					show.Banners = item.Banners;

					show.DoWork();
				}
			}
			catch (Exception ex)
			{
				ErrorMethods.LogError(ex.Message);
			}
		}

		private async Task CheckForMissingLocalData()
		{
			MyViewModel.SetStatus("Verifying local data");

			try
			{
				List<Show> missingShows = new List<Show>();
				foreach (Show show in AppGlobal.User.Shows)
				{
					string tvdbPath = Path.Combine(AppGlobal.Paths.SeriesDirectory, show.Id.ToString());

					if (!Directory.Exists(tvdbPath) || !File.Exists(show.LocalJsonPath))
						missingShows.Add(show);
				}

				if (missingShows.Count > 0)
				{
					int counter = 1;
					foreach (Show show in missingShows)
					{
						MyViewModel.SetStatus($"Downloading data {counter++}/{missingShows.Count} - {show.SeriesName}");

						await MethodCollection.RetrieveTvdbDataForSeriesAsync(show.Id);
					}
				}
			}
			catch (Exception ex)
			{
				ErrorMethods.LogError(ex.Message);
			}
		}

		private async Task<bool> CheckForShowUpdates()
		{
			MyViewModel.SetStatus("Checking for show updates");

			bool didUpdate = false;

			// Check for show updates
			string url = string.Format("https://api.thetvdb.com/updated/query?fromTime={0}", Properties.Settings.Default.TvdbUpdateEpochTime);
			TvdbAPI jsonData = await Request.ExecuteAndDeserializeAsync<TvdbAPI>("GET", url);

			if (jsonData.Data != null)
			{
				List<TvdbUpdate> tvdbUpdates = JsonConvert.DeserializeObject<List<TvdbUpdate>>(jsonData.Data.ToString());

				string _s = "Need to update {0} shows:";
				List<Show> updates = new List<Show>();

				foreach (TvdbUpdate update in tvdbUpdates)
				{
					Show showToUpdate = AppGlobal.User.Shows.SingleOrDefault(x => x.Id == update.Id);

					if (showToUpdate != null)
					{
						_s += "\n" + showToUpdate.SeriesName;

						updates.Add(showToUpdate);
					}
				}

				if (updates.Count > 0)
				{
					didUpdate = true;

					await UpdateShows(updates);
				}
			}

			// Update last updated
			Properties.Settings.Default.TvdbUpdateEpochTime = CommonMethods.GetEpochTime();
			Properties.Settings.Default.Save();

			MyViewModel.ResetStatus();

			return didUpdate;
		}

		private async Task CheckForUpdates()
		{
			await CheckForMissingLocalData();
			await CheckForShowUpdates();
		}

		private void AutoDetectLocalSeriesPaths()
		{
			// Try auto find local series paths for series ona ccount
			ProgressDialogResult result = ProgressDialog.Execute(this, "Loading data", () =>
			{
				string seriesFolderPath = AppGlobal.Paths.LocalSeriesDirectory;

				if (string.IsNullOrEmpty(seriesFolderPath))
					return;

				// Get directory information of series folder
				DirectoryInfo dInfo = new DirectoryInfo(seriesFolderPath);
				DirectoryInfo[] series = dInfo.GetDirectories();

				try
				{
					if (AppGlobal.Settings.LocalSeriesPaths == null || AppGlobal.Settings.LocalSeriesPaths.Count > 0)
					{
						AppGlobal.Settings.ClearSeriesPaths();
					}

					int count = 1;
					foreach (Show show in AppGlobal.User.Shows)
					{
						int pacent = (int)((double)count++ / AppGlobal.User.Shows.Count * 100);
						ProgressDialog.Current.Report(pacent, "Checking {0}", show.SeriesName);

						// Look for exact match first
						DirectoryInfo showDir = series.SingleOrDefault(x => x.Name == show.SeriesName);

						// Match of how shows are displayed in the list
						if (showDir == null) showDir = series.SingleOrDefault(x => x.Name == show.DisplayName);

						// Match of names without brackets
						if (showDir == null) showDir = series.SingleOrDefault(x => x.Name == show.GetNameWithoutBrackets());

						// Match of listed names (the at end)
						if (showDir == null) showDir = series.SingleOrDefault(x => x.Name == show.GetListedName());

						// Match for ignoring case
						if (showDir == null) showDir = series.SingleOrDefault(x => x.Name.ToLower() == show.SeriesName.ToLower());

						// Match for ignoring periods
						if (showDir == null) showDir = series.SingleOrDefault(x => x.Name.Replace(".", "") == show.SeriesName.Replace(".", ""));
						if (showDir == null) showDir = series.SingleOrDefault(x => x.Name.Replace(".", "") == show.DisplayName.Replace(".", ""));

						// Match for ignoring '
						if (showDir == null) showDir = series.SingleOrDefault(x => x.Name.Replace("'", "") == show.SeriesName.Replace("'", ""));
						if (showDir == null) showDir = series.SingleOrDefault(x => x.Name.Replace("'", "") == show.DisplayName.Replace("'", ""));

						// Check to see if there's a directory for the show
						if (showDir != null)
						{
							show.LocalSeriesPath = showDir.FullName;

							// Add to settings
							AppGlobal.Settings.AddSeriesPath(show.Id, showDir.FullName);
						}

						Thread.Sleep(100);
					}

					AppGlobal.Settings.Save();
				}
				catch (Exception)
				{

				}

			}, ProgressDialogSettings.WithSubLabel);

			if (result.OperationFailed)
				MessageBox.Show("ProgressDialog failed.");
		}
		#endregion

		#region DataGrid List
		private void DataGrid_Sorting(object sender, DataGridSortingEventArgs e)
		{
			e.Handled = true;

			SortDataGrid(e.Column);
		}

		private void SortDataGrid(string columnName, ListSortDirection? direction = null)
		{
			SortDataGrid(view_DataGridView.Columns.Single(x => x.Header.ToString() == columnName), direction);
		}

		private void SortDataGrid(DataGridColumn column, ListSortDirection? direction = null)
		{
			try
			{
				if (direction == null)
					direction = (column.SortDirection != ListSortDirection.Ascending) ? ListSortDirection.Ascending : ListSortDirection.Descending;
				column.SortDirection = direction;

				ListCollectionView lcv = (ListCollectionView)MyViewModel.Collection.View;
				lcv.CustomSort = new DefaultComparer(column.Header.ToString(), (ListSortDirection)direction);
			}
			catch (Exception ex)
			{
				ErrorMethods.LogError(ex.Message);
			}
		}

		private void DataGrid_ContextMenu_Opened(object sender, RoutedEventArgs e)
		{
			//ContextMenu cm = (ContextMenu)sender;

			//if (view_DataGridView.SelectedItems.Count == 1)
			//{
			//	cm.FindChild<MenuItem>("View").Visibility = Visibility.Visible;
			//	cm.FindChild<MenuItem>("SetCategory").Visibility = Visibility.Visible;
			//}
			//else if (view_DataGridView.SelectedItems.Count > 1)
			//{
			//	cm.FindChild<MenuItem>("View").Visibility = Visibility.Collapsed;
			//	cm.FindChild<MenuItem>("SetCategory").Visibility = Visibility.Collapsed;
			//}
		}

		private void DataGrid_RowDoubleClick(object sender, MouseButtonEventArgs e)
		{
			if (sender == null)
				return;

			CM_View_Click(null, null);
		}

		private async void CM_View_Click(object sender, RoutedEventArgs e)
		{
			if (view_DataGridView.SelectedItem != null)
			{
				if (WindowViewShow == null || !WindowViewShow.IsLoaded)
				{
					//WindowViewShow = new WindowViewShow((Show)view_DataGridView.SelectedItem);
					WindowViewShow = new WindowViewShow((Show)view_DataGridView.SelectedItem);
					WindowViewShow.Show();
				}
				else
				{
					//await WindowViewShow.SetShow((Show)view_DataGridView.SelectedItem);
				}
			}
		}

		private async void CM_UpdateAll_Click(object sender, RoutedEventArgs e)
		{
			await UpdateShows(AppGlobal.User.Shows);
		}

		private async void CM_UpdateSelected_Click(object sender, RoutedEventArgs e)
		{
			await UpdateShows(view_DataGridView.SelectedItems.Cast<Show>().ToList());
		}

		private void CM_Copy_Click(object sender, RoutedEventArgs e)
		{
			if (view_DataGridView.SelectedItems.Count > 0)
			{
				string returnText = "";

				for (int i = 0; i < view_DataGridView.SelectedItems.Count; i++)
				{
					Show item = (Show)view_DataGridView.SelectedItems[i];

					returnText += string.Format("{0}{1}", "Name:".PadRight(20), item.SeriesName);
					returnText += string.Format("\n{0}{1}", "Status:".PadRight(20), item.Status);
					if (item.Status == "Continuing")
					{
						returnText += string.Format("\n{0}{1}", "Next Episode:".PadRight(20), item.NextEpisodeDisplay);
						returnText += string.Format("\n{0}{1} ({2})", "Next Air Date:".PadRight(20), item.NextEpisodeDateDisplay, item.HowLongDisplay);
					}

					if (i < view_DataGridView.SelectedItems.Count - 1)
						returnText += "\n\n";
				}

				Clipboard.SetText(returnText);

				//PopupNotification("Text copied to clipboard");
			}
		}

		private async void CM_Delete_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				string text = "Are you sure you would like to delete " + view_DataGridView.SelectedItems.Count + " show" + (view_DataGridView.SelectedItems.Count == 1 ? "" : "s") + "?";

				if (MessageBox.Show(text, "Confirm deletion", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
				{
					List<Show> toDelete = view_DataGridView.SelectedItems.Cast<Show>().ToList();

					foreach (Show show in toDelete)
					{
						SeriesResult<Show> result = await AppGlobal.Db.UserShowDeleteAsync(show.UserShowID);
						if (result.Result == SQLResult.Success)
						{
							AppGlobal.User.Shows.Remove(show);

							AppGlobal.Settings.RemoveSeriesPath(show.Id);
							AppGlobal.Settings.Save();
						}
						else
						{
							MessageBox.Show("Something went wrong!\n\n" + result.Message);
						}
					}

					MyViewModel.RefreshView();
				}
			}
			catch (Exception ex)
			{
				ErrorMethods.LogError(ex.Message);
			}
		}

		private void CM_OpenIMDB_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				List<Show> selected = view_DataGridView.SelectedItems.Cast<Show>().ToList();
				foreach (Show sel in selected)
				{
					CommonMethods.StartProcess(sel.GetIMDbLink());
				}
			}
			catch (Exception ex)
			{
				ErrorMethods.LogError(ex.Message);
			}
		}

		private void CM_Eztv_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				List<Show> selected = view_DataGridView.SelectedItems.Cast<Show>().ToList();
				foreach (Show sel in selected)
				{
					if (sel.HasEztvData())
					{
						CommonMethods.StartProcess(sel.GetEZTVLink());
					}
					else
					{
						MessageBox.Show("No Eztv data found for " + sel.SeriesName);
					}
				}
			}
			catch (Exception ex)
			{
				ErrorMethods.LogError(ex.Message);
			}
		}

		private void CM_OpenFolder_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				List<Show> selected = view_DataGridView.SelectedItems.Cast<Show>().ToList();

				foreach (Show show in selected)
				{
					OpenSeriesFolder(show);
				}
			}
			catch (Exception ex)
			{
				ErrorMethods.LogError(ex.Message);
			}
		}

		private void CM_SetCategory_Click(object sender, RoutedEventArgs e)
		{
			WindowSetCategory w = new WindowSetCategory(view_DataGridView.SelectedItem as Show);
			w.ShowDialog();
		}

		private async void CM_DownloadLastEpisode_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				if (MyViewModel.IsBusy) return;

				Show show = view_DataGridView.SelectedItem as Show;

				MyViewModel.SetStatus($"Finding magnets for {show.SeriesName} {show.LatestEpisode.FullEpisodeString}");

				var torrents = await MethodCollection.GetEpisodeTorrentList(show, show.LatestEpisode);
				if (torrents == null || torrents.Count == 0)
				{
					PopupNotification($"Failed to find last episode for {show.SeriesName}");
					return;
				}

				var view = new EpisodeDownloadsDialog(torrents);

				//show the dialog
				await DialogHost.Show(view, "RootDialog");

				MyViewModel.ResetStatus();

				//List<Show> shows = view_DataGridView.SelectedItems.Cast<Show>().ToList();

				//foreach (Show show in shows)
				//{
				//	bool success = await MethodCollection.DownloadEpisode(show, show.LatestEpisode);
				//	if (!success)
				//	{
				//		MessageBox.Show("Failed to find last episode for " + show.SeriesName);
				//	}
				//}
			}
			catch (Exception ex)
			{
				ErrorMethods.LogError(ex.Message);
			}
		}

		private void OpenSeriesFolder(Show show)
		{
			if (string.IsNullOrWhiteSpace(show.LocalSeriesPath) || !Directory.Exists(show.LocalSeriesPath))
			{
				WinForms.FolderBrowserDialog fbd = new WinForms.FolderBrowserDialog
				{
					Description = "Select the folder where your series are stored",
					RootFolder = Environment.SpecialFolder.Desktop,
					SelectedPath = AppGlobal.Paths.LocalSeriesDirectory.Replace('/', '\\')
				};

				if (fbd.ShowDialog() == WinForms.DialogResult.OK)
				{
					show.LocalSeriesPath = fbd.SelectedPath;

					AppGlobal.Settings.AddSeriesPath(show.Id, show.LocalSeriesPath);
					AppGlobal.Settings.Save();
				}
			}

			if (!string.IsNullOrWhiteSpace(show.LocalSeriesPath))
				CommonMethods.StartProcess(show.LocalSeriesPath);
		}
		#endregion

		#region Misc
		private void PopupNotification(string content)
		{
			stTrayIcon.BalloonTipText = content;
			stTrayIcon.ShowBalloonTip(0);
		}
		#endregion

		#region HamburgerMenu
		private async Task AddSeries()
		{

			// 257655 - arrow
			// 279121 - flash

			WindowAddShow win = new WindowAddShow();
			win.ShowDialog();

			if (win.selectedShow == null)
				return;

			MyViewModel.SetStatus($"Loading data for {win.selectedShow.SeriesName}");

			Show show = await MethodCollection.RetrieveTvdbDataForSeriesAsync(win.selectedShow.Id);

			// Add to database
			SeriesResult<Show> result = await AppGlobal.Db.UserShowAddAsync(show);
			show.UserShowID = result.Data.UserShowID;

			AppGlobal.User.Shows.Add(show);

			// Reload UI
			MyViewModel.RefreshView();

			// Notification
			PopupNotification(show.SeriesName + " has been added");

			MyViewModel.ResetStatus();

			//ProgressDialogResult dialogResult = ProgressDialog.Execute(this, string.Format("Loading data for {0}", win.selectedShow.SeriesName), async () =>
			//{
			//	Show show = await MethodCollection.RetrieveTvdbDataForSeriesAsync(win.selectedShow.Id);

			//	// Add to database
			//	SeriesResult<Show> result = await AppGlobal.Db.UserShowAddAsync(show);
			//	show.UserShowID = result.Data.UserShowID;

			//	AppGlobal.User.Shows.Add(show);

			//	// Reload UI
			//	Dispatcher.Invoke(() => MyViewModel.RefreshView());

			//	// Notification
			//	PopupNotification(show.SeriesName + " has been added");
			//}, ProgressDialogSettings.WithSubLabel);

			//if (dialogResult.OperationFailed)
			//MessageBox.Show("ProgressDialog failed.");
		}

		private async Task CheckForNewEpisodes()
		{
			try
			{
				MyViewModel.SetStatus("Looking for new episodes");

				string text = "Opening ";
				int seriesFound = 0;

				foreach (Show show in AppGlobal.User.Shows)
				{
					Episode episode = show.LatestEpisode;

					if (episode == null) continue;
					if (episode.AirDate.Value.Date != DateTime.Now.Date) continue;

					await MethodCollection.DownloadEpisode(show, episode);

					text += show.DisplayName + ", ";

					seriesFound++;
				}

				PopupNotification(seriesFound == 0 ? "No shows found" : text.Substring(0, text.Length - 2));

				MyViewModel.ResetStatus();
			}
			catch (Exception ex)
			{
				ErrorMethods.LogError(ex.Message);
			}
		}

		private async Task DetectLocalSeriesPaths()
		{
			if (!string.IsNullOrWhiteSpace(AppGlobal.Paths.LocalSeriesDirectory))
			{
				AutoDetectLocalSeriesPaths();
			}
			else
			{
				MessageBox.Show("Enter local series folder in settings first");
			}
		}

		private async void DemoItemsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (DemoItemsListBox.SelectedItem == null) return;

			var selected = DemoItemsListBox.SelectedItem as HamburgerMenuItem;
			switch (selected.Id)
			{
				case "AddSeries": await AddSeries(); break;
				case "ForceUpdate": await UpdateShows(AppGlobal.User.Shows); break;
				case "Updates": await CheckForUpdates(); break;
				case "NewEpisodes": await CheckForNewEpisodes(); break;
				case "LocalSeries": await DetectLocalSeriesPaths(); break;
				default:
					break;
			}

			// Deselect item
			DemoItemsListBox.SelectedItem = null;

			// Close drawer
			MenuToggleButton.IsChecked = false;
		}

		private async void MenuPopupButton_OnClick(object sender, RoutedEventArgs e)
		{
			var sampleMessageDialog = new SampleMessageDialog
			{
				Message = { Text = "Hello" }
			};

			await DialogHost.Show(sampleMessageDialog, "RootDialog");
		}
		#endregion
	}
}
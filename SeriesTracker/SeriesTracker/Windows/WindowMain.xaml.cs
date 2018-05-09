using MaterialDesignThemes.Wpf;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SeriesTracker.Core;
using SeriesTracker.Dialogs;
using SeriesTracker.Enums;
using SeriesTracker.Models;
using SeriesTracker.Utilities.Comparers;
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
	public partial class WindowMain : Window
	{
		#region Variables
		// ViewModel
		private MainViewModel MyViewModel;

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
		private Dictionary<Key, ExecutedRoutedEventHandler> commands = new Dictionary<Key, ExecutedRoutedEventHandler>();

		// Grid view settings
		private int[,] gridResize = { { 1300, 4 }, { 1500, 5 }, { 0, 6 } };
		#endregion

		#region Window Events
		public WindowMain()
		{
			InitializeComponent();
		}

		private async void Window_Loaded(object sender, RoutedEventArgs e)
		{
			await Startup();

			AppMenuListBox.SelectionChanged += AppMenuListBox_SelectionChanged;
		}

		private void Window_Activated(object sender, EventArgs e)
		{
			if (!windowHasInit)
			{
				WindowState = AppGlobal.Settings.Windows["Main"].Maximized ? WindowState.Maximized : WindowState.Normal;

				windowHasInit = true;
			}
		}

		private void Window_Closed(object sender, EventArgs e)
		{
			if (WindowState != WindowState.Maximized)
			{
				AppGlobal.Settings.Windows["Main"].Width = Width;
				AppGlobal.Settings.Windows["Main"].Height = Height;
			}

			AppGlobal.Settings.Windows["Main"].Maximized = WindowState == WindowState.Maximized;
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
			if (e.PreviousSize.Height == 0 && e.PreviousSize.Width == 0) return;

			for (int i = 0; i < gridResize.GetLength(0); i++)
			{
				if (e.NewSize.Width <= gridResize[i, 0] || i == gridResize.GetLength(0) - 1)
				{
					if (MyViewModel.GridViewColumnCount != gridResize[i, 1])
					{
						MyViewModel.GridViewColumnCount = gridResize[i, 1];
					}

					break;
				}
			}
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
			if (txt_FilterText.Text == string.Empty) return;

			txt_FilterText.Text = string.Empty;
			txt_FilterText.Focus();
		}
		#endregion

		#region Tray Icon Events
		private void NotifyIcon_MouseDoubleClick(object sender, WinForms.MouseEventArgs e)
		{
			if (e != null && e.Button == WinForms.MouseButtons.Left)
			{
				if (WindowState == WindowState.Minimized)
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

				await Task.Delay(1000);

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
			MyViewModel = DataContext as MainViewModel;
			MyViewModel.SetStatus("Starting up");

			// Setup events
			StateChanged += Window_StateChanged;
			SizeChanged += Window_SizeChanged;
			Closed += Window_Closed;

			MyViewModel.CurrentView = ViewType.DataGrid;

			Width = AppGlobal.Settings.Windows["Main"].Width;
			Height = AppGlobal.Settings.Windows["Main"].Height;
			Left = (SystemParameters.PrimaryScreenWidth - Width) / 2;
			Top = (SystemParameters.PrimaryScreenHeight - Height) / 2;

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
			commands.Add(Key.N, async delegate { await DoAppMenuAction("AddSeries"); });
			commands.Add(Key.O, async delegate { await DoAppMenuAction("ForceUpdateSeries"); });
			commands.Add(Key.U, async delegate { await DoAppMenuAction("CheckUpdates"); });
			commands.Add(Key.E, async delegate { await DoAppMenuAction("NewEpisodes"); });
			commands.Add(Key.L, async delegate { await DoAppMenuAction("DetectLocalSeries"); });

			commands.Add(Key.P, async delegate { await DoAppMenuAction("Profile"); });
			commands.Add(Key.S, async delegate { await DoAppMenuAction("Settings"); });
			commands.Add(Key.X, async delegate { await DoAppMenuAction("Exit"); });

			commands.Add(Key.C, delegate { CM_Copy_Click(null, null); });

			commands.Add(Key.F, delegate { txt_FilterText.Focus(); });

			foreach (var kvp in commands)
			{
				RoutedCommand newCmd = new RoutedCommand();
				newCmd.InputGestures.Add(new KeyGesture(kvp.Key, ModifierKeys.Control));
				CommandBindings.Add(new CommandBinding(newCmd, kvp.Value));
			}
		}

		private void SetupNetChange()
		{
			NetworkChange.NetworkAvailabilityChanged += (sender, e) =>
			{
				if (!e.IsAvailable)
				{
					MyViewModel.MyTitle = "Offline";
					PopupNotification("Internet connection lost. Some features have been disabled");
				}
			};
		}

		private async Task SetupTvdbAPI()
		{
			MyViewModel.SetStatus("Contacting TheTVDb API");

			try
			{
				TimeSpan diff = DateTime.Now.Subtract(Properties.Settings.Default.TvdbTokenTime);
				if (!string.IsNullOrWhiteSpace(Properties.Settings.Default.TvdbToken) && diff.TotalHours < 20)
				{
					AppGlobal.thetvdbToken = Properties.Settings.Default.TvdbToken;
					return;
				}

				JObject jObject = new JObject { ["apikey"] = AppPrivate.thetvdbAPIKey };
				ReturnResult<TvdbAPI> data = await Request.ExecuteAndDeserializeAsync<TvdbAPI>("POST", "https://api.thetvdb.com/login", jObject.ToString());

				if (data.Result != null && !string.IsNullOrEmpty(data.Result.Token))
				{
					MainSnackbar.MessageQueue.Enqueue("New TVDb Token :-D");

					Properties.Settings.Default.TvdbTokenTime = DateTime.Now;
					Properties.Settings.Default.TvdbToken = data.Result.Token;
					Properties.Settings.Default.Save();

					AppGlobal.thetvdbToken = data.Result.Token;
				}
				else
				{
					GoOfflineMode();

					PopupNotification("Failed to contact theTVDB api!");
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
					MessageBox.Show($"Series path '{AppGlobal.Paths.LocalSeriesDirectory}' does not exist or cannot be read. Please set a new path.");

					AppGlobal.Settings.LocalSeriesFolder = "";
					AppGlobal.Settings.Save();
				}

				// Check for files
				if (!File.Exists(AppGlobal.Paths.EztvIDFile) || DateTime.Now > Properties.Settings.Default.EztvLastUpdate.AddDays(7))
				{
					MyViewModel.SetStatus("Updating Eztv data");

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
			ReturnResult<TvdbAPI> jsonData = await Request.ExecuteAndDeserializeAsync<TvdbAPI>("GET", url);

			if (jsonData.Result.Data != null)
			{
				List<TvdbUpdate> tvdbUpdates = JsonConvert.DeserializeObject<List<TvdbUpdate>>(jsonData.Result.Data.ToString());

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
			ContextMenu cm = (ContextMenu)sender;

			if (view_DataGridView.SelectedItems.Count == 1)
			{
				foreach (var item in cm.Items)
				{
					MenuItem _item = item as MenuItem;
					if (_item == null) continue;
					if (_item.Name == "View" || _item.Name == "SetCategory")
					{
						_item.Visibility = Visibility.Visible;
					}
				}
			}
			else if (view_DataGridView.SelectedItems.Count > 1)
			{
				foreach (var item in cm.Items)
				{
					MenuItem _item = item as MenuItem;
					if (_item == null) continue;
					if (_item.Name == "View" || _item.Name == "SetCategory")
					{
						_item.Visibility = Visibility.Collapsed;
					}
				}
			}
		}

		private void DataGrid_RowDoubleClick(object sender, MouseButtonEventArgs e)
		{
			if (sender == null)
				return;

			CM_View_Click(null, null);
		}

		private void CM_View_Click(object sender, RoutedEventArgs e)
		{
			if (view_DataGridView.SelectedItem != null)
			{
				if (WindowViewShow == null || !WindowViewShow.IsLoaded)
				{
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

				PopupNotification("Text copied to clipboard");
				MainSnackbar.MessageQueue.Enqueue("Text copied to clipboard");
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
					MyViewModel.ResetStatus();
					return;
				}

				await DialogHost.Show(new EpisodeDownloadsDialog(torrents), "RootDialog");

				MyViewModel.ResetStatus();
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
		private async void AppMenuListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (AppMenuListBox.SelectedItem == null) return;

			// Close drawer
			MenuToggleButton.IsChecked = false;

			var selected = AppMenuListBox.SelectedItem as HamburgerMenuItem;
			// Do action
			await DoAppMenuAction(selected.Id);

			// Deselect item
			AppMenuListBox.SelectedItem = null;
		}

		private async Task AddSeries()
		{
			// 257655 - arrow
			// 279121 - flash

			WindowAddShow win = new WindowAddShow();
			win.ShowDialog();

			if (win.SelectedShow == null)
				return;

			MyViewModel.SetStatus($"Loading data for {win.SelectedShow.SeriesName}");

			Show show = await MethodCollection.RetrieveTvdbDataForSeriesAsync(win.SelectedShow.Id);

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

		private async Task CheckForUpdates()
		{
			await CheckForMissingLocalData();
			await CheckForShowUpdates();
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

		private void DetectLocalSeriesPaths()
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

		private void OpenSettings()
		{
			WindowSettings win = new WindowSettings() { Owner = this };
			win.CloseHandler += SettingsClosedHandler;
			win.ShowDialog();
		}

		private async Task DoAppMenuAction(string selected)
		{
			if (MyViewModel.IsBusy) return;

			switch (selected)
			{
				case "AddSeries": await AddSeries(); break;
				case "ForceUpdateSeries": await UpdateShows(AppGlobal.User.Shows); break;
				case "CheckUpdates": await CheckForUpdates(); break;
				case "NewEpisodes": await CheckForNewEpisodes(); break;
				case "DetectLocalSeries": DetectLocalSeriesPaths(); break;
				case "Settings": OpenSettings(); break;
				case "Exit": Close(); break;
				default: break;
			}
		}

		private void MenuPopupButton_OnClick(object sender, RoutedEventArgs e)
		{
			var obj = sender as MenuItem;

			if (obj.Header.ToString() == "DataGridView")
			{
				MyViewModel.CurrentView = ViewType.DataGrid;
			}
			else
			{
				MyViewModel.CurrentView = ViewType.Grid;
			}
		}

		private void SettingsClosedHandler(List<string> changes)
		{
			if (changes.Contains("UpdateFolders"))
			{
				AutoDetectLocalSeriesPaths();
			}

			if (changes.Contains("ReloadView"))
			{
				MyViewModel.RefreshView();
			}

			if (changes.Contains("UpdateCategory"))
			{
				MyViewModel.RefreshCategory(true);
			}

			//if (changes.Contains("UpdateTheme"))
			//{
			//	//ThemeManager.ChangeAppStyle(Application.Current, ThemeManager.GetAccent(AppGlobal.Settings.Accent), ThemeManager.GetAppTheme(AppGlobal.Settings.Theme));
			//}
		}
		#endregion

		#region Test
		private async void MenuPopupButton_Test_OnClick(object sender, RoutedEventArgs e)
		{
			CommonMethods.PlayNotification();
			//await StartProgress();
		}

		private async Task StartProgress()
		{
			var result = await DialogHost.Show(new Dialogs.ProgressDialog(), "RootDialog", OpenedEventHandler);
		}

		private async void OpenedEventHandler(object sender, DialogOpenedEventArgs eventArgs)
		{
			await Task.Delay(3000);
			MainSnackbar.MessageQueue.Enqueue("Welcome to Material Design In XAML Tookit");

			eventArgs.Session.Close(false);
			//Task.Delay(TimeSpan.FromSeconds(3))
			//	.ContinueWith((t, _) => eventArgs.Session.Close(false), null, TaskScheduler.FromCurrentSynchronizationContext());
		}
		#endregion
	}
}
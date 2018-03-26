using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SeriesTracker.Comparers;
using SeriesTracker.Core;
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
	public partial class WindowMainOld : Window
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
		private SeriesView currentView = SeriesView.List;
		private int[,] gridResize = { { 1300, 4 }, { 1500, 5 }, { 0, 6 } };

		public enum SeriesView
		{
			List,
			Detail,
			Grid
		}
		#endregion

		#region Main window
		#region Window Events
		public WindowMainOld()
		{
			InitializeComponent();
		}

		private async void Window_Loaded(object sender, RoutedEventArgs e)
		{
			await Startup();

			//if (AppGlobal.User.Shows.Count == 0)
			//{
			//	var result = await this.ShowMessageAsync("Hello!", "Your list looks a bit empty there!\nWould you like to add your first series now?",
			//		MessageDialogStyle.AffirmativeAndNegative,
			//		new MetroDialogSettings
			//		{
			//			AffirmativeButtonText = "Yes please!",
			//			NegativeButtonText = "Maybe later",
			//			ColorScheme = MetroDialogColorScheme.Theme,
			//			AnimateHide = false,
			//			DefaultButtonFocus = MessageDialogResult.Affirmative
			//		});

			//	if (result == MessageDialogResult.Affirmative)
			//	{
			//		Menu_Series_AddShow_Click(null, null);
			//	}
			//}
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

		#region RightWindowCommands
		private void Menu_Settings_Click(object sender, RoutedEventArgs e)
		{
			WindowSettings win = new WindowSettings() { Owner = this };
			win.CloseHandler += SettingsClosedHandler;
			win.ShowDialog();
		}

		private void Menu_Account_View_Click(object sender, RoutedEventArgs e)
		{
			WindowMyAccount about = new WindowMyAccount();
			if ((bool)about.ShowDialog())
			{
				Menu_Account_Logout_Click(null, null);
			}
		}

		private async void Menu_Account_Sync_Click(object sender, RoutedEventArgs e)
		{
			await CheckForUpdates();
		}

		private void Menu_Account_Logout_Click(object sender, RoutedEventArgs e)
		{
			AppGlobal.User = null;

			Properties.Settings.Default.UserEmail = string.Empty;
			Properties.Settings.Default.UserPassword = string.Empty;
			Properties.Settings.Default.UserRemember = false;
			Properties.Settings.Default.Save();

			Window m = new WindowLogin();
			m.Show();
			Application.Current.MainWindow = m;

			Close();
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
				MyViewModel.RefreshCategory();
			}

			if (changes.Contains("UpdateTheme"))
			{
				//ThemeManager.ChangeAppStyle(Application.Current, ThemeManager.GetAccent(AppGlobal.Settings.Accent), ThemeManager.GetAppTheme(AppGlobal.Settings.Theme));
			}
		}
		#endregion

		#region Action events
		private void ClearFilter_Click(object sender, RoutedEventArgs e)
		{
			//txt_FilterText.Text = string.Empty;
			//txt_FilterText.Focus();
		}

		private void Image_List_MouseDown(object sender, MouseButtonEventArgs e)
		{
			if (currentView == SeriesView.List)
				return;

			currentView = SeriesView.List;

			view_DataGridView.Visibility = Visibility.Visible;
			view_DetailView.Visibility = Visibility.Hidden;
			view_GridView.Visibility = Visibility.Hidden;
		}

		private void Image_Detail_MouseDown(object sender, MouseButtonEventArgs e)
		{
			if (currentView == SeriesView.Detail)
				return;

			currentView = SeriesView.Detail;

			view_DataGridView.Visibility = Visibility.Hidden;
			view_DetailView.Visibility = Visibility.Visible;
			view_GridView.Visibility = Visibility.Hidden;
		}

		private void Image_Grid_MouseDown(object sender, MouseButtonEventArgs e)
		{
			if (currentView == SeriesView.Grid)
				return;

			currentView = SeriesView.Grid;

			view_DataGridView.Visibility = Visibility.Hidden;
			view_DetailView.Visibility = Visibility.Hidden;
			view_GridView.Visibility = Visibility.Visible;

			//TESTPANEL.Visibility = Visibility.Visible;
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
		#endregion

		#region Menu Events
		private void Menu_File_Exit_Click(object sender, RoutedEventArgs e)
		{
			Close();
		}

		private async void Menu_Series_AddShow_Click(object sender, RoutedEventArgs e)
		{
			// 257655 - arrow
			// 279121 - flash

			WindowAddShow win = new WindowAddShow();
			win.ShowDialog();

			if (win.selectedShow == null)
				return;

			SetViewOverlayText("Loading data for {0}", win.selectedShow.SeriesName);

			Show show = await MethodCollection.RetrieveTvdbDataForSeriesAsync(win.selectedShow.Id);

			// Add to database
			SeriesResult<Show> result = await AppGlobal.Db.UserShowAddAsync(show);
			show.UserShowID = result.Data.UserShowID;

			AppGlobal.User.Shows.Add(show);

			// Reload UI
			MyViewModel.RefreshView();

			// Notification
			PopupNotification(show.SeriesName + " has been added");

			HideViewOverlay();

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

		private async void Menu_Series_ForceUpdate_Click(object sender, RoutedEventArgs e)
		{
			await UpdateShows(AppGlobal.User.Shows);
		}

		private async void Menu_Series_CheckForUpdates_Click(object sender, RoutedEventArgs e)
		{
			await CheckForUpdates();
		}

		private async void Menu_Series_CheckForNewEpisodes_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				SetViewOverlayText("Looking for new episodes");

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

				HideViewOverlay();
			}
			catch (Exception ex)
			{
				ErrorMethods.LogError(ex.Message);
			}
		}

		private void Menu_Series_DetectLocal_Click(object sender, RoutedEventArgs e)
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
				lcv.CustomSort = new DefaultComparer(column.Header.ToString(), (ListSortDirection)direction); ;
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

				PopupNotification("Text copied to clipboard");
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
				List<Show> shows = view_DataGridView.SelectedItems.Cast<Show>().ToList();

				foreach (Show show in shows)
				{
					bool success = await MethodCollection.DownloadEpisode(show, show.LatestEpisode);
					if (!success)
					{
						MessageBox.Show("Failed to find last episode for " + show.SeriesName);
					}
				}
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

		#region Detail View
		private void DetailView_OpenFolder_Click(object sender, RoutedEventArgs e)
		{
			//Button b = (Button)sender;
			//Grid p = b.TryFindParent<Grid>();

			//int userShowID = int.Parse(p.Tag.ToString());
			//Show show = AppGlobal.User.Shows.Single(s => s.UserShowID == userShowID);

			//OpenSeriesFolder(show);
		}

		private async void DetailView_DownloadLatest_Click(object sender, RoutedEventArgs e)
		{
			//Button b = (Button)sender;
			//Grid p = b.TryFindParent<Grid>();

			//int userShowID = int.Parse(p.Tag.ToString());
			//Show show = AppGlobal.User.Shows.Single(s => s.UserShowID == userShowID);

			//bool success = await MethodCollection.DownloadEpisode(show, show.LatestEpisode);
			//if (!success)
			//{
			//	MessageBox.Show("Failed to find last episode for " + show.SeriesName);
			//}
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

				HideViewOverlay();
			}
			catch (Exception ex)
			{
				ErrorMethods.LogError(ex.Message);
			}
		}

		private void LoadWindowSettings()
		{
			DataContext = MyViewModel = new MainViewModel();

			view_DataGridView.Visibility = currentView == SeriesView.List ? Visibility.Visible : Visibility.Hidden;
			view_DetailView.Visibility = currentView == SeriesView.Detail ? Visibility.Visible : Visibility.Hidden;
			view_GridView.Visibility = currentView == SeriesView.Grid ? Visibility.Visible : Visibility.Hidden;

			Width = AppGlobal.Settings.LayoutMain.Width;
			Height = AppGlobal.Settings.LayoutMain.Height;
			Left = (SystemParameters.PrimaryScreenWidth - Width) / 2;
			Top = (SystemParameters.PrimaryScreenHeight - Height) / 2;

			view_DataGridView.Sorting += DataGrid_Sorting;

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
			commands.Add(Key.S, Menu_Settings_Click);

			commands.Add(Key.N, Menu_Series_AddShow_Click);
			commands.Add(Key.O, Menu_Series_ForceUpdate_Click);
			commands.Add(Key.U, Menu_Series_CheckForUpdates_Click);
			commands.Add(Key.E, Menu_Series_CheckForNewEpisodes_Click);
			commands.Add(Key.L, Menu_Series_DetectLocal_Click);

			commands.Add(Key.C, CM_Copy_Click);

			//commands.Add(Key.F, delegate { txt_FilterText.Focus(); });

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
			SetViewOverlayText("Contacting theTVDB api");

			try
			{
				TvdbAPI data = new TvdbAPI
				{
					Token = "eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9.eyJleHAiOjE1MjEzOTE4MjMsImlkIjoiIiwib3JpZ19pYXQiOjE1MjEzMDU0MjN9.NSjcWGyXbKtejG6aw07yr2xVtY1GKeBsZi1VO-vxScTl9-Dtjn1Fw30wlakP333yjcnxV2e_h_P7MWAls3NXaqiBv5fxfhX2kRyB5kG3mra9AVTEkIZ26LndeUcyTDtyHGldmPWRNyWCa3mHQlC2k7zQcqm2KzddA-HGMAIyjw9n0PBhbba6ZFAgJLFdFdXYL8OeZqRTojxUr-3vVPlSFMEwspzzzLohSlt2XLeUBcgY866k3Qhx9bdUFiK456-Y-kLzG0gkewknBC84bw35AO1r2mDKWX5uCqebWnlgsbzK6Kae3qcm4v-xLju_T-kCJvIp4xT9EoCmESeqKu6cuA"
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
			SetViewOverlayText("Setting up structure");

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

			PopupNotification(popup);

			// Set the timer to elapse only once, at 4:00.
			ShowChecker.Change((int)(triggerDiff.TotalMilliseconds), Timeout.Infinite);
		}

		private void GoOfflineMode()
		{

		}
		#endregion

		#region Shows
		private async Task UpdateShows(List<Show> shows)
		{
			try
			{
				shows.ForEach(s => s.Updating = true);

				int counter = 1;
				foreach (Show show in shows)
				{
					string overlayText = string.Format("Updating {0}/{1} - {2}", counter++, shows.Count, show.SeriesName);
					SetViewOverlayText(overlayText);

					await MethodCollection.RetrieveTvdbDataForSeriesAsync(show.Id);
						
					show.Updating = false;
				}

				HideViewOverlay();
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
			SetViewOverlayText("Verifying local data");

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
						SetViewOverlayText(string.Format("Downloading data {0}/{1} - {2}", counter++, missingShows.Count, show.SeriesName));

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
			SetViewOverlayText("Checking for show updates");

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

			HideViewOverlay();

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

						//Thread.Sleep(250);
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

		#region Misc
		private void PopupNotification(string content)
		{
			stTrayIcon.BalloonTipText = content;
			stTrayIcon.ShowBalloonTip(0);
		}

		private void HideViewOverlay()
		{
			Dispatcher.Invoke(() => Overlay.Visibility = Visibility.Hidden );
		}

		private void SetViewOverlayText(string text, params string[] args)
		{
			if (!string.IsNullOrWhiteSpace(text))
			{
				Dispatcher.Invoke(() =>
				{
					if (Overlay.Visibility == Visibility.Hidden)
						Overlay.Visibility = Visibility.Visible;

					lbl_OverlayText.Content = string.Format(text, args);
				});
			}
		}
		#endregion

		#region TESTING
		private async Task ReAddSeriesFromFile()
		{
			// Re-add all series from file
			string[] allseries = File.ReadAllLines("allseries.txt");

			int counter = 1;
			foreach (string sid in allseries)
			{
				SetViewOverlayText(string.Format("Adding {0} - {1}/{2}", sid, counter, allseries.Length));

				int id = int.Parse(sid);

				Show show = await MethodCollection.RetrieveTvdbDataForSeriesAsync(id);

				SeriesResult<Show> r = await AppGlobal.Db.UserShowAddAsync(show);
				show.UserShowID = r.Data.UserShowID;

				if (AppGlobal.User.Shows == null)
					AppGlobal.User.Shows = new List<Show>();

				AppGlobal.User.Shows.Add(show);

				counter++;
			}

			HideViewOverlay();
		}

		private void DownloadImagesForShow(int id, string type)
		{
			/*
			 * TYPES
			 * fanart
			 * poster
			 * season
			 * seasonwide
			 * series
			 * 
			 * ID's
			 * Arrow - 257655
			 */

			//string text = txt_Test.Text;
			//string[] parts = text.Split('|');

			//string tvdbID = parts[0];
			//string type = parts[1];

			//ProgressDialogResult result = ProgressDialog.Execute(this, "Downloading images for " + id, () =>
			//{
			//	ProgressDialog.Current.Report("Getting data from thetvdb.com ...");

			//	Show show = new Show();
			//	Request request = new Request(AppGlobal.thetvdbToken)
			//	{
			//		URL = string.Format("https://api.thetvdb.com/series/{0}/images/query?keyType={1}", id, type)
			//	};
			//	object response = request.Execute();
			//	tvdbAPI Data = JsonConvert.DeserializeObject<tvdbAPI>(response.ToString());

			//	show.Posters = JsonConvert.DeserializeObject<List<Image>>(Data.data.ToString());

			//	// Download images
			//	string rootPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "Images");
			//	string seriesPath = Path.Combine(rootPath, id.ToString());
			//	string savePath = Path.Combine(seriesPath, type);

			//	if (!Directory.Exists(rootPath))
			//		Directory.CreateDirectory(rootPath);
			//	if (!Directory.Exists(seriesPath))
			//		Directory.CreateDirectory(seriesPath);
			//	if (!Directory.Exists(savePath))
			//		Directory.CreateDirectory(savePath);

			//	int count = 1;
			//	foreach (Image image in show.Posters)
			//	{
			//		ProgressDialog.Current.Report("Downloading image {0}/{1}", count, show.Posters.Count);

			//		string url = Path.Combine(AppGlobal.posterURL, image.FileName);
			//		string imagePath = Path.Combine(savePath, Path.GetFileName(image.FileName));

			//		using (System.Net.WebClient client = new System.Net.WebClient())
			//		{
			//			client.DownloadFile(new Uri(url), imagePath);
			//		}

			//		count++;
			//	}
			//}, ProgressDialogSettings.WithSubLabel);
		}

		private void Btn_Test1_Click(object sender, RoutedEventArgs e)
		{
			//Window w = new WindowTestMD();
			//w.ShowDialog();
		}

		private void Btn_Test2_Click(object sender, RoutedEventArgs e)
		{
			WindowViewShow = new WindowViewShow((Show)view_DataGridView.Items[0]);
			WindowViewShow.Show();
		}
		#endregion
	}
}
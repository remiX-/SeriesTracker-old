using CsQuery;
using SeriesTracker.Core;
using SeriesTracker.Models;
using SeriesTracker.ViewModels;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace SeriesTracker.Views
{
	public partial class Overview : Page
	{
		private ViewShowViewModel MyViewModel;

		#region Page Events
		public Overview()
		{
			InitializeComponent();
		}

		private async void Page_Loaded(object sender, RoutedEventArgs e)
		{
			MyViewModel = DataContext as ViewShowViewModel;

			await Task.WhenAll(LoadBannerAsync(), LoadCastAsync(), LoadImdbAsync());
		}
		#endregion

		private void Imdb_RequestNavigate(object sender, RequestNavigateEventArgs e)
		{
			e.Handled = true;

			CommonMethods.StartProcess(e.Uri.AbsoluteUri);
		}

		private async Task LoadBannerAsync()
		{
			try
			{
				Models.Image banner = MyViewModel.MyShow.Banners.First();
				if (!File.Exists(MyViewModel.MyShow.LocalBannerPath) && !File.Exists(banner.LocalImagePath))
				{
					using (WebClient client = new WebClient())
					{
						await client.DownloadFileTaskAsync(new Uri(banner.OnlineImageUrl), banner.LocalImagePath);
					}

					if (File.Exists(MyViewModel.MyShow.LocalBannerPath))
						File.Delete(MyViewModel.MyShow.LocalBannerPath);

					File.Copy(banner.LocalImagePath, MyViewModel.MyShow.LocalBannerPath);

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
				foreach (Actor actor in MyViewModel.MyShow.Actors)
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
				CQ htmlText = await client.DownloadStringTaskAsync(MyViewModel.MyShow.GetIMDbLink());

				if (htmlText != null)
				{
					var rating = htmlText.Select("span[itemprop='ratingValue']");
					lbl_IMDBUserRating.Content = rating.Html() + "/10";
				}
			}
		}
	}
}
using Newtonsoft.Json;
using GalaSoft.MvvmLight;
using SeriesTracker.Core;
using System;
using System.IO;
using System.Windows.Media.Imaging;

namespace SeriesTracker.Models
{
	public class Actor : ViewModelBase
	{
		#region Variables
		private int id;
		private int seriesId;
		private string name;
		private string role;
		private int? sortOrder;
		private string image;

		private int? imageAuthor;
		private string imageAdded;
		private string lastUpdated;

		public int Id
		{
			get => id;
			set => id = value;
		}
		public int SeriesId
		{
			get => seriesId;
			set => seriesId = value;
		}
		public string Name
		{
			get => name;
			set => name = value;
		}
		public string Role
		{
			get => role;
			set => role = value;
		}
		public int? SortOrder
		{
			get => sortOrder;
			set => sortOrder = value;
		}
		public string Image
		{
			get => image;
			set => image = value;
		}

		public int? ImageAuthor
		{
			get => imageAuthor;
			set => imageAuthor = value;
		}
		public string ImageAdded
		{
			get => imageAdded;
			set => imageAdded = value;
		}
		public string LastUpdated
		{
			get => lastUpdated;
			set => lastUpdated = value;
		}

		[JsonIgnore] public string OnlineImageUrl { get { return Path.Combine(AppGlobal.posterURL, Image); } }

		[JsonIgnore] public string LocalImagePath { get; private set; }

		[JsonIgnore] public BitmapImage LocalImage { get; private set; }
		#endregion

		public Actor()
		{

		}

		public void Init(string rootShowPath)
		{
			LocalImagePath = Path.Combine(rootShowPath, Path.GetFileName(Image));

			//if (File.Exists(LocalImagePath))
			//{
			//	try
			//	{
			//		LocalImage = new BitmapImage();
			//		LocalImage.BeginInit();
			//		LocalImage.CacheOption = BitmapCacheOption.OnLoad;
			//		LocalImage.UriSource = new Uri(LocalImagePath);
			//		LocalImage.EndInit();
			//	}
			//	catch (Exception ex)
			//	{

			//	}
			//}
		}

		public void RefreshImage()
		{
			LocalImage = new BitmapImage();
			LocalImage.BeginInit();
			LocalImage.CacheOption = BitmapCacheOption.OnLoad;
			LocalImage.UriSource = new Uri(LocalImagePath);
			LocalImage.EndInit();

			RaisePropertyChanged("LocalImage");
		}
	}
}
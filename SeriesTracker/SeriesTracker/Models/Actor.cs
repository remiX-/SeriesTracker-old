using Newtonsoft.Json;
using Prism.Mvvm;
using SeriesTracker.Core;
using System;
using System.IO;
using System.Windows.Media.Imaging;

namespace SeriesTracker.Models
{
	public class Actor : BindableBase
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
			get { return id; }
			set { id = value; }
		}
		public int SeriesId
		{
			get { return seriesId; }
			set { seriesId = value; }
		}
		public string Name
		{
			get { return name; }
			set { name = value; }
		}
		public string Role
		{
			get { return role; }
			set { role = value; }
		}
		public int? SortOrder
		{
			get { return sortOrder; }
			set { sortOrder = value; }
		}
		public string Image
		{
			get { return image; }
			set { image = value; }
		}

		public int? ImageAuthor
		{
			get { return imageAuthor; }
			set { imageAuthor = value; }
		}
		public string ImageAdded
		{
			get { return imageAdded; }
			set { imageAdded = value; }
		}
		public string LastUpdated
		{
			get { return lastUpdated; }
			set { lastUpdated = value; }
		}

		[JsonIgnore] public string OnlineImageUrl { get { return Path.Combine(AppGlobal.posterURL, Image); } }

		[JsonIgnore] public string LocalImagePath { get; private set; }

		[JsonIgnore] public BitmapImage LocalImage { get; set; }
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
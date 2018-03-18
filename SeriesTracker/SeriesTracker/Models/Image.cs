using Newtonsoft.Json;
using SeriesTracker.Core;
using System.IO;

namespace SeriesTracker.Models
{
	public class Image
	{
		private int id;
		private string keyType;
		private string fileName;
		private string thumbnail;
		private string resolution;

		public int Id
		{
			get { return id; }
			set { id = value; }
		}
		public string KeyType
		{
			get { return keyType; }
			set { keyType = value; }
		}
		public string FileName
		{
			get { return fileName; }
			set { fileName = value; }
		}
		public string Thumbnail
		{
			get { return thumbnail; }
			set { thumbnail = value; }
		}
		public string Resolution
		{
			get { return resolution; }
			set { resolution = value; }
		}

		[JsonIgnore]
		public string OnlineImageUrl
		{
			get
			{
				return Path.Combine(AppGlobal.posterURL, FileName);
			}
		}

		[JsonIgnore]
		public string LocalImagePath { get; set; }
	}
}
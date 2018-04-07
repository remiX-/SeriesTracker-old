using System.Collections.Generic;

namespace SeriesTracker.Models
{
	public class Eztv
	{
		private string id;
		private string href;
		private string name;

		public string Id { get => id; set => id = value; }
		public string Href { get => href; set => href = value; }
		public string Name { get => name; set => name = value; }

		public Eztv(string id, string href, string name)
		{
			Id = id;
			Href = href;
			Name = name;
		}
	}

	public class EztvAPI
	{
		private string imdb_id;
		private int torrents_count;
		private int limit;
		private int page;

		private List<EztvTorrent> torrents;

		public string Imdb_Id { get => imdb_id; set => imdb_id = value; }
		public int Torrents_Count { get => torrents_count; set => torrents_count = value; }
		public int Limit { get => limit; set => limit = value; }
		public int Page { get => page; set => page = value; }

		public List<EztvTorrent> Torrents { get => torrents; set => torrents = value; }
	}

	public class EztvTorrent
	{
		private int id;
		private string hash;

		private string title;
		private string filename;

		private string episode_url;
		private string torrent_url;
		private string magnet_url;


		private string date_released_unix;
		private string size_bytes;

		public int Id { get => id; set => id = value; }
		public string Hash { get => hash; set => hash = value; }

		public string Title { get => title; set => title = value; }
		public string Filename { get => filename; set => filename = value; }

		public string Episode_Url { get => episode_url; set => episode_url = value; }
		public string Torrent_Url { get => torrent_url; set => torrent_url = value; }
		public string Magnet_Url { get => magnet_url; set => magnet_url = value; }

		public string Date_Released_Unix { get => date_released_unix; set => date_released_unix = value; }
		public string Size_Bytes { get => size_bytes; set => size_bytes = value; }

		public string Size => GetSize();

		private static readonly string[] Units = { "B", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB" };

		public bool IsHD()
		{
			return Title.Contains("720p") || Title.Contains("1080p");
		}

		public string GetSize()
		{
			if (!double.TryParse(Size_Bytes, out double sizeInBytes))
				return "0";

			var unit = 0;

			while (sizeInBytes >= 1024)
			{
				sizeInBytes /= 1024;
				++unit;
			}

			return $"{sizeInBytes:0.##} {Units[unit]}";
		}
	}
}

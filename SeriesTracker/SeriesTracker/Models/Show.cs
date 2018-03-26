using Newtonsoft.Json;
using Prism.Mvvm;
using SeriesTracker.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;

namespace SeriesTracker.Models
{
	public class Show : BindableBase
	{
		#region Variables
		[JsonIgnore] public int UserShowID { get; set; }

		#region Tvdb variables
		private int id;
		private string seriesName;
		private string overview;
		private List<string> genre;

		private string firstAired;
		private string runtime;
		private string network;
		private string status;

		private string airsDayOfWeek;
		private string airsTime;
		private string rating;

		private string imdbId;
		private string zap2itId;

		public int Id { get => id; set => id = value; }
		public string SeriesName { get => seriesName; set => seriesName = value; }
		public string Overview { get => overview; set => overview = value; }
		public List<string> Genre { get => genre; set => genre = value; }
		public string FirstAired { get => firstAired; set => firstAired = value; }
		public string Runtime { get => runtime; set => runtime = value; }
		public string Network { get => network; set => network = value; }
		public string Status { get => status; set => status = value; }
		public string AirsDayOfWeek { get => airsDayOfWeek; set => airsDayOfWeek = value; }
		public string AirsTime { get => airsTime; set => airsTime = value; }
		public string Rating { get => rating; set => rating = value; }
		public string ImdbId { get => imdbId; set => imdbId = value; }
		public string Zap2itId { get => zap2itId; set => zap2itId = value; }

		public List<Episode> Episodes { get; set; }
		public List<Actor> Actors { get; set; }
		public List<Image> Posters { get; set; }
		public List<Image> Banners { get; set; }
		#endregion

		#region Generated
		[JsonIgnore] public List<UserShowWatch> EpisodesWatched { get; set; }
		[JsonIgnore] public List<Category> Categories { get; set; }

		[JsonIgnore] public DateTime FirstAiredDate { get; private set; }
		[JsonIgnore] public DateTime AirDateTime { get; private set; }
		[JsonIgnore] public DayOfWeek AirWeekDay { get; private set; }

		[JsonIgnore] public Episode LatestEpisode { get; private set; }
		[JsonIgnore] public Episode NextEpisode { get; private set; }

		[JsonIgnore] private Eztv EztvData { get; set; }
		#endregion

		#region Display
		private bool updating;
		[JsonIgnore]
		public bool Updating
		{
			get { return updating; }
			set { SetProperty(ref updating, value); RaisePropertyChanged("DisplayName"); }
		}

		[JsonIgnore]
		public string DisplayName
		{
			get
			{
				if (Updating)
					return "Updating ...";

				string _t = seriesName;

				if (AppGlobal.Settings.IgnoreBracketsInNames)
					_t = CommonMethods.GetNameWithoutBrackets(_t);

				if (AppGlobal.Settings.UseListedName)
					_t = CommonMethods.GetListedName(_t);

				return _t;
			}
		}

		[JsonIgnore] public string YearDisplay { get; private set; } = "Unknown";

		[JsonIgnore] public string GenreDisplay { get; private set; } = "-";
		[JsonIgnore] public string AirDayDisplay { get; private set; } = "-";

		[JsonIgnore] public string NextEpisodeDisplay { get; private set; } = "-";
		[JsonIgnore] public string NextEpisodeDateDisplay { get; private set; } = "-";

		[JsonIgnore] public string HowLongDisplay { get; private set; } = "-";
		#endregion

		#region Paths
		private string localSeriesPath;

		[JsonIgnore]
		public string LocalSeriesPath
		{
			get { return localSeriesPath; }
			set { SetProperty(ref localSeriesPath, value); }
		}

		[JsonIgnore] public string LocalDataPath { get; private set; }
		[JsonIgnore] public string LocalJsonPath { get; private set; }
		[JsonIgnore] public string LocalImagesPath { get; private set; }
		[JsonIgnore] public string LocalImagesActorsPath { get; private set; }
		[JsonIgnore] public string LocalImagesEpisodesPath { get; private set; }
		[JsonIgnore] public string LocalImagesPostersPath { get; private set; }
		[JsonIgnore] public string LocalImagesBannersPath { get; private set; }

		[JsonIgnore] public string LocalPicturePath { get; private set; }

		[JsonIgnore] public string LocalBannerPath { get; private set; }
		#endregion

		string[] SearchTerms;
		#endregion

		public Show()
		{
			UserShowID = -1;
			Categories = new List<Category>();
		}

		public Show(int userShowId, int TvdbId, string seriesName) : this()
		{
			UserShowID = userShowId;
			Id = TvdbId;
			SeriesName = seriesName;

			SetupPaths();
		}

		public void DoWork()
		{
			SetupVariables();
			SetupPaths();

			OrderLists();

			EztvData = MethodCollection.GetEztvShowDetails(seriesName);

			if (!Directory.Exists(LocalDataPath))
				Directory.CreateDirectory(LocalDataPath);

			if (!Directory.Exists(LocalImagesPostersPath))
				Directory.CreateDirectory(LocalImagesPostersPath);

			if (!Directory.Exists(LocalImagesBannersPath))
				Directory.CreateDirectory(LocalImagesBannersPath);

			Series s = AppGlobal.Settings.GetSeriesPath(id);
			LocalSeriesPath = s?.Path;

			#region Actors
			Actors.RemoveAll(a => string.IsNullOrEmpty(a.Image));
			Actors.ForEach(a => a.Init(LocalImagesActorsPath));
			#endregion

			#region Episodes
			EpisodesWatched = new List<UserShowWatch>();

			// Retrieve latest and next episode
			LatestEpisode = null;
			NextEpisode = null;

			foreach (Episode episode in Episodes)
			{
				if (episode.AiredSeason == 0 || episode.AiredEpisodeNumber == 0) continue;

				episode.DoWork(this);

				// Set latest episode
				if (!episode.AirDate.HasValue) continue;

				if (episode.AirDate.Value < DateTime.Now)
				{
					LatestEpisode = episode;
				}

				// Check to see if this is next episodes
				if (NextEpisode == null && episode.AirDate.Value > DateTime.Now)
				{
					NextEpisode = episode;
				}
			}

			// If next episode is still null, then dates have not been set after latest episode
			if (NextEpisode == null)
			{
				if (Status == "Continuing")
					Status = "Returning";

				if (Status == "Ended")
				{
					AirDateTime = new DateTime(9999, 12, 25, 23, 59, 59);
				}
			}
			#endregion

			#region Posters
			foreach (Image image in Posters)
			{
				image.LocalImagePath = Path.Combine(LocalImagesPostersPath, Path.GetFileName(image.FileName));
			}

			Image first = Posters.First();
			if (!File.Exists(first.LocalImagePath))
			{
				try
				{
					using (WebClient client = new WebClient())
					{
						client.DownloadFile(new Uri(first.OnlineImageUrl), first.LocalImagePath);
					}

					if (File.Exists(LocalPicturePath))
						File.Delete(LocalPicturePath);

					File.Copy(first.LocalImagePath, LocalPicturePath);
				}
				catch (WebException)
				{

				}
			}
			#endregion

			#region Banners
			foreach (Image image in Banners)
			{
				image.LocalImagePath = Path.Combine(LocalImagesBannersPath, Path.GetFileName(image.FileName));
			}

			first = Banners.First();
			if (!File.Exists(first.LocalImagePath))
			{
				try
				{
					using (WebClient client = new WebClient())
					{
						client.DownloadFile(new Uri(first.OnlineImageUrl), first.LocalImagePath);
					}

					if (File.Exists(LocalBannerPath))
						File.Delete(LocalBannerPath);

					File.Copy(first.LocalImagePath, LocalBannerPath);
				}
				catch (WebException)
				{

				}
			}
			#endregion

			#region Display variables
			if (Genre != null && Genre.Count > 0)
			{
				GenreDisplay = Genre.OrderBy(g => g).Aggregate((a, b) => string.Format("{0}/{1}", a, b));
			}

			if (Status != "Ended")
			{
				AirDayDisplay = AirDateTime.ToString("hh:mm tt, dddd");

				if (NextEpisode != null)
				{
					NextEpisodeDisplay = NextEpisode.FullEpisodeString;
					NextEpisodeDateDisplay = NextEpisode.FullDateString;

					HowLongDisplay = NextEpisode.TimeToEpisode();
				}
				else
				{
					NextEpisodeDisplay = "TBA";
					NextEpisodeDateDisplay = "TBA";

					HowLongDisplay = "TBA";
				}
			}
			#endregion

			SearchTerms = new[]
			{
				Id.ToString(), ImdbId, SeriesName, DisplayName, Status, Network, GenreDisplay,
				AirDayDisplay, LatestEpisode.FullEpisodeString, LatestEpisode.FullDateString,
				NextEpisodeDisplay, NextEpisodeDateDisplay
			};
			SearchTerms = SearchTerms.Where(search => !string.IsNullOrWhiteSpace(search)).ToArray();
		}

		#region Public Methods
		public bool HasText(string filter)
		{
			var upper = filter.ToLower().Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries).Distinct();
			if (upper.Count() > 1)
				upper = upper.Where(search => search.Length >= 2);

			return upper.Any(term => SearchTerms.Any(search => search.ToLower().Contains(term)));
		}

		public string GetNameWithoutBrackets()
		{
			return CommonMethods.GetNameWithoutBrackets(seriesName);
		}

		public string GetListedName()
		{
			return CommonMethods.GetListedName(seriesName);
		}

		public DateTime? GetFirstAiredAsDateTime()
		{
			DateTime? dt = CommonMethods.GetPossibleDateTime(firstAired);

			if (dt.HasValue)
				dt = CommonMethods.GetDateTimeAsLocal(firstAired, AirsTime);

			return dt;
		}

		public Image GetMostRecentImage()
		{
			Image d = Posters.OrderByDescending(i => i.Id).ToList()[0];
			return Posters.OrderByDescending(i => i.Id).ToList()[0];
		}

		public List<Episode> GetEpisodesBySeason(int seasonNo)
		{
			return Episodes.Where(ep => ep.AiredSeason == seasonNo).OrderBy(ep => ep.AiredEpisodeNumber).ToList();
		}

		public Episode GetEpisode(int seasonNo, int episodeNo)
		{
			return Episodes.Single(ep => ep.AiredSeason == seasonNo && ep.AiredEpisodeNumber == episodeNo);
		}

		public void SetupVariables()
		{
			if (string.IsNullOrEmpty(network)) network = "Unknown";
			if (string.IsNullOrEmpty(status)) status = "Unknown";

			if (!string.IsNullOrEmpty(firstAired))
			{
				FirstAiredDate = CommonMethods.GetDateTimeAsLocal(firstAired, AirsTime);
				YearDisplay = FirstAiredDate.Year.ToString();
			}
			else
			{
				FirstAiredDate = CommonMethods.GetDateTimeAsLocal(DateTime.Now.ToString("dd/MM/yyyy"), AirsTime);
			}

			if (!string.IsNullOrEmpty(AirsDayOfWeek))
			{
				switch (AirsDayOfWeek.ToLower())
				{
					case "sunday": AirWeekDay = DayOfWeek.Sunday; break;
					case "monday": AirWeekDay = DayOfWeek.Monday; break;
					case "tuesday": AirWeekDay = DayOfWeek.Tuesday; break;
					case "wednesday": AirWeekDay = DayOfWeek.Wednesday; break;
					case "thursday": AirWeekDay = DayOfWeek.Thursday; break;
					case "friday": AirWeekDay = DayOfWeek.Friday; break;
					case "saturday": AirWeekDay = DayOfWeek.Saturday; break;
					default: AirWeekDay = DayOfWeek.Sunday; break;
				}

				DateTime temp1 = DateTime.Parse(AirsTime);

				DateTime temp = new DateTime(2017, 1, 1 + (int)AirWeekDay, temp1.Hour, temp1.Minute, 0);
				AirDateTime = CommonMethods.GetDateTimeAsLocal(temp);
			}
		}
		#endregion

		#region Private Methods
		private void OrderLists()
		{
			if (Episodes?.Count > 0) Episodes = Episodes.OrderBy(ep => ep.AiredSeason).ThenBy(ep => ep.AiredEpisodeNumber).ToList();
			if (Actors?.Count > 0) Actors = Actors.OrderBy(act => act.SortOrder).ToList();
			if (Posters?.Count > 0) Posters = Posters.OrderBy(p => p.Id).ToList();
		}

		private void SetupPaths()
		{
			LocalDataPath = Path.Combine(AppGlobal.Paths.SeriesDirectory, id.ToString());
			LocalJsonPath = Path.Combine(LocalDataPath, "data.json");
			LocalImagesPath = Path.Combine(LocalDataPath, "Images");
			LocalImagesActorsPath = Path.Combine(LocalImagesPath, "Actors");
			LocalImagesEpisodesPath = Path.Combine(LocalImagesPath, "Episodes");
			LocalImagesPostersPath = Path.Combine(LocalImagesPath, "Posters");
			LocalImagesBannersPath = Path.Combine(LocalImagesPath, "Banners");
			LocalPicturePath = Path.Combine(LocalImagesPath, "main.jpg");
			LocalBannerPath = Path.Combine(LocalImagesPath, "banner.jpg");
		}
		#endregion

		#region Links
		public string GetIMDbLink()
		{
			return string.Format(AppGlobal.imdbLink, ImdbId);
		}

		public bool HasEztvData()
		{
			return EztvData != null;
		}

		public string GetEZTVLink()
		{
			return string.Format(AppGlobal.eztvLink, EztvData.Href);
		}
		#endregion

		public override bool Equals(object obj)
		{
			return obj is Show ? Id == (obj as Show).Id : base.Equals(obj);
		}

		public override int GetHashCode()
		{
			return base.GetHashCode();
		}
	}
}
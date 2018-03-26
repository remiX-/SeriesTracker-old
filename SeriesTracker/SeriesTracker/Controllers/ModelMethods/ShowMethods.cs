using CsQuery;
using Newtonsoft.Json;
using SeriesTracker.Core;
using SeriesTracker.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SeriesTracker
{
	public class ShowMethods : IShowRepository
	{
		string[] episodeFormats = { "s{0:d2}e{1:d2}", "{0}x{1:d2}", "{0:d2}x{1:d2}" };

		public Show RetrieveTvdbDataForSeries(int TvdbID)
		{
			TvdbAPI showData = Request.ExecuteAndDeserialize("GET", string.Format("https://api.thetvdb.com/series/{0}", TvdbID));
			TvdbAPI actorData = Request.ExecuteAndDeserialize("GET", string.Format("https://api.thetvdb.com/series/{0}/actors", TvdbID));
			TvdbAPI posterData = Request.ExecuteAndDeserialize("GET", string.Format("https://api.thetvdb.com/series/{0}/images/query?keyType=poster", TvdbID));
			TvdbAPI bannerData = Request.ExecuteAndDeserialize("GET", string.Format("https://api.thetvdb.com/series/{0}/images/query?keyType=series", TvdbID));

			List<Episode> episodes = new List<Episode>();
			string next = "1";
			do
			{
				TvdbAPI episodeData = Request.ExecuteAndDeserialize("GET", string.Format("https://api.thetvdb.com/series/{0}/episodes?page={1}", TvdbID, next));

				var nextEpisodes = JsonConvert.DeserializeObject<List<Episode>>(episodeData.Data.ToString());
				episodes.AddRange(nextEpisodes);

				next = episodeData.Links.Next;
			} while (!string.IsNullOrWhiteSpace(next));

			// Put data together
			Show show = JsonConvert.DeserializeObject<Show>(showData.Data.ToString());
			show.Actors = JsonConvert.DeserializeObject<List<Actor>>(actorData.Data.ToString()).ToList();
			show.Episodes = episodes;
			show.Posters = JsonConvert.DeserializeObject<List<Image>>(posterData.Data.ToString());
			show.Banners = JsonConvert.DeserializeObject<List<Image>>(bannerData.Data.ToString());

			show.DoWork();

			// Write json data to a local file
			File.WriteAllText(show.LocalJsonPath, JsonConvert.SerializeObject(show));

			return show;
		}

		public async Task<Show> RetrieveTvdbDataForSeriesAsync(int TvdbID)
		{
			TvdbAPI showData = await Request.ExecuteAndDeserializeAsync<TvdbAPI>("GET", string.Format("https://api.thetvdb.com/series/{0}", TvdbID));
			TvdbAPI actorData = await Request.ExecuteAndDeserializeAsync<TvdbAPI>("GET", string.Format("https://api.thetvdb.com/series/{0}/actors", TvdbID));
			TvdbAPI posterData = await Request.ExecuteAndDeserializeAsync<TvdbAPI>("GET", string.Format("https://api.thetvdb.com/series/{0}/images/query?keyType=poster", TvdbID));
			TvdbAPI bannerData = await Request.ExecuteAndDeserializeAsync<TvdbAPI>("GET", string.Format("https://api.thetvdb.com/series/{0}/images/query?keyType=series", TvdbID));

			List<Episode> episodes = new List<Episode>();
			string next = "1";
			await Task.Run(() =>
			{
				do
				{
					TvdbAPI episodeData = Request.ExecuteAndDeserialize("GET", string.Format("https://api.thetvdb.com/series/{0}/episodes?page={1}", TvdbID, next));

					var nextEpisodes = JsonConvert.DeserializeObject<List<Episode>>(episodeData.Data.ToString());
					episodes.AddRange(nextEpisodes);

					next = episodeData.Links.Next;
				} while (!string.IsNullOrWhiteSpace(next));
			});

			// Put data together
			Show show = JsonConvert.DeserializeObject<Show>(showData.Data.ToString());
			show.Actors = JsonConvert.DeserializeObject<List<Actor>>(actorData.Data.ToString()).ToList();
			show.Episodes = episodes;
			show.Posters = JsonConvert.DeserializeObject<List<Image>>(posterData.Data.ToString());
			show.Banners = JsonConvert.DeserializeObject<List<Image>>(bannerData.Data.ToString());

			show.DoWork();

			// Write json data to a local file
			File.WriteAllText(show.LocalJsonPath, JsonConvert.SerializeObject(show));

			return show;
		}

		private async Task<EztvTorrent> GetMagnetForEpisode(Show show, Episode episode, bool getHD = false)
		{
			try
			{
				string url = show.GetEZTVLink();

				if (string.IsNullOrEmpty(url)) return null;


				EztvAPI eztvData = await Request.ExecuteAndDeserializeAsync<EztvAPI>("GET", $"https://eztv.ag/api/get-torrents?imdb_id={show.ImdbId.Substring(2)}&limit=100");
				var torrents = eztvData.Torrents;
				var episodeLinks = new List<EztvTorrent>();

				foreach (EztvTorrent torrent in torrents)
				{
					bool magnetLinkIsHD = torrent.IsHD();

					foreach (string format in episodeFormats)
					{
						string fullEpisode = string.Format(format, episode.AiredSeason, episode.AiredEpisodeNumber);

						if (torrent.Torrent_Url.ToLower().Contains(fullEpisode))
						{
							episodeLinks.Add(torrent);
							break;
						}
					}
				}

				foreach (EztvTorrent link in episodeLinks)
				{
					if (getHD && link.IsHD())
					{
						return link;
					}
				}

				//CQ htmlText;
				//using (WebClient client = new WebClient())
				//{
				//	htmlText = await client.DownloadStringTaskAsync(url);
				//}

				//if (htmlText == null) return null;

				//List<IDomObject> magnetLinks = htmlText.Select("a[class='magnet']").ToList();
				//List<string> episodeMagnetLinks = new List<string>();

				//foreach (var obj in magnetLinks)
				//{
				//	string magnetLink = obj.GetAttribute("href");
				//	bool magnetLinkIsHD = magnetLink.Contains("720p") || magnetLink.Contains("1080p");

				//	foreach (string format in episodeFormats)
				//	{
				//		string fullEpisode = string.Format(format, episode.AiredSeason, episode.AiredEpisodeNumber);

				//		if (magnetLink.ToLower().Contains(fullEpisode))
				//		{
				//			episodeMagnetLinks.Add(magnetLink);
				//			break;
				//			//if (getHD && magnetLinkIsHD)
				//			//	return magnetLink;
				//		}
				//	}
				//}


			}
			catch (Exception ex)
			{
				ErrorMethods.LogError(ex.Message);
			}

			return null;
		}

		public Eztv GetEztvShowDetails(string showName)
		{
			try
			{
				if (!File.Exists(AppGlobal.Paths.EztvIDFile))
				{
					return null;
				}

				// Check if showName starts with "The"
				showName = CommonMethods.GetListedName(showName);

				string[] lines = File.ReadAllLines(AppGlobal.Paths.EztvIDFile);
				string[] parts;
				string tmp1 = showName.ToLower();
				string tmp2;

				// Remove some strings
				tmp1 = tmp1.Replace("dc's ", "");

				foreach (string line in lines)
				{
					parts = line.Split('|');
					tmp2 = parts[2].ToLower();

					// Remove brackets if there is only if they're NOT equal
					if (tmp2 != tmp1)
					{
						if (tmp2.Contains("("))
							tmp2 = tmp2.Substring(0, tmp2.IndexOf("(") - 1);

						if (tmp1.Contains("("))
							tmp1 = tmp1.Substring(0, tmp1.IndexOf("(") - 1);
					}

					if (tmp1 == tmp2 || tmp2 == tmp1.Replace("'", "") || tmp2.Contains(tmp1 + ":"))
					{
						return new Eztv(parts[0], parts[1], parts[2]);
					}
				}
			}
			catch (Exception ex)
			{
				ErrorMethods.LogError(ex.Message);
			}

			return null;
		}

		public bool UpdateEztvShowFile()
		{
			CQ htmlText = null;
			using (WebClient client = new WebClient())
			{
				htmlText = client.DownloadString(AppGlobal.eztvShowlist);
			}

			if (htmlText != null)
			{
				File.WriteAllText(AppGlobal.Paths.EztvIDFile, "");
				var elements = htmlText.Select("tr[name='hover']").ToList();

				List<Eztv> EZTVShowList = new List<Eztv>();
				foreach (var element in elements)
				{
					var child = element.FirstElementChild;

					string href = Regex.Match(child.InnerHTML, "href=\"(.*)\"").Groups[1].Value.Replace("/shows/", "");
					string name = Regex.Match(child.InnerHTML, ">(.*)<").Groups[1].Value;

					if (!string.IsNullOrEmpty(href) && !string.IsNullOrEmpty(name))
					{
						EZTVShowList.Add(new Eztv(href.Split('/')[0], href, name));
					}
				}

				EZTVShowList = EZTVShowList.OrderBy(i => int.Parse(i.Id)).ToList();

				using (StreamWriter writer = File.AppendText(AppGlobal.Paths.EztvIDFile))
				{
					foreach (Eztv e in EZTVShowList)
					{
						writer.WriteLine(e.Id + "|" + e.Href + "|" + e.Name);
					}
				}

				return true;
			}

			return false;
		}

		public async Task<bool> UpdateEztvShowFileAsync()
		{
			CQ htmlText = null;
			using (WebClient client = new WebClient())
			{
				await Task.Delay(2000);
				htmlText = await client.DownloadStringTaskAsync(AppGlobal.eztvShowlist);
			}

			if (htmlText != null)
			{
				File.WriteAllText(AppGlobal.Paths.EztvIDFile, "");
				var elements = htmlText.Select("tr[name='hover']").ToList();

				List<Eztv> EZTVShowList = new List<Eztv>();
				foreach (var element in elements)
				{
					var child = element.FirstElementChild;

					string href = Regex.Match(child.InnerHTML, "href=\"(.*)\"").Groups[1].Value.Replace("/shows/", "");
					string name = Regex.Match(child.InnerHTML, ">(.*)<").Groups[1].Value;

					if (!string.IsNullOrEmpty(href) && !string.IsNullOrEmpty(name))
					{
						EZTVShowList.Add(new Eztv(href.Split('/')[0], href, name));
					}
				}

				EZTVShowList = EZTVShowList.OrderBy(i => int.Parse(i.Id)).ToList();

				using (StreamWriter writer = File.AppendText(AppGlobal.Paths.EztvIDFile))
				{
					foreach (Eztv e in EZTVShowList)
					{
						await writer.WriteLineAsync(e.Id + "|" + e.Href + "|" + e.Name);
					}
				}

				return true;
			}

			return false;
		}

		public async Task<List<EztvTorrent>> GetEpisodeTorrentList(Show show, Episode episode)
		{
			try
			{
				string url = show.GetEZTVLink();

				if (string.IsNullOrEmpty(url)) return null;


				EztvAPI eztvData = await Request.ExecuteAndDeserializeAsync<EztvAPI>("GET", $"https://eztv.ag/api/get-torrents?imdb_id={show.ImdbId.Substring(2)}&limit=100");
				var torrents = eztvData.Torrents;
				var episodeTorrents = new List<EztvTorrent>();

				foreach (EztvTorrent torrent in torrents)
				{
					bool magnetLinkIsHD = torrent.IsHD();

					foreach (string format in episodeFormats)
					{
						string fullEpisode = string.Format(format, episode.AiredSeason, episode.AiredEpisodeNumber);

						if (torrent.Torrent_Url.ToLower().Contains(fullEpisode))
						{
							episodeTorrents.Add(torrent);
							break;
						}
					}
				}

				return episodeTorrents;
			}
			catch (Exception ex)
			{
				ErrorMethods.LogError(ex.Message);
			}

			return null;
		}

		public async Task<bool> DownloadEpisode(Show show, Episode episode)
		{
			bool hasMagnet = false;

			if (show.HasEztvData())
			{
				EztvTorrent torrent = await GetMagnetForEpisode(show, episode, true);
				if (torrent != null)
				{
					hasMagnet = true;
					CommonMethods.StartProcess(torrent.Torrent_Url);
				}
			}

			if (!hasMagnet)
			{
				string searchText = show.SeriesName;
				searchText = CommonMethods.GetNameWithoutBrackets(searchText);
				searchText = CommonMethods.RemoveNonAlphabetLetters(searchText);

				string url = string.Format(AppGlobal.searchURL, searchText, episode.FullEpisodeString);
				CommonMethods.StartProcess(url);
			}

			return hasMagnet;
		}
	}
}

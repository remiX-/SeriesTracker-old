using SeriesTracker.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SeriesTracker
{
	public interface IShowRepository
	{
		Show RetrieveTvdbDataForSeries(int TvdbID);
		Task<Show> RetrieveTvdbDataForSeriesAsync(int TvdbID);

		Eztv GetEztvShowDetails(string showName);
		bool UpdateEztvShowFile();
		Task<bool> UpdateEztvShowFileAsync();

		Task<bool> DownloadEpisode(Show show, Episode episode);

		Task<List<EztvTorrent>> GetEpisodeTorrentList(Show show, Episode episode);
	}
}
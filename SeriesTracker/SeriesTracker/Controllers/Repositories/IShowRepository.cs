using SeriesTracker.Models;
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
		Task<string> GetMagnetForEpisode(string url, Episode episode, bool getHD);
	}
}
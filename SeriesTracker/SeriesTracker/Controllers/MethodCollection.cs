using SeriesTracker.Core;
using SeriesTracker.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SeriesTracker
{
	public static class MethodCollection
	{
		private static IRepositoryContainer _repository { get { return RepositorySession.GetRepository(); } }

		#region User Methods
		public static async Task<LoginResult> UserLoginAsync(string Email, string Password)
		{
			return await _repository.UserRepository.UserLoginAsync(Email, Password);
		}

		public static async Task<SeriesResult<User>> UserRegisterAsync(string Username, string Email, string Name, string Password)
		{
			return await _repository.UserRepository.UserRegisterAsync(Username, Email, Name, Password);
		}
		#endregion

		#region Show Methods
		public static Show RetrieveTvdbDataForSeries(int TvdbID)
		{
			return _repository.ShowRepository.RetrieveTvdbDataForSeries(TvdbID);
		}

		public static async Task<Show> RetrieveTvdbDataForSeriesAsync(int TvdbID)
		{
			return await _repository.ShowRepository.RetrieveTvdbDataForSeriesAsync(TvdbID);
		}

		public static Eztv GetEztvShowDetails(string showName)
		{
			return _repository.ShowRepository.GetEztvShowDetails(showName);
		}

		public static bool UpdateEztvShowFile()
		{
			return _repository.ShowRepository.UpdateEztvShowFile();
		}

		public static async Task<bool> UpdateEztvShowFileAsync()
		{
			return await _repository.ShowRepository.UpdateEztvShowFileAsync();
		}

		public static async Task<bool> DownloadEpisode(Show show, Episode episode)
		{
			return await _repository.ShowRepository.DownloadEpisode(show, episode);
		}

		public static async Task<List<EztvTorrent>> GetEpisodeTorrentList(Show show, Episode episode)
		{
			return await _repository.ShowRepository.GetEpisodeTorrentList(show, episode);
		}
		#endregion
	}
}

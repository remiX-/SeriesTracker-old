using Onova;
using Onova.Services;
using System;
using System.Threading.Tasks;

namespace SeriesTracker.Services
{
	public class UpdateService : IUpdateService
	{
		private readonly ISettingsService _settingsService;
		private readonly IUpdateManager _manager;

		private Version _updateVersion;
		private bool _updateFinalized;

		public bool NeedRestart { get; set; }

		public UpdateService(ISettingsService settingsService)
		{
			_settingsService = settingsService;

			_manager = new UpdateManager(new GithubPackageResolver("remiX-", "YouTubeTool", "YouTubeTool.zip"), new ZipPackageExtractor());
		}

		public async Task<Version> CheckForUpdateAsync()
		{
			// If auto-update is disabled - don't check for updates
			if (!_settingsService.IsAutoUpdateEnabled)
				return null;

#if DEBUG
			// Never update in DEBUG mode
			return null;
#endif

			// Check for updates
			var check = await _manager.CheckForUpdatesAsync();
			if (!check.CanUpdate)
				return null;

			return _updateVersion = check.LastVersion;
		}

		public async Task PrepareUpdateAsync()
		{
			// Prepare the update
			if (!_manager.IsUpdatePrepared(_updateVersion))
				await _manager.PrepareUpdateAsync(_updateVersion);
		}

		public void FinalizeUpdate()
		{
			// Check if an update is pending
			if (_updateVersion == null)
				return;

			// Check if the update has already been finalized
			if (_updateFinalized)
				return;

			// Launch the updater
			_manager.LaunchUpdater(_updateVersion, NeedRestart);
			_updateFinalized = true;
		}
	}
}

using GalaSoft.MvvmLight.CommandWpf;

namespace SeriesTracker.ViewModels
{
	public interface ILoginViewModel
	{
		// Window Events
		RelayCommand ViewLoadedCommand { get; }
	}
}

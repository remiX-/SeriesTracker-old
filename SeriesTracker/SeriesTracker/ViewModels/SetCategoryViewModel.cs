using GalaSoft.MvvmLight;
using SeriesTracker.Core;
using SeriesTracker.Models;
using System.Collections.Generic;
using System.Linq;

namespace SeriesTracker.ViewModels
{
	public class SetCategoryViewModel : ViewModelBase, ISetCategoryViewModel
	{
		#region Variables
		#region Fields

		#endregion

		#region Properties
		public string MyTitle { get; }

		public List<Category> Categories { get; }
		#endregion
		#endregion

		public SetCategoryViewModel()
		{
			MyTitle = "Set Lists";

			Categories = AppGlobal.User.Categories.OrderBy(x => x.Name).ToList();
		}
	}
}

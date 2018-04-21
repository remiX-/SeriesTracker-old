using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SeriesTracker.ViewModels
{
	internal interface IViewModelBase
	{
		bool IsBusy { get; set; }
	}
}
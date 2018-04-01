using SeriesTracker.Core;
using System.Windows.Controls;

namespace SeriesTracker.Dialogs
{
	public partial class AboutDialog : UserControl
	{
		#region Variables
		string ProductName { get; }
		string Version { get; }
		string Copyright { get; }
		string CompanyName { get; }
		string Description { get; }
		#endregion

		public AboutDialog()
		{
			InitializeComponent();

			ProductName = AppGlobal.AssemblyProduct;
			Version = $"v{AppGlobal.AssemblyVersion}";
			Copyright = AppGlobal.AssemblyCopyright;
			CompanyName = AppGlobal.AssemblyCompany;
			Description = AppGlobal.AssemblyDescription;

			DataContext = this;
		}
	}
}

using SeriesTracker.Models;
using System.Reflection;

namespace SeriesTracker.Core
{
	public static class AppGlobal
	{
		public static User User = null;
		public static Database Db = new Database();

		// Tvdb settings
		public static string thetvdbToken = "";
		public static int thetvdbTimezone = -4;

		// App settings
		public static AppSettings Settings;
		public static AppPaths Paths = new AppPaths();

		// App links
		public static string searchURL = "https://www.thepiratebay.org/search/{0} {1}";
		public static string dbUrl = "www.thetvdb.com";
		public static string dbAPI = "https://api.thetvdb.com/";
		public static string posterURL = "http://www.thetvdb.com/banners/";
		public static string eztvLink = "http://eztv.ag/shows/{0}";
		public static string eztvShowlist = "http://eztv.ag/showlist/";
		public static string imdbLink = "http://www.imdb.com/title/{0}";

		#region Assembly Attribute Accessors
		public static string AssemblyTitle
		{
			get
			{
				object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyTitleAttribute), false);
				if (attributes.Length > 0)
				{
					AssemblyTitleAttribute titleAttribute = (AssemblyTitleAttribute)attributes[0];
					if (titleAttribute.Title != "")
					{
						return titleAttribute.Title;
					}
				}
				return System.IO.Path.GetFileNameWithoutExtension(Assembly.GetExecutingAssembly().CodeBase);
			}
		}

		public static string AssemblyVersion
		{
			get
			{
				return Assembly.GetExecutingAssembly().GetName().Version.ToString();
			}
		}

		public static string AssemblyDescription
		{
			get
			{
				object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyDescriptionAttribute), false);
				if (attributes.Length == 0)
				{
					return "";
				}
				return ((AssemblyDescriptionAttribute)attributes[0]).Description;
			}
		}

		public static string AssemblyProduct
		{
			get
			{
				object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyProductAttribute), false);
				if (attributes.Length == 0)
				{
					return "";
				}
				return ((AssemblyProductAttribute)attributes[0]).Product;
			}
		}

		public static string AssemblyCopyright
		{
			get
			{
				object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false);
				if (attributes.Length == 0)
				{
					return "";
				}
				return ((AssemblyCopyrightAttribute)attributes[0]).Copyright;
			}
		}

		public static string AssemblyCompany
		{
			get
			{
				object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyCompanyAttribute), false);
				if (attributes.Length == 0)
				{
					return "";
				}
				return ((AssemblyCompanyAttribute)attributes[0]).Company;
			}
		}
		#endregion
	}
}
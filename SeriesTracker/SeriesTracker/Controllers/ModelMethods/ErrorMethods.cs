using SeriesTracker.Core;
using System;
using System.Configuration;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;

namespace SeriesTracker
{
	public class ErrorMethods
	{
		private static string ErrorFile = @"ErrorLog.txt";

		/// <summary>
		/// Used to handle errors in the application. Logs in file and database
		/// </summary>
		/// <param name="errorMessage"></param>
		/// <param name="methodName"></param>
		/// <param name="lineNumber"></param>
		public static void LogError(string errorMessage = "No error message given", [CallerMemberName] string methodName = null, [CallerLineNumber] int lineNumber = 0)
		{
			try
			{
				AppGlobal.Db.LogError(methodName, lineNumber, errorMessage);

				using (StreamWriter sw = File.AppendText(ErrorFile))
				{
					sw.WriteLine("".PadRight(30, '-'));
					sw.WriteLine("\t" + "Timestamp".PadRight(15) + DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss"));
					sw.WriteLine("\t" + "Method".PadRight(15) + methodName);
					sw.WriteLine("\t" + "Line".PadRight(15) + lineNumber);
					sw.WriteLine("\t" + "Message".PadRight(15) + errorMessage);
					sw.WriteLine("".PadRight(30, '-'));
					sw.WriteLine();
				}

			}
			catch { }

			MessageBox.Show(ConfigurationManager.AppSettings["GenericErrorMessage"] + "\n\n" + errorMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
		}
	}
}

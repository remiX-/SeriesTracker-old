using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Media;
using System.Net;
using System.Net.NetworkInformation;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace SeriesTracker.Core
{
	public static class CommonMethods
	{
		#region File Methods
		public static void AppendToFile(string filePath, string text)
		{
			try
			{
				using (StreamWriter streamWriter = File.AppendText(filePath))
					streamWriter.WriteLine(text);
			}
			catch (Exception e)
			{
				ErrorMethods.LogError(e.Message);
			}
		}

		public static void DownloadImageToPath(string dir, string imageName, string url)
		{
			try
			{
				using (WebClient client = new WebClient())
					client.DownloadFile(url, Path.Combine(dir, imageName));
			}
			catch (Exception e)
			{
				ErrorMethods.LogError(e.Message);
			}
		}

		public static void DownloadImageToPath(string path, string url)
		{
			try
			{
				using (WebClient client = new WebClient())
					client.DownloadFile(url, path);
			}
			catch (Exception e)
			{
				ErrorMethods.LogError(e.Message);
			}
		}
		#endregion

		#region Connection Methods
		public static bool HasInternetConnection()
		{
			return HasConnectionToURL("www.google.com");
		}

		public static Task<bool> HasInternetConnectionn()
		{
			return Task.Run(() => HasInternetConnection());
		}

		public static async Task<bool> HasInternetConnectionAsync()
		{
			return await HasConnectionToURLAsync("www.google.com");
		}

		public static bool HasConnectionToURL(string url)
		{
			Ping p = new Ping();

			try
			{
				PingReply reply = p.Send(url, 10000);
				if (reply.Status == IPStatus.Success)
					return true;
			}
			catch { }

			return false;
		}

		public static async Task<bool> HasConnectionToURLAsync(string url)
		{
			Ping p = new Ping();

			try
			{
				PingReply reply = await p.SendPingAsync(url, 10000);
				if (reply.Status == IPStatus.Success)
					return true;
			}
			catch { }

			return false;
		}

		public static bool ConnectionAvailable(string strServer)
		{
			try
			{
				HttpWebRequest reqFP = (HttpWebRequest)WebRequest.Create(strServer);

				HttpWebResponse rspFP = (HttpWebResponse)reqFP.GetResponse();
				if (HttpStatusCode.OK == rspFP.StatusCode)
				{
					// HTTP = 200 - Internet connection available, server online
					rspFP.Close();
					return true;
				}
				else
				{
					// Other status - Server or connection not available
					rspFP.Close();
					return false;
				}
			}
			catch (WebException)
			{
				// Exception - connection not available
				return false;
			}
		}
		#endregion

		#region Mics Methods
		public static void PlayNotification()
		{
			SoundPlayer sp = new SoundPlayer(Properties.Resources.NotificationSound);
			sp.Play();
		}

		/// <summary>
		/// Replaces incorrect characters
		/// </summary>
		public static string FixSpecialChars(string s)
		{
			return s
				.Replace("â€™", "'")
				.Replace("â€“", "-")
				.Replace("â€", "\"");
		}

		/// <summary>
		/// Takes a series name and returns it without any brackets
		/// Must be bracketed at the end of the text
		/// </summary>
		public static string GetNameWithoutBrackets(string str)
		{
			return str.Substring(str.Length - 1, 1) == ")" ? str.Substring(0, str.IndexOf("(") - 1) : str;
		}

		/// <summary>
		/// Removes any non-alphabet characters
		/// </summary>
		public static string RemoveNonAlphabetLetters(string str)
		{
			//string[] stringsToRemove =
			//{
			//	"'", "\""
			//};

			//stringsToRemove.ToList().ForEach(x => str = str.Replace(x, ""));

			new List<string>() { "'", "\"" }.ForEach(x => str = str.Replace(x, ""));

			return str;
		}

		// Takes text and capitilizes first letter of every word 
		public static string TitleCase(string s)
		{
			return Thread.CurrentThread.CurrentCulture.TextInfo.ToTitleCase(s.ToLower());
		}

		// Simply starts a process
		public static Process StartProcess(string url)
		{
			return Process.Start(url);
		}

		//
		// Checks to see if a series starts with 'The' and puts it at the end
		// of the name
		public static string GetListedName(string str)
		{
			return str.Substring(0, 3).ToLower() == "the" ? (str.Substring(4) + ", The") : str;
		}

		//
		// Checks to see if an email is in a correct format
		public static bool IsValidEmail(string emailAddress)
		{
			const string validEmailPattern = @"^(?!\.)(""([^""\r\\]|\\[""\r\\])*""|"
											 + @"([-a-z0-9!#$%&'*+/=?^_`{|}~]|(?<!\.)\.)*)(?<!\.)"
											 + @"@[a-z0-9][\w\.-]*[a-z0-9]\.[a-z][a-z\.]*[a-z]$";

			return new Regex(validEmailPattern, RegexOptions.IgnoreCase).IsMatch(emailAddress);
		}
		#endregion

		#region Date/Time Methods
		public static int GetEpochTime()
		{
			return (int)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
		}


		//public static string ConvertDateTimeToStringIfValid(string date, string format)
		//{
		//	return IsValidDate(date) ? GetDateTime(date).ToString(format, new CultureInfo("en-US")) : date;
		//}

		public static DateTime? GetPossibleDateTime(string date)
		{
			if (DateTime.TryParse(date, out DateTime datetime))
				return datetime;

			return null;
		}

		public static DateTime GetDateTimeAsLocal(string date, string time)
		{
			// Get datetime variable with saved date and time
			DateTime dt = DateTime.Parse(date + " " + time);

			// Get local timezone
			//TimeZone localZone = TimeZone.CurrentTimeZone;
			//TimeSpan currentOffset = localZone.GetUtcOffset(DateTime.Now);
			//int difference = currentOffset.Hours - AppGlobal.thetvdbTimezone;

			/*
			
			-4 2  = 6		2 + 4 = 6
			-4 -2 = 2		-2 + 4 + 2
			-4 -6 = -2		-6 + 4 = -2

			*/

			return dt.AddHours(6); // hard code for now
		}

		public static DateTime GetDateTimeAsLocal(DateTime date)
		{
			return date.AddHours(6); // hard code for now
		}

		public static string GetDateTimeAsFormattedString(DateTime? dateTime)
		{
			return dateTime.Value.ToString(AppGlobal.Settings.DateFormat);
		}

		/// <summary>
		/// Converts a string datetime to a specific formatted date
		/// </summary>
		public static string ConvertDateTimeToString(DateTime date, string format)
		{
			return date.ToString(format);
		}

		public static DateTime GetDateTime(string date)
		{
			DateTime dt = new DateTime();
			try
			{
				dt = DateTime.Parse(date, new CultureInfo("fr-FR", true), DateTimeStyles.AssumeLocal);
			}
			catch (Exception e)
			{
				ErrorMethods.LogError(e.Message);
			}

			return dt;
		}

		// Checks to see if a date is valid
		public static bool IsValidDate(string date)
		{
			return DateTime.TryParse(date, out DateTime d);

			//return DateTime.TryParse(date, new CultureInfo("fr-FR", true), DateTimeStyles.AssumeLocal, out d);
		}
		#endregion
	}
}

using Newtonsoft.Json;
using SeriesTracker.Models;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace SeriesTracker.Core
{
	public class Request
	{
		internal const string CONTENT = "application/json";

		internal static CookieContainer COOKIECONTAINER = new CookieContainer();

		public static object Execute(string verb, string url, string obj)
		{
			var HttpRequest = CreateRequest(url, verb);

			if (obj != null)
			{
				WriteStream(ref HttpRequest, obj);
			}

			try
			{
				using (HttpWebResponse Response = (HttpWebResponse)(HttpRequest.GetResponse()))
				{
					return ReadResponse(Response);
				}
			}
			catch (WebException error)
			{
				return ReadResponseFromError(error);
			}
		}

		//public static object Execute(string verb, string url)
		//{
		//	return Execute(verb, url, null);
		//}

		public static async Task<object> ExecuteAsync(string verb, string url, string obj)
		{
			var HttpRequest = CreateRequest(url, verb);

			if (obj != null)
			{
				WriteStream(ref HttpRequest, obj);
			}

			try
			{
				using (HttpWebResponse Response = (HttpWebResponse)(await HttpRequest.GetResponseAsync()))
				{
					return await ReadResponseAsync(Response);
				}
			}
			catch (WebException error)
			{
				return ReadResponseFromError(error);
			}
		}

		//public static async Task<object> ExecuteAsync(string verb, string url)
		//{
		//	return await ExecuteAsync(verb, url, null);
		//}

		public static TvdbAPI ExecuteAndDeserialize(string verb, string url, string obj)
		{
			object response = Execute(verb, url, obj);
			return JsonConvert.DeserializeObject<TvdbAPI>(response.ToString());
		}

		public static TvdbAPI ExecuteAndDeserialize(string verb, string url)
		{
			return ExecuteAndDeserialize(verb, url, null);
		}

		public static async Task<TvdbAPI> ExecuteAndDeserializeAsync(string verb, string url, string obj)
		{
			object response = await ExecuteAsync(verb, url, obj);
			return await Task.Run(() => JsonConvert.DeserializeObject<TvdbAPI>(response.ToString()));
		}

		public static async Task<TvdbAPI> ExecuteAndDeserializeAsync(string verb, string url)
		{
			return await ExecuteAndDeserializeAsync(verb, url, null);
		}

		internal static HttpWebRequest CreateRequest(string URL, string Verb)
		{
			var basicRequest = (HttpWebRequest)WebRequest.Create(URL);
			basicRequest.ContentType = CONTENT;
			basicRequest.Method = Verb;
			basicRequest.CookieContainer = COOKIECONTAINER;

			if (Verb == "GET")
				basicRequest.Headers.Add("Authorization", "Bearer " + AppGlobal.thetvdbToken);

			return basicRequest;
		}

		internal static void WriteStream(ref HttpWebRequest HttpRequest, object obj)
		{
			using (var streamWriter = new StreamWriter(HttpRequest.GetRequestStream()))
			{
				if (obj is string)
					streamWriter.Write(obj);
				else
					streamWriter.Write(JsonConvert.SerializeObject(obj));
			}
		}

		//internal static async Task WriteStreamAsync(ref HttpWebRequest HttpRequest, object obj)
		//{
		//	using (var streamWriter = new StreamWriter(HttpRequest.GetRequestStream()))
		//	{
		//		if (obj is string)
		//			await streamWriter.WriteAsync(obj);
		//		else
		//			await streamWriter.WriteAsync(JsonConvert.SerializeObject(obj));
		//	}
		//}

		internal static string ReadResponse(HttpWebResponse HttpResponse)
		{
			if (HttpResponse != null)
			{
				using (var streamReader = new StreamReader(HttpResponse.GetResponseStream()))
				{
					return streamReader.ReadToEnd();
				}
			}

			return string.Empty;
		}

		internal static async Task<string> ReadResponseAsync(HttpWebResponse HttpResponse)
		{
			if (HttpResponse != null)
			{
				using (var streamReader = new StreamReader(HttpResponse.GetResponseStream()))
				{
					return await streamReader.ReadToEndAsync();
				}
			}

			return string.Empty;
		}

		internal static string ReadResponseFromError(WebException error)
		{
			using (var streamReader = new StreamReader(error.Response.GetResponseStream()))
			{
				return streamReader.ReadToEnd();
			}
		}
	}
}
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Web;

namespace SharpExpress
{
	/// <summary>
	/// Composite HTTP handler.
	/// </summary>
	public sealed class MultiHttpHandler : IHttpHandler, IEnumerable<KeyValuePair<string, IHttpHandler>>
	{
		private struct Entry
		{
			public readonly string Path;
			public readonly IHttpHandler Handler;
			private readonly Regex _regex;

			public Entry(string path, IHttpHandler handler)
			{
				Path = path;
				Handler = handler;
				_regex = path.WildcardRegex();
			}

			public IHttpHandler Match(HttpContext context)
			{
				return _regex.IsMatch(context.Request.Path) ? Handler : null;
			}
		}

		private readonly List<Entry> _entries = new List<Entry>();

		public void Add(string path, IHttpHandler handler)
		{
			_entries.Add(new Entry(path, handler));
		}

		public void ProcessRequest(HttpContext context)
		{
			var handler = _entries.Select(x => x.Match(context)).FirstOrDefault() ?? NotFound;
			handler.ProcessRequest(context);
		}

		public bool IsReusable
		{
			get { return true; }
		}

		public IEnumerator<KeyValuePair<string, IHttpHandler>> GetEnumerator()
		{
			foreach (var entry in _entries)
			{
				yield return new KeyValuePair<string, IHttpHandler>(entry.Path, entry.Handler);
			}
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		private static readonly IHttpHandler NotFound = new NotFoundHandler();

		private class NotFoundHandler : IHttpHandler
		{
			public void ProcessRequest(HttpContext context)
			{
				var res = context.Response;
				res.StatusCode = (int) HttpStatusCode.NotFound;
				res.ContentType = "text/plain";
				res.Write(string.Format("Resource '{0}' not found!", context.Request.Path));
				res.End();
			}

			public bool IsReusable { get; private set; }
		}
	}
}

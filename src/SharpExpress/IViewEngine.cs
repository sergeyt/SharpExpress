using System.IO;

namespace SharpExpress
{
	public interface IViewEngine
	{
		void Render(Template template, object data, TextWriter output);
	}

	public struct Template
	{
		public readonly string Name;
		public readonly string Content;

		public Template(string name, string content)
		{
			Name = name;
			Content = content;
		}
	}
}

using System;
using System.Globalization;
using System.IO;
using Spark;

namespace SharpExpress.Spark
{
	public class SparkEngine : IViewEngine
	{
		private readonly SparkViewEngine _engine;

		public SparkEngine()
		{
			// TODO read settings from config
			var settings = new SparkSettings
			{
				DefaultLanguage = LanguageType.CSharp
			};
			settings.SetPageBaseType(typeof(ViewBase));
			_engine = new SparkViewEngine();
		}

		public void Render(Template template, object data, TextWriter output)
		{
			var descriptor = new SparkViewDescriptor().AddTemplate(template.Name + ".spark");

			var view = (ViewBase)_engine.CreateInstance(descriptor);
			try
			{
				view.Data = data;
				view.RenderView(output);
			}
			finally
			{
				_engine.ReleaseInstance(view);
			}
		}
	}

	internal abstract class ViewBase : AbstractSparkView
	{
		public object Data { get; set; }

		// TODO Eval for late binding
		public object Eval(string expression)
		{
			// TODO
			return null;
		}

		public string Eval(string expression, string format)
		{
			var val = Eval(expression);
			var formattable = val as IFormattable;
			if (formattable != null)
			{
				return formattable.ToString(format, CultureInfo.CurrentCulture);
			}
			return val != null ? val.ToString() : "";
		}
	}
}

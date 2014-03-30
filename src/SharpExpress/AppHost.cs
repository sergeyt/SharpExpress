using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Web;
using System.Web.Configuration;
using System.Web.Hosting;

namespace SharpExpress
{
	// TODO run server in separate domain

	internal sealed class AppHost : MarshalByRefObject, IApplicationHost
	{
		private const string SiteName = "express-site";
		private const string SiteId = "1";

		private readonly HttpServerSettings _settings;

		public AppHost(HttpServerSettings settings)
		{
			if (settings == null) throw new ArgumentNullException("settings");

			_settings = settings;

			InitDomain(settings);
			InitHostingEnvironment();
		}

		public void Init()
		{
		}

		private void InitHostingEnvironment()
		{
			try
			{
				var env = new HostingEnvironment();

				var type = env.GetType();
				var init = type
					.GetMethods(BindingFlags.Instance | BindingFlags.NonPublic)
					.FirstOrDefault(m => m.Name == "Initialize");

				var args = new object[init.GetParameters().Length];
				args[0] = ApplicationManager.GetApplicationManager();
				args[1] = this;
				args[2] = GetConfigMapPathFactory();

				init.Invoke(env, args);
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
			}
		}

		public static void InitDomain(HttpServerSettings settings)
		{
			var uniqueAppString = string.Format(
					CultureInfo.InvariantCulture, "{0}{1}:{2}",
					settings.VirtualDir, settings.PhisycalDir, settings.Port)
					.ToLowerInvariant();
			var appId = (uniqueAppString.GetHashCode()).ToString("x", CultureInfo.InvariantCulture);

			var domain = Thread.GetDomain();
			domain.SetData(".appDomain", "express-domain");
			domain.SetData(".appId", appId);
			domain.SetData(".appPath", settings.PhisycalDir);
			domain.SetData(".appVPath", settings.VirtualDir);
			domain.SetData(".domainId", appId);
		}

		public string GetVirtualPath()
		{
			return _settings.VirtualDir;
		}

		public string GetPhysicalPath()
		{
			return _settings.PhisycalDir;
		}

		public IConfigMapPathFactory GetConfigMapPathFactory()
		{
			return new ConfigMapPathFactoryImpl(this);
		}

		public IntPtr GetConfigToken()
		{
			return IntPtr.Zero;
		}

		public string GetSiteName()
		{
			return SiteName;
		}

		public string GetSiteID()
		{
			return SiteId;
		}

		public void MessageReceived()
		{
		}

		private class ConfigMapPathFactoryImpl : IConfigMapPathFactory, IConfigMapPath
		{
			private readonly AppHost _host;

			public ConfigMapPathFactoryImpl(AppHost host)
			{
				_host = host;
			}

			public IConfigMapPath Create(string virtualPath, string physicalPath)
			{
				return this;
			}

			public string GetMachineConfigFilename()
			{
				var dir = Path.Combine(RuntimeEnvironment.GetRuntimeDirectory(), "Config");
				return Path.Combine(dir, "machine.config");
			}

			public string GetRootWebConfigFilename()
			{
				return null;
			}

			public void GetPathConfigFilename(string siteID, string path, out string directory, out string baseName)
			{
				directory = Environment.CurrentDirectory;
				baseName = "web.config";
			}

			public void GetDefaultSiteNameAndID(out string siteName, out string siteID)
			{
				siteName = SiteName;
				siteID = SiteId;
			}

			public void ResolveSiteArgument(string siteArgument, out string siteName, out string siteID)
			{
				siteName = SiteName;
				siteID = SiteId;
			}

			public string MapPath(string siteID, string path)
			{
				if (string.IsNullOrEmpty(path))
				{
					return null;
				}

				if (path.StartsWith("/"))
				{
					path = path.Substring(1);
				}

				return Path.Combine(_host.GetPhysicalPath(), path);
			}

			public string GetAppPathForPath(string siteID, string path)
			{
				return "";
			}
		}

		// TODO use this hack for running in separate domain

		/// <remarks>
		/// This is Dmitry's hack to enable running outside of GAC.
		/// There are some errors being thrown when running in proc
		/// </remarks>
		private object CreateWorkerAppDomainWithHost(string virtualPath, string physicalPath, Type hostType, int port)
		{
			string uniqueAppString = string.Format(CultureInfo.InvariantCulture, "{0}{1}:{2}", virtualPath, physicalPath, port).ToLowerInvariant();
			var appId = (uniqueAppString.GetHashCode()).ToString("x", CultureInfo.InvariantCulture);

			// create BuildManagerHost in the worker app domain
			var buildManagerHostType = typeof(HttpRuntime).Assembly.GetType("System.Web.Compilation.BuildManagerHost");
			var applicationManager = ApplicationManager.GetApplicationManager();

			var buildManagerHost = applicationManager.CreateObject(appId, buildManagerHostType, virtualPath, physicalPath, false);

			// call BuildManagerHost.RegisterAssembly to make Host type loadable in the worker app domain
			buildManagerHostType.InvokeMember("RegisterAssembly",
											  BindingFlags.Instance | BindingFlags.InvokeMethod | BindingFlags.NonPublic,
											  null,
											  buildManagerHost,
											  new object[] { hostType.Assembly.FullName, hostType.Assembly.Location });

			// create Host in the worker app domain
			// FIXME: getting FileLoadException Could not load file or assembly 'WebDev.WebServer20, Version=4.0.1.6, Culture=neutral, PublicKeyToken=f7f6e0b4240c7c27' or one of its dependencies. Failed to grant permission to execute. (Exception from HRESULT: 0x80131418)
			// when running dnoa 3.4 samples - webdev is registering trust somewhere that we are not
			return applicationManager.CreateObject(appId, hostType, virtualPath, physicalPath, false);
		}
	}
}

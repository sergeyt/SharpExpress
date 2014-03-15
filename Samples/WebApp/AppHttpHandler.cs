using SharpExpress;

namespace WebApp
{
	public class AppHttpHandler : ExpressApplication
	{
		public AppHttpHandler()
		{
			Get("app/hi/{user}", req => req.Text(string.Format("Hi, {0}!", req.Param<string>("user"))));
		}
	}
}
using System.Web.Services;

namespace ConsoleServer
{
	internal class MathService
	{
		[WebMethod]
		public object Square(double x)
		{
			return x * x;
		}
	}
}

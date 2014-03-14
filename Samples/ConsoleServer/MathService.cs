using System.Web.Services;

namespace ConsoleServer
{
	internal class MathService
	{
		[WebMethod]
		public double Square(double x)
		{
			return x * x;
		}

		[WebMethod]
		public double Add(double x, double y)
		{
			return x + y;
		}
	}
}

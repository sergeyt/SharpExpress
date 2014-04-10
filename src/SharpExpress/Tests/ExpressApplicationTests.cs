using System.Collections.Specialized;
using System.IO;
#if NUNIT
using System;
using System.Web;

using Moq;
using NUnit.Framework;

namespace SharpExpress.Tests
{
	[TestFixture]
	public class ExpressApplicationTests
	{
		[Test]
		public void ProcessStubContext()
		{
			var ctx = new Mock<HttpContextBase>();
			var app = new ExpressApplication();
			Assert.IsFalse(app.Process(ctx.Object));
		}

		[Test]
		public void GetText()
		{
			var app = new ExpressApplication();
			app.Get("test", req => req.Text("test"));

			var request = new Mock<HttpRequestBaseImpl> {CallBase = true};
			request.Setup(x => x.HttpMethod).Returns("GET");
			request.Setup(x => x.Url).Returns(new Uri("http://localhost/test"));
			request.Setup(x => x.Headers).Returns(new NameValueCollection());
			
			var response = new Mock<HttpResponseBase>();

			var ctx = new Mock<HttpContextBase>();
			ctx.Setup(x => x.Request).Returns(request.Object);
			ctx.Setup(x => x.Response).Returns(response.Object);

			Assert.IsTrue(app.Process(ctx.Object));
		}
	}
}
#endif
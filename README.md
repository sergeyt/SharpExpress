[![Build Status](https://drone.io/github.com/sergeyt/SharpExpress/status.png)](https://drone.io/github.com/sergeyt/SharpExpress/latest)
[![Build status](https://ci.appveyor.com/api/projects/status/c3ji8hbtc8qgu62v)](https://ci.appveyor.com/project/sergeyt/sharpexpress)
[![NuGet Version](http://img.shields.io/nuget/v/SharpExpress.svg?style=flat)](https://www.nuget.org/packages/SharpExpress/)
[![NuGet Downloads](http://img.shields.io/nuget/dt/SharpExpress.svg?style=flat)](https://www.nuget.org/packages/SharpExpress/)

# SharpExpress

Functional API to process HTTP requests with ASP.NET MVC-like routing inspired by [express.js](http://expressjs.com/)

## Features
* Lightweight library - now ~164kb
* Extensible
* Built-in light http server
* Based on .NET BCL only without external dependencies
* Using System.Web.Routing for url routing
* Compilable and runnable on Mono
* Runnable in ASP.NET applications since ExpressApplication implements IHttpHandler
* Use WebService extension to handle requests using existing web service class
* Use HttpHandler extension to handle requests using existing IHttpHandler implementations
* Use Static extension to serve static files
* Use Embedded extension to serve emmbedded resources of application assemblies

## Examples

### Simple Static Server

```c#
var app = new ExpressApplication();
app.Static("", Environment.CurrentDirectory);

var settings = new HttpServerSettings { Port = 81, WorkerCount = 4 };
using (new HttpServer(app, settings))
{
	Console.WriteLine("Listening port {0}. Press enter to stop the server.", port);
	Console.ReadLine();
}
```

### Routes API

```c#
var app = new ExpressApplication();
app.Get(
	"",
	req => req.Text("Hi!")
	);

app.Get(
	"text/{x}/{y}",
	req => req.Text(
		string.Format("x={0}, y={1}",
			req.RouteData.Values["x"],
			req.RouteData.Values["y"])
		));

app.Get(
	"json/{x}/{y}",
	req => req.Json(
		new
		{
			x = req.RouteData.Values["x"],
			y = req.RouteData.Values["y"]
		}));

var settings = new HttpServerSettings { Port = 81, WorkerCount = 4 };
using (new HttpServer(app, settings))
{
	Console.WriteLine("Listening port {0}. Press enter to stop the server.", port);
	Console.ReadLine();
}
```

### Functional API

```c#
Func<double, double> square = x => x * x;

var app = new ExpressApplication();
app.Json("math/json/square/{x}", square)
   .Xml("math/xml/square/{x}", square);

var settings = new HttpServerSettings { Port = 81, WorkerCount = 4 };
using (new HttpServer(app, settings))
{
	Console.WriteLine("Listening port {0}. Press enter to stop the server.", port);
	Console.ReadLine();
}
```

### WebService Extension

```c#
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

var app = new ExpressApplication();
app.WebService<MathService>("math.svc");

var settings = new HttpServerSettings { Port = 81, WorkerCount = 4 };
using (new HttpServer(app, settings))
{
	Console.WriteLine("Listening port {0}. Press enter to stop the server.", port);
	Console.ReadLine();
}
```

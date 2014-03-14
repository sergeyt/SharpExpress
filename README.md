[![Build Status](https://drone.io/github.com/sergeyt/SharpExpress/status.png)](https://drone.io/github.com/sergeyt/SharpExpress/latest)
[![NuGet version](https://badge.fury.io/nu/SharpExpress.png)](http://badge.fury.io/nu/SharpExpress)

# SharpExpress

Simple HTTP handler with ASP.NET MVC-like routing inspired by [express.js](http://expressjs.com/)

## Features
* Lightweight library - now ~164kb
* Built-in light http server
* Based on .NET BCL only without external dependencies
* Using System.Web.Routing for url routing
* Compilable and runnable on Mono

## Examples

### Simple Static Server

```c#
var app = new ExpressApplication();
app.Static("", Environment.CurrentDirectory);

var port = 81;
using (new HttpServer(app, port, 4))
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

var port = 1111;
var workerCount = 4;
using (var server = new HttpServer(app, port, workerCount))
{
	Console.WriteLine("Listening port {0}. Press enter to stop the server.", port);
	Console.ReadLine();
}
```

### LINQ API

```c#
Func<double, double> square = x => x * x;

var app = new ExpressApplication();
app.Json("math/json/square/{x}", square)
   .Xml("math/xml/square/{x}", square);

var port = 81;
using (new HttpServer(app, port, 4))
{
	Console.WriteLine("Listening port {0}. Press enter to stop the server.", port);
	Console.ReadLine();
}
```

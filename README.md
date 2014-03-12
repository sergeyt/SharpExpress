# SharpExpress

Small HTTP server for .NET inspired by [express.js](http://expressjs.com/)

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
app.Json<double, double>("math/json/square/{x}", x => square(x))
   .Xml<double, double>("math/xml/square/{x}", x => square(x));

var port = 81;
using (new HttpServer(app, port, 4))
{
	Console.WriteLine("Listening port {0}. Press enter to stop the server.", port);
	Console.ReadLine();
}
```

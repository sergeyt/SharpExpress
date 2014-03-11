# SharpExpress

Small HTTP server for .NET inspired by [express.js](http://expressjs.com/)

## Example

```c#
var port = 1111;
using (var server = new HttpServer(port))
{
	var app = server.App;
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

	Console.WriteLine("Listening port {0}. Press enter to stop the server.", port);
	Console.ReadLine();
}
```

# ASP.NET Core Html Injection Example

This project shows how we can intercept an HTML response from middleware and rewrite it.

It uses a neat trick that replaces the `HttpResponseStream` with a `MemoryStream` before any other middleware has started writing to the response.

This allows us to manipulate the buffer and ultimately write to the `HttpResponseStream` (which we hold in a temporary variable).

**Note: We want to register our middleware before the `StaticFileMiddleware`, since we could be serving static HTML files. That said, register it how you want it to behave. I'm not the boss of you.**

## License

Don't sue me, do whatever you want with this code.
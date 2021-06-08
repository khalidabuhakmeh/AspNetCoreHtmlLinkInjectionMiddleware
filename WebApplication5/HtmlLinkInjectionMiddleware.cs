using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Random = System.Security.Cryptography.RandomNumberGenerator;

namespace WebApplication5
{
    public class HtmlLinkInjectionMiddleware : IMiddleware
    {
        private readonly IReadOnlyList<string> _links = new[]
        {
            new { href = "https://google.com", text = "haute OSS code in your area!" },
            new { href = "https://google.com", text = "Find OSS success with this one trick!" }
        }
        .Select(l => $"<p style=\"text-align:center;display: block; width: 100%\"><strong><a href=\"{l.href}\">{l.text}</a></strong></p>")
        .ToList();
        
        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            context.Response.OnStarting(state =>
            {
                if (state is not HttpContext httpContext) 
                    return Task.CompletedTask;
                
                // static middleware will throw
                // if you have content-length
                // when checking this header,
                // so let's remove it 😈
                var response = httpContext.Response;
                response.Headers.Remove("Content-Length");

                return Task.CompletedTask;
            }, context);
            
            var stream = context.Response.Body;
            await using var buffer = new MemoryStream();
            context.Response.Body = buffer;
            await next(context);

            buffer.Seek(0, SeekOrigin.Begin);
            
            // when using "using var" the
            // variable becomes immutable
            using var reader = new StreamReader(buffer);
            
            var body = await reader.ReadToEndAsync();
            var isHtml = context.Response.ContentType?.Contains("text/html");
            
            if (context.Response.StatusCode == (int) HttpStatusCode.OK && isHtml == true)
            {
                var link = _links
                    .OrderBy(_ => Random.GetInt32(int.MaxValue))
                    .First();
                        
                body = body.Replace("</body>", $"{link}</body>");

                await using var memoryStream = new MemoryStream();
                var bytes = Encoding.UTF8.GetBytes(body);
                memoryStream.Write(bytes, 0, bytes.Length);
                memoryStream.Seek(0, SeekOrigin.Begin);
                await memoryStream.CopyToAsync(stream);
            }
            else
            {
                buffer.Seek(0, SeekOrigin.Begin);
                await buffer.CopyToAsync(stream);
            }
        }
    }
}
using Core.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InfrastructureService
{
    public class LogMiddleware
    {
        const string MessageTemplate =
               "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms with {RequestBody} and {ResponseBody}";

        static readonly ILogger Log = Serilog.Log.ForContext<LogMiddleware>();

        readonly RequestDelegate _next;

        public LogMiddleware(RequestDelegate next)
        {
            if (next == null) throw new ArgumentNullException(nameof(next));
            _next = next;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            if (httpContext == null) throw new ArgumentNullException(nameof(httpContext));
            var stopWatch = Stopwatch.StartNew();

            try
            {
                var request = httpContext.Request;
                if (request.Path.StartsWithSegments(new PathString("/api")))
                {
                    var requestTime = DateTime.UtcNow;
                    var requestBodyContent = await ReadRequestBody(request);
                    var originalBodyStream = httpContext.Response.Body;
                    using (var responseBody = new MemoryStream())
                    {
                        var response = httpContext.Response;
                        response.Body = responseBody;
                        await _next(httpContext);
                        stopWatch.Stop();

                        string responseBodyContent = null;
                        responseBodyContent = await ReadResponseBody(response);
                        await responseBody.CopyToAsync(originalBodyStream);

                        var statusCode = httpContext.Response?.StatusCode;
                        var level = statusCode > 499 ? LogEventLevel.Error : LogEventLevel.Information;

                        SafeLog(request.Path,
                            ref requestBodyContent,
                            ref responseBodyContent);

                        var log = level == LogEventLevel.Error ? LogForErrorContext(httpContext) : Log;
                        log.Write(level, MessageTemplate, httpContext.Request.Method, httpContext.Request.Path, statusCode, stopWatch.Elapsed.TotalMilliseconds, requestBodyContent, responseBodyContent);
                    }
                }
                else
                {
                    await _next(httpContext);
                }
            }
            catch (Exception ex) when (LogException(httpContext, stopWatch, ex))
            {  }
        }

        static bool LogException(HttpContext httpContext, Stopwatch sw, Exception ex)
        {
            sw.Stop();

            LogForErrorContext(httpContext)
                .Error(ex, MessageTemplate, httpContext.Request.Method, httpContext.Request.Path, 500, sw.Elapsed.TotalMilliseconds);

            return false;
        }

        static ILogger LogForErrorContext(HttpContext httpContext)
        {
            var request = httpContext.Request;

            var result = Log
                .ForContext("RequestHeaders", request.Headers.ToDictionary(h => h.Key, h => h.Value.ToString()), destructureObjects: true)
                .ForContext("RequestHost", request.Host)
                .ForContext("RequestProtocol", request.Protocol);

            if (request.HasFormContentType)
                result = result.ForContext("RequestForm", request.Form.ToDictionary(v => v.Key, v => v.Value.ToString()));

            return result;
        }
        private async Task<string> ReadRequestBody(HttpRequest request)
        {
            //request.EnableRewind();
           
            var buffer = new byte[Convert.ToInt32(request.ContentLength)];
            await request.Body.ReadAsync(buffer, 0, buffer.Length);
            var bodyAsText = Encoding.UTF8.GetString(buffer);
            request.Body.Seek(0, SeekOrigin.Begin);

            return bodyAsText;
        }

        private async Task<string> ReadResponseBody(HttpResponse response)
        {
            response.Body.Seek(0, SeekOrigin.Begin);
            var bodyAsText = await new StreamReader(response.Body).ReadToEndAsync();
            response.Body.Seek(0, SeekOrigin.Begin);

            return bodyAsText;
        }

        private void SafeLog(string path,
                            ref string requestBody,
                            ref string responseBody)
        {
            int limit = 500;

            if (requestBody.Length > limit)
            {
                requestBody = $"(Truncated to {limit} chars) {requestBody.Substring(0, limit)}";
            }

            if (responseBody.Length > limit)
            {
                responseBody = $"(Truncated to {limit} chars) {responseBody.Substring(0, limit)}";
            }
            if (path.ToLower().StartsWith("/api/account/resetpassword"))
            {
                requestBody = "(Request logging disabled for /api/Customers)";
            }
            if (path.ToLower().StartsWith("/api/account/authenticate"))
            {
                requestBody = "(Request logging disabled for /api/Account/authenticate)";
            }
        }
    }
}

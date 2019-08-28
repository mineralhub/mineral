using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using System.IO.Compression;

namespace Mineral.Common.Net.RPC
{
    public abstract class RpcServer : IDisposable
    {
        #region Field
        private IWebHost host;
        #endregion


        #region Property
        #endregion


        #region Contructor
        #endregion


        #region Event Method
        #endregion


        #region Internal Method
        protected abstract JObject Process(JToken id, string method, JArray parameters);

        protected async Task ProcessAsync(HttpContext context)
        {
            context.Response.Headers["Access-Control-Allow-Origin"] = "*";
            context.Response.Headers["Access-Control-Allow-Methods"] = "GET, POST";
            context.Response.Headers["Access-Control-Allow-Headers"] = "Content-Type";
            context.Response.Headers["Access-Control-Max-Age"] = "31536000";
            if (context.Request.Method != "GET" && context.Request.Method != "POST")
                return;

            JObject request = null;
            if (context.Request.Method == "GET")
            {
                string jsonrpc = context.Request.Query["jsonrpc"];
                string id = context.Request.Query["id"];
                string method = context.Request.Query["method"];
                string type = context.Request.Query["type"];
                string parameters = context.Request.Query["params"];
                if (!string.IsNullOrEmpty(id) && !string.IsNullOrEmpty(method) && !string.IsNullOrEmpty(parameters))
                {
                    try
                    {
                        parameters = Encoding.UTF8.GetString(Convert.FromBase64String(parameters));
                    }
                    catch (FormatException) { }
                    request = new JObject();
                    if (!string.IsNullOrEmpty(jsonrpc))
                        request["jsonrpc"] = jsonrpc;
                    request["id"] = double.Parse(id);
                    request["method"] = method;
                    request["type"] = type;
                    request["params"] = JArray.Parse(parameters);
                }
            }
            else if (context.Request.Method == "POST")
            {
                using (StreamReader reader = new StreamReader(context.Request.Body))
                {
                    try
                    {
                        request = JObject.Parse(reader.ReadToEnd());
                    }
                    catch (FormatException) { }
                }
            }
            JObject response = RpcMessage.CreateResponse(request["id"]);
            if (request == null)
            {
                response["error"] = RpcMessage.CreateErrorResult(null, RpcMessage.PARSE_ERROR, "Parse error");
            }
            else
            {
                response = ProcessRequest(context, request);
            }
            if (response == null)
                return;
            context.Response.ContentType = "application/json-rpc";
            await context.Response.WriteAsync(response.ToString(), Encoding.UTF8);
        }

        protected JObject ProcessRequest(HttpContext context, JObject request)
        {
            if (!request.ContainsKey("id"))
                return null;
            if (!request.ContainsKey("method") || !request.ContainsKey("params") || !(request["params"] is JArray))
                return RpcMessage.CreateErrorResult(request["id"], RpcMessage.INVALID_REQUEST, "Invalid Request");
            JObject result = null;
            JObject response = RpcMessage.CreateResponse(request["id"]);
            try
            {
                JToken id = request["id"];
                string method = request["method"].Value<string>();
                JArray parameters = (JArray)request["params"];

                result = Process(id, method, parameters);
                JToken token = null;
                if (result.TryGetValue("error", out token))
                    response["error"] = token;
                else
                    response["result"] = result;

            }
            catch (Exception e)
            {
                result = RpcMessage.CreateErrorResult(request["id"], e.HResult, e.Message);
                JToken token = null;
                if (result.TryGetValue("error", out token))
                    response["error"] = token;
                else
                    response["error"] = result;
                return response;
            }
            return response;
        }
        #endregion


        #region External Method
        public void Start(int port, long maxBodySize = 10 * 1024, string sslCert = null, string password = null)
        {
            host = new WebHostBuilder().UseKestrel(options =>
            {
                options.Listen(IPAddress.Any, port, listenOptions =>
                {
                    if (!string.IsNullOrEmpty(sslCert))
                        listenOptions.UseHttps(sslCert, password);
                });
                options.Limits.MaxRequestBodySize = maxBodySize;
            }
            )
            .Configure(app =>
            {
                app.UseResponseCompression();
                app.Run(ProcessAsync);
            })
            .ConfigureServices(services =>
            {
                services.AddResponseCompression(options =>
                {
                    options.Providers.Add<GzipCompressionProvider>();
                    options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(new[] { "application/json-rpc" });
                });

                services.Configure<GzipCompressionProviderOptions>(options =>
                {
                    options.Level = CompressionLevel.Fastest;
                });
            })
            .Build();
            host.Start();
        }

        public void Dispose()
        {
            if (this.host != null)
            {
                this.host.Dispose();
                this.host = null;
            }
        }
        #endregion
    }
}

using System;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using Sky;
using Sky.Core;

namespace Sky.Network.RPC
{
    public class RpcServer : IDisposable
    {
        protected readonly LocalNode _localNode;
        IWebHost _host;

        public RpcServer(LocalNode localNode)
        {
            _localNode = localNode;
        }

        static JObject CreateErrorResponse(JToken id, int code, string message, string data = null)
        {
            JObject response = CreateResponse(id);
            response["error"] = new JObject();
            response["error"]["code"] = code;
            response["error"]["message"] = message;
            if (data != null)
                response["error"]["data"] = data;
            return response;
        }

        static JObject CreateResponse(JToken id)
        {
            JObject response = new JObject();
            response["jsonrpc"] = "2.0";
            response["id"] = id;
            return response;
        }

        public void Dispose()
        {
            if (_host != null)
            {
                _host.Dispose();
                _host = null;
            }
        }

        protected virtual JObject Process(string method, JArray parameters)
        {
            switch (method)
            {
                case "getheight":
                    JObject json = new JObject();
                    json["blockHeight"] = Blockchain.Instance.CurrentBlockHeight;
                    json["headerHeight"] = Blockchain.Instance.CurrentHeaderHeight;
                    return json;
                case "getblock":
                    Block block = null;
                    if (parameters[0].Type == JTokenType.Integer)
                        block = Blockchain.Instance.GetBlock(parameters[0].Value<int>());
                    else
                        block = Blockchain.Instance.GetBlock(UInt256.FromHexString(parameters[0].Value<string>()));
                    return block.ToJson();
                case "account":
                    string address = parameters[0].Value<string>();
                    UInt160 addressHash = Wallets.WalletAccount.ToAddressHash(address);
                    AccountState state = Blockchain.Instance.GetAccountState(addressHash);
                    return state.ToJson();
            }
            return null;
        }

        async Task ProcessAsync(HttpContext context)
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
            JObject response;
            if (request == null)
            {
                response = CreateErrorResponse(null, -32700, "Parse error");
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

        JObject ProcessRequest(HttpContext context, JObject request)
        {
            if (!request.ContainsKey("id"))
                return null;
            if (!request.ContainsKey("method") || !request.ContainsKey("params") || !(request["params"] is JArray))
                return CreateErrorResponse(request["id"], -32600, "Invalid Request");
            JObject result = null;
            try
            {
                string method = request["method"].Value<string>();
                JArray parameters = (JArray)request["params"];
                result = Process(method, parameters);
            }
            catch (Exception e)
            {
#if DEBUG
                return CreateErrorResponse(request["id"], e.HResult, e.Message, e.StackTrace);
#else
                return CreateErrorResponse(request["id"], e.HResult, e.Message);
#endif
            }
            JObject response = CreateResponse(request["id"]);
            response["result"] = result;
            return response;
        }

        public void Start(ushort port, string sslCert = null, string password = null)
        {
            _host = new WebHostBuilder().UseKestrel(options => options.Listen(IPAddress.Any, port, listenOptions =>
            {
                if (!string.IsNullOrEmpty(sslCert))
                    listenOptions.UseHttps(sslCert, password);
            }))
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
            _host.Start();
        }
    }
}

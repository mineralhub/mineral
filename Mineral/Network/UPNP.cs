using System;
using System.IO;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Xml;
using System.Linq;

namespace Mineral.Network
{
    public class UPNP
    {
        private static string _serviceUrl;
        public static readonly TimeSpan TimeOut = TimeSpan.FromSeconds(3);

        public static bool Enable => string.IsNullOrEmpty(_serviceUrl) == false;

        public static bool Discovery()
        {
            if (string.IsNullOrEmpty(_serviceUrl) == false)
                return true;

            Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            s.ReceiveTimeout = (int)TimeOut.TotalMilliseconds;
            s.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, 1);
            string req = "M-SEARCH * HTTP/1.1\r\n" +
                "HOST: 239.255.255.250:1900\r\n" +
                "ST:upnp:rootdevice\r\n" +
                "MAN:\"ssdp:discover\"\r\n" +
                "MX:3\r\n\r\n";
            byte[] data = Encoding.ASCII.GetBytes(req);
            IPEndPoint ep = new IPEndPoint(IPAddress.Broadcast, 1900);
            DateTime reqTime = DateTime.Now;

            s.SendTo(data, ep);
            s.SendTo(data, ep);
            s.SendTo(data, ep);

            byte[] buffer = new byte[0x1000];
            do
            {
                int length;
                try
                {
                    length = s.Receive(buffer);
                }
                catch (SocketException)
                {
                    continue;
                }
                string res = Encoding.ASCII.GetString(buffer, 0, length).ToLower();
                if (res.Contains("upnp:rootdevice"))
                {
                    res = res.Substring(res.ToLower().IndexOf("location:") + 9);
                    res = res.Substring(0, res.IndexOf("\r")).Trim();
                    if (!string.IsNullOrEmpty(_serviceUrl = GetServiceUrl(res)))
                        return true;
                }
            }
            while (DateTime.Now - reqTime < TimeOut);
            return false;
        }

        private static string GetServiceUrl(string res)
        {
            try
            {
                HttpWebRequest request = WebRequest.CreateHttp(res);
                WebResponse response = request.GetResponse();
                XmlDocument doc = new XmlDocument();
                doc.Load(response.GetResponseStream());
                XmlNamespaceManager nsmgr = new XmlNamespaceManager(doc.NameTable);
                nsmgr.AddNamespace("tns", "urn:schemas-upnp-org:device-1-0");
                XmlNode typenode = doc.SelectSingleNode("//tns:device/tns:deviceType/text()", nsmgr);
                if (!typenode.Value.Contains("InternetGatewayDevice"))
                    return null;
                XmlNode node = doc.SelectSingleNode("//tns:service[contains(tns:serviceType,\"WANIPConnection\")]/tns:controlURL/text()", nsmgr);
                if (node == null)
                    return null;
                XmlNode evtnode = doc.SelectSingleNode("//tns:service[contains(tns:serviceType,\"WANIPConnection\")]/tns:eventSubURL/text()", nsmgr);
                return CombineUrls(res, node.Value);
            }
            catch { return null; }
        }

        private static string CombineUrls(string res, string p)
        {
            int n = res.IndexOf("://");
            n = res.IndexOf('/', n + 3);
            return res.Substring(0, n) + p;
        }

        public static IPAddress GetExternalIP()
        {
            if (string.IsNullOrEmpty(_serviceUrl))
                throw new Exception("_serviceUrl is null or empty");
            XmlDocument doc = SOAPRequest(_serviceUrl, "<u:GetExternalIPAddress xmlns:u=\"urn:schemas-upnp-org:service:WANIPConnection:1\">" +
                "</u:GetExternalIPAddress>", "GetExternalIPAddress");
            if (doc == null)
                return null;
            XmlNamespaceManager nsmgr = new XmlNamespaceManager(doc.NameTable);
            nsmgr.AddNamespace("tns", "urn:schemas-upnp-org:device-1-0");
            string ip = doc.SelectSingleNode("//NewExternalIPAddress/text()", nsmgr).Value;
            return IPAddress.Parse(ip);
        }

        public static void PortMapping(int port, ProtocolType protocol, string desc)
        {
            if (string.IsNullOrEmpty(_serviceUrl))
                throw new Exception("_serviceUrl is null or empty");
            XmlDocument xdoc = SOAPRequest(_serviceUrl, "<u:AddPortMapping xmlns:u=\"urn:schemas-upnp-org:service:WANIPConnection:1\">" +
                "<NewRemoteHost></NewRemoteHost><NewExternalPort>" + port.ToString() + "</NewExternalPort><NewProtocol>" + protocol.ToString().ToUpper() + "</NewProtocol>" +
                "<NewInternalPort>" + port.ToString() + "</NewInternalPort><NewInternalClient>" + (Dns.GetHostAddresses(Dns.GetHostName())).First(p => p.AddressFamily == AddressFamily.InterNetwork).ToString() +
                "</NewInternalClient><NewEnabled>1</NewEnabled><NewPortMappingDescription>" + desc +
                "</NewPortMappingDescription><NewLeaseDuration>0</NewLeaseDuration></u:AddPortMapping>", "AddPortMapping");
        }

        private static XmlDocument SOAPRequest(string url, string soap, string func)
        {
            try
            {
                string req = "<?xml version=\"1.0\"?>" +
                    "<s:Envelope xmlns:s=\"http://schemas.xmlsoap.org/soap/envelope/\" s:encodingStyle=\"http://schemas.xmlsoap.org/soap/encoding/\">" +
                    "<s:Body>" +
                    soap +
                    "</s:Body>" +
                    "</s:Envelope>";
                HttpWebRequest request = WebRequest.CreateHttp(url);
                request.Method = "POST";
                byte[] data = Encoding.UTF8.GetBytes(req);
                request.Headers["SOAPACTION"] = "\"urn:schemas-upnp-org:service:WANIPConnection:1#" + func + "\"";
                request.ContentType = "text/xml; charset=\"utf-8\"";
                Stream reqs = request.GetRequestStream();
                reqs.Write(data, 0, data.Length);
                WebResponse response = request.GetResponse();
                Stream ress = response.GetResponseStream();
                XmlDocument doc = new XmlDocument();
                doc.Load(ress);
                return doc;
            }
            catch { return null; }
        }
    }
}

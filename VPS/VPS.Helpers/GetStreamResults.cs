using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Xml.Linq;
using VPS.Helpers.Logging;
using System;
using System.Xml;

namespace VPS.Helpers
{
    public class GetStreamResults : IGetStreamResults
    {
        private readonly ILoggerAdapter<GetStreamResults> _log;
        public GetStreamResults(ILoggerAdapter<GetStreamResults> log)
        {
            _log = log;
        }

        public async Task<string> GetResults<T>(T requestObject, NetworkStream networkStream)
        {
            string xmlString = RemoveAllNamespaces(XmlConvert.Serialize(requestObject));
            return await RunCommand(networkStream, xmlString);
        }

        public string RemoveAllNamespaces(string? xmlDocument)
        {
            if (string.IsNullOrWhiteSpace(xmlDocument)) return string.Empty;

            XElement xmlDocumentWithoutNs = RemoveAllNamespaces(XElement.Parse(xmlDocument));

            return xmlDocumentWithoutNs.ToString().Replace("\r", "").Replace("\n", "");
        }

        public async Task<string> GetNetworkResult<T>(T requestObject, NetworkStream networkStream)
        {
            return await GetResults(requestObject, networkStream);
        }

        #region Helpers
        private async Task<string> RunCommand(NetworkStream stream, string cmd)
        {
            StringBuilder result = new();
            try
            {
                byte[] data = Encoding.UTF8.GetBytes(cmd + "\r");
                stream.Write(data, 0, data.Length);

                int count = 0;

                do
                {

                    if (count > 20) throw new FormatException("Too many loops");

                    byte[]? response = new byte[16 * 1024];
                    int bytesRead = await stream.ReadAsync(response);
                    result.Append(Encoding.ASCII.GetString(response.Take(bytesRead).ToArray()).Replace("\n", ""));
                    count++;

                } while (!result.ToString().EndsWith("</response>"));

            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Command:{cmd}, Error Message: {message}", MethodBase.GetCurrentMethod()?.Name ?? string.Empty, cmd, ex.Message);
            }

            return result.ToString();
        }

        private static XElement RemoveAllNamespaces(XElement xmlDocument)
        {

            if (!xmlDocument.HasElements)
            {
                XElement xElement = new(xmlDocument.Name.LocalName)
                {
                    Value = xmlDocument.Value
                };

                foreach (XAttribute attribute in xmlDocument.Attributes())
                    xElement.Add(attribute);

                return xElement;
            }

            return new XElement(xmlDocument.Name.LocalName, xmlDocument.Elements().Select(el => RemoveAllNamespaces(el)));
        }
        #endregion
    }
}
